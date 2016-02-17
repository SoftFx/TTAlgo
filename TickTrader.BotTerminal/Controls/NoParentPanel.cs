using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Collections;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace TickTrader.BotTerminal
{
    public class NoParentPanel : Panel
    {
        public NoParentPanel()
        {
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            (Children as NoParentCollection).Initialize();
        }

        protected override sealed UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            NoParentCollection children = new NoParentCollection(this);
            children.CollectionChanged += new NotifyCollectionChangedEventHandler(OnChildrenCollectionChanged);
            return children;
        }

        protected virtual void OnChildAdded(UIElement child, int index)
        {
            if (ChildAdded != null)
                ChildAdded(child, index);
        }

        protected virtual void OnChildRemoved(UIElement child, int index)
        {
            if (ChildRemoved != null)
                ChildRemoved(child, index);
        }

        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnChildAdded(e.NewItems[0] as UIElement, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    OnChildRemoved(e.OldItems[0] as UIElement, e.OldStartingIndex);
                    break;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return _visualChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visualChildren.Count)
                throw new ArgumentOutOfRangeException();
            return _visualChildren[index];
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is Visual)
            {
                _visualChildren.Add(visualAdded as Visual);
            }

            if (visualRemoved is Visual)
            {
                _visualChildren.Remove(visualRemoved as Visual);
            }

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private readonly List<Visual> _visualChildren = new List<Visual>();

        public event Action<UIElement, int> ChildAdded;
        public event Action<UIElement, int> ChildRemoved;

        public class NoParentCollection : UIElementCollection, INotifyCollectionChanged
        {
            private Dictionary<UIElement, DegenerateSibling> _degenerateSiblings = new Dictionary<UIElement, DegenerateSibling>();
            private Collection<UIElement> _elements = new Collection<UIElement>();
            private Panel _ownerPanel;
            private SurrogateVisualParent _surrogateVisualParent;

            public NoParentCollection(UIElement owner)
                : this(owner, new SurrogateVisualParent())
            {
            }

            private NoParentCollection(UIElement owner, SurrogateVisualParent surrogateVisualParent)
                : base(surrogateVisualParent, null)
            {
                _ownerPanel = owner as Panel;
                _surrogateVisualParent = surrogateVisualParent;
                _surrogateVisualParent.InitializeOwner(this);
                _elements = new Collection<UIElement>();
            }

            #region UIElementCollection

            public override int Add(UIElement element)
            {
                VerifyWriteAccess();
                return base.Add(element);
            }

            public override int Capacity
            {
                get { return base.Capacity; }
                set
                {
                    VerifyWriteAccess();
                    base.Capacity = value;
                }
            }

            public override void Clear()
            {
                VerifyWriteAccess();
                base.Clear();
            }

            public override bool Contains(UIElement element)
            {
                return _elements.Contains(element);
            }

            public override void CopyTo(Array array, int index)
            {
                ((ICollection)_elements).CopyTo(array, index);
            }

            public override void CopyTo(UIElement[] array, int index)
            {
                _elements.CopyTo(array, index);
            }

            public override int Count
            {
                get { return _elements.Count; }
            }

            public override IEnumerator GetEnumerator()
            {
                return (_elements as ICollection).GetEnumerator();
            }

            public override int IndexOf(UIElement element)
            {
                return _elements.IndexOf(element);
            }

            public override void Insert(int index, UIElement element)
            {
                VerifyWriteAccess();
                base.Insert(index, element);
            }

            public override void Remove(UIElement element)
            {
                VerifyWriteAccess();
                base.Remove(_degenerateSiblings[element]);
            }

            public override void RemoveAt(int index)
            {
                VerifyWriteAccess();
                base.RemoveAt(index);
            }

            public override void RemoveRange(int index, int count)
            {
                VerifyWriteAccess();
                base.RemoveRange(index, count);
            }

            public override UIElement this[int index]
            {
                get { return _elements[index] as UIElement; }
                set
                {
                    VerifyWriteAccess();
                    base[index] = value;
                }
            }

            #endregion

            public void Initialize()
            {
            }

            private int BaseIndexOf(UIElement element)
            {
                return base.IndexOf(element);
            }

            private void BaseInsert(int index, UIElement element)
            {
                base.Insert(index, element);
            }

            private void BaseRemoveAt(int index)
            {
                base.RemoveAt(index);
            }

            private UIElement EnsureUIElement(object value)
            {
                if (value == null)
                    throw new ArgumentException("Cannot add a null value to a DisconnectedUIElementCollection");

                if (!(value is UIElement))
                    throw new ArgumentException("Only objects of type UIElement can be added to a DisconnectedUIElementCollection");

                return value as UIElement;
            }

            private void VerifyWriteAccess()
            {
                if (_ownerPanel == null) return;

                if (_ownerPanel.IsItemsHost && ItemsControl.GetItemsOwner(_ownerPanel) != null)
                    throw new InvalidOperationException("Disconnected children cannot be explicitly added to this "
                        + "collection while the panel is serving as an items host. However, visual children can "
                        + "be added by simply calling the AddVisualChild method.");
            }


            public event NotifyCollectionChangedEventHandler CollectionChanged;
            private void RaiseCollectionChanged(NotifyCollectionChangedAction action, object changedItem, int index)
            {
                if (CollectionChanged != null)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, changedItem, index));
            }

            private class DegenerateSibling : UIElement
            {
                public DegenerateSibling(UIElement element)
                {
                    _element = element;
                }

                public UIElement Element
                {
                    get { return _element; }
                }

                private UIElement _element;
            }

            private class SurrogateVisualParent : UIElement
            {
                internal void InitializeOwner(NoParentCollection owner)
                {
                    _owner = owner;
                }

                protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
                {
                    if (_internalUpdate) return;

                    _internalUpdate = true;
                    try
                    {
                        if (visualAdded != null)
                        {
                            Debug.Assert(!(visualAdded is DegenerateSibling),
                                "Unexpected addition of degenerate... All degenerates should be added during internal updates.");

                            UIElement element = visualAdded as UIElement;
                            DegenerateSibling sibling = new DegenerateSibling(element);
                            int index = _owner.BaseIndexOf(element);
                            _owner.BaseRemoveAt(index);
                            _owner.BaseInsert(index, sibling);
                            _owner._degenerateSiblings[element] = sibling;
                            _owner._elements.Insert(index, element);
                            _owner.RaiseCollectionChanged(NotifyCollectionChangedAction.Add, element, index);
                        }

                        if (visualRemoved != null)
                        {
                            Debug.Assert(visualRemoved is DegenerateSibling,
                                "Unexpected removal of UIElement... All non degenerates should be removed during internal updates.");

                            DegenerateSibling sibling = visualRemoved as DegenerateSibling;
                            int index = _owner._elements.IndexOf(sibling.Element);
                            _owner._elements.RemoveAt(index);
                            _owner.RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, sibling.Element, index);
                            _owner._degenerateSiblings.Remove(sibling.Element);
                        }
                    }
                    finally
                    {
                        _internalUpdate = false;
                    }
                }

                private NoParentCollection _owner;
                private bool _internalUpdate = false;
            }
        }
    }
}
