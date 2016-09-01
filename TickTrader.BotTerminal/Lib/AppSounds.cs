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
        public static AppSound Blop { get { return new AppSound(Resx.Sounds.Sounds.Blop); } }
        public static AppSound Clinking { get { return new AppSound(Resx.Sounds.Sounds.Clinking); } }
        public static AppSound Negative { get { return new AppSound(Resx.Sounds.Sounds.Negative); } }
        public static AppSound Positive { get { return new AppSound(Resx.Sounds.Sounds.Positive); } }
        public static AppSound Save { get { return new AppSound(Resx.Sounds.Sounds.Save); } }
        public static AppSound SharpEcho { get { return new AppSound(Resx.Sounds.Sounds.SharpEcho); } }
        public static AppSound Woosh { get { return new AppSound(Resx.Sounds.Sounds.Woosh); } }
        
    }

    internal interface IPlayable
    {
        void Play();
    }

    internal class AppSound : IPlayable
    {
        private UnmanagedMemoryStream _stream;

        public AppSound(UnmanagedMemoryStream stream)
        {
            _stream = stream;
        }

        void IPlayable.Play()
        {
            using (_stream)
            {
                using (var sp = new System.Media.SoundPlayer(_stream))
                {
                    sp.Play();
                }
            }
        }
    }
}
