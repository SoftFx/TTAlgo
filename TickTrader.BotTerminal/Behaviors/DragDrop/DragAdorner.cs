using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace TickTrader.BotTerminal
{
    public class DragAdorner : Adorner, IDisposable
    {
        private AdornerLayer _adornerLayer;
        private UIElement _adornElement;

        public DragAdorner(UIElement ownerLayer, UIElement adornElement, double opacity = 1, bool dropShadow = true) : base(ownerLayer)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement);
            _adornerLayer.Add(this);
            _adornElement = adornElement;
            IsHitTestVisible = false;
            Opacity = opacity;
            if(dropShadow)
                Effect = new DropShadowEffect();
        }

        public void Update()
        {
            _adornerLayer.Update(AdornedElement);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pointFromScreen = AdornedElement.PointFromScreen(WinApiHelper.GetMousePosition());
            var correctedPoint = new Point(pointFromScreen.X - _adornElement.DesiredSize.Width / 2, pointFromScreen.Y - _adornElement.DesiredSize.Height / 2);
            drawingContext.DrawRectangle(new VisualBrush(_adornElement), null, new Rect(correctedPoint, _adornElement.DesiredSize));
        }

        public void Dispose()
        {
            _adornerLayer.Remove(this);
            _adornElement = null;
        }
    }
}
