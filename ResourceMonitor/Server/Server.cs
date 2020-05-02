using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        private Dictionary<string, object> computerStates;
        private List<string> computerList;
        private DateTime currentTime;
        private readonly MainForm mainForm;
        private int counter;
        private bool change;
        private SnmpManager snmpManager;
        private bool isRunning;

        public Server(int port, MainForm serverParent = null)
        {
            this.snmpManager = new SnmpManager();

            this.port = port;
            this.computerDatabase = new Dictionary<string, string>();
            this.computerTimes = new Dictionary<string, DateTime>();
            this.computerStates = new Dictionary<string, object>();
            this.computerList = new List<string>();
            this.currentTime = DateTime.Now;
            this.counter = 0;
            this.change = false;

            if (serverParent != null)
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
                ReportError(ex.Message);
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
            this.isRunning = true;

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
                ReportError(ex.Message);
                OutputMessage("{Start()}" + ex.Message);
                return false;
            }
            return true;
        }

        public Boolean Stop()
        {
            this.isRunning = false;

            StopDiscovery();

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
                ReportError(ex.Message);
                OutputMessage("{Stop()}" + ex.Message);
                return false;
            }
            return true;
        }

        public void StartDiscovery(Dictionary<int, NetworkInterface> interfacesToDiscover)
        {
            this.snmpManager.StartDiscovery(interfacesToDiscover);
        }

        public void StopDiscovery()
        {
            this.snmpManager.StopDiscovery();
        }

        public string GetDiscoveryData()
        {
            if (snmpManager.DiscoveryDone)
            {
                return snmpManager.GetJsonData();
            }
            return null;
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
                ReportError(ex.Message);
                OutputMessage("{ServerCallback()}" + ex.Message);
                return;
            }

            HttpListenerRequest request = context.Request;
            string requestedFile = request.RawUrl.Substring(1);

            OutputMessage(requestedFile);

            if ((DateTime.Now - currentTime).TotalSeconds >= 10.0)
            {
                CheckComputers();
                currentTime = DateTime.Now;
            }

            if (requestedFile == "computerList.json")
            {
                try
                {
                    SendComputerList(context.Response);
                }
                catch (Exception ex)
                {
                    ReportError(ex.Message);
                }
                return;
            }
            else if (requestedFile == "deviceList.json")
            {
                try
                {
                    SendDeviceList(context.Response);
                }
                catch (Exception ex)
                {
                    ReportError(ex.Message);
                }
                return;
            }
            else
            {
                if (computerList.Count != 0)
                {
                    if (requestedFile.Contains(".json"))
                    {
                        string computerName = requestedFile.Substring(0, requestedFile.IndexOf(".json"));

                        SendComputerData(context.Response, computerName);
                        return;
                    }
                }
            }

            if (requestedFile.Contains(".curl"))
            {
                try
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
                            Dictionary<string, object> status = new Dictionary<string, object>();
                            status.Add("Name", computerName);
                            status.Add("State", true);
                            this.computerStates.Add(counter.ToString(), status);
                            this.computerList.Add(computerName);
                            this.counter++;
                        }
                        this.change = true;
                    }
                    else
                    {
                        this.computerDatabase[computerName] = receivedData;
                        this.computerTimes[computerName] = DateTime.Now;
                        for (int i = 0; i < computerStates.Count; i++)
                        {
                            if (((Dictionary<string, object>)computerStates[i.ToString()])["Name"].ToString() == computerName)
                            {
                                if (!(bool)((Dictionary<string, object>)computerStates[i.ToString()])["State"])
                                {
                                    this.change = true;
                                }
                                ((Dictionary<string, object>)computerStates[i.ToString()])["State"] = true;
                            }
                        }
                    }

                    SendComputerData(context.Response, computerName);
                }
                catch (Exception ex)
                {
                    ReportError(ex.Message);
                }
                return;
            }

            if (string.IsNullOrEmpty(requestedFile))
            {
                requestedFile = "index1.html";
            }

            string[] splits = requestedFile.Split('.');
            string extension = splits[splits.Length - 1];
            SendRequestedFile(context.Response, requestedFile, extension);
        }

        private void CheckComputers()
        {
            DateTime referenceTime = DateTime.Now;

            for (int i = 0; i < computerList.Count; i++)
            {
                if (((referenceTime - computerTimes[computerList[i]]).TotalSeconds >= 10.0) && Convert.ToBoolean(((Dictionary<string, object>)computerStates[i.ToString()])["State"]))
                {
                    try
                    {
                        if (computerList[i] == ((Dictionary<string, object>)computerStates[i.ToString()])["Name"].ToString())
                        {
                            ((Dictionary<string, object>)this.computerStates[i.ToString()])["State"] = false;
                            this.change = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ReportError(ex.Message);
                    }
                }
            }
        }

        private void SendDeviceList(HttpListenerResponse response)
        {
            string json = "{\"empty\":\"empty\"}";

            if(this.InterfacesDiscovered != null)
            {
                if (this.InterfacesDiscovered.Count <= 0)
                {
                    json = "{\"empty\":\"empty\"}";
                }
                else
                {
                    json = "{\"Devices\": ";
                    json += JsonConvert.SerializeObject(this.InterfacesDiscovered);
                    json += ",\"Count\": " + this.InterfacesDiscovered.Count;
                    json += ",\"Change\": \"" + this.snmpManager.DataChanged + "\"}";
                }
            }

            try
            {
                File.WriteAllText("deviceList.json", json);
            }
            catch
            {

            }

            SendJson(response, json);

            this.snmpManager.DataChanged = false;
        }

        private void SendComputerList(HttpListenerResponse response)
        {
            string json;

            if (this.computerList.Count <= 0)
            {
                json = "{\"empty\":\"empty\"}";
            }
            else
            {
                json = "{\"Computers\": ";
                json += JsonConvert.SerializeObject(this.computerStates);
                json += ",\"Count\": " + this.counter;
                json += ",\"Change\": \"" + this.change + "\"}";
            }

            try
            {
                File.WriteAllText("computerList.json", json);
            }
            catch
            {

            }

            SendJson(response, json);

            this.change = false;
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
                ReportError(ex.Message);
                OutputMessage("{SendComputerData()}" + ex.Message);
                json = "";
            }

            try
            {
                //File.WriteAllText(computerName + ".json", json);
            }
            catch (Exception ex)
            {
                ReportError(ex.Message);
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
                        ReportError(ex.Message);
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
                ReportError(ex.Message);
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
        }

        private static void ReportError(string Message)
        {
            StackFrame CallStack = new StackFrame(1, true);
            Console.WriteLine("Error: " + Message + ", File: " + CallStack.GetFileName() + ", Line: " + CallStack.GetFileLineNumber());
        }

        public Boolean DiscoveryDone
        {
            get { return this.snmpManager.DiscoveryDone; }
        }

        public string NetworkProgress
        {
            get { return snmpManager.NetworkProgress; }
        }

        public string IpProgress
        {
            get { return snmpManager.IpProgress; }
        }

        public Boolean IsRunning
        {
            get { return this.isRunning; }
        }

        public Dictionary<int, NetworkInterface> NetworkInterfaces
        {
            get { return snmpManager.NetworkInterfaces; }
            set { }
        }

        public Dictionary<int, object> InterfacesDiscovered
        {
            get { return snmpManager.InterfacesDiscovered; }
            set { }
        }

    }
}
