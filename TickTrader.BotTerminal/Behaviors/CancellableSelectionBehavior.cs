﻿using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TickTrader.BotTerminal
{
    internal class CancellableSelectionBehavior : Behavior<Selector>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(CancellableSelectionBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty IgnoreNullSelectionProperty =
            DependencyProperty.Register("IgnoreNullSelection", typeof(bool), typeof(CancellableSelectionBehavior), new PropertyMetadata(true));

        /// <summary>
        /// Determines whether null selection (which usually occurs since the combobox is rebuilt or its list is refreshed) should be ignored.
        /// True by default.
        /// </summary>
        public bool IgnoreNullSelection
        {
            get { return (bool)GetValue(IgnoreNullSelectionProperty); }
            set { SetValue(IgnoreNullSelectionProperty, value); }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (CancellableSelectionBehavior)d;

            // OnSelectedItemChanged can be raised before AssociatedObject is assigned
            if (behavior.AssociatedObject == null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    var selector = behavior.AssociatedObject;
                    selector.SelectedValue = e.NewValue;
                }));
            }
            else
            {
                var selector = behavior.AssociatedObject;
                selector.SelectedValue = e.NewValue;
            }
        }

        /// <summary>
        /// Called when the associated selector's selection is changed.
        /// Tries to assign it to the <see cref="SelectedItem"/> property.
        /// If it fails, updates the selector's with  <see cref="SelectedItem"/> property's current value.
        /// </summary>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreNullSelection && (e.AddedItems == null || e.AddedItems.Count == 0))
                return;

            SelectedItem = AssociatedObject.SelectedItem;
            if (SelectedItem != AssociatedObject.SelectedItem)
                AssociatedObject.SelectedItem = SelectedItem;
        }
    }
}
