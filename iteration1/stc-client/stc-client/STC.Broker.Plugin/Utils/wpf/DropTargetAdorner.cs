using System;
using System.Windows;
using System.Windows.Documents;

namespace  STC.Broker.Plugin.Utils.wpf
{
    public abstract class DropTargetAdorner : Adorner
    {
        private AdornerLayer m_AdornerLayer;

        public DropInfo DropInfo
        {
            get;
            set;
        }

        public DropTargetAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            m_AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            m_AdornerLayer.Add(this);
            base.IsHitTestVisible = false;
        }

        public void Detatch()
        {
            m_AdornerLayer.Remove(this);
        }

        internal static DropTargetAdorner Create(Type type, UIElement adornedElement)
        {
            if (!typeof(DropTargetAdorner).IsAssignableFrom(type))
            {
                throw new InvalidOperationException("The requested adorner class does not derive from DropTargetAdorner.");
            }

            return (DropTargetAdorner)type.GetConstructor(new Type[1]
            {
                typeof(UIElement)
            }).Invoke(new UIElement[1]
            {
                adornedElement
            });
        }
    }
}
