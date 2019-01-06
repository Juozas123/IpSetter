using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IpSetter
{
    public class NetManager
    {
        NetworkInterface[] _ifaceObjs;
        Dictionary<string, ManagementObject> _managementObjs;
        
        public List<string> InterfaceNames { get; private set; }


        public NetManager()
        {
            _ifaceObjs = NetworkInterface.GetAllNetworkInterfaces();
            _managementObjs = new Dictionary<string, ManagementObject>();

            foreach (NetworkInterface niObj in _ifaceObjs)
            {
                var mObj = GetNetworkAdapterManagementObject(niObj);
                _managementObjs.Add(niObj.Name, mObj);
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

        public Dictionary<string, NetIfaceController> GetControllers()
        {

            var tempDict = new Dictionary<string, NetIfaceController>();

            foreach(KeyValuePair<string, ManagementObject> pair in _managementObjs)
            {
                var tempController = new NetIfaceController(pair.Key, pair.Value);
                tempDict.Add(tempController.Name, tempController);
            }

            return tempDict;
        }







    }
}
