using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using IOFile = System.IO.File;

namespace TickTrader.Algo.Indicators.Other.DisplayDisconnect
{
    [Indicator(Category = "Other", DisplayName = "Display Disconnect", Version = "1.0")]
    public class DisplayDisconnect : Indicator
    {
        private bool _connection = true;
        private double _count = 0;

        [Output(DisplayName = "State Server", Target = OutputTargets.Window1)]
        public MarkerSeries StateServer { get; set; }

        [Parameter(DisplayName = "Data File", DefaultValue = ConfigFileName)]
        [FileFilter("Toml Config (*.tml)", "*.tml")]
        public Api.File DataFile { get; set; }

        public const string ConfigFileName = "StateServerDataFile.tml";

        public int LastPositionChanged { get { return 0; } }

        public DisplayDisconnect() { }

        protected void InitializeIndicator()
        {
            //Connected += ConnectServer;
            //WriteMessageInFile($"Init indicator {DateTime.Now.Second} {DateTime.Now.Millisecond}\n");
            WriteConnectionTime();
        }

        async private void WriteConnectionTime()
        {
            await Task.Run(() =>
            {
                if (!IOFile.Exists(DataFile.FullPath))
                    IOFile.Create(DataFile.FullPath);
                while (true)
                {
                    IOFile.AppendAllText(DataFile.FullPath, $"{DateTime.Now.ToLongTimeString()} {DateTime.Now.Millisecond}\n");
                    Thread.Sleep(1000);
                }           
            });
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            SetMarker(pos, ++_count % 101);
        }

        private void SetMarker(int pos, double value)
        {
            var marker = StateServer[pos];
            marker.Y = value;
            marker.Color = (Colors)CalculatedColors(value);
            marker.DisplayText = value.ToString();
        }

        private int CalculatedColors(double value)
        {
            int v = (int)Math.Round(value * 2.55);

            int r = 255 - v;
            int g = v;
            int b = 0;

            return (r << 16) | (g << 8) | (b << 0);
        }
    }
}
