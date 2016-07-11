using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace TickTrader.BotTerminal
{
    internal class ComputerInfo
    {
        public static Processor Processor { get { return new Processor(); } }
        public static OperatingSystem OperatingSystem { get { return new OperatingSystem(); } }
    }

    internal class OperatingSystem
    {
        public OperatingSystem()
        {
            var os = new ManagementObjectSearcher("SELECT Caption, Version, MaxNumberOfProcesses, MaxProcessMemorySize, SerialNumber, BuildNumber, TotalVisibleMemorySize, FreePhysicalMemory, CurrentTimeZone FROM Win32_OperatingSystem")
            .Get()
            .Cast<ManagementObject>()
            .FirstOrDefault();

            var cs = new ManagementObjectSearcher("SELECT SystemType FROm Win32_ComputerSystem")
            .Get()
            .Cast<ManagementObject>()
            .FirstOrDefault();

            Name = ((string)os["Caption"]).Trim();
            Version = (string)os["Version"];
            MaxProcessCount = (uint)os["MaxNumberOfProcesses"];
            MaxProcessRAM = (ulong)os["MaxProcessMemorySize"];
            Architecture = (string)cs["SystemType"];
            SerialNumber = (string)os["SerialNumber"];
            Build = ((string)os["BuildNumber"]);
            TotalVisibleMemorySize = (ulong)os["TotalVisibleMemorySize"];
            FreePhysicalMemory = (ulong)os["FreePhysicalMemory"];
            CurrentTimeZone = (short)os["CurrentTimeZone"];
        }

        public string Architecture { get; private set; }
        public string Build { get; private set; }
        public uint MaxProcessCount { get; private set; }
        public ulong MaxProcessRAM { get; private set; }
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string SerialNumber { get; private set; }
        public ulong TotalVisibleMemorySize { get; private set; }
        public ulong FreePhysicalMemory { get; private set; }
        public short CurrentTimeZone { get; private set; }
        
    }

    public class Processor
    {
        public Processor()
        {
            var cpu = new ManagementObjectSearcher("SELECT ProcessorId, SocketDesignation, Name, Caption, AddressWidth, DataWidth, Architecture, MaxClockSpeed, ExtClock, L2CacheSize, L3CacheSize, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor")
            .Get()
            .Cast<ManagementObject>()
            .FirstOrDefault();

            ID = (string)cpu["ProcessorId"];
            Socket = (string)cpu["SocketDesignation"];
            Name = (string)cpu["Name"];
            Description = (string)cpu["Caption"];
            AddressWidth = (ushort)cpu["AddressWidth"];
            DataWidth = (ushort)cpu["DataWidth"];
            Architecture = (ushort)cpu["Architecture"];
            SpeedMHz = (uint)cpu["MaxClockSpeed"];
            BusSpeedMHz = (uint)cpu["ExtClock"];
            L2Cache = (uint)cpu["L2CacheSize"] * (ulong)1024;
            L3Cache = (uint)cpu["L3CacheSize"] * (ulong)1024;
            Cores = (uint)cpu["NumberOfCores"];
            Threads = (uint)cpu["NumberOfLogicalProcessors"];
        }

        public ushort AddressWidth { get; private set; }
        public ushort Architecture { get; private set; }
        public uint BusSpeedMHz { get; private set; }
        public uint Cores { get; private set; }
        public ushort DataWidth { get; private set; }
        public string Description { get; private set; }
        public string ID { get; private set; }
        public ulong L2Cache { get; private set; }
        public ulong L3Cache { get; private set; }
        public string Name { get; set; }
        public string Socket { get; private set; }
        public uint SpeedMHz { get; private set; }
        public uint Threads { get; private set; }
    }
}
