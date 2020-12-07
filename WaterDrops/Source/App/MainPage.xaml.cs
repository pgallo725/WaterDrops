using System;
using Microsoft.Toolkit.Extensions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        // Reference to application-wide water manager
        private Water water;

        // Layout measurements
        const double SETTINGS_PANEL_WIDTH = 370f;
        const double SETTINGS_PANEL_HEIGHT = 210f;
        double verticalSize;
        double horizontalSize;


        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                DrinkAmountTextBox.Text = "500";
                WaterAmountTextBlock.Text = water.Amount.ToString("0' mL'");
                WaterBar.Value = water.Amount;

                WaterTargetTextBlock.Text = Water.Target.ToString("'/'0");
                WaterBar.Maximum = Water.Target;

                NotificationsToggle.IsOn = App.Settings.NotificationsEnabled;
                StartupToggle.IsOn = App.Settings.AutoStartup;
                StartupToggle.IsEnabled = App.Settings.CanToggleAutoStartup;

                // Calculate UI layout size
                horizontalSize = WaterBar.Width + 50f + SETTINGS_PANEL_WIDTH;
                verticalSize = WaterBar.Margin.Top + WaterBar.Height + 50f + SETTINGS_PANEL_HEIGHT;

                // Hook up event delegates to the corresponding events
                water.WaterChanged += OnWaterChanged;
                Window.Current.SizeChanged += OnSizeChanged;

                // The first SizeChanged event is missed because it happens before Loaded
                // and so we trigger the function manually to adjust the window layout properly
                Rect bounds = Window.Current.Bounds;
                AdjustPageLayout(bounds.Width, bounds.Height);
            };

            this.Unloaded += (sender, e) =>
            {
                // Disconnect event handlers
                water.WaterChanged -= OnWaterChanged;
                Window.Current.SizeChanged -= OnSizeChanged;
            };

            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                water = (Water)e.Parameter;
            }

            base.OnNavigatedTo(e);
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


        private void OnWaterChanged(Water waterObj, EventArgs args)
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
                water.Amount += amount;
            }
        }


        private void NotificationsSetting_Toggled(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            // Update setting only if different from the current value
            if (App.Settings.NotificationsEnabled != toggleSwitch.IsOn)
                App.Settings.NotificationsEnabled = toggleSwitch.IsOn;
        }


        private void StartupSetting_Toggled(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            // Update setting only if it has changed from the previous value
            if (App.Settings.CanToggleAutoStartup && App.Settings.AutoStartup != toggleSwitch.IsOn)
                App.Settings.AutoStartup = toggleSwitch.IsOn;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BMICalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the personal schedule page
            this.Frame.Navigate(typeof(BMICalculatorPage), new UserData());
        }
    }
}
