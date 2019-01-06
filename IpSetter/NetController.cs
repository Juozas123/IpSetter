using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IpSetter
{
    public class NetController
    {
        Dictionary<string, ManagementObject> _mObjects = new Dictionary<string, ManagementObject>();
        Dictionary<string, string> _ips = new Dictionary<string, string>();

        public List<string> NiNames = new List<string>();

        public NetController()
        {
            var ifaceObjs = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface niObj in ifaceObjs)
            {
                NiNames.Add(niObj.Name);
                var mObj = GetNetworkAdapterManagementObject(niObj);
                _mObjects.Add(niObj.Name, mObj);
                _ips.Add(niObj.Name, null);
            }
        }

        private ManagementObject GetNetworkAdapterManagementObject(NetworkInterface netInterface)
        {
            ManagementObject oMngObj = null;

            if (netInterface == null) return null;

            string sNI = netInterface.Id;

            ManagementClass oMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection oMOC = oMC.GetInstances();

            foreach (ManagementObject oMO in oMOC)
            {
                string sMO = oMO["SettingID"].ToString();
                if (sMO == sNI)
                {
                    oMngObj = oMO;
                    break;
                }
            }

            return oMngObj;
        }

        public bool ApplyConfig(IpConfig Config)
        {
            var mObj = _mObjects[Config.IfaceName];

            bool operationOk = true;

            try
            {
                var paramStruct = mObj.GetMethodParameters("EnableStatic");
                string[] aIp = new string[1];
                string[] aSubnet = new string[1];
                aIp[0] = Config.Ip;
                aSubnet[0] = Config.Subnet;
                paramStruct["IPAddress"] = aIp;
                paramStruct["SubnetMask"] = aSubnet;

                mObj.InvokeMethod("EnableStatic", paramStruct, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                operationOk = false;
            }

            return operationOk;
        }

        public bool EnableDHCP(string ifaceName)
        {
            var mObj = _mObjects[ifaceName];

            if (!(bool)mObj["IPEnabled"])
                return false;

            var ndns = mObj.GetMethodParameters("SetDNSServerSearchOrder");
            ndns["DNSServerSearchOrder"] = null;
            var enableDhcp = mObj.InvokeMethod("EnableDHCP", null, null);
            var setDns = mObj.InvokeMethod("SetDNSServerSearchOrder", ndns, null);
            return true;
        }

        public Dictionary<string, string> GetIps()
        {
            /*
            foreach(KeyValuePair<string, ManagementObject> pair in _mObjects)
            {
                var mObj = pair.Value;
                if (mObj == null)
                    continue;

                string[] arrIpAddress = (string[])(mObj["IPAddress"]);
                if (arrIpAddress != null)
                {
                    var ipAddress = arrIpAddress.FirstOrDefault(s => s.Contains('.'));
                    _ips[pair.Key] = ipAddress;
                }     
            }
            */

            

            StringBuilder sb = new StringBuilder();

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                IPInterfaceProperties properties = network.GetIPProperties();

                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    _ips[network.Name] = address.Address.ToString();
                    //sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")");
                }
            }

            return _ips;

            //MessageBox.Show(sb.ToString());

        }
    }
}
