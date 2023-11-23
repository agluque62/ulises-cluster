using System;
using System.Collections.Generic;
using System.Text;

namespace ClusterLib
{
    public class ClusterIpSetting
    {
        public string AdapterIp { get; set; }
        public int AdapterMask { get; set; }
        public string ClusterIp { get; set; }
        public string ClusterMsk { get; set; }

        public int AdapterIndex { get; set; }
        public int VirtualIpContext { get; set; }

        public ClusterIpSetting() { }
        public ClusterIpSetting(ClusterIpSetting other)
        {
            AdapterIp = other.AdapterIp;
            AdapterMask = other.AdapterMask;
            AdapterIndex = other.AdapterIndex;

            ClusterIp = other.ClusterIp;
            ClusterMsk = other.ClusterMsk;
            VirtualIpContext = other.VirtualIpContext;
        }
    }
    public class ClusterSettings
    {
        public string NodeId;
        public string Ip;
        public int Port;
        public string EpIp;
        public int EpPort;
        //public string AdapterIp1;
        //public string AdapterIp2;
        //public string ClusterIp1;
        //public string ClusterIp2;
        //public string ClusterMask1;
        //public string ClusterMask2;
        public string MaintenanceServerForTraps;
        public int TimeToStart;
        public List<ClusterIpSetting> VirtualIps;
        public int Tick;
        public int RemoteTimeout;

        public override string ToString()
        {
            var str = $"NodeId: {NodeId}, Local: {Ip}:{Port}, Remote: {EpIp}:{EpPort}";
            VirtualIps.ForEach(vp =>
            {
                var stra = $"\n\t AdapterIp: {vp.AdapterIp}, Ipv: {vp.ClusterIp}";
                str += stra;
            });

            return str;
        }
    }
}
