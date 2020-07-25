using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.DB.Conexao;
using Server.DB.Modelos;
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

namespace Server {
    class Server {
        private int port;
        private HttpListener listener;
        private Thread listenerThread;
        private Dictionary<string, string> computerDatabase;
        private Dictionary<string, DateTime> computerTimes;
        private DateTime currentTime;
        private readonly MainForm mainForm;
        private int computerCount;
        private bool change;
        private SnmpManager snmpManager;
        private bool isRunning;
        private bool webTrigger;
        private readonly object dbLock = new object();
        private string clientVersion = "0.7.2";

        public Server(int port, MainForm serverParent = null) {
            this.snmpManager = new SnmpManager();

            this.port = port;
            this.computerDatabase = new Dictionary<string, string>();
            this.computerTimes = new Dictionary<string, DateTime>();
            this.currentTime = DateTime.Now;
            this.computerCount = 0;
            this.change = false;
            this.webTrigger = false;

            if (serverParent != null) {
                this.mainForm = serverParent;
            }

            try {
                this.listener = new HttpListener();
                this.listener.IgnoreWriteExceptions = true;
            }
            catch (Exception ex) {
                ReportError(ex.Message);
                OutputMessage("{Server()}" + ex.Message);
                this.listener = null;
            }
        }

        public Boolean PlatformNotSupported() {
            if (this.listener == null) {
                return true;
            }
            return false;
        }

        public Boolean Start() {
            this.isRunning = true;

            if (PlatformNotSupported()) {
                return false;
            }

            try {
                if (this.listener.IsListening) {
                    return true;
                }

                string prefix = "http://+:" + this.port + "/";
                this.listener.Prefixes.Clear();
                this.listener.Prefixes.Add(prefix);
                this.listener.Start();

                if (this.listenerThread == null) {
                    this.listenerThread = new Thread(HandleRequests);
                    this.listenerThread.Start();
                }
            }
            catch (Exception ex) {
                ReportError(ex.Message);
                OutputMessage("{Start()}" + ex.Message);
                return false;
            }
            return true;
        }

        public Boolean Stop() {
            this.isRunning = false;

            StopDiscovery();

            if (PlatformNotSupported()) {
                return false;
            }

            try {
                this.listenerThread.Abort();
                this.listener.Stop();
                this.listenerThread = null;
            }
            catch (Exception ex) {
                ReportError(ex.Message);
                OutputMessage("{Stop()}" + ex.Message);
                return false;
            }
            return true;
        }

        public void StartDiscovery(Dictionary<int, NetworkInterface> interfacesToDiscover) {
            this.snmpManager.StartDiscovery(interfacesToDiscover);
        }

        public void StopDiscovery() {
            this.snmpManager.StopDiscovery();
        }

        public string GetDiscoveryData() {
            if (snmpManager.DiscoveryDone) {
                return snmpManager.GetJsonData();
            }
            return null;
        }

        private void HandleRequests() {
            while (this.listener.IsListening) {
                IAsyncResult context;
                context = this.listener.BeginGetContext(new AsyncCallback(ServerCallback), this.listener);
                context.AsyncWaitHandle.WaitOne();
            }
        }

        private void ServerCallback(IAsyncResult result) {
            HttpListener listener = (HttpListener)result.AsyncState;
            if (listener == null || !listener.IsListening) {
                return;
            }

            HttpListenerContext context;
            try {
                context = listener.EndGetContext(result);
            }
            catch (Exception ex) {
                ReportError(ex.Message);
                OutputMessage("{ServerCallback()}" + ex.Message);
                return;
            }

            HttpListenerRequest request = context.Request;
            string requestedFile = request.RawUrl.Substring(1);

            OutputMessage(requestedFile);

            if ((DateTime.Now - currentTime).TotalSeconds >= 1.0) {
                CheckComputers();
                currentTime = DateTime.Now;
            }

            if(requestedFile == "GetClientVersion") {
                try {
                    SendClientVersion(context.Response);
                }
                catch(Exception ex) {
                    ReportError(ex.Message);
                }
                return;
            }

            if (requestedFile == "startDiscovery") {
                Dictionary<int, NetworkInterface> interfacesToDiscover = new Dictionary<int, NetworkInterface>();
                interfacesToDiscover.Add(0, snmpManager.NetworkInterfaces[0]);
                StartDiscovery(interfacesToDiscover);

                try {
                    SendDeviceList(context.Response);
                }
                catch (Exception ex) {
                    ReportError(ex.Message);
                }

                this.webTrigger = true;
                return;
            }

            if (requestedFile == "computerList.json") {
                try {
                    SendComputerList(context.Response);
                }
                catch (Exception ex) {
                    ReportError(ex.Message);
                }
                return;
            }
            else if (requestedFile == "deviceList.json") {
                try {
                    SendDeviceList(context.Response);
                }
                catch (Exception ex) {
                    ReportError(ex.Message);
                }
                return;
            }
            else {
                if (computerCount != 0) {
                    if (requestedFile.Contains(".json")) {
                        string computerName = requestedFile.Substring(0, requestedFile.IndexOf(".json"));

                        SendComputerData(context.Response, computerName);
                        return;
                    }
                }
            }

            if (requestedFile.Contains(".curl")) {
                try {
                    StreamReader stream = new StreamReader(request.InputStream);
                    string receivedData = stream.ReadToEnd();
                    string computerName = request.RawUrl.Substring(1);
                    computerName = computerName.Substring(0, computerName.IndexOf(".curl"));

                    if (!computerDatabase.ContainsKey(computerName)) {
                        computerDatabase.Add(computerName, receivedData);
                    }
                    else {
                        computerDatabase[computerName] = receivedData;
                    }


                    using (var dbContext = new DatabaseContext()) {
                        this.computerCount = dbContext.Computadores.Count();

                        dynamic jObject = JObject.Parse(receivedData);

                        dynamic cpus = jObject.Hardware.CPU;
                        dynamic ram = jObject.Hardware.RAM;
                        dynamic armazenamentos = jObject.Hardware.HDD;
                        dynamic gpusNvidia = jObject.Hardware.GpuNvidia;
                        dynamic gpusAti = jObject.Hardware.GpuAti;

                        Computador computador = dbContext.Computadores.SingleOrDefault(c =>
                            c.Nome == computerName
                        );
                        ICollection<CPU> cpuList = new List<CPU>();
                        ICollection<GPU> gpuList = new List<GPU>();
                        ICollection<Armazenamento> armazenamentoList = new List<Armazenamento>();
                        Memoria memoria = new Memoria() {
                            Total = ram[0].Sensors.Data.TotalMemory,
                            DataCriacao = DateTime.Now,
                            DataUpdate = DateTime.Now
                        };

                        int numeroCpu = 0;
                        foreach (var cpuObj in cpus) {
                            double power;
                            if(((JObject)cpuObj.Sensors.Power).Count <= 0) {
                                power = -1;
                            }
                            else {
                                power = cpuObj.Sensors.Power.Maximum;
                            }

                            CPU cpu = new CPU() {
                                Nome = cpuObj.Name,
                                Clock = cpuObj.Sensors.Clock.Maximum,
                                Nucleos = cpuObj.Cores,
                                Numero = numeroCpu,
                                Potencia = power,
                                Temperatura = cpuObj.Sensors.Temperature.Maximum,
                                DataCriacao = DateTime.Now,
                                DataUpdate = DateTime.Now
                            };

                            cpuList.Add(cpu);
                            numeroCpu++;
                        }

                        int numeroGpu = 0;
                        bool errorFlag = false;
                        foreach (var gpuObj in gpusNvidia) {
                            GPU gpu = new GPU() {
                                Nome = gpuObj.Name,
                                ClockMemoria = gpuObj.Sensors.Clock.GPUMemory.Maximum,
                                ClockNucleo = gpuObj.Sensors.Clock.GPUCore.Maximum,
                                Numero = numeroGpu,
                                Temperatura = gpuObj.Sensors.Temperature.Maximum,
                                DataCriacao = DateTime.Now,
                                DataUpdate = DateTime.Now
                            };
                            numeroGpu++;

                            if (gpu.Temperatura == -1) {
                                errorFlag = true;
                                break;
                            }
                            gpuList.Add(gpu);
                        }
                        foreach (var gpuObj in gpusAti) {
                            GPU gpu = new GPU() {
                                Nome = gpuObj.Name,
                                ClockMemoria = gpuObj.Sensors.GPUMemory.Maximum,
                                ClockNucleo = gpuObj.Sensors.GPUCore.Maximum,
                                Numero = numeroGpu,
                                Temperatura = gpuObj.Sensors.Temperature.Maximum,
                                DataCriacao = DateTime.Now,
                                DataUpdate = DateTime.Now
                            };

                            gpuList.Add(gpu);
                            numeroGpu++;
                        }

                        foreach (var armazenamentoObj in armazenamentos) {
                            Armazenamento armazenamento = new Armazenamento() {
                                Nome = armazenamentoObj.Name,
                                Capacidade = armazenamentoObj.Size,
                                Discos = armazenamentoObj.Letters,
                                DataCriacao = DateTime.Now,
                                DataUpdate = DateTime.Now
                            };

                            armazenamentoList.Add(armazenamento);
                        }

                        if (computador == null && !errorFlag) {

                            computador = new Computador() {
                                Nome = computerName,
                                Estado = true,
                                Armazenamentos = armazenamentoList,
                                Memoria = memoria,
                                GPUs = gpuList,
                                CPUs = cpuList,
                                SistemaOperacional = null,
                                IP = null,
                                MAC = null,
                                SNMP = null,
                                DataCriacao = DateTime.Now,
                                DataUpdate = DateTime.Now
                            };

                            lock (dbLock) {
                                Console.WriteLine(computerName + " adicionado");
                                dbContext.Computadores.Add(computador);
                            }

                            try {
                                dbContext.SaveChanges();
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else if (!computador.Estado && !errorFlag) {
                            computador.Estado = true;
                            computador.Armazenamentos = armazenamentoList;
                            computador.Memoria = memoria;
                            computador.CPUs = cpuList;
                            computador.GPUs = gpuList;
                            computador.DataUpdate = DateTime.Now;

                            try {
                                dbContext.SaveChanges();
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else {
                            computador.DataUpdate = DateTime.Now;
                            computador.Estado = true;

                            try {
                                dbContext.SaveChanges();
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }

                    SendComputerData(context.Response, computerName);
                }
                catch (Exception ex) {
                    SendErrorJson(context.Response, "Erro ao receber o CURL.");
                    ReportError(ex.Message);
                }
                return;
            }

            if (string.IsNullOrEmpty(requestedFile)) {
                requestedFile = "index1.html";
            }

            string[] splits = requestedFile.Split('.');
            string extension = splits[splits.Length - 1];
            SendRequestedFile(context.Response, requestedFile, extension);
        }

        private void SendClientVersion(HttpListenerResponse response) {
            string json = "{\"ClientVersion\":\"" + clientVersion + "\"}";
            SendJson(response, json);
        }

        private void CheckComputers() {
            DateTime referenceTime = DateTime.Now;
            bool changed = false;

            using(var dbContext = new DatabaseContext()) {
                foreach(var computador in dbContext.Computadores) {
                    double minutes = (referenceTime - computador.DataUpdate).Value.TotalMinutes;
                    if(minutes >= 1.0) {
                        computador.Estado = false;
                        changed = true;
                    }
                }

                if(changed) {
                    try {
                        dbContext.SaveChanges();
                    }
                    catch (Exception ex) {
                        ReportError(ex.Message);
                    }
                }
            }
        }

        private void SendErrorJson(HttpListenerResponse response, string errorMessage) {
            string json = "{\"ERRO\":\"" + errorMessage + "\"}";
            SendJson(response, json);
        }

        private void SendDeviceList(HttpListenerResponse response) {
            string json = "{\"empty\":\"empty\"}";

            if (this.InterfacesDiscovered != null) {
                if (this.InterfacesDiscovered.Count <= 0) {
                    json = "{\"empty\":\"empty\"}";
                }
                else {
                    json = "{\"Devices\": ";
                    json += JsonConvert.SerializeObject(this.InterfacesDiscovered);
                    json += ",\"Count\": " + this.InterfacesDiscovered.Count;
                    if (this.webTrigger) {
                        this.webTrigger = false;
                        json += ",\"Change\": \"true\"}";
                    }
                    else {

                    }
                    json += ",\"Change\": \"" + this.snmpManager.DataChanged + "\"}";
                }
            }

            try {
                File.WriteAllText("deviceList.json", json);
            }
            catch {

            }

            SendJson(response, json);

            this.snmpManager.DataChanged = false;
        }

        private void SendComputerList(HttpListenerResponse response) {
            string json;

            using (var context = new DatabaseContext()) {
                if (context.Computadores.Count() <= 0) {
                    json = "{\"empty\":\"empty\"}";
                }
                else {
                    json = JsonConvert.SerializeObject(new {
                        Computadores = context.Computadores.Select(c => new { c.Nome, c.Estado }),
                        Quantidade = context.Computadores.Count(),
                        Mudanca = this.change
                    });
                    /*
                    Console.WriteLine(JsonConvert.SerializeObject(
                        context.Computadores
                            .Include("CPUs")
                            .Include("GPUs")
                            .Include("Armazenamentos")
                            .Include("Memoria").ToList()));
                    */
                    /*
                    Console.WriteLine(JsonConvert.SerializeObject(new {
                        Computadores = context.Computadores.Select(c => new { c.Nome, c.Estado }),
                        Quantidade = context.Computadores.Count(),
                        Mudanca = this.change
                    }));
                    */
                }
            }

            SendJson(response, json);

            this.change = false;
        }

        private void SendComputerData(HttpListenerResponse response, string computerName) {
            string json;
            try {
                json = this.computerDatabase[computerName];
            }
            catch (Exception ex) {
                ReportError(ex.Message);
                OutputMessage("{SendComputerData()}" + ex.Message);
                json = "";
            }

            try {
                //File.WriteAllText(computerName + ".json", json);
            }
            catch (Exception ex) {
                ReportError(ex.Message);
            }
            SendJson(response, json);
        }

        private void SendRequestedFile(HttpListenerResponse response, string requestedFile, string extension) {
            requestedFile = requestedFile.Replace("/", ".");
            requestedFile = typeof(Server).Namespace + ".Web." + requestedFile;

            string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            for (int i = 0; i < names.Length; i++) {
                if (names[i].Replace("\\", ".") == requestedFile) {
                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(names[i]);
                    response.ContentType = GetContentType("." + extension);
                    response.ContentLength64 = stream.Length;
                    byte[] buffer = new byte[512 * 1024];
                    int length;
                    try {
                        Stream output = response.OutputStream;
                        while ((length = stream.Read(buffer, 0, buffer.Length)) > 0) {
                            output.Write(buffer, 0, length);
                        }
                        output.Flush();
                        output.Close();
                        response.Close();
                    }
                    catch (Exception ex) {
                        ReportError(ex.Message);
                        OutputMessage("{SendRequestedFile()}" + ex.Message);
                    }
                    return;
                }
            }

            response.StatusCode = 404;
            response.Close();
        }

        private void SendJson(HttpListenerResponse response, string json) {
            string responseContent = json;
            byte[] buffer = Encoding.UTF8.GetBytes(responseContent);

            response.AddHeader("Cache-Control", "no-cache");
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";

            try {
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception ex) {
                ReportError(ex.Message);
                OutputMessage("{SendJson()}" + ex.Message);
            }

            response.Close();
        }

        private string GetContentType(string extension) {
            switch (extension) {
                case ".css": return "text/css";
                case ".htm":
                case ".html": return "text/html";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".js": return "application/x-javascript";
                case ".png": return "image/png";
                case ".ico": return "text/plain";
                default: return "application/octet-stream";
            }
        }

        private void OutputMessage(string message) {
            if (this.mainForm != null) {
                this.mainForm.serverOutputText = message;
            }
            else {
                Console.WriteLine(message);
            }
        }

        private static void ReportError(string Message) {
            StackFrame CallStack = new StackFrame(1, true);
            Console.WriteLine("Error: " + Message + ", File: " + CallStack.GetFileName() + ", Line: " + CallStack.GetFileLineNumber());
        }

        public Boolean DiscoveryDone {
            get { return this.snmpManager.DiscoveryDone; }
        }

        public string NetworkProgress {
            get { return snmpManager.NetworkProgress; }
        }

        public string IpProgress {
            get { return snmpManager.IpProgress; }
        }

        public Boolean IsRunning {
            get { return this.isRunning; }
        }

        public Dictionary<int, NetworkInterface> NetworkInterfaces {
            get { return snmpManager.NetworkInterfaces; }
            set { }
        }

        public Dictionary<int, object> InterfacesDiscovered {
            get { return snmpManager.InterfacesDiscovered; }
            set { }
        }

    }
}
