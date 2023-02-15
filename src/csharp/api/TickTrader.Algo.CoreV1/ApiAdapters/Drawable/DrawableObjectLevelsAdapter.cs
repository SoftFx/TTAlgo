using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableObjectLevelsAdapter : DrawablePropsChangedBase, IDrawableObjectLevels
    {
        private readonly DrawableObjectLevelsInfo _info;


        public int Count => _info.Count;


        public DrawableObjectLevelsAdapter(DrawableObjectLevelsInfo info, IDrawableChangedWatcher watcher) : base(watcher)
        {
            IsSupported = info != null;
            _info = info ?? new DrawableObjectLevelsInfo();
        }


        public double GetValue(int index) => _info.Value[index];

        public void SetValue(int index, double value)
        {
            _info.Value[index] = value;
            OnChanged();
        }

        public int GetWidth(int index) => _info.Width[index];

        public void SetWidth(int index, int width)
        {
            _info.Width[index] = width;
            OnChanged();
        }

        public Colors GetColor(int index) => _info.ColorArgb[index].FromArgb();

        public void SetColor(int index, Colors color)
        {
            _info.ColorArgb[index] = color.ToArgb();
            OnChanged();
        }

        public LineStyles GetLineStyle(int index) => _info.LineStyle[index].ToApiEnum();

        public void SetLineStyle(int index, LineStyles style)
        {
            _info.LineStyle[index] = style.ToDomainEnum();
            OnChanged();
        }

        public string GetDescription(int index) => _info.Description[index];

        public void SetDescription(int index, string description)
        {
            _info.Description[index] = description;
            OnChanged();
        }
    }
}
