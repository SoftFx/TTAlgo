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
    internal class DragAdorner : Adorner, IDragAdorner
    {
        private VisualBrush _visualBrush;
        private AdornerLayer _adornerLayer;
        private UIElement _adornElement;
        private Point _currentPosition;
        private DropState _currentAdornerDropState;

        public DropState DropState
        {
            get { return _currentAdornerDropState; }
            set
            {
                _currentAdornerDropState = value;
                if (_currentAdornerDropState == DropState.CanDrop)
                    Opacity = 1;
                else
                    Opacity = 0.5;
                _adornerLayer.Update(AdornedElement);
            }
        }

        public Point Position
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                _adornerLayer.Update(AdornedElement);
            }
        }

        public DragAdorner(UIElement ownerLayer, UIElement adornElement) : base(ownerLayer)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement);
            _adornerLayer.Add(this);

            _adornElement = adornElement;
            _visualBrush = new VisualBrush(adornElement as Visual);

            IsHitTestVisible = false;
            Effect = new DropShadowEffect();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(_visualBrush, null, new Rect(_currentPosition, _adornElement.RenderSize));
        }

        public void Dispose()
        {
            _adornerLayer.Remove(this);
            _adornElement = null;
        }
    }
}
