﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    [TemplatePart(Name = "PART_DateTimeTextBox", Type = typeof(DatePickerTextBox))]
    public class DateTimePicker : Control
    {
        private enum Direction
        {
            Up = 1,
            Down = -1
        }

        private bool _dateTimeIsUpdating;
        private const string PartDateTimeTextBox = "PART_DateTimeTextBox";
        private DatePickerTextBox datePickerTextBox;

        public static readonly ICommand IncreaseDateTimeCommand = new RoutedUICommand(nameof(IncreaseDateTimeCommand), nameof(IncreaseDateTimeCommand), typeof(DateTimePicker));
        public static readonly ICommand DecreaseDateTimeCommand = new RoutedUICommand(nameof(DecreaseDateTimeCommand), nameof(DecreaseDateTimeCommand), typeof(DateTimePicker));

        #region Dependency Property

        public bool CoerceInvalidText
        {
            get { return (bool)GetValue(CoerceInvalidTextProperty); }
            set { SetValue(CoerceInvalidTextProperty, value); }
        }

        public static readonly DependencyProperty CoerceInvalidTextProperty =
            DependencyProperty.Register(nameof(CoerceInvalidText), typeof(bool), typeof(DateTimePicker), new UIPropertyMetadata(true));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(DateTimePicker), new UIPropertyMetadata(false));

        public bool CanIncrease
        {
            get { return (bool)GetValue(CanIncreaseProperty); }
            set { SetValue(CanIncreaseProperty, value); }
        }

        public static readonly DependencyProperty CanIncreaseProperty =
            DependencyProperty.Register(nameof(CanIncrease), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(true));

        public bool CanDecrease
        {
            get { return (bool)GetValue(CanDecreaseProperty); }
            set { SetValue(CanDecreaseProperty, value); }
        }

        public static readonly DependencyProperty CanDecreaseProperty =
            DependencyProperty.Register(nameof(CanDecrease), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(true));

        public bool ShowDropDownButton
        {
            get { return (bool)GetValue(ShowDropDownButtonProperty); }
            set { SetValue(ShowDropDownButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowDropDownButtonProperty =
            DependencyProperty.Register(nameof(ShowDropDownButton), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(true));

        public bool ShowUpDownButton
        {
            get { return (bool)GetValue(ShowUpDownButtonProperty); }
            set { SetValue(ShowUpDownButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowUpDownButtonProperty =
            DependencyProperty.Register(nameof(ShowUpDownButton), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(true));

        public bool ShowCalendarButton
        {
            get { return (bool)GetValue(ShowCalendarButtonProperty); }
            set { SetValue(ShowCalendarButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowCalendarButtonProperty =
            DependencyProperty.Register(nameof(ShowCalendarButton), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(true));

        public DateTime SelectedDateTime
        {
            get { return (DateTime)GetValue(SelectedDateTimeProperty); }
            set { SetValue(SelectedDateTimeProperty, value); }
        }

        public static readonly DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register(nameof(SelectedDateTime), typeof(DateTime), typeof(DateTimePicker), new UIPropertyMetadata(DateTime.Now, OnSelectedDateTimeChanged, OnCoerceSelectedDateTime));

        public bool DateTextIsWrong
        {
            get { return (bool)GetValue(DateTextIsWrongProperty); }
            set { SetValue(DateTextIsWrongProperty, value); }
        }

        public static readonly DependencyProperty DateTextIsWrongProperty =
            DependencyProperty.Register(nameof(DateTextIsWrong), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(false));

        public string DisplayedDateTime
        {
            get { return (string)GetValue(DisplayedDateTimeProperty); }
            set { SetValue(DisplayedDateTimeProperty, value); }
        }

        public static readonly DependencyProperty DisplayedDateTimeProperty =
            DependencyProperty.Register(nameof(DisplayedDateTime), typeof(string), typeof(DateTimePicker), new UIPropertyMetadata("", OnDisplayedDateTimeChanged, OnCoerceDisplayedDateTime));

        public DateTime Maximum
        {
            get { return (DateTime)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(DateTime), typeof(DateTimePicker), new PropertyMetadata(DateTime.MaxValue));

        public DateTime Minimum
        {
            get { return (DateTime)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(DateTime), typeof(DateTimePicker), new PropertyMetadata(DateTime.MinValue));

        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register(nameof(Format), typeof(string), typeof(DateTimePicker), new PropertyMetadata("g", OnFormatChanged));

        #endregion

        static DateTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker), new FrameworkPropertyMetadata(typeof(DateTimePicker)));
            CommandManager.RegisterClassCommandBinding(typeof(DateTimePicker), new CommandBinding(IncreaseDateTimeCommand, OnIncreaseDateTimeCommand));
            CommandManager.RegisterClassCommandBinding(typeof(DateTimePicker), new CommandBinding(DecreaseDateTimeCommand, OnDecreaseDateTimeCommand));
        }

        #region Override

        public override void OnApplyTemplate()
        {
            if (datePickerTextBox != null)
            {
                datePickerTextBox.PreviewKeyDown -= datePickerTextBox_PreviewKeyDown;
                datePickerTextBox.TextChanged -= DatePickerTextBox_TextChanged;
                datePickerTextBox = null;
            }

            base.OnApplyTemplate();

            datePickerTextBox = GetTemplateChild(PartDateTimeTextBox) as DatePickerTextBox;

            if (datePickerTextBox != null)
            {
                datePickerTextBox.PreviewKeyDown += datePickerTextBox_PreviewKeyDown;
                datePickerTextBox.TextChanged += DatePickerTextBox_TextChanged;
                datePickerTextBox.LostFocus += DatePickerTextBox_LostFocus;
            }
        }

        #endregion

        #region Event hendlers

        private static void OnDecreaseDateTimeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var self = sender as DateTimePicker;
            self.OnUpDown(Direction.Down);
        }

        private static void OnIncreaseDateTimeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var self = sender as DateTimePicker;
            self.OnUpDown(Direction.Up);
        }

        private void OnUpDown(Direction direction)
        {
            SelectedDateTime = SmartUpdateDateTime((int)direction);
        }

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as DateTimePicker;
            self.ApplyNewSelectedDate((DateTime?)e.NewValue);
        }

        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as DateTimePicker;

            self.RefreshDisplayedDateTime();
        }

        private void ApplyNewSelectedDate(DateTime? newValue)
        {
            if (!_dateTimeIsUpdating)
            {
                _dateTimeIsUpdating = true;

                DisplayedDateTime = GetFormattedDateTimeString(newValue, Format);
                IsDropDownOpen = false;

                _dateTimeIsUpdating = false;
            }
        }

        private void RefreshDisplayedDateTime()
        {
            ApplyNewSelectedDate(SelectedDateTime);
        }

        private static object OnCoerceSelectedDateTime(DependencyObject d, object baseValue)
        {
            var self = d as DateTimePicker;
            var value = (DateTime?)baseValue;

            self.CanIncrease = true;
            self.CanDecrease = true;

            if (value.HasValue)
            {
                if (value.Value > self.Maximum)
                {
                    self.CanIncrease = false;
                    return self.Maximum;
                }
                else if (value.Value < self.Minimum)
                {
                    self.CanDecrease = false;
                    return self.Minimum;
                }
            }

            return baseValue;
        }

        private static void OnDisplayedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var self = d as DateTimePicker;
            if (!self._dateTimeIsUpdating)
            {
                self._dateTimeIsUpdating = true;

                var parsedDate = self.ParseDateTimeText((string)e.NewValue, self.Format);
                self.DateTextIsWrong = parsedDate == null;
                if (parsedDate != null)
                    self.SelectedDateTime = parsedDate.Value;

                self._dateTimeIsUpdating = false;
            }
        }

        private static object OnCoerceDisplayedDateTime(DependencyObject d, object baseValue)
        {
            var self = (DateTimePicker)d;

            if (string.IsNullOrEmpty((string)baseValue) || self.IsDateInExpectedFormat((string)baseValue))
            {
                self.DateTextIsWrong = false;
                return baseValue;
            }
            else
            {
                self.DateTextIsWrong = true;
                return DependencyProperty.UnsetValue;
            }
        }

        private void datePickerTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsDateInExpectedFormat(datePickerTextBox.Text))
                return;

            switch (e.Key)
            {
                case Key.Up:
                    OnUpDown(Direction.Up);
                    break;
                case Key.Down:
                    OnUpDown(Direction.Down);
                    break;
                    //case Key.Enter:
                    //    DisplayedDateTime = datePickerTextBox.Text;
                    //    break;
            }
        }

        private void DatePickerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayedDateTime = datePickerTextBox.Text;
        }

        private void DatePickerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!DateTextIsWrong || CoerceInvalidText)
            {
                ApplyNewSelectedDate(SelectedDateTime);
                DateTextIsWrong = false;
            }
        }

        #endregion

        #region Private methods

        private string GetFormattedDateTimeString(DateTime? value, string format)
        {
            return value.HasValue ? value.Value.ToString(format, AppBootstrapper.CultureCache) : null;
        }

        private DateTime SmartUpdateDateTime(int direction)
        {
            var dt = SelectedDateTime;

            try
            {
                if (Format.Contains("s"))
                    dt = dt.AddSeconds(direction);
                else if (Format.Contains("m"))
                    dt = dt.AddMinutes(direction);
                else if (Format.Contains("h") || Format.Contains("H"))
                    dt = dt.AddHours(direction);
                else if (Format.Contains("d"))
                    dt = dt.AddDays(direction);
                else if (Format.Contains("M"))
                    dt = dt.AddMonths(direction);
                else if (Format.Contains("y"))
                    dt = dt.AddYears(direction);
            }
            catch (ArgumentException)
            {
                //DateTime must be between DateTime.MinValue and DateTime.MaxValue
            }

            return dt;
        }

        private DateTime? ParseDateTimeText(string value, string format)
        {
            if (!DateTime.TryParseExact(value, format, AppBootstrapper.CultureCache, DateTimeStyles.AllowWhiteSpaces, out var datetime))
                return null;

            return datetime;
        }

        private bool IsDateInExpectedFormat(string date)
        {
            return ParseDateTimeText(date, Format).HasValue;
        }

        #endregion
    }
}
