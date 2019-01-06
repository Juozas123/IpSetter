using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace IpSetter
{
    [Serializable()]
    public class IpConfig
    {
        public string Name { get; private set; }
        public string Ip { get; private set; }
        public string Subnet { get; private set; }
        public string IfaceName { get; private set; }

        public IpConfig(string IpAddress, string SubnetMask, string InterfaceName, string Name)
        {
            this.Name = Name;
            this.Ip = IpAddress;
            this.Subnet = SubnetMask;
            this.IfaceName = InterfaceName;
        }


    }
}
