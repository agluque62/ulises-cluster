using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Utilities;
using ClusterLib;
using ClusterSrv.Properties;

namespace ClusterSrv
{

    public partial class ClusterSrv : ServiceBase
    {
        static string NodeId => System.Environment.MachineName;
        public ClusterSrv()
        {
            InitializeComponent();
        }

        public static void RunAsServer(string[] args)
        {
            ServiceBase.Run(new ClusterSrv());
        }

        public static void RunAsProcess(string[] args)
        {
            ClusterSrv Server = new ClusterSrv();
            try
            {
                Native.Kernel32.SetConsoleTitle(" CLUSTER SRV (" + NodeId + ") ");

                Console.WriteLine(Resources.ConsoleMenu.Replace("\\n", Environment.NewLine), NodeId);

                Server.OnStart(args);
                //_Cluster = new Cluster(GetClusterSettings());
                //_Cluster.Run();

                // Asegurar que libera la IP virtual antes de entrar.
                // Puede ser que ambos servidores, hayan cogido la IP virtual porque est�n incomunicados
                //_Cluster.Deactivate();

                ConsoleKeyInfo cki = new ConsoleKeyInfo();

                do
                {
                    while (Console.KeyAvailable == false)
                    {
                        System.Threading.Thread.Sleep(500);
                    }

                    cki = Console.ReadKey(true);

                    try
                    {
                        switch (cki.Key)
                        {
                            case ConsoleKey.M:
                                Console.WriteLine(Resources.ConsoleMenu.Replace("\\n", Environment.NewLine), NodeId);
                                break;
                            case ConsoleKey.A:
                                _Cluster?.Activate();
                                break;
                            case ConsoleKey.C:
                                if (_Cluster?.State.LocalNode.State == NodeState.NoActive)
                                {
                                    _Cluster?.Activate();
                                }
                                else
                                {
                                    _Cluster?.Deactivate();
                                }
                                break;
                            case ConsoleKey.I:
                                Console.WriteLine(_Cluster?.State);
                                break;
                            case ConsoleKey.Q:
                                break;
                            case ConsoleKey.H:
                                Console.WriteLine(Resources.ConsoleMenu.Replace("\n", Environment.NewLine), NodeId);
                                break;
#if DEBUG
                            case ConsoleKey.N:
                                _Cluster.WorkingNetworkSimulOff = !_Cluster.WorkingNetworkSimulOff;
                                break;
#endif
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception<ClusterSrv>(ex, Resources.ConsoleCommandError);
                    }

                } while (cki.Key != ConsoleKey.Q);

                //_Cluster.Dispose();
                Server.OnStop();
            }
            catch (Exception ex)
            {
                Logger.Exception<ClusterSrv>(ex, Resources.ServerRunningError);
                Logger.Fatal<ClusterSrv>($"{Resources.ServerRunningError} => {ex}");
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _Cluster = null;
                if (!Initiate())
                {
                    _Cluster = new Cluster(GetClusterSettings());
                    _Cluster.Run();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception<ClusterSrv>(ex, Resources.ServerRunningError);
                Logger.Fatal<ClusterSrv>($"{Resources.ServerRunningError} => {ex}");
                throw;
            }
        }

        protected override void OnStop()
        {
            if (!Finish())
            {
                _Cluster?.Dispose();
            }
            _Cluster = null;
            Task.Delay(TimeSpan.FromSeconds(2)).Wait();
        }

        private static ClusterSettings GetClusterSettings()
        {
            Settings st = Settings.Default;
            ClusterSettings cst = new ClusterSettings();

            cst.NodeId = NodeId;
            cst.Ip = st.Ip;
            cst.Port = st.Port;
            cst.EpIp = st.EpIp;
            cst.EpPort = st.EpPort;
            // AGL. 3� IP
            //cst.AdapterIp1 = st.AdapterIp1;
            //cst.AdapterIp2 = st.AdapterIp2;
            //cst.ClusterIp1 = st.ClusterIp1;
            //cst.ClusterIp2 = st.ClusterIp2;
            //cst.ClusterMask1 = st.ClusterMask1;
            //cst.ClusterMask2 = st.ClusterMask2;
            if (st.SactaProxyPresent == "1")
            {
                cst.VirtualIps = new List<ClusterIpSetting>()
                {
                    new ClusterIpSetting(){ AdapterIp = st.AdapterIp1, ClusterIp = st.ClusterIp1, ClusterMsk = st.ClusterMask1, AdapterMask=1, AdapterIndex=-1, VirtualIpContext=-1 },
                    new ClusterIpSetting(){ AdapterIp = st.AdapterIp2, ClusterIp = st.ClusterIp2, ClusterMsk = st.ClusterMask2, AdapterMask=2, AdapterIndex=-1, VirtualIpContext=-1 },
                    new ClusterIpSetting(){ AdapterIp = st.AdapterIp3, ClusterIp = st.ClusterIp3, ClusterMsk = st.ClusterMask3, AdapterMask=4, AdapterIndex=-1, VirtualIpContext=-1 },
                };
            }
            else
            {
                cst.VirtualIps = new List<ClusterIpSetting>()
                {
                    new ClusterIpSetting(){ AdapterIp = st.AdapterIp1, ClusterIp = st.ClusterIp1, ClusterMsk = st.ClusterMask1, AdapterMask=1, AdapterIndex=-1, VirtualIpContext=-1 },
                };
            }
            cst.TimeToStart = st.TimeToStart;
            cst.MaintenanceServerForTraps = st.MaintenanceServerForTraps;
            cst.Tick = st.Tick;
            cst.RemoteTimeout = st.RemoteTimeout;
            return cst;
        }

        static Cluster _Cluster;
        /** */
        Object ClusterControl2 { get; set; }
        bool Initiate()
        {
            ClusterControl2 = CustomAssemblyHelper.GetInstanceOf("ClusterLib2.dll", "ClusterLib2.ClusterControl2");
            if (ClusterControl2 != null)
            {
                CustomAssemblyHelper.InvokeMethod(ClusterControl2, "Run", new object[] { });
            }
            return ClusterControl2 != null;
        }
        bool Finish()
        {
            if (ClusterControl2 != null)
            {
                CustomAssemblyHelper.InvokeMethod(ClusterControl2, "Stop", new object[] { });
                return true;
            }
            return false;
        }
    }
}
