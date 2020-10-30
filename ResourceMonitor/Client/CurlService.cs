using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client {
    class CurlService {
        private string server;
        private Thread curlThread;
        private MainForm mainForm;
        private bool isRunning;
        private Computer computer;
        private int counter;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string versaoErrada = "";

        public CurlService(string server, MainForm parentForm = null) {
            logger.Debug("Client v" + Application.ProductVersion + " foi iniciado.");

            this.server = server;
            this.isRunning = false;
            this.computer = new Computer() {
                CPUEnabled = true,
                FanControllerEnabled = true,
                GPUEnabled = true,
                HDDEnabled = true,
                MainboardEnabled = true,
                RAMEnabled = true
            };
            logger.Debug("Instância do computador criada.");

            this.counter = 0;

            if (parentForm != null) {
                this.mainForm = parentForm;
            }
        }

        public Boolean IsRunning {
            get {
                return this.isRunning;
            }
            set { }
        }

        public Boolean Start() {
            try {
                this.computer.Open();

                if(!CheckVersion()) {
                    return false;
                }

                this.isRunning = true;
                if (this.curlThread == null) {
                    this.curlThread = new Thread(CurlCallback);
                    this.curlThread.Start();
                }
                OutputMessage("Curl Service Started!!");
                logger.Debug("Serviço CURL iniciado.");
            }
            catch (Exception ex) {
                OutputMessage("{Start()}" + ex.Message);
                logger.Debug("[ERRO] Erro ou iniciar o serviço CURL.");
                logger.Debug("[ERRO] " + ex.Message);
                return false;
            }
            return true;
        }

        public Boolean Stop() {
            this.isRunning = false;
            try {
                this.curlThread.Abort();
                this.curlThread = null;
            }
            catch (Exception ex) {
                OutputMessage("{Stop()}" + ex.Message);
                return false;
            }
            return true;
        }

        private void CurlCallback() {
            logger.Debug("CurlCallback foi iniciada.");
            IAsyncResult firstContext;
            AsyncCallback firstAsyncCallback = new AsyncCallback(firstCallback);
            firstContext = firstAsyncCallback.BeginInvoke(null, firstCallback, null);
            firstContext.AsyncWaitHandle.WaitOne();
            while (this.IsRunning) {
                logger.Debug("CurlCallback aguardando para enviar requisições HTTP.");
                OutputMessage("Curl Callback Listening!!");
                IAsyncResult context;
                AsyncCallback asyncCallback = new AsyncCallback(callback);
                context = asyncCallback.BeginInvoke(null, callback, null);
                context.AsyncWaitHandle.WaitOne();
                Thread.Sleep(1000);
            }
        }

        public void callback(IAsyncResult result) {
            ComputerData computerData = new ComputerData(this.computer);
            string json = computerData.GetJsonData();
            json = json.Replace("GPU Memory", "GPUMemory");
            json = json.Replace("GPU Core", "GPUCore");
            json = json.Replace("Total Memory", "TotalMemory");
            SendCurl(this, json);
        }
        public void firstCallback(IAsyncResult result) {
            ComputerData computerData = new ComputerData(this.computer);
            string json = computerData.GetJsonData();
            json = json.Replace("GPU Memory", "GPUMemory");
            json = json.Replace("GPU Core", "GPUCore");
            json = json.Replace("Total Memory", "TotalMemory");
            _SendCurl(this, json);
        }

        private bool CheckVersion() {
            WebRequest webRequest = WebRequest.Create(this.server + "versao");
            webRequest.Method = "GET";
            try {
                OutputMessage("Verificando versão ...");
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                string data = string.Empty;
                if (response.StatusCode == HttpStatusCode.OK) {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    data = streamReader.ReadToEnd();
                    streamReader.Close();
                    streamReader.Dispose();
                }
                dynamic jObject = JObject.Parse(data);
                string versaoDetectada = jObject.ClientVersion;
                string versaoVerificada = Application.ProductVersion;
                OutputMessage("Versão Atual: " + versaoDetectada);
                OutputMessage("Versão Verificada: " + versaoVerificada);

                if(versaoDetectada != versaoVerificada) {
                    versaoErrada = $"Versão do cliente incorreta...\nVersão Detectada: {versaoDetectada}\nVersão Correta: {versaoVerificada}\nPor Favor atualize o seu Cliente!";
                    return false;                    
                }

                webRequest.Abort();
                response.Close();
            }
            catch (Exception ex) {
                OutputMessage("{CheckVersion()}" + ex.Message);
            }

            return true;
        }

        public Task SendCurl(CurlService instance, string json) {
            WebRequest webRequest = WebRequest.Create(instance.server + "leitura");
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = buffer.Length;
            try {
                Stream stream = webRequest.GetRequestStream();
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();

                OutputMessage("HTTP Package Sent!!");
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK) {
                    string data = string.Empty;
                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    data = streamReader.ReadToEnd();
                    streamReader.Close();
                    streamReader.Dispose();
                }

                webRequest.Abort();
                response.Close();

                this.counter++;
                OutputMessage("[" + this.counter + "] Curl sent.");
            }
            catch (Exception ex) {
                OutputMessage("{SendCurl()}" + ex.Message);
            }

            return null;
        }

        public Task _SendCurl(CurlService instance, string json) {
            Console.WriteLine(json);
            dynamic obj = JsonConvert.DeserializeObject(json);
            var cpus = new List<Object>();
            int index = 0;
            foreach (dynamic cpu in obj.Hardware.CPU) {
                cpus.Add(new {
                    id = -1,
                    number = index,
                    name = cpu.Name,
                    temperature = cpu.Sensors.Temperature.Maximum,
                    clock = cpu.Sensors.Clock.Maximum,
                    power = cpu.Sensors.Power.Maximum,
                    cores = cpu.Cores
                });
                index++;
            }

            var gpus = new List<Object>();
            index = 0;
            foreach (dynamic gpu in obj.Hardware.GpuNvidia) {
                gpus.Add(new {
                    id = -1,
                    number = index,
                    name = gpu.Name,
                    temperature = gpu.Sensors.Temperature.Maximum,
                    coreclock = gpu.Sensors.Clock.GPUCore.Maximum,
                    memoryclock = gpu.Sensors.Clock.GPUMemory.Maximum
                });
                index++;
            }
            foreach (dynamic gpu in obj.Hardware.GpuAti) {
                gpus.Add(new {
                    id = -1,
                    number = index,
                    name = gpu.Name,
                    temperature = gpu.Sensors.Temperature.Maximum,
                    coreclock = gpu.Sensors.Clock.GPUCore.Maximum,
                    memoryclock = gpu.Sensors.Power.GPUMemory.Maximum
                });
                index++;
            }

            var armazenamentos = new List<Object>();
            index = 0;
            foreach (dynamic armazenamento in obj.Hardware.HDD) {
                armazenamentos.Add(new {
                    id = -1,
                    number = index,
                    name = armazenamento.Name,
                    size = armazenamento.Size,
                    disks = armazenamento.Letters,
                });
                index++;
            }

            var ram = new {
                id = -1,
                total = Double.Parse(obj.Hardware.RAM[0].Sensors.Data.TotalMemory.ToString()),
                modules = -1
            };

            dynamic newObj = new {
                id = -1,
                name = obj.Name,
                cpus = cpus,
                gpus = gpus,
                storages = armazenamentos,
                ram = ram,
                status = true,
            };
            WebRequest webRequest = WebRequest.Create("http://samjviana.ddns.net:9002/computador");
            byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newObj));
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = buffer.Length;
            try {
                Stream stream = webRequest.GetRequestStream();
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();

                OutputMessage("HTTP Package Sent!!");
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK) {
                    string data = string.Empty;
                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    data = streamReader.ReadToEnd();
                    streamReader.Close();
                    streamReader.Dispose();
                }

                webRequest.Abort();
                response.Close();

                this.counter++;
                OutputMessage("[" + this.counter + "] Curl sent.");
            }
            catch (Exception ex) {
                OutputMessage("{SendCurl()}" + ex.Message);
            }

            return null;
        }


        private void OutputMessage(string message) {
            if (this.mainForm != null) {
                this.mainForm.serverOutputText = message;
            }
            else {
                Console.WriteLine(message);
            }
        }
    }
}
