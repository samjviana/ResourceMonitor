using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    class Server
    {
        private int port;
        private HttpListener listener;
        private Thread listenerThread;
        private Dictionary<string, string> computerDatabase;
        private Dictionary<string, DateTime> computerTimes;
        private List<string> computerList;
        private DateTime currentTime;
        private readonly MainForm mainForm;
        private Logger logger;

        public Server(int port, MainForm serverParent = null)
        {
            this.port = port;
            this.computerDatabase = new Dictionary<string, string>();
            this.computerTimes = new Dictionary<string, DateTime>();
            this.computerList = new List<string>();
            this.currentTime = DateTime.Now;
            this.logger = new Logger(Path.Combine(Application.StartupPath, "serverLog.txt"));

            if(serverParent != null)
            {
                this.mainForm = serverParent;
            }

            try
            {
                this.listener = new HttpListener();
                this.listener.IgnoreWriteExceptions = true;
            }
            catch (Exception ex)
            {
                OutputMessage("{Server()}" + ex.Message);
                this.listener = null;
            }
        }

        public Boolean PlatformNotSupported()
        {
            if (this.listener == null)
            {
                return true;
            }
            return false;
        }

        public Boolean Start()
        {
            if (PlatformNotSupported())
            {
                return false;
            }

            try
            {
                if (this.listener.IsListening)
                {
                    return true;
                }

                string prefix = "http://+:" + this.port + "/";
                this.listener.Prefixes.Clear();
                this.listener.Prefixes.Add(prefix);
                this.listener.Start();

                if (this.listenerThread == null)
                {
                    this.listenerThread = new Thread(HandleRequests);
                    this.listenerThread.Start();
                }
            }
            catch (Exception ex)
            {
                OutputMessage("{Start()}" + ex.Message);
                return false;
            }
            return true;
        }

        public Boolean Stop()
        {
            if (PlatformNotSupported())
            {
                return false;
            }

            try
            {
                this.listenerThread.Abort();
                this.listener.Stop();
                this.listenerThread = null;
            }
            catch (Exception ex)
            {
                OutputMessage("{Stop()}" + ex.Message);
                return false;
            }
            return true;
        }

        private void HandleRequests()
        {
            while (this.listener.IsListening)
            {
                IAsyncResult context;
                context = this.listener.BeginGetContext(new AsyncCallback(ServerCallback), this.listener);
                context.AsyncWaitHandle.WaitOne();
            }
        }

        private void ServerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            if (listener == null || !listener.IsListening)
            {
                return;
            }

            HttpListenerContext context;
            try
            {
                context = listener.EndGetContext(result);
            }
            catch (Exception ex)
            {
                OutputMessage("{ServerCallback()}" + ex.Message);
                return;
            }

            HttpListenerRequest request = context.Request;
            string requestedFile = request.RawUrl.Substring(1);

            OutputMessage(requestedFile + "requested, total of " + computerList.Count.ToString()) ;

            if ((DateTime.Now - currentTime).TotalSeconds >= 10.0)
            {
                CheckComputers();
                currentTime = DateTime.Now;
            }

            if (computerList.Count != 0)
            {
                if (requestedFile == "computerList.json")
                {
                    SendComputerList(context.Response);
                    return;
                }

                if (requestedFile.Contains(".json"))
                {
                    string computerName = requestedFile.Substring(0, requestedFile.IndexOf(".json"));

                    SendComputerData(context.Response, computerName);
                    return;
                }
            }

            if (requestedFile.Contains(".curl"))
            {
                StreamReader stream = new StreamReader(request.InputStream);
                string receivedData = stream.ReadToEnd();
                string computerName = request.RawUrl.Substring(1);
                computerName = computerName.Substring(0, computerName.IndexOf(".curl"));

                if (!this.computerDatabase.ContainsKey(computerName))
                {
                    this.computerDatabase.Add(computerName, receivedData);
                    this.computerTimes.Add(computerName, DateTime.Now);
                    if (!this.computerList.Contains(computerName))
                    {
                        this.computerList.Add(computerName);
                    }
                }
                else
                {
                    this.computerDatabase[computerName] = receivedData;
                    this.computerTimes[computerName] = DateTime.Now;
                }

                SendComputerData(context.Response, computerName);
                return;
            }

            if (string.IsNullOrEmpty(requestedFile))
            {
                requestedFile = "index.html";
            }

            string[] splits = requestedFile.Split('.');
            string extension = splits[splits.Length - 1];
            SendRequestedFile(context.Response, requestedFile, extension);
        }

        private void CheckComputers()
        {
            DateTime referenceTime = DateTime.Now;
            int iterations = this.computerTimes.Count;

            for (int i = 0; i < iterations; i++)
            {
                if ((referenceTime - computerTimes[computerList[i]]).TotalSeconds >= 10.0)
                {
                    try
                    {
                        this.computerDatabase.Remove(computerList[i]);
                        this.computerTimes.Remove(computerList[i]);
                        this.computerList.Remove(computerList[i]);
                        iterations--;
                        i--;
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void SendComputerList(HttpListenerResponse response)
        {
            string json = JsonConvert.SerializeObject(this.computerList);

            SendJson(response, json);
        }

        private void SendComputerData(HttpListenerResponse response, string computerName)
        {
            string json;
            try
            {
                json = this.computerDatabase[computerName];
            }
            catch (Exception ex)
            {
                OutputMessage("{SendComputerData()}" + ex.Message);
                json = "";
            }

            SendJson(response, json);
        }

        private void SendRequestedFile(HttpListenerResponse response, string requestedFile, string extension)
        {
            requestedFile = requestedFile.Replace("/", ".");
            requestedFile = typeof(Server).Namespace + ".Web." + requestedFile;

            string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Replace("\\", ".") == requestedFile)
                {
                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(names[i]);
                    response.ContentType = GetContentType("." + extension);
                    response.ContentLength64 = stream.Length;
                    byte[] buffer = new byte[512 * 1024];
                    int length;
                    try
                    {
                        Stream output = response.OutputStream;
                        while ((length = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, length);
                        }
                        output.Flush();
                        output.Close();
                        response.Close();
                    }
                    catch (Exception ex)
                    {
                        OutputMessage("{SendRequestedFile()}" + ex.Message);
                    }
                    return;
                }
            }

            response.StatusCode = 404;
            response.Close();
        }

        private void SendJson(HttpListenerResponse response, string json)
        {
            string responseContent = json;
            byte[] buffer = Encoding.UTF8.GetBytes(responseContent);

            response.AddHeader("Cache-Control", "no-cache");
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";

            try
            {
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception ex)
            {
                OutputMessage("{SendJson()}" + ex.Message);
            }

            response.Close();
        }

        private string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".css": return "text/css";
                case ".htm":
                case ".html": return "text/html";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".js": return "application/x-javascript";
                case ".png": return "image/png";
                default: return "application/octet-stream";
            }
        }

        private void OutputMessage(string message)
        {
            if (this.mainForm != null)
            {
                this.mainForm.serverOutputText = message;
            }
            else
            {
                Console.WriteLine(message);
            }
            this.logger.Log(message);
        }
    }
}
