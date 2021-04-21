using System.Collections.Generic;
using System.Windows;
using System.Windows.Interactivity;

namespace TickTrader.BotTerminal
{
    internal class Behaviors : List<Behavior>
    {
    }

    internal class Triggers : List<System.Windows.Interactivity.TriggerBase>
    {
    }

    internal static class StyleInteraction
    {
        public static readonly DependencyProperty BehaviorsProperty =
            DependencyProperty.RegisterAttached("Behaviors", typeof(Behaviors), typeof(StyleInteraction), new UIPropertyMetadata(null, OnPropertyBehaviorsChanged));

        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.RegisterAttached("Triggers", typeof(Triggers), typeof(StyleInteraction), new UIPropertyMetadata(null, OnPropertyTriggersChanged));


        public static Behaviors GetBehaviors(DependencyObject obj)
        {
            return (Behaviors)obj.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(DependencyObject obj, Behaviors value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        public static Triggers GetTriggers(DependencyObject obj)
        {
            return (Triggers)obj.GetValue(TriggersProperty);
        }

        public static void SetTriggers(DependencyObject obj, Triggers value)
        {
            obj.SetValue(TriggersProperty, value);
        }


        private static void OnPropertyBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(d);
            foreach (var behavior in e.NewValue as Behaviors)
               behaviors.Add(behavior);
        }

        private static void OnPropertyTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var triggers = Interaction.GetTriggers(d);
            foreach (var trigger in e.NewValue as Triggers)
                triggers.Add(trigger);
        }
    }
}
