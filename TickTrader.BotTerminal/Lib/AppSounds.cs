using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class AppSounds
    {
        public static IAppSound Blop { get { return new AppSound(Resx.Sounds.Sounds.Blop); } }
        public static IAppSound Clinking { get { return new AppSound(Resx.Sounds.Sounds.Clinking); } }
        public static IAppSound Negative { get { return new AppSound(Resx.Sounds.Sounds.Negative); } }
        public static IAppSound Positive { get { return new AppSound(Resx.Sounds.Sounds.Positive); } }
        public static IAppSound Save { get { return new AppSound(Resx.Sounds.Sounds.Save); } }
        public static IAppSound SharpEcho { get { return new AppSound(Resx.Sounds.Sounds.SharpEcho); } }
        public static IAppSound Woosh { get { return new AppSound(Resx.Sounds.Sounds.Woosh); } }
        
    }

    internal interface IAppSound
    {
        void Play();
    }

    internal class AppSound : IAppSound
    {
        private UnmanagedMemoryStream _stream;

        public AppSound(UnmanagedMemoryStream stream)
        {
            _stream = stream;
        }
        public void Play()
        {
            using (_stream)
            {
                using (var sp = new SoundPlayer(_stream))
                {
                    sp.Play();
                }
            }
        }
    }
}
