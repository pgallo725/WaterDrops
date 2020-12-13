using System;
using Microsoft.Toolkit.Extensions;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Foundation;

namespace WaterDrops
{
    public sealed partial class MainPage : Page
    {
        // Layout measurements
        const double SETTINGS_PANEL_WIDTH = 370f;
        const double SETTINGS_PANEL_HEIGHT = 210f;
        double verticalSize;
        double horizontalSize;

        // ComboBox index conversion table
        private readonly int[] intervals = new int[15] 
        { 
            10, 15, 20, 25, 30, 40, 50, 60, 75, 90, 105, 120, 150, 180, 240 
        };


        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                DrinkAmountTextBox.Text = "500";
                WaterAmountTextBlock.Text = App.User.Water.Amount.ToString("0' mL'");
                WaterBar.Value = App.User.Water.Amount;

                WaterTargetTextBlock.Text = App.User.Water.Target.ToString("'/'0");
                WaterBar.Maximum = App.User.Water.Target;

                switch (App.Settings.NotificationSetting)
                {
                    case Settings.NotificationLevel.Disabled:
                        RadioButtonDisabled.IsChecked = true;
                        break;

                    case Settings.NotificationLevel.Normal:
                        RadioButtonStandard.IsChecked = true;
                        break;

                    case Settings.NotificationLevel.Alarm:
                        RadioButtonAlarm.IsChecked = true;
                        break;
                }

                SolidColorBrush brush = new SolidColorBrush();
                if (App.Settings.NotificationsEnabled)
                {
                    ReminderIntervalComboBox.IsEnabled = true;
                    brush.Color = Colors.Black;
                    ReminderIntervalTextBlock.Foreground = brush;
                }
                else
                {
                    ReminderIntervalComboBox.IsEnabled = false;
                    brush.Color = Colors.DimGray;
                    ReminderIntervalTextBlock.Foreground = brush;
                }

                ReminderIntervalComboBox.SelectedIndex = ConvertIntervalToIndex(App.User.Water.ReminderInterval);

                GlassSizeTextBox.Text = App.User.Water.GlassSize.ToString();

                // Calculate UI layout size
                horizontalSize = WaterBar.Width + 50f + SETTINGS_PANEL_WIDTH;
                verticalSize = WaterBar.Margin.Top + WaterBar.Height + 50f + SETTINGS_PANEL_HEIGHT;

                // Hook up event delegates to the corresponding events
                App.User.Water.WaterAmountChanged += OnWaterAmountChanged;
                Window.Current.SizeChanged += OnSizeChanged;

                // The first SizeChanged event is missed because it happens before Loaded
                // and so we trigger the function manually to adjust the window layout properly
                Rect bounds = Window.Current.Bounds;
                AdjustPageLayout(bounds.Width, bounds.Height);
            };

            this.Unloaded += (sender, e) =>
            {
                // Disconnect event handlers
                App.User.Water.WaterAmountChanged -= OnWaterAmountChanged;
                Window.Current.SizeChanged -= OnSizeChanged;
            };

            
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the settings page
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private void BMICalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the BMI calculator page
            this.Frame.Navigate(typeof(BMICalculatorPage));
        }


        private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            AdjustPageLayout(e.Size.Width, e.Size.Height);
        }

        private void AdjustPageLayout(double newWidth, double newHeight)
        {
            // Adjust layout orientation based on window size
            if (RootPanel.Orientation == Orientation.Vertical)
            {
                if (newWidth > newHeight && newWidth > horizontalSize)
                {
                    RootPanel.Orientation = Orientation.Horizontal;
                    CircleGrid.Margin = new Thickness()
                    {
                        Left = 0,
                        Top = 20,
                        Right = 50,
                        Bottom = 20
                    };
                }
            }
            else    /* Orientation.Horizontal */
            {
                if (newHeight > newWidth || newWidth < horizontalSize)
                {
                    RootPanel.Orientation = Orientation.Vertical;
                    CircleGrid.Margin = new Thickness()
                    {
                        Left = 0,
                        Top = 20,
                        Right = 0,
                        Bottom = 50
                    };
                }
            }
        }


        private void OnWaterAmountChanged(Water waterObj, EventArgs args)
        {
            WaterBar.Value = waterObj.Amount;
            WaterAmountTextBlock.Text = waterObj.Amount.ToString("0' mL'");
        }


        private void TextBox_CheckEnter(object sender, KeyRoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Accept)
            {
                this.Focus(FocusState.Pointer);
            }
        }

        private void WaterTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!this.IsLoaded)
                return;
            
            // Only allow integer values
            args.Cancel = !(args.NewText.IsNumeric() || args.NewText.Length == 0);
        }


        private void DrinkButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            Button button = sender as Button;

            if (DrinkAmountTextBox.Text.Length == 0)
                DrinkAmountTextBox.Text = "0";

            // Add the specified water amount to the current total
            int amount = int.Parse(DrinkAmountTextBox.Text);
            if (amount > 0)
            {
                App.User.Water.Amount += amount;
            }
            else
            {
                DrinkAmountTextBox.Text = "0";
            }
        }


        private void NotificationsLevel_Changed(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            RadioButton radioButton = sender as RadioButton;

            SolidColorBrush brush = new SolidColorBrush();
            switch (radioButton.Tag)
            {
                case "off":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Disabled;
                    brush.Color = Colors.DimGray;
                    break;

                case "standard":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Normal;
                    brush.Color = Colors.Black;
                    break;

                case "alarm":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Alarm;
                    brush.Color = Colors.Black;
                    break;

                default:
                    throw new ApplicationException("Invalid RadioButon tag");
            }

            ReminderIntervalComboBox.IsEnabled = App.Settings.NotificationsEnabled;
            ReminderIntervalTextBlock.Foreground = brush;
        }


        private void ReminderIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            ComboBox comboBox = sender as ComboBox;

            App.User.Water.ReminderInterval = intervals[comboBox.SelectedIndex];
        }

        private int ConvertIntervalToIndex(int value)
        { 
            for (int i = 0; i < intervals.Length; i++)
            {
                if (intervals[i] == value)
                    return i;
            }

            // If the value doesn't fall in the range of ComboBox options, reset it to default
            App.User.Water.ReminderInterval = 30;
            return 5;
        }


        private void GlassSizeTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            // Only allow integer values
            args.Cancel = !(args.NewText.IsNumeric() || args.NewText.Length == 0);
        }

        private void GlassSizeTextBox_Apply(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            if (GlassSizeTextBox.Text.Length == 0)
                GlassSizeTextBox.Text = "0";

            // Add the specified water amount to the current total
            int size = int.Parse(GlassSizeTextBox.Text);
            if (size > 0)
            {
                App.User.Water.GlassSize = size;
            }
            else
            {
                GlassSizeTextBox.Text = App.User.Water.GlassSize.ToString();
            }
        }

    }
}
