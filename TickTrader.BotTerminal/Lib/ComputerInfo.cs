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

            var cs = new ManagementObjectSearcher("SELECT SystemType FROM Win32_ComputerSystem")
            .Get()
            .Cast<ManagementObject>()
            .FirstOrDefault();

            if (os.TryGet("Caption", out var name))
                Name = ((string)name).Trim();

            if (os.TryGet("Version", out var version))
                Version = (string)version;

            if (os.TryGet("MaxNumberOfProcesses", out var maxProcessCount))
                MaxProcessCount = (uint)maxProcessCount;

            if (os.TryGet("MaxProcessMemorySize", out var maxProcessRAM))
                MaxProcessRAM = (ulong)maxProcessRAM;

            if (cs.TryGet("SystemType", out var architecture))
                Architecture = (string)architecture;

            if (os.TryGet("SerialNumber", out var serialNumber))
                SerialNumber = (string)serialNumber;

            if (os.TryGet("BuildNumber", out var build))
                Build = (string)build;

            if (os.TryGet("TotalVisibleMemorySize", out var totalVisibleMemorySize))
                TotalVisibleMemorySize = (ulong)totalVisibleMemorySize;

            if (os.TryGet("FreePhysicalMemory", out var freePhysicalMemory))
                FreePhysicalMemory = (ulong)freePhysicalMemory;

            if (os.TryGet("CurrentTimeZone", out var currentTimeZone))
                CurrentTimeZone = (short)currentTimeZone;
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

            if (cpu.TryGet("ProcessorId", out var id))
                ID = (string)id;

            if (cpu.TryGet("SocketDesignation", out var socket))
                Socket = (string)socket;

            if (cpu.TryGet("Name", out var name))
                Name = (string)name;

            if (cpu.TryGet("Caption", out var description))
                Description = (string)description;

            if (cpu.TryGet("AddressWidth", out var width))
                AddressWidth = (ushort)width;

            if (cpu.TryGet("DataWidth", out var dataWidth))
                DataWidth = (ushort)dataWidth;

            if (cpu.TryGet("Architecture", out var architecture))
                Architecture = (ushort)architecture;

            if (cpu.TryGet("MaxClockSpeed", out var maxClockSpeed))
                SpeedMHz = (uint)maxClockSpeed;

            if (cpu.TryGet("ExtClock", out var extClock))
                BusSpeedMHz = (uint)extClock;

            if (cpu.TryGet("L2CacheSize", out var l2CacheSize))
                L2Cache = (uint)l2CacheSize * (ulong)1024;

            if (cpu.TryGet("L3CacheSize", out var l3CacheSize))
                L3Cache = (uint)l3CacheSize * (ulong)1024;

            if (cpu.TryGet("NumberOfCores", out var numberOfCores))
                Cores = (uint)numberOfCores;

            if (cpu.TryGet("NumberOfLogicalProcessors", out var numberOfLogicalProcessors))
                Threads = (uint)numberOfLogicalProcessors;
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

    internal static class ManagementObjectExtension
    {
        internal static bool TryGet(this ManagementObject obj, string key, out object val)
        {
            val = obj[key];

            return val != null;
        }
    }
}
