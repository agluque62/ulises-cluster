using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Utilities
{
   public static class Native
   {
      public static class Ntdll
      {
         [StructLayout(LayoutKind.Sequential)]
         public struct PROCESS_BASIC_INFORMATION
         {
            public int ExitStatus;
            public int PebBaseAddress;
            public int AffinityMask;
            public int BasePriority;
            public int UniqueProcessId;
            public int InheritedFromUniqueProcessId;

            public int Size
            {
               get { return (6 * 4); }
            }
         }

         public enum PROCESSINFOCLASS : int
         {
            ProcessBasicInformation = 0,
            ProcessQuotaLimits,
            ProcessIoCounters,
            ProcessVmCounters,
            ProcessTimes,
            ProcessBasePriority,
            ProcessRaisePriority,
            ProcessDebugPort,
            ProcessExceptionPort,
            ProcessAccessToken,
            ProcessLdtInformation,
            ProcessLdtSize,
            ProcessDefaultHardErrorMode,
            ProcessIoPortHandlers, // Note: this is kernel mode only
            ProcessPooledUsageAndLimits,
            ProcessWorkingSetWatch,
            ProcessUserModeIOPL,
            ProcessEnableAlignmentFaultFixup,
            ProcessPriorityClass,
            ProcessWx86Information,
            ProcessHandleCount,
            ProcessAffinityMask,
            ProcessPriorityBoost,
            MaxProcessInfoClass,
            ProcessWow64Information = 26
         };

         [DllImport("ntdll.dll", SetLastError = true)]
         static extern int NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);

         public static int GetParentProcessID(int processId)
         {
            IntPtr process = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.QueryInformation, false, processId);
            if (process == IntPtr.Zero)
            {
               throw new InvalidOperationException("Error opening process handle");
            }

            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            int retLen;

            int ntStatus = NtQueryInformationProcess(process, PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, pbi.Size, out retLen);
            Kernel32.CloseHandle(process);

            if (ntStatus != 0)
            {
               throw new InvalidOperationException("Error getting parent PID");
            }

            return pbi.InheritedFromUniqueProcessId;
         }
      }

      public static class Kernel32
      {
         [Flags]
         public enum ProcessAccessFlags : int
         {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
         }

         [DllImport("kernel32.dll", SetLastError = true)]
         public static extern bool AllocConsole();

         [DllImport("kernel32.dll", SetLastError = true)]
         public static extern bool FreeConsole();

         [DllImport("kernel32", SetLastError = true)]
         public static extern bool AttachConsole(int dwProcessId);

         [DllImport("kernel32.dll")]
         public static extern bool SetConsoleTitle(string lpConsoleTitle);

         [DllImport("kernel32.dll")]
         public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
         
         [DllImport("kernel32.dll", SetLastError = true)]
         [return: MarshalAs(UnmanagedType.Bool)]
         public static extern bool CloseHandle(IntPtr hObject);
      }

      public static class User32
      {
         [DllImport("user32.dll")]
         public static extern IntPtr GetForegroundWindow();

         [DllImport("user32.dll", SetLastError = true)]
         public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
      }

      public static class IpHlpApi
      {
         const int ERROR_BUFFER_OVERFLOW = 111;

         const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
         const int MAX_ADAPTER_NAME_LENGTH = 256;
         const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

         const int MAX_INTERFACE_NAME_LEN = 256;
         const int MAXLEN_PHYSADDR = 8;
         const int MAXLEN_IFDESCR = 256;

         public const int MIB_IF_TYPE_OTHER = 1;
         public const int MIB_IF_TYPE_ETHERNET = 6;
         public const int MIB_IF_TYPE_TOKENRING = 9;
         public const int MIB_IF_TYPE_FDDI = 15;
         public const int MIB_IF_TYPE_PPP = 23;
         public const int MIB_IF_TYPE_LOOPBACK = 24;
         public const int MIB_IF_TYPE_SLIP = 28;

         public const int IF_OPER_STATUS_NON_OPERATIONAL = 0;
         public const int IF_OPER_STATUS_UNREACHABLE = 1;
         public const int IF_OPER_STATUS_DISCONNECTED = 2;
         public const int IF_OPER_STATUS_CONNECTING = 3;
         public const int IF_OPER_STATUS_CONNECTED = 4;
         public const int IF_OPER_STATUS_OPERATIONAL = 5;

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         public struct IP_ADDRESS_STRING
         {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Address;
         }

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         public struct IP_ADDR_STRING
         {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public int Context;
         }

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         public struct IP_ADAPTER_INFO
         {
            public IntPtr Next;
            public int ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string AdapterDescription;
            public int AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;
            public int Index;
            public int Type;
            public int DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public int LeaseObtained;
            public int LeaseExpires;
         }

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
         public struct MIB_IFROW
         {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_INTERFACE_NAME_LEN)]
            public string wszName;
            public int dwIndex;
            public int dwType;
            public int dwMtu;
            public int dwSpeed;
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXLEN_PHYSADDR)]
            public byte[] bPhysAddr;
            public int dwAdminStatus;
            public int dwOperStatus;
            public int dwLastChange;
            public int dwInOctets;
            public int dwInUcastPkts;
            public int dwInNUcastPkts;
            public int dwInDiscards;
            public int dwInErrors;
            public int dwInUnknownProtos;
            public int dwOutOctets;
            public int dwOutUcastPkts;
            public int dwOutNUcastPkts;
            public int dwOutDiscards;
            public int dwOutErrors;
            public int dwOutQLen;
            public int dwDescrLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXLEN_IFDESCR)]
            public byte[] bDescr;
         }

         [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
         public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int GetAdapterIndex(string adapter, out int index);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int GetIfEntry(IntPtr pIfRow);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int AddIPAddress(int Address, int IpMask, int IfIndex, out int NTEContext, out int NTEInstance);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int DeleteIPAddress(int NTEContext);

         public static int GetAdapterIndex(string adapter)
         {
            int index;

            int ret = GetAdapterIndex(adapter, out index);
            if (ret != 0)
            {
                throw new System.ComponentModel.Win32Exception(ret, String.Format("GetAdapterIndex Error {0}, Adapter/IP: {1}", ret, adapter));
            }

            return index;
         }

         public static int GetAdapterInfo(string adapter, out IP_ADAPTER_INFO info)
         {
            long structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));
            IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

            try
            {
               int ret = GetAdaptersInfo(pArray, ref structSize);

               if (ret == ERROR_BUFFER_OVERFLOW)
               {
                  pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));
                  ret = GetAdaptersInfo(pArray, ref structSize);
               }

               if (ret != 0)
               {
                  throw new System.ComponentModel.Win32Exception(ret, String.Format("GetAdapterInfo Error {0}, Adapter/IP: {1}", ret, adapter));
               }

               IntPtr pEntry = pArray;

               while (pEntry != IntPtr.Zero)
               {
                  info = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

                  if (info.IpAddressList.IpAddress.Address == adapter)
                  {
                     return info.IpAddressList.Context;
                  }

                  IntPtr pAddr = info.IpAddressList.Next;

                  while (pAddr != IntPtr.Zero)
                  {
                     IP_ADDR_STRING addr = (IP_ADDR_STRING)Marshal.PtrToStructure(pAddr, typeof(IP_ADDR_STRING));
                     if (addr.IpAddress.Address == adapter)
                     {
                        return addr.Context;
                     }

                     pAddr = addr.Next;
                  }

                  pEntry = info.Next;
               }

               throw new InvalidOperationException(String.Format("Adapter for ip {0}, not found...", adapter));
            }
            finally
            {
               Marshal.FreeHGlobal(pArray);
            }
         }

         public static void GetIfEntry(int index, out MIB_IFROW info)
         {
            info = new MIB_IFROW();
            info.dwIndex = index;

            long structSize = Marshal.SizeOf(typeof(MIB_IFROW));
            IntPtr pIfRow = Marshal.AllocHGlobal(new IntPtr(structSize));
            Marshal.StructureToPtr(info, pIfRow, false);

            try
            {
               int ret = GetIfEntry(pIfRow);

               if (ret != 0)
               {
                   throw new System.ComponentModel.Win32Exception(ret, String.Format("GetInfEntry Error {0}, Entry: {1}", ret, index));
               }

               info = (MIB_IFROW)Marshal.PtrToStructure(pIfRow, typeof(MIB_IFROW));
            }
            finally
            {
               Marshal.FreeHGlobal(pIfRow);
            }
         }

         public static int AddIPAddress(string ip, string mask, int index)
         {
            int NTEContext, NTEInstance;

            int ret = AddIPAddress(BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0),
               BitConverter.ToInt32(IPAddress.Parse(mask).GetAddressBytes(), 0), index, out NTEContext, out NTEInstance);
            if (ret != 0)
            {
                throw new System.ComponentModel.Win32Exception(ret, String.Format("AddIPAdress Error {0}, Adapter/IP: {1}", ret, ip));
            }

            return NTEContext;
         }

         public static void DeleteIPAddress(string ip)
         {
            IP_ADAPTER_INFO adapter;
            int context = GetAdapterInfo(ip, out adapter);

            int ret = DeleteIPAddress(context);
            if (ret != 0)
            {
                throw new System.ComponentModel.Win32Exception(ret, String.Format("DeleteIpAddress Error {0}, IP: {1}", ret, ip));
            }
         }

         public static void DeleteIPAddressOnContext(int context)
         {
             int ret = DeleteIPAddress(context);
             if (ret != 0)
             {
                 throw new System.ComponentModel.Win32Exception(ret, String.Format("DeleteIPAddressOnContext Error {0}", ret));
             }
         }
      }
   }
}
