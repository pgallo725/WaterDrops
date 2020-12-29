using System;
using Microsoft.Toolkit.Extensions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace WaterDrops
{
    public sealed partial class WaterPage : Page
    {
        // ComboBox index conversion table
        private readonly int[] intervals = new int[15]
        {
            10, 15, 20, 25, 30, 40, 50, 60, 75, 90, 105, 120, 150, 180, 240
        };


        public WaterPage()
        {
            this.InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                WaterAmountTextBlock.Text = App.User.Water.Amount.ToString("0' mL'");
                WaterBar.Value = App.User.Water.Amount;

                WaterTargetTextBlock.Text = App.User.Water.Target.ToString("'/ '0");
                WaterBar.Maximum = App.User.Water.Target;

                switch (App.Settings.NotificationSetting)
                {
                    case Settings.NotificationLevel.Disabled:
                        NotificationDisabledRadioButton.IsChecked = true;
                        break;

                    case Settings.NotificationLevel.Standard:
                        NotificationStandardRadioButton.IsChecked = true;
                        break;

                    case Settings.NotificationLevel.Alarm:
                        NotificationAlarmRadioButton.IsChecked = true;
                        break;
                }

                if (App.Settings.NotificationsEnabled)
                {
                    ReminderIntervalComboBox.IsEnabled = true;
                    ReminderIntervalTextBlock.Opacity = 1;
                }
                else
                {
                    ReminderIntervalComboBox.IsEnabled = false;
                    ReminderIntervalTextBlock.Opacity = 0.5;
                }

                ReminderIntervalComboBox.SelectedIndex = ConvertIntervalToIndex(App.User.Water.ReminderInterval);

                GlassSizeTextBox.Text = App.User.Water.GlassSize.ToString();
                RegisterDrinkAmountTextBox.Text = App.User.Water.GlassSize.ToString();

                // Hook up event delegates to the corresponding events
                App.User.Water.WaterAmountChanged += OnWaterAmountChanged;
            };

            this.Unloaded += (sender, e) =>
            {
                // Disconnect event handlers
                App.User.Water.WaterAmountChanged -= OnWaterAmountChanged;
            };

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


        private void RegisterDrinkAmountTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            // Only allow integer values
            args.Cancel = !(args.NewText.IsNumeric() || args.NewText.Length == 0);
        }

        private void RegisterDrinkAmountTextBox_Apply(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
            {
                textBox.Text = "0";
            }
            else if (int.Parse(textBox.Text) > 2000)
            {
                textBox.Text = "2000";
            }
        }


        private void RegisterDrinkButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            Button button = sender as Button;

            if (RegisterDrinkAmountTextBox.Text.Length == 0)
                RegisterDrinkAmountTextBox.Text = "0";

            // Add the specified water amount to the current total
            int amount = int.Parse(RegisterDrinkAmountTextBox.Text);
            if (amount > 0)
            {
                App.User.Water.Amount += amount;
            }
            else
            {
                RegisterDrinkAmountTextBox.Text = "0";
            }
        }


        private void NotificationsLevel_Changed(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            RadioButton radioButton = sender as RadioButton;

            double opacity;
            switch (radioButton.Tag)
            {
                case "off":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Disabled;
                    opacity = 0.5;
                    break;

                case "standard":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Standard;
                    opacity = 1;
                    break;

                case "alarm":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Alarm;
                    opacity = 1;
                    break;

                default:
                    throw new ApplicationException("Invalid RadioButon tag");
            }

            ReminderIntervalComboBox.IsEnabled = App.Settings.NotificationsEnabled;
            ReminderIntervalTextBlock.Opacity = opacity;
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
            return 4;
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

            // Update the GlassSize with the value written in the TextBox
            int size = int.Parse(GlassSizeTextBox.Text);
            if (size > 0)
            {
                // Cap the value at 2000mL
                if (size > 2000)
                {
                    size = 2000;
                    GlassSizeTextBox.Text = "2000";
                }
                App.User.Water.GlassSize = size;
                RegisterDrinkAmountTextBox.Text = App.User.Water.GlassSize.ToString();
            }
            else
            {
                GlassSizeTextBox.Text = App.User.Water.GlassSize.ToString();
            }
        }

    }
}
