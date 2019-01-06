using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace IpSetter
{
    public class NetIfaceController
    {
        public string Name { get; private set; }
        public string IpAddress
        {
            get => GetCurrentIp();
        }
        public string Subnet { get; }

        ManagementObject _mObj;
        
        public NetIfaceController(string Name, ManagementObject MObj)
        {
            this.Name = Name;
            _mObj = MObj;
        }

        public bool SetStaticIp(string IpAddress, string Subnet)
        {
            bool operationOk = true;

            try
            {
                var paramStruct = _mObj.GetMethodParameters("EnableStatic");
                paramStruct["IPAddress"] = IpAddress;
                paramStruct["SubnetMask"] = Subnet;

                _mObj.InvokeMethod("EnableStatic", paramStruct, null);
            }
            catch (Exception ex)
            {
                operationOk = false;
            }
          
            return operationOk;
        }
        public bool EnableDHCP()
        {
            if (!(bool)_mObj["IPEnabled"])
                return false;

            var ndns = _mObj.GetMethodParameters("SetDNSServerSearchOrder");
            ndns["DNSServerSearchOrder"] = null;
            var enableDhcp = _mObj.InvokeMethod("EnableDHCP", null, null);
            var setDns = _mObj.InvokeMethod("SetDNSServerSearchOrder", ndns, null);
            return true;
        }

        private string GetCurrentIp()
        {
            string[] arrIpAddress = (string[])(_mObj["IPAddress"]);
            var ipAddress = arrIpAddress.FirstOrDefault(s => s.Contains('.'));

            return ipAddress;
        }
    }
}
