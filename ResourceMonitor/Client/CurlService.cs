using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class CurlService
    {
        private string server;
        private Thread curlThread;
        private MainForm mainForm;
        private bool isRunning;
        private Computer computer;
        private Logger logger;

        public CurlService(string server, MainForm parentForm = null)
        {
            this.server = server;
            this.isRunning = false;
            this.computer = new Computer()
            {
                CPUEnabled = true,
                FanControllerEnabled = true,
                GPUEnabled = true,
                HDDEnabled = true,
                MainboardEnabled = true,
                RAMEnabled = true
            };

            this.logger = new Logger("curlLog.txt");

            if (parentForm != null)
            {
                this.mainForm = parentForm;
            }
        }

        public Boolean IsRunning
        {
            get
            {
                return this.isRunning;
            }
            set { }
        }

        public Boolean Start()
        {
            try
            {
                this.computer.Open();
                if (this.curlThread == null)
                {
                    this.curlThread = new Thread(CurlCallback);
                    this.curlThread.Start();
                }
                this.isRunning = true;
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
            this.isRunning = false;
            try
            {
                this.curlThread.Abort();
                this.curlThread = null;
            }
            catch(Exception ex)
            {
                OutputMessage("{Stop()}" + ex.Message);
                return false;
            }
            return true;
        }

        private void CurlCallback()
        {
            while (this.IsRunning)
            {
                IAsyncResult context;
                AsyncCallback asyncCallback = new AsyncCallback(callback);
                context = asyncCallback.BeginInvoke(null, callback, null);
                context.AsyncWaitHandle.WaitOne();
            }
        }

        public void callback(IAsyncResult result)
        {
            ComputerData computerData = new ComputerData(this.computer);
            string json = computerData.GetJsonData();
            SendCurl(this, json);
        }

        public Task SendCurl(CurlService instance, string json)
        {
            WebRequest webRequest = WebRequest.Create(instance.server + Environment.MachineName + ".curl");
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = buffer.Length;
            try
            {
                Stream stream = webRequest.GetRequestStream();
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string data = string.Empty;
                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    data = streamReader.ReadToEnd();
                    streamReader.Close();
                    streamReader.Dispose();
                }

                webRequest.Abort();
                response.Close();

                OutputMessage("Curl send.");
            }
            catch (Exception ex)
            {
                OutputMessage("{SendCurl()}" + ex.Message);
            }

            return null;
        }

        private void OutputMessage(string message)
        {
            if (this.mainForm != null)
            {
                this.mainForm.serverOutputText = message;
                this.logger.Log(message);
            }
            else
            {
                Console.WriteLine(message);
                this.logger.Log(message);
            }
        }
    }
}
