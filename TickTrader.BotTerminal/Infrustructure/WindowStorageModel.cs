using System;
using System.Runtime.Serialization;
using System.Windows;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "WindowSettings")]
    internal class WindowStorageModel : StorageModelBase<WindowStorageModel>
    {
        [DataMember]
        public double Top { get; set; }

        [DataMember]
        public double Left { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public WindowState State { get; set; }


        public WindowStorageModel()
        {
            Top = double.NaN;
            Left = double.NaN;
            Width = double.NaN;
            Height = double.NaN;
            State = WindowState.Normal;
        }


        public override WindowStorageModel Clone()
        {
            return new WindowStorageModel
            {
                Top = Top,
                Left = Left,
                Width = Width,
                Height = Height,
                State = State,
            };
        }
    }
}
