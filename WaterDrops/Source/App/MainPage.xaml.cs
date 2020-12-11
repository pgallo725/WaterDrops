using System;
using Microsoft.Toolkit.Extensions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Input;
using Windows.Foundation;


namespace WaterDrops
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Reference to application-wide user data manager
        private UserData userData;

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
                WaterAmountTextBlock.Text = userData.Water.Amount.ToString("0' mL'");
                WaterBar.Value = userData.Water.Amount;

                WaterTargetTextBlock.Text = userData.Water.Target.ToString("'/'0");
                WaterBar.Maximum = userData.Water.Target;

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

                ReminderIntervalComboBox.SelectedIndex = ConvertIntervalToIndex(userData.Water.ReminderInterval);
                GlassSizeTextBox.Text = userData.Water.GlassSize.ToString();

                // Calculate UI layout size
                horizontalSize = WaterBar.Width + 50f + SETTINGS_PANEL_WIDTH;
                verticalSize = WaterBar.Margin.Top + WaterBar.Height + 50f + SETTINGS_PANEL_HEIGHT;

                // Hook up event delegates to the corresponding events
                userData.Water.WaterAmountChanged += OnWaterAmountChanged;
                Window.Current.SizeChanged += OnSizeChanged;

                // The first SizeChanged event is missed because it happens before Loaded
                // and so we trigger the function manually to adjust the window layout properly
                Rect bounds = Window.Current.Bounds;
                AdjustPageLayout(bounds.Width, bounds.Height);
            };

            this.Unloaded += (sender, e) =>
            {
                // Disconnect event handlers
                userData.Water.WaterAmountChanged -= OnWaterAmountChanged;
                Window.Current.SizeChanged -= OnSizeChanged;
            };

            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                userData = (UserData)e.Parameter;
            }

            base.OnNavigatedTo(e);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the settings page
            this.Frame.Navigate(typeof(SettingsPage), App.Settings);
        }

        private void BMICalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the BMI calculator page
            this.Frame.Navigate(typeof(BMICalculatorPage), userData);
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
                userData.Water.Amount += amount;
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

            switch (radioButton.Tag)
            {
                case "off":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Disabled;
                    break;

                case "standard":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Normal;
                    break;

                case "alarm":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Alarm;
                    break;

                default:
                    throw new ApplicationException("Invalid RadioButon tag");
            }
        }


        private void ReminderIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            ComboBox comboBox = sender as ComboBox;

            userData.Water.ReminderInterval = intervals[comboBox.SelectedIndex];
        }

        private int ConvertIntervalToIndex(int value)
        { 
            for (int i = 0; i < intervals.Length; i++)
            {
                if (intervals[i] == value)
                    return i;
            }

            // If the value doesn't fall in the range of ComboBox options, reset it to default
            userData.Water.ReminderInterval = 30;
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
                userData.Water.GlassSize = size;
            }
            else
            {
                GlassSizeTextBox.Text = userData.Water.GlassSize.ToString();
            }
        }

    }
}
