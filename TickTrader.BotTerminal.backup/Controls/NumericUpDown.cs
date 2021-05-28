using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TickTrader.BotTerminal
{
    public class NumericUpDown : Control
    {
        #region Dependency Properties
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(decimal), typeof(NumericUpDown), new UIPropertyMetadata(default(decimal), OnValueChanged, OnCoerceValueMinMax));

        

        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(decimal), typeof(NumericUpDown), new UIPropertyMetadata((decimal)100));

        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(decimal), typeof(NumericUpDown), new UIPropertyMetadata((decimal)-100));

        public decimal Increment
        {
            get { return (decimal)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }
        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register(nameof(Increment), typeof(decimal), typeof(NumericUpDown), new PropertyMetadata((decimal)0.1));

        public bool CanIncrease
        {
            get { return (bool)GetValue(CanIncreaseProperty); }
            set { SetValue(CanIncreaseProperty, value); }
        }
        public static readonly DependencyProperty CanIncreaseProperty =
            DependencyProperty.Register(nameof(CanIncrease), typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true));

        public bool CanDecrease
        {
            get { return (bool)GetValue(CanDecreaseProperty); }
            set { SetValue(CanDecreaseProperty, value); }
        }
        public static readonly DependencyProperty CanDecreaseProperty =
            DependencyProperty.Register(nameof(CanDecrease), typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true));
        #endregion

        #region Commands
        public static ICommand IncreaseCommand;
        public static ICommand DecreaseCommand;
        #endregion

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));

            IncreaseCommand = new RoutedUICommand(nameof(IncreaseCommand), nameof(IncreaseCommand), typeof(NumericUpDown));
            DecreaseCommand = new RoutedUICommand(nameof(DecreaseCommand), nameof(DecreaseCommand), typeof(NumericUpDown));

            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(IncreaseCommand, OnIncrease));
            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(DecreaseCommand, OnDecrease));
        }

        #region private methods
        private bool IsGreaterThan(decimal? value1, decimal? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return value1.Value > value2.Value;
        }

        private bool IsLowerThan(decimal? value1, decimal? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return value1.Value < value2.Value;
        }
        #endregion

        #region Event Handlers
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericCtrl = d as NumericUpDown;
            numericCtrl.CanIncrease = true;
            numericCtrl.CanDecrease = true;

            if ((decimal)e.NewValue == numericCtrl.MaxValue)
                numericCtrl.CanIncrease = false;
            else if ((decimal)e.NewValue == numericCtrl.MinValue)
                numericCtrl.CanDecrease = false;
        }

        private static void OnIncrease(object sender, ExecutedRoutedEventArgs e)
        {
            var numericCtrl = sender as NumericUpDown;
            numericCtrl.Value += numericCtrl.Increment;
        }

        private static void OnDecrease(object sender, ExecutedRoutedEventArgs e)
        {
            var numericCtrl = sender as NumericUpDown;
            numericCtrl.Value -= numericCtrl.Increment;
        }

        private static object OnCoerceValueMinMax(DependencyObject d, object baseValue)
        {
            var numericCtrl = d as NumericUpDown;
            var value = (decimal)baseValue;

            if (numericCtrl.IsGreaterThan(value, numericCtrl.MaxValue))
                return numericCtrl.MaxValue;
            else if (numericCtrl.IsLowerThan(value, numericCtrl.MinValue))
                return numericCtrl.MinValue;

            return value;
        }
        #endregion
    }


    ///TODO: Strongly Typed
    //public class DoubleNumericUpDown : NumericUpDown<decimal>
    //{
    //    static DoubleNumericUpDown()
    //    {
    //        UpdateMetadata(typeof(DoubleNumericUpDown), 1d, decimal.NegativeInfinity, decimal.PositiveInfinity);
    //    }

    //    public DoubleNumericUpDown() : base((v1, v2) => v1 < v2, (v1, v2) => v1 > v2)
    //    {

    //    }

    //    protected override decimal DecrementValue(decimal value, decimal increment)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override decimal IncrementValue(decimal value, decimal increment)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public abstract class NumericUpDown<T> : NumericUpDownBase where T : struct, IFormattable, IComparable<T>
    //{
    //    private Func<T, T, bool> _isLowerThan;
    //    private Func<T, T, bool> _isGreaterThan;

    //    #region Dependency Properties
    //    public T Value
    //    {
    //        get { return (T)GetValue(ValueProperty); }
    //        set { SetValue(ValueProperty, value); }
    //    }
    //    public static readonly DependencyProperty ValueProperty =
    //        DependencyProperty.Register(nameof(Value), typeof(decimal), typeof(NumericUpDown<T>), new UIPropertyMetadata(default(T), null, CoerceValueMinMax));

    //    public T MaxValue
    //    {
    //        get { return (T)GetValue(MaxValueProperty); }
    //        set { SetValue(MaxValueProperty, value); }
    //    }
    //    public static readonly DependencyProperty MaxValueProperty =
    //        DependencyProperty.Register(nameof(MaxValue), typeof(T), typeof(NumericUpDown<T>), null);

    //    public T MinValue
    //    {
    //        get { return (T)GetValue(MinValueProperty); }
    //        set { SetValue(MinValueProperty, value); }
    //    }
    //    public static readonly DependencyProperty MinValueProperty =
    //        DependencyProperty.Register(nameof(MinValue), typeof(T), typeof(NumericUpDown<T>), new UIPropertyMetadata(Double.NegativeInfinity));

    //    public T Increment
    //    {
    //        get { return (T)GetValue(IncrementProperty); }
    //        set { SetValue(IncrementProperty, value); }
    //    }
    //    public static readonly DependencyProperty IncrementProperty =
    //        DependencyProperty.Register(nameof(Increment), typeof(T), typeof(NumericUpDown<T>), new PropertyMetadata(1d));

    //    public bool CanIncrease
    //    {
    //        get { return (bool)GetValue(CanIncreaseProperty); }
    //        set { SetValue(CanIncreaseProperty, value); }
    //    }
    //    public static readonly DependencyProperty CanIncreaseProperty =
    //        DependencyProperty.Register(nameof(CanIncrease), typeof(bool), typeof(NumericUpDown<T>), new PropertyMetadata(true));

    //    public bool CanDecrease
    //    {
    //        get { return (bool)GetValue(CanDecreaseProperty); }
    //        set { SetValue(CanDecreaseProperty, value); }
    //    }
    //    public static readonly DependencyProperty CanDecreaseProperty =
    //        DependencyProperty.Register(nameof(CanDecrease), typeof(bool), typeof(NumericUpDown<T>), new PropertyMetadata(true));
    //    #endregion

    //    #region Commands
    //    public static ICommand IncreaseCommand;
    //    public static ICommand DecreaseCommand;
    //    #endregion

    //    protected NumericUpDown(Func<T, T, bool> isLowerThan, Func<T, T, bool> isGreaterThan)
    //    {
    //        _isLowerThan = isLowerThan;
    //        _isGreaterThan = isGreaterThan;
    //    }

    //    protected static void UpdateMetadata(Type type, T? increment, T? minValue, T? maxValue)
    //    {
    //        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));

    //        CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown<T>), new CommandBinding(IncreaseCommand, OnIncrease));
    //        CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown<T>), new CommandBinding(DecreaseCommand, OnDecrease));

    //        IncrementProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(increment));
    //        MaxValueProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(maxValue));
    //        MinValueProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(minValue));

    //        IncreaseCommand = new RoutedUICommand(nameof(IncreaseCommand), nameof(IncreaseCommand), type);
    //        DecreaseCommand = new RoutedUICommand(nameof(DecreaseCommand), nameof(DecreaseCommand), type);
    //    }

    //    private bool IsGreaterThan(T? value1, T? value2)
    //    {
    //        if (value1 == null || value2 == null)
    //            return false;

    //        return _isGreaterThan(value1.Value, value2.Value);
    //    }

    //    private bool IsLowerThan(T? value1, T? value2)
    //    {
    //        if (value1 == null || value2 == null)
    //            return false;

    //        return _isLowerThan(value1.Value, value2.Value);
    //    }

    //    #region Event Handlers
    //    private static void OnIncrease(object sender, ExecutedRoutedEventArgs e)
    //    {
    //        var numericCtrl = sender as NumericUpDown<T>;
    //        numericCtrl.Value = numericCtrl.IncrementValue(numericCtrl.Value, numericCtrl.Increment);
    //    }

    //    private static void OnDecrease(object sender, ExecutedRoutedEventArgs e)
    //    {
    //        var numericCtrl = sender as NumericUpDown<T>;
    //        numericCtrl.Value = numericCtrl.DecrementValue(numericCtrl.Value, numericCtrl.Increment);
    //    }

    //    private static object CoerceValueMinMax(DependencyObject d, object baseValue)
    //    {
    //        var numericCtrl = d as NumericUpDown<T>;
    //        var value = (T)baseValue;

    //        if (numericCtrl.IsGreaterThan(value, numericCtrl.MaxValue))
    //            return numericCtrl.MaxValue;
    //        else if (numericCtrl.IsLowerThan(value, numericCtrl.MinValue))
    //            return numericCtrl.MinValue;

    //        return value;
    //    }
    //    #endregion

    //    #region Abstract Methods
    //    protected abstract T IncrementValue(T value, T increment);
    //    protected abstract T DecrementValue(T value, T increment);
    //    #endregion
    //}

    //public class NumericUpDownBase: Control {  }
}
