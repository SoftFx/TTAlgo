using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace TickTrader.BotAgent.Configurator.IPHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public uint owningPid;
    }

    // https://msdn2.microsoft.com/en-us/library/aa366921.aspx
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
        public MIB_TCPROW_OWNER_PID[] table;
    }

    // https://msdn.microsoft.com/en-us/library/aa366896
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCP6ROW_OWNER_PID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] localAddr;
        public uint localScopeId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] remoteAddr;
        public uint remoteScopeId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public uint state;
        public uint owningPid;
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366905
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCP6TABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
        public MIB_TCP6ROW_OWNER_PID[] table;
    }

    // https://msdn2.microsoft.com/en-us/library/aa366386.aspx
    public enum TCP_TABLE_CLASS
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    // https://msdn.microsoft.com/en-us/library/aa366896.aspx
    public enum MIB_TCP_STATE
    {
        MIB_TCP_STATE_CLOSED,
        MIB_TCP_STATE_LISTEN,
        MIB_TCP_STATE_SYN_SENT,
        MIB_TCP_STATE_SYN_RCVD,
        MIB_TCP_STATE_ESTAB,
        MIB_TCP_STATE_FIN_WAIT1,
        MIB_TCP_STATE_FIN_WAIT2,
        MIB_TCP_STATE_CLOSE_WAIT,
        MIB_TCP_STATE_CLOSING,
        MIB_TCP_STATE_LAST_ACK,
        MIB_TCP_STATE_TIME_WAIT,
        MIB_TCP_STATE_DELETE_TCB
    }

    public static class IPHelperAPI
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        internal static extern uint GetExtendedTcpTable(
            IntPtr tcpTable,
            ref int tcpTableLength,
            bool sort,
            int ipVersion,
            TCP_TABLE_CLASS tcpTableType,
            int reserved = 0);
    }

    public class TcpRowCustom
    {
        public IPEndPoint LocalEndPoint { get; }

        public IPEndPoint RemoteEndPoint { get; }

        public TcpState State { get; }

        public uint ProcessId { get; }

        public IPAddress Adress { get; }

        public TcpRowCustom(MIB_TCP6ROW_OWNER_PID tcpRow)
        {
            State = (TcpState)tcpRow.state;
            ProcessId = tcpRow.owningPid;

            int localPort = (tcpRow.localPort[0] << 8) + (tcpRow.localPort[1]) + (tcpRow.localPort[2] << 24) + (tcpRow.localPort[3] << 16);
            LocalEndPoint = new IPEndPoint(new IPAddress(tcpRow.localAddr), localPort);

            int remotePort = (tcpRow.remotePort[0] << 8) + (tcpRow.remotePort[1]) + (tcpRow.remotePort[2] << 24) + (tcpRow.remotePort[3] << 16);
            RemoteEndPoint = new IPEndPoint(new IPAddress(tcpRow.remoteAddr), remotePort);
        }

        public TcpRowCustom(MIB_TCPROW_OWNER_PID tcpRow)
        {
            State = (TcpState)tcpRow.state;
            ProcessId = tcpRow.owningPid;

            int localPort = (tcpRow.localPort[0] << 8) + (tcpRow.localPort[1]) + (tcpRow.localPort[2] << 24) + (tcpRow.localPort[3] << 16);
            LocalEndPoint = new IPEndPoint(new IPAddress(tcpRow.localAddr), localPort);

            int remotePort = (tcpRow.remotePort[0] << 8) + (tcpRow.remotePort[1]) + (tcpRow.remotePort[2] << 24) + (tcpRow.remotePort[3] << 16);
            RemoteEndPoint = new IPEndPoint(new IPAddress(tcpRow.remoteAddr), remotePort);
        }
    }

    public class IPHelperWrapper : IDisposable
    {
        public const int AF_INET = 2;    // IP_v4 = System.Net.Sockets.AddressFamily.InterNetwork
        public const int AF_INET6 = 23;  // IP_v6 = System.Net.Sockets.AddressFamily.InterNetworkV6

        public IPHelperWrapper() { }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public List<TcpRowCustom> GetAllTCPv4Connections()
        {
            return GetTCPConnections<MIB_TCPROW_OWNER_PID, MIB_TCPTABLE_OWNER_PID>(AF_INET).Select(u => new TcpRowCustom(u)).ToList();
        }

        public List<TcpRowCustom> GetAllTCPv6Connections()
        {
            return GetTCPConnections<MIB_TCP6ROW_OWNER_PID, MIB_TCP6TABLE_OWNER_PID>(AF_INET6).Select(u => new TcpRowCustom(u)).ToList();
        }

        public List<IPR> GetTCPConnections<IPR, IPT>(int ipVersion)
        { //IPR = Row Type, IPT = Table Type

            IPR[] tableRows;
            int buffSize = 0;
            var dwNumEntriesField = typeof(IPT).GetField("dwNumEntries");

            // how much memory do we need?
            uint ret = IPHelperAPI.GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, ipVersion, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
            IntPtr tcpTablePtr = Marshal.AllocHGlobal(buffSize);

            try
            {
                ret = IPHelperAPI.GetExtendedTcpTable(tcpTablePtr, ref buffSize, true, ipVersion, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
                if (ret != 0) return new List<IPR>();

                // get the number of entries in the table
                IPT table = (IPT)Marshal.PtrToStructure(tcpTablePtr, typeof(IPT));
                int rowStructSize = Marshal.SizeOf(typeof(IPR));
                uint numEntries = (uint)dwNumEntriesField.GetValue(table);

                // buffer we will be returning
                tableRows = new IPR[numEntries];

                IntPtr rowPtr = (IntPtr)((long)tcpTablePtr + 4);
                for (int i = 0; i < numEntries; i++)
                {
                    IPR tcpRow = (IPR)Marshal.PtrToStructure(rowPtr, typeof(IPR));
                    tableRows[i] = tcpRow;
                    rowPtr = (IntPtr)((long)rowPtr + rowStructSize);   // next entry
                }
            }
            finally
            {
                // Free the Memory
                Marshal.FreeHGlobal(tcpTablePtr);
            }
            return tableRows != null ? tableRows.ToList() : new List<IPR>();
        }

        ~IPHelperWrapper()
        {
            Dispose();
        }
    }
}
