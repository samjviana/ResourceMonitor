using Newtonsoft.Json;
using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    class SnmpManager
    {
        private Dictionary<int, NetworkInterface> networkInterfaces;
        private Dictionary<int, object> discoveryData;
        private Dictionary<int, IPAddress> masks;
        private Dictionary<int, IPAddress> ips;
        private Dictionary<int, IPAddress> networks;
        private Dictionary<int, IPAddress> broadcasts;
        private Dictionary<int, int> usableIps;
        private Dictionary<string, object> pingData;
        private object threadsObj;
        private bool discoveryDone;

        public SnmpManager()
        {
            this.discoveryDone = false;
            GetNetworkInterfaces();
            GetIpAndMask();
            GetNetAndBroadcast();
            ShowData();
        }

        public void StartDiscovery()
        {
            Thread discoveryThread = new Thread(new ThreadStart(NetworkDiscovery));
            discoveryThread.Start();
        }

        public void StopDiscovery()
        {
            foreach (var thread in (Thread[])threadsObj)
            {
                thread.Join();
            }
        }

        private void GetNetworkInterfaces()
        {
            int id = 0;
            networkInterfaces = new Dictionary<int, NetworkInterface>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                bool isPseudo = false;
                foreach (UnicastIPAddressInformation uipi in ni.GetIPProperties().UnicastAddresses)
                {
                    if (uipi.Address.ToString() == "127.0.0.1")
                    {
                        isPseudo = true;
                        break;
                    }
                }
                if (ni.OperationalStatus != OperationalStatus.Up || isPseudo)
                {
                    continue;
                }

                networkInterfaces.Add(id, ni);
                id++;
            }
        }

        private void GetIpAndMask()
        {
            ips = new Dictionary<int, IPAddress>();
            masks = new Dictionary<int, IPAddress>();
            foreach (var networkInterface in networkInterfaces)
            {
                foreach (UnicastIPAddressInformation uipi in networkInterface.Value.GetIPProperties().UnicastAddresses)
                {
                    if (uipi.IPv4Mask.ToString() != ("0.0.0.0"))
                    {
                        ips.Add(networkInterface.Key, uipi.Address);
                        masks.Add(networkInterface.Key, uipi.IPv4Mask);
                        break;
                    }
                }
                if (ips.Count == 0)
                {
                    ips.Add(networkInterface.Key, IPAddress.Parse("0.0.0.0"));
                }
                if (masks.Count == 0)
                {
                    masks.Add(networkInterface.Key, IPAddress.Parse("0.0.0.0"));
                }
            }
        }

        private void GetNetAndBroadcast()
        {
            networks = new Dictionary<int, IPAddress>();
            broadcasts = new Dictionary<int, IPAddress>();
            usableIps = new Dictionary<int, int>();
            foreach (var networkInterface in networkInterfaces)
            {
                IPAddress mask = masks[networkInterface.Key];
                IPAddress ip = ips[networkInterface.Key];

                int ipInt = BitConverter.ToInt32(ip.GetAddressBytes(), 0);
                int maskInt = BitConverter.ToInt32(mask.GetAddressBytes(), 0);
                int networkInt = ipInt & maskInt;
                int broadcastInt = ipInt | ~maskInt;

                networks.Add(networkInterface.Key, new IPAddress(BitConverter.GetBytes(networkInt)));
                broadcasts.Add(networkInterface.Key, new IPAddress(BitConverter.GetBytes(broadcastInt)));

                string binaryMask = "";
                foreach (var bytes in mask.GetAddressBytes())
                {
                    binaryMask += Convert.ToString(~bytes & 0xFF, 2);
                }

                usableIps.Add(networkInterface.Key, Convert.ToInt32(binaryMask, 2) - 1);
            }
        }

        private int currentNetworkId;
        private int currentIp;
        private int threadCount;
        private void NetworkDiscovery()
        {
            discoveryData = new Dictionary<int, object>();
            foreach (var networkInterface in networkInterfaces)
            {
                this.currentNetworkId = networkInterface.Key;

                byte[] startIp = networks[networkInterface.Key].GetAddressBytes();
                int[] octets = new int[4];

                octets[0] = startIp[0];
                octets[1] = startIp[1];
                octets[2] = startIp[2];
                octets[3] = startIp[3] + 1;

                pingData = new Dictionary<string, object>();
                Thread[] pingThreads = new Thread[usableIps[networkInterface.Key]];
                threadsObj = pingThreads;

                this.currentIp = 0;
                this.threadCount = 0;
                for (int i = 0; i < usableIps[networkInterface.Key]; i++)
                {
                    string ipToPing = octets[0] + "." + octets[1] + "." + octets[2] + "." + octets[3];

                    pingThreads[i] = new Thread(new ThreadStart(PingAsync));
                    pingThreads[i].Name = ipToPing;
                    pingThreads[i].Start();
                    this.threadCount++;

                    octets[3]++;
                    if (octets[3] > 255)
                    {
                        octets[3] = 0;
                        octets[2]++;
                        if (octets[2] > 255)
                        {
                            octets[2] = 0;
                            octets[1]++;
                            if (octets[1] > 255)
                            {
                                octets[1] = 0;
                                octets[0]++;
                            }
                        }
                    }

                    if(this.threadCount >= 256)
                    {
                        pingThreads[i].Join();
                    }
                }

                foreach (var thread in pingThreads)
                {
                    thread.Join();
                }

                Console.WriteLine("Teste");
                discoveryData.Add(networkInterface.Key, pingData);
            }

            Console.WriteLine("NETWORK DISCOVERY COMPLETED");
            this.discoveryDone = true;
        }

        private void PingAsync()
        {
            AutoResetEvent waiter = new AutoResetEvent(false);

            Ping pingSender = new Ping();

            pingSender.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            int timeout = 1000;

            PingOptions options = new PingOptions(64, true);

            object[] asyncData = { waiter, Thread.CurrentThread.Name };

            pingSender.SendAsync(Thread.CurrentThread.Name, timeout, buffer, options, asyncData);

            waiter.WaitOne();
        }

        private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            AutoResetEvent waiter = (AutoResetEvent)((object[])e.UserState)[0];
            string sourceIP = (string)((object[])e.UserState)[1];

            if (e.Cancelled)
            {
                Console.WriteLine("Ping canceled.");

                waiter.Set();
            }

            if (e.Error != null)
            {
                Console.WriteLine("Ping failed:");
                Console.WriteLine(e.Error.ToString());

                waiter.Set();
            }

            Console.WriteLine(sender);
            PingReply reply = e.Reply;

            ManageResult(reply, sourceIP);

            waiter.Set();
        }

        public void ManageResult(PingReply reply, string sourceIP)
        {
            if (reply == null)
                return;

            if (reply.Status == IPStatus.Success)
            {
                string hostname = "";
                try
                {
                    IPHostEntry ipToDomainName = Dns.GetHostEntry(reply.Address);
                    hostname = ipToDomainName.HostName;
                }
                catch
                {
                    hostname = "";
                }

                if (SnmpGet(reply.Address.ToString(), "local", false))
                {
                    pingData[sourceIP] = string.Format("Hostname: {0} - SNMP: {1}", hostname.PadRight(20, ' '), "OK");
                }
                else
                {
                    pingData[sourceIP] = string.Format("Hostname: {0} - SNMP: {1}", hostname.PadRight(20, ' '), "ERROR");
                }
            }
            else
            {
                pingData[sourceIP] = "ERROR";
            }

            this.currentIp++;
            this.threadCount--;
            /*textBox5.Invoke((Action)delegate
            {
                textBox5.AppendText(string.Format("{0} - {1}{2}", sourceIP.PadRight(14, ' '), pingData[sourceIP], Environment.NewLine));
            });*/
        }

        private bool SnmpGet(string agent, string community, bool log)
        {
            OctetString octetCommunity = new OctetString(community);

            // Define agent parameters class
            AgentParameters param = new AgentParameters(octetCommunity);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver1;
            // Construct the agent address object
            // IpAddress class is easy to use here because
            //  it will try to resolve constructor parameter if it doesn't
            //  parse to an IP address
            IpAddress agentIp = new IpAddress(agent);

            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agentIp, 161, 2000, 1);

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);
            pdu.VbList.Add("1.3.6.1.2.1.1.1.0"); //sysDescr
            pdu.VbList.Add("1.3.6.1.2.1.1.2.0"); //sysObjectID
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); //sysUpTime
            pdu.VbList.Add("1.3.6.1.2.1.1.4.0"); //sysContact
            pdu.VbList.Add("1.3.6.1.2.1.1.5.0"); //sysName

            // Make SNMP request
            SnmpV1Packet result;
            try
            {
                result = (SnmpV1Packet)target.Request(pdu, param);
            }
            catch (SnmpSharpNet.SnmpException)
            {
                if (log)
                {
                    //textBox3.AppendText("Número de tentativas foi excedido!" + Environment.NewLine);
                    //textBox3.AppendText("O agente não respondeu" + Environment.NewLine);
                }
                return false;
            }

            // If result is null then agent didn't reply or we couldn't parse the reply.
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by 
                // the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0 && log)
                {
                    // agent reported an error with the request
                    //textBox3.AppendText("Error in SNMP reply." + Environment.NewLine);
                    //textBox3.AppendText(string.Format("Error {0} index {1}{2}", result.Pdu.ErrorStatus, result.Pdu.ErrorIndex, Environment.NewLine));
                    target.Close();
                    return false;
                }
                else if (log)
                {
                    /*
                    // Reply variables are returned in the same order as they were added
                    //  to the VbList
                    textBox3.AppendText(string.Format("sysDescr({0}) ({1}): {2}{3}{4}",
                        result.Pdu.VbList[0].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type),
                        Environment.NewLine + "    • ",
                        result.Pdu.VbList[0].Value.ToString(),
                        Environment.NewLine));
                    textBox3.AppendText(string.Format("sysObjectID({0}) ({1}): {2}{3}{4}",
                        result.Pdu.VbList[1].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[1].Value.Type),
                        Environment.NewLine + "    • ",
                        result.Pdu.VbList[1].Value.ToString(),
                        Environment.NewLine));
                    textBox3.AppendText(string.Format("sysUpTime({0}) ({1}): {2}{3}{4}",
                        result.Pdu.VbList[2].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[2].Value.Type),
                        Environment.NewLine + "    • ",
                        result.Pdu.VbList[2].Value.ToString(),
                        Environment.NewLine));
                    textBox3.AppendText(string.Format("sysContact({0}) ({1}): {2}{3}{4}",
                        result.Pdu.VbList[3].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[3].Value.Type),
                        Environment.NewLine + "    • ",
                        result.Pdu.VbList[3].Value.ToString(),
                        Environment.NewLine));
                    textBox3.AppendText(string.Format("sysName({0}) ({1}): {2}{3}{4}",
                        result.Pdu.VbList[4].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[4].Value.Type),
                        Environment.NewLine + "    • ",
                        result.Pdu.VbList[4].Value.ToString(),
                        Environment.NewLine));
                        */
                }
            }
            else if (log)
            {
                //textBox3.AppendText(string.Format("{0}{1}", "No response received from SNMP agent.", Environment.NewLine));
                target.Close();
                return false;
            }
            target.Close();

            return true;
        }

        public Boolean DiscoveryDone
        {
            get { return this.discoveryDone; }
            set { }
        }

        public string GetJsonData()
        {
            Dictionary<int, object> jsonData = new Dictionary<int, object>();

            foreach (var networkInterface in networkInterfaces)
            {
                List<object> data = new List<object>();

                data.Add(networkInterface.Value.Name);
                data.Add(networkInterface.Value.GetPhysicalAddress().ToString());
                foreach (GatewayIPAddressInformation gipi in networkInterface.Value.GetIPProperties().GatewayAddresses)
                {
                    data.Add(gipi.Address.ToString());
                    break;
                }
                data.Add(ips[networkInterface.Key].ToString());
                data.Add(masks[networkInterface.Key].ToString());
                data.Add(networks[networkInterface.Key].ToString());
                data.Add(broadcasts[networkInterface.Key].ToString());
                data.Add(discoveryData[networkInterface.Key]);

                jsonData.Add(networkInterface.Key, data);
                data = new List<object>();
            }

            return JsonConvert.SerializeObject(jsonData);
        }

        private void ShowData()
        {
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("NETWORK INTERFACES INFORMATION");
            Console.WriteLine("-------------------------------------------------------------------------");
            foreach (var networkInterface in networkInterfaces)
            {
                Console.WriteLine(string.Format("ID: {0}", networkInterface.Key));
                Console.WriteLine(string.Format("    • Name: {0}", networkInterface.Value.Name));
                Console.WriteLine(string.Format("    • MAC: {0}", networkInterface.Value.GetPhysicalAddress()));
                Console.WriteLine(string.Format("    • Gateways:"));
                foreach (GatewayIPAddressInformation gipi in networkInterface.Value.GetIPProperties().GatewayAddresses)
                {
                    Console.WriteLine(string.Format("        - {0}", gipi.Address));
                }
                Console.WriteLine(string.Format("    • IP Addresses:"));
                foreach (UnicastIPAddressInformation uipi in networkInterface.Value.GetIPProperties().UnicastAddresses)
                {
                    Console.WriteLine(string.Format("        - {0} / {1}", uipi.Address, uipi.IPv4Mask));
                }
            }
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("ADDRESSES INFORMATION");
            Console.WriteLine("-------------------------------------------------------------------------");
            foreach (var networkInterface in networkInterfaces)
            {
                Console.WriteLine(string.Format("ID: {0} / {1}", networkInterface.Key, networkInterface.Value.Name));
                Console.WriteLine(string.Format("    • IP: {0}", ips[networkInterface.Key]));
                Console.WriteLine(string.Format("    • Mask: {0}", masks[networkInterface.Key]));
                Console.WriteLine(string.Format("    • Network: {0}", networks[networkInterface.Key]));
                Console.WriteLine(string.Format("    • Broadcast: {0}", broadcasts[networkInterface.Key]));
                Console.WriteLine(string.Format("    • Usable Ips: {0}", usableIps[networkInterface.Key]));
            }
        }

        public string NetworkProgress
        {
            get { return string.Format("{0} / {1}", this.currentNetworkId + 1, this.networkInterfaces.Count); }
        }

        public string IpProgress
        {
            get { return string.Format("{0} / {1}", this.currentIp + 1, this.usableIps[this.currentNetworkId]); }
        }
    }
}
