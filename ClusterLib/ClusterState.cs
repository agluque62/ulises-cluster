using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;

using ClusterLib.Properties;
using Utilities;

namespace ClusterLib
{
    public enum MsgType { Activate = 1, Deactivate, GetState }

    public enum NodeState { NoValid, Activating, Active, NoActive }

    [Serializable]
    public class NodeInfo
    {
        public string Name;
        public string AdapterIp1;
        public string AdapterIp2;
        public string VirtualIp1;
        public string VirtualIp2;
        public string ReplicationServiceState;

        [NonSerialized]
        private static object _Sync = new object();

        [NonSerialized]
        public string LocalPrivateIp;
        private string _LocalPrivateIp
        {
            get { return _LocalPrivateIp; }
        }
        [NonSerialized]
        public string RemotePrivateIp;
        private string _RemotePrivateIp
        {
            get { return _RemotePrivateIp; }
        }

        [NonSerialized]
        private string _MaintenanceServerIpForTraps;

        [NonSerialized]
        private bool _NodoLocal;


        public NodeState State
        {
            get { return _State; }
        }

        public DateTime StateBegin
        {
            get { return _StateBegin; }
        }

        public string ChangeCause
        {
            get { return _ChangeCause; }
        }

        public int ValidAdaptersMask
        {
            // todo
            get { return _ValidAdaptersMask; }
            set
            {
                int changes = _FirstTime ? 0x03 : (value ^ _ValidAdaptersMask);

                if ((changes & 1) != 0)
                {
                    if ((value & 1) == 0)
                    {
                        Logger.Warn<ClusterState>($"Node {Name}: " + String.Format(Resources.AdapterNotOperational, 1, AdapterIp1));
                    }
                    else
                    {
                        Logger.Info<ClusterState>($"Node {Name}: " + String.Format(Resources.AdapterDetected, 1, AdapterIp1));
                    }
                }
                if ((changes & 2) != 0)
                {
                    if ((value & 2) == 0)
                    {
                        Logger.Warn<ClusterState>($"Node {Name}: " + String.Format(Resources.AdapterNotOperational, 2, AdapterIp2));
                    }
                    else
                    {
                        Logger.Info<ClusterState>($"Node {Name}: " + String.Format(Resources.AdapterDetected, 2, AdapterIp2));
                    }
                }

                _ValidAdaptersMask = value;
                _FirstTime = false;
            }
        }

        public NodeInfo()
        {
            _NodoLocal = false;
            ReplicationServiceState = "0";
        }

        public NodeInfo(ClusterSettings settings)
        {
            _NodoLocal = true;

            Name = settings.NodeId;

            AdapterIp1 = settings.VirtualIps.ElementAt(0).AdapterIp;
            VirtualIp1 = settings.VirtualIps.ElementAt(0).ClusterIp + "/" + settings.VirtualIps.ElementAt(0).ClusterMsk;

            AdapterIp2 = settings.VirtualIps.Count > 1 ? settings.VirtualIps.ElementAt(1).AdapterIp : "127.0.0.1";
            VirtualIp2 = settings.VirtualIps.Count > 1 ? settings.VirtualIps.ElementAt(1).ClusterIp + "/" + settings.VirtualIps.ElementAt(1).ClusterMsk : "127.0.0.1/255.255.255.0";
            // La tercera no se mete por compatibilidad....

            RemotePrivateIp = settings.EpIp;
            LocalPrivateIp = settings.Ip;
            _MaintenanceServerIpForTraps = settings.MaintenanceServerForTraps;

            ReplicationServiceState = "0";

        }

        public void SetState(NodeState state, string changeCause)
        {
            if (state != _State)
            {
                Logger.Info<ClusterState>($"Node {Name}: Change State to {state}. Cause: {changeCause ?? ""}");
                _State = state;
                _StateBegin = DateTime.Now;
                _ChangeCause = changeCause;
            }
        }

        public override string ToString()
        {
            if (State == NodeState.NoValid)
            {
                return "   " + Resources.NodeNotOperational;
            }

            return string.Format("   {0,-18}: {1}\n   {2,-18}: {3}\n   {4,-18}: {5}\n   {6,-18}: {7}\n   {8,-18}: {9}\n   {10,-18}: {11}\n   {12,-18}: {13}",
                Resources.Name, Name, Resources.State, State, Resources.StateBegin, StateBegin,
                Resources.Adapter1, (ValidAdaptersMask & 1) != 0 ? AdapterIp1 : Resources.NotOperational,
                Resources.Adapter2, (ValidAdaptersMask & 2) != 0 ? AdapterIp2 : Resources.NotOperational,
                "Ip Virtual 1", VirtualIp1, "Ip Virtual 2", VirtualIp2);
        }

        public void UpdateInfo(NodeInfo n)
        {
            this.Name = n.Name;
            this.AdapterIp1 = n.AdapterIp1;
            this.AdapterIp2 = n.AdapterIp2;
            this.VirtualIp1 = n.VirtualIp1;
            this.VirtualIp2 = n.VirtualIp2;
            this._State = n._State;
            this._StateBegin = n._StateBegin;
            this._ChangeCause = n._ChangeCause;
            this._ValidAdaptersMask = n._ValidAdaptersMask;
            this.ReplicationServiceState = n.ReplicationServiceState;
        }

        #region Private

        private NodeState _State = NodeState.NoValid;
        private DateTime _StateBegin;
        private string _ChangeCause;
        private int _ValidAdaptersMask = 0;
        private bool _FirstTime = false;

        #endregion
    }

    [Serializable]
    public class MasterStatus
    {
        public string File;
        public string Position;
    }

    [Serializable]
    public class SlaveStatus
    {
        public string Master_Log_File;
        public string Read_Master_Log_Pos;
        public string Last_Errno;
        public string Last_IO_Errno;
        public string Last_SQL_Errno;
    }

    [Serializable]
    public class DataReplicacionState
    {
        public MasterStatus Master;
        public SlaveStatus Slave;

        public DataReplicacionState()
        {
            Master = new MasterStatus();
            Slave = new SlaveStatus();
        }
    }

    [Serializable]
    public class ClusterState
    {
        public NodeInfo LocalNode;
        public NodeInfo RemoteNode;
        public DataReplicacionState DataReplication;

        [NonSerialized]
        private Timer _ReplicationServiceTimer;
        [NonSerialized]
        private int _ReplicationServiceInterval = 60000;

        public ClusterState()
        {
            LocalNode = new NodeInfo();
            RemoteNode = new NodeInfo();
            DataReplication = new DataReplicacionState();
        }

        public ClusterState(ClusterSettings settings)
        {
            LocalNode = new NodeInfo(settings);
            RemoteNode = new NodeInfo();
            DataReplication = new DataReplicacionState();

            _ReplicationServiceTimer = new Timer(_ReplicationServiceInterval);
            _ReplicationServiceTimer.AutoReset = true;
            _ReplicationServiceTimer.Enabled = true;
            _ReplicationServiceTimer.Elapsed += ReplicationServiceTask;

            ReplicationServiceTask(null, null);
        }

        public override string ToString()
        {
            return string.Format(Resources.ClusterInfo.Replace("\\n", Environment.NewLine), LocalNode, RemoteNode);
        }

        void ReplicationServiceTask(object sender, ElapsedEventArgs e)
        {
            try
            {
#if DEBUG

                Logger.Info<ClusterState>("ReplicationServiceTask!");
#else
                // Cada servidor se va a encargar de recopilar la informaci�n del estado 
                // del servicio de la replicaci�n
                string proceso = "replication_status.bat";
                LocalNode.ReplicationServiceState = ExecCommand(PrepareCommand(proceso)).ToString();
                Logger.Info< ClusterState>("Replication status: " + LocalNode.ReplicationServiceState);

                // Cada servidor va a recopilar la informaci�n del 
                // estado de los datos de la replicaci�n propios
                GetDataReplicationStatus();
                GetInfo();
#endif
            }
            catch (Exception x)
            {
                Logger.Exception<ClusterState>(x);
            }
        }

        private void GetDataReplicationStatus()
        {
            try
            {
                // Ejecuta el bat data_replication_status
                // que va a generar los ficheros de texto
                // estado_data_master.txt y estado_data_slave.txt.
                string proceso = "data_replication_status.bat";
                ExecCommand(PrepareCommand(proceso));
            }
            catch (Exception x)
            {
                Logger.Exception<ClusterState>(x);
            }
        }

        private void GetInfo()
        {
            // Procesar estado_data_master
            foreach (var line in System.IO.File.ReadAllLines("estado_data_master.txt"))
            {
                if (line.Contains(("File")))
                {
                    string[] data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length > 1)
                        DataReplication.Master.File = data[1];
                }
                else if (line.Contains("Position"))
                {
                    string[] data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length > 1)
                        DataReplication.Master.Position = data[1];

                    break;
                }
            }

            // Procesar estado_data_slave
            foreach (var line in System.IO.File.ReadAllLines("estado_data_slave.txt"))
            {
                if (line.Contains(("Master_Log_File")))
                {
                    string[] data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length > 1)
                        DataReplication.Slave.Master_Log_File = data[1];
                }
                else if (line.Contains("Read_Master_Log_Pos"))
                {
                    string[] data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length > 1)
                        DataReplication.Slave.Read_Master_Log_Pos = data[1];
                }
                else if (line.Contains(("Last_Errno")))
                {
                    string[] data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length > 1)
                        DataReplication.Slave.Last_Errno = data[1];
                }
                else if (line.Contains("Last_IO_Errno"))
                {
                    string[] data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length > 1)
                        DataReplication.Slave.Last_IO_Errno = data[1];
                }
                else if (line.Contains("Last_SQL_Errno"))
                {
                    string[] data = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length > 1)
                        DataReplication.Slave.Last_SQL_Errno = data[1];

                    break;
                }
            }
        }

        private System.Diagnostics.ProcessStartInfo PrepareCommand(string proceso)
        {
            // OJO CON LAS CLAVES!!!!!!!
            string argumentos = LocalNode.RemotePrivateIp;

            System.Diagnostics.ProcessStartInfo p = new System.Diagnostics.ProcessStartInfo("%SystemRoot%\\Sysnative\\cmd.exe", "/c" + proceso);
            p.FileName = proceso; p.Arguments = argumentos;

            p.CreateNoWindow = false;
            p.UseShellExecute = false;
            p.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
            p.RedirectStandardError = true;
            p.RedirectStandardOutput = true;

            return p;
        }

        private int ExecCommand(System.Diagnostics.ProcessStartInfo p)
        {
            System.Diagnostics.Process pFinal = System.Diagnostics.Process.Start(p);
            pFinal.WaitForExit();

            int exitCode = pFinal.ExitCode;
            pFinal.Close();

            return exitCode;
        }
    }
}
