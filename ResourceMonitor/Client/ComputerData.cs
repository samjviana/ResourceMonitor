using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;
using Newtonsoft.Json;
using System.Reflection;
using System.Management;
//using NvAPIWrapper;

namespace Client
{
    class ComputerData
    {
        private Dictionary<string, object> computer;

        public ComputerData(Computer computer)
        {
            this.computer = new Dictionary<string, object>();
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
                foreach (var sensor in sensors)
                {
                    if (sensor.SensorType == (SensorType)sensorType)
                    {
                        if (hardwareType == HardwareType.CPU && sensor.SensorType == SensorType.Clock)
                        {
                            if (valuesDict.ContainsKey("MaxClockSpeed"))
                            {
                                valuesDict["MaxClockSpeed"] = ReadMaxClockSpeed();
                            }
                            else
                            {
                                valuesDict["MaxClockSpeed"] = ReadMaxClockSpeed();  
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
                        }

                        sensorDict.Add("Value", sensor.Value.GetValueOrDefault());

                        valuesDict.Add(sensor.Name, sensorDict);

                        sensorDict = new Dictionary<string, object>();
                    }
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

        public string GetJsonData()
        {
            return JsonConvert.SerializeObject(this.computer);
        }

        private string ReadMaxClockSpeed()
        {
            uint eax, edx;
            uint MSR_TURBO_RATIO_LIMIT = 0x01AD;
            string maxClockSpeed = "";
            Ring0.Rdmsr(MSR_TURBO_RATIO_LIMIT, out eax, out edx);
            maxClockSpeed = (((eax >> 0) & 0xFF) * 100).ToString();
            return maxClockSpeed;
        }
    }
}
