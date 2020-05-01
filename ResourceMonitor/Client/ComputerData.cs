using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;
using Newtonsoft.Json;
using System.Reflection;
using System.Management;
using NvAPIWrapper.GPU;
using System.IO;
using System.Windows.Forms;

namespace Client
{
    class ComputerData
    {
        private Dictionary<string, object> computer;
        private PhysicalGPU[] nvidiaGpus;
        private int cpuCoreNumber;
        private bool msrRdError;

        public ComputerData(Computer computer)
        {
            this.computer = new Dictionary<string, object>();
            this.cpuCoreNumber = 0;
            this.msrRdError = false;
            try
            {
                this.nvidiaGpus = PhysicalGPU.GetPhysicalGPUs();
            }
            catch
            {

            }

            this.computer.Add("Name", Environment.MachineName);
            this.computer.Add("Hardware", ParseHardware(computer.Hardware, false));
        }

        private object ParseHardware(IHardware[] hardwares, bool subHardware)
        {
            Dictionary<string, object> hardwaresDict = new Dictionary<string, object>();
            Dictionary<string, object> hardwareDict = new Dictionary<string, object>();

            foreach (var hardwareType in Enum.GetValues(typeof(HardwareType)))
            {
                List<object> hardwareList = new List<object>();
                hardwareDict = new Dictionary<string, object>();
                foreach (var hardware in hardwares)
                {
                    hardware.Update();
                    if (hardware.HardwareType == (HardwareType)hardwareType)
                    {
                        hardwareDict.Add("Name", hardware.Name);
                        if (hardware.SubHardware.Length > 0)
                        {
                            hardwareDict.Add("SubHardware", ParseHardware(hardware.SubHardware, true));
                        }
                        else
                        {
                            hardwareDict.Add("SubHardware", new Dictionary<string, object>());
                        }
                        if (hardware.Sensors.Length > 0)
                        {
                            hardwareDict.Add("Sensors", ParseSensors(hardware.Sensors, (HardwareType)hardwareType));
                            if(hardware.HardwareType == HardwareType.CPU)
                            {
                                hardwareDict.Add("Cores", this.cpuCoreNumber);
                            }
                            this.cpuCoreNumber = 0;
                        }
                        else
                        {
                            hardwareDict.Add("Sensors", new Dictionary<string, object>());
                        }

                        hardwareList.Add(hardwareDict);
                        hardwareDict = new Dictionary<string, object>();
                    }
                }
                if (subHardware)
                {
                    if (hardwareList.Count > 0)
                    {
                        hardwaresDict.Add(hardwareType.ToString(), hardwareList);
                    }
                }
                else
                {
                    hardwaresDict.Add(hardwareType.ToString(), hardwareList);
                }
            }

            return hardwaresDict;
        }

        private object ParseSensors(ISensor[] sensors, HardwareType hardwareType)
        {
            Dictionary<string, object> sensorsDict = new Dictionary<string, object>();
            Dictionary<string, object> sensorDict = new Dictionary<string, object>();
            Dictionary<string, object> valuesDict = new Dictionary<string, object>();

            foreach (SensorType sensorType in Enum.GetValues(typeof(SensorType)))
            {
                List<object> sensorList = new List<object>();
                sensorDict = new Dictionary<string, object>();
                valuesDict = new Dictionary<string, object>();
                double averageClock = 0;
                double averageTemperature = 0;
                int cores = 0;
                foreach (var sensor in sensors)
                {
                    if (sensor.SensorType == (SensorType)sensorType)
                    {
                        if (hardwareType == HardwareType.CPU && sensor.SensorType == SensorType.Clock)
                        {
                            if (valuesDict.ContainsKey("Maximum"))
                            {
                                valuesDict["Maximum"] = ReadCPUMaxClockSpeed();
                            }
                            else
                            {
                                valuesDict.Add("Maximum", ReadCPUMaxClockSpeed());
                            }
                            if (sensor.Name.Contains("CPU Core"))
                            {
                                averageClock += sensor.Value.GetValueOrDefault();
                                cores++;
                                if (valuesDict.ContainsKey("Average"))
                                {
                                    valuesDict["Average"] = averageClock / cores;
                                }
                                else
                                {
                                    valuesDict.Add("Average", averageClock / cores);
                                }
                            }
                        }
                        else if(hardwareType == HardwareType.CPU && sensor.SensorType == SensorType.Power)
                        {
                            if(valuesDict.ContainsKey("Maximum"))
                            {
                                valuesDict["Maximum"] = ReadCPUMaxTDP();
                            }
                            else
                            {
                                valuesDict.Add("Maximum", ReadCPUMaxTDP());
                            }
                        }
                        else if(hardwareType == HardwareType.CPU && sensor.SensorType == SensorType.Temperature)
                        {
                            if(valuesDict.ContainsKey("Maximum"))
                            {
                                valuesDict["Maximum"] = ReadCPUMaxTemperature();
                            }
                            else
                            {
                                valuesDict.Add("Maximum", ReadCPUMaxTemperature());
                            }
                            if (sensor.Name.Contains("CPU Core"))
                            {
                                averageTemperature += sensor.Value.GetValueOrDefault();
                                cores++;
                                if (valuesDict.ContainsKey("Average"))
                                {
                                    valuesDict["Average"] = averageTemperature / cores;
                                }
                                else
                                {
                                    valuesDict.Add("Average", averageTemperature / cores);
                                }
                            }
                        }
                        else if (hardwareType == HardwareType.GpuNvidia && sensor.SensorType == SensorType.Temperature)
                        {
                            double maxTemperature = ReadGPUMaxTemperature();
                            if (valuesDict.ContainsKey("Maximum"))
                            {
                                if (maxTemperature != 0)
                                {
                                    valuesDict["Maximum"] = ReadGPUMaxTemperature();
                                }
                            }
                            else
                            {
                                if(maxTemperature != 0)
                                {
                                    valuesDict.Add("Maximum", ReadGPUMaxTemperature());
                                }
                            }
                        }
                        else if (hardwareType == HardwareType.GpuNvidia && sensor.SensorType == SensorType.Clock)
                        {
                            if(sensor.Name.Contains("GPU Core"))
                            {
                                if (sensorDict.ContainsKey("Maximum"))
                                {
                                    sensorDict["Maximum"] = ReadGPUMaxCoreClock();
                                }
                                else
                                {
                                    sensorDict.Add("Maximum", ReadGPUMaxCoreClock());
                                }
                            }
                            else if(sensor.Name.Contains("GPU Memory"))
                            {
                                if (sensorDict.ContainsKey("Maximum"))
                                {
                                    sensorDict["Maximum"] = ReadGPUMaxMemoryClock();
                                }
                                else
                                {
                                    sensorDict.Add("Maximum", ReadGPUMaxMemoryClock());
                                }
                            }
                        }
                        else if(hardwareType == HardwareType.RAM && sensor.SensorType == SensorType.Data)
                        {
                            if(valuesDict.ContainsKey("Total Memory"))
                            {
                                valuesDict["Total Memory"] = GetSystemTotalRAM();
                            }
                            else
                            {
                                valuesDict.Add("Total Memory", GetSystemTotalRAM());
                            }
                        }

                        sensorDict.Add("Value", sensor.Value.GetValueOrDefault());

                        valuesDict.Add(sensor.Name, sensorDict);

                        sensorDict = new Dictionary<string, object>();
                    }
                }
                if(hardwareType == HardwareType.CPU && cores > 0)
                {
                    this.cpuCoreNumber = cores;
                }
                sensorsDict.Add(sensorType.ToString(), valuesDict);
            }

            return sensorsDict;
        }

        private string GetWmiProperty(string wmiNamespace, string wmiClass, string property)
        {
            ManagementScope scope = new ManagementScope(wmiNamespace);
            ObjectQuery query = new ObjectQuery("SELECT " + property + " FROM " + wmiClass);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject wmiObject in collection)
            {
                return wmiObject["MaxClockSpeed"].ToString();
            }
            return "";
        }

        private List<object> GetWmiProperties(string wmiNamespace, string wmiClass, string property)
        {
            ManagementScope scope = new ManagementScope(wmiNamespace);
            ObjectQuery query = new ObjectQuery("SELECT " + property + " FROM " + wmiClass);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection collection = searcher.Get();

            List<object> wmiProperties = new List<object>();
            foreach (ManagementObject wmiObject in collection)
            {
                try
                {
                    wmiProperties.Add(wmiObject[property]);
                }
                catch
                {

                }
            }

            return wmiProperties;
        }

        public string GetJsonData()
        {
            string jsonData = JsonConvert.SerializeObject(this.computer);
            try
            {
                File.WriteAllText(Environment.MachineName + ".json", jsonData);
            }
            catch
            { 
            }
            return jsonData;
        }

        private double ReadCPUMaxClockSpeed()
        {
            uint eax, edx;
            uint MSR_TURBO_RATIO_LIMIT = 0x1AD;
            uint MSR_PLATFORM_INFO = 0xCE;

            Ring0.Rdmsr(MSR_PLATFORM_INFO, out eax, out edx);
            double baseClock = (eax >> 8) & 0xFF;

            double maxClock = 0;
            if(baseClock == 0)
            {
                maxClock = GetMaxCPUClockWMI();
            }
            else
            {
                Ring0.Rdmsr(MSR_TURBO_RATIO_LIMIT, out eax, out edx);
                maxClock = (((eax >> 0) & 0xFF) * 100);
            }

            return maxClock;
        }

        private double ReadCPUMaxTDP()
        {
            uint eax, edx;
            uint MSR_RAPL_POWER_UNIT = 0x606;
            uint MSR_RAPL_POWER_INFO = 0x614;

            Ring0.Rdmsr(MSR_RAPL_POWER_UNIT, out eax, out edx);
            float powerUnit = 1.0f / (1 << (int)(eax & 0xF));

            Ring0.Rdmsr(MSR_RAPL_POWER_INFO, out eax, out edx);
            float maxTdp = eax & 0x7FFF;
            maxTdp *= powerUnit;

            return maxTdp;
        }

        private double ReadCPUMaxTemperature()
        {
            uint eax, edx;
            uint MSR_TEMPERATURE_TARGET = 0x1A2;

            Ring0.Rdmsr(MSR_TEMPERATURE_TARGET, out eax, out edx);
            float maxTemperature = (eax >> 16) & 0xFF;

            return maxTemperature;
        }

        private double ReadGPUMaxTemperature()
        {
            float maxTemperature = 0;
            try
            {
                PhysicalGPU gpu = this.nvidiaGpus[0];
                GPUThermalSensor[] thermalSensors = gpu.ThermalInformation.ThermalSensors.ToArray();

                foreach (var thermalSensor in thermalSensors)
                {
                    maxTemperature = thermalSensor.DefaultMaximumTemperature;
                    break;
                }
            }
            catch
            {
                maxTemperature = -1;
            }

            return maxTemperature;
        }

        private double ReadGPUMaxCoreClock()
        {
            double maxCoreClock = 0;
            try
            {
                PhysicalGPU gpu = this.nvidiaGpus[0];

                maxCoreClock = gpu.BoostClockFrequencies.GraphicsClock.Frequency / 1000.0;
            }
            catch
            {
                maxCoreClock = -1;
            }

            return maxCoreClock;
        }

        private double ReadGPUMaxMemoryClock()
        {
            double maxMemoryClock = 0;
            try
            {
                PhysicalGPU gpu = this.nvidiaGpus[0];

                maxMemoryClock = gpu.BoostClockFrequencies.MemoryClock.Frequency / 1000.0;
            }
            catch
            {
                maxMemoryClock = -1;
            }

            return maxMemoryClock;
        }

        private double GetSystemTotalRAM()
        {
            string wmiNamespace = @"\\.\root\cimv2";
            string wmiClass = "Win32_PhysicalMemory";
            string property = "Capacity";

            double totalRam = 0;
            foreach (var ramSize in GetWmiProperties(wmiNamespace, wmiClass, property))
            {
                totalRam += Convert.ToDouble(ramSize);
            }
            totalRam /= (1 << 30);

            return totalRam;
        }

        private double GetMaxCPUClockWMI()
        {
            string wmiNamespace = @"\\.\root\cimv2";
            string wmiClass = "Win32_Processor";
            string property = "MaxClockSpeed";

            double maxClockSpeed = 0;
            foreach (var result in GetWmiProperties(wmiNamespace, wmiClass, property))
            {
                maxClockSpeed = Convert.ToDouble(result);
            }

            return maxClockSpeed;
        }
    }
}

/*foreach (var a in ((Dictionary<string, object>)wmiData["CPU0"]))
{
    Console.Write("\t" + a.Key);
    for(int i = a.Key.Length; i < 25; i++)
    {
        Console.Write("_");
    }
    Console.WriteLine(": " + a.Value);
}*/
