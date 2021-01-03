using System;
using System.Reflection;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WaterDrops
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                // Initialize AutoStartup and ColorTheme settings UI
                UpdateStartupSettingToggle(App.Settings.AutoStartupEnabled, EventArgs.Empty);

                switch (App.Settings.ColorThemeSetting)
                {
                    case Settings.ColorTheme.Light:
                        LightThemeRadioButton.IsChecked = true;
                        break;

                    case Settings.ColorTheme.Dark:
                        DarkThemeRadioButton.IsChecked = true;
                        break;

                    case Settings.ColorTheme.System:
                        SystemThemeRadioButton.IsChecked = true;
                        break;
                }

                // Retrieve application information from the current assembly using AssemblyInfo
                Assembly assembly = Assembly.GetExecutingAssembly();
                AppTitleTextBlock.Text = AssemblyInfo.GetAttribute<AssemblyTitleAttribute>(assembly).Title;
                CopyrightTextBlock.Text = AssemblyInfo.GetAttribute<AssemblyCopyrightAttribute>(assembly).Copyright;
                AuthorTextBlock.Text = AssemblyInfo.GetAttribute<AssemblyCompanyAttribute>(assembly).Company;
                VersionTextBlock.Text = AssemblyInfo.GetAttribute<AssemblyFileVersionAttribute>(assembly).Version;
                ReleaseTextBlock.Text = AssemblyInfo.GetAttribute<AssemblyDescriptionAttribute>(assembly).Description;

                // Register UI Controls callback
                StartupToggle.Toggled += StartupSetting_Toggled;
                LightThemeRadioButton.Checked += ColorTheme_Changed;
                DarkThemeRadioButton.Checked += ColorTheme_Changed;
                SystemThemeRadioButton.Checked += ColorTheme_Changed;

                // Register callbacks for updating UI elements and layout when the underlying values change
                App.Settings.AutoStartupSettingChanged += UpdateStartupSettingToggle;
                Window.Current.SizeChanged += OnSizeChanged;

                // The first SizeChanged event is missed because it happens before Loaded
                // and so we trigger the function manually to adjust the window layout properly
                AdjustPageLayout(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
            };

            this.Unloaded += (sender, e) =>
            {
                // Detach all event handlers
                App.Settings.AutoStartupSettingChanged -= UpdateStartupSettingToggle;
                Window.Current.SizeChanged -= OnSizeChanged;

                StartupToggle.Toggled -= StartupSetting_Toggled;
                LightThemeRadioButton.Checked -= ColorTheme_Changed;
                DarkThemeRadioButton.Checked -= ColorTheme_Changed;
                SystemThemeRadioButton.Checked -= ColorTheme_Changed;
            };
        }


        private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            AdjustPageLayout(e.Size.Width, e.Size.Height);
        }

        private void AdjustPageLayout(double newWidth, double newHeight)
        {
            const double MIN_HEIGHT_FOR_VERTICAL_LAYOUT = 620;
            const double MIN_WIDTH_FOR_HORIZONTAL_LAYOUT = 680;

            // Adjust layout orientation based on window size
            if (RootPanel.Orientation == Orientation.Vertical)
            {
                if (newHeight < MIN_HEIGHT_FOR_VERTICAL_LAYOUT 
                    && newWidth >= MIN_WIDTH_FOR_HORIZONTAL_LAYOUT)
                {
                    RootPanel.Orientation = Orientation.Horizontal;
                    StartupDescriptionTextBlock.Width = 250;
                    StartupDescriptionTextBlock.Margin = new Thickness
                    {
                        Left = 10,
                        Top = 10,
                        Right = 90,
                        Bottom = (StartupDescriptionTextBlock.Text == string.Empty ? 10 : 20)
                    };
                    ColorThemeSettingStackPanel.Margin = new Thickness
                    {
                        Left = 10,
                        Top = 0,
                        Right = 0,
                        Bottom = 0
                    };
                }
            }
            else    /* Orientation.Horizontal */
            {
                if (newWidth < MIN_WIDTH_FOR_HORIZONTAL_LAYOUT 
                    || newHeight >= MIN_HEIGHT_FOR_VERTICAL_LAYOUT)
                {
                    RootPanel.Orientation = Orientation.Vertical;
                    StartupDescriptionTextBlock.Width = 500;
                    StartupDescriptionTextBlock.Margin = new Thickness
                    {
                        Left = 10,
                        Top = 10,
                        Right = 0,
                        Bottom = (StartupDescriptionTextBlock.Text == string.Empty ? 10 : 20)
                    };
                    ColorThemeSettingStackPanel.Margin = new Thickness
                    {
                        Left = 10,
                        Top = 0,
                        Right = 0,
                        Bottom = 30
                    };
                }
            }
        }


        private void StartupSetting_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            // Update setting only if it has changed from the previous value
            if (App.Settings.CanToggleAutoStartup)
                App.Settings.TryChangeAutoStartupSetting(toggleSwitch.IsOn);
        }

        private void UpdateStartupSettingToggle(bool autoStartupEnabled, EventArgs args)
        {
            StartupToggle.IsOn = autoStartupEnabled;
            StartupToggle.IsEnabled = App.Settings.CanToggleAutoStartup;
            StartupDescriptionTextBlock.Text = App.Settings.AutoStartupStateDescription;
            StartupDescriptionTextBlock.Margin = new Thickness
            {
                Left = 10,
                Top = 10,
                Right = (RootPanel.Orientation == Orientation.Vertical ? 0 : 90),
                Bottom = (StartupDescriptionTextBlock.Text == string.Empty ? 10 : 20)
            };
        }


        private void ColorTheme_Changed(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            switch (radioButton.Tag)
            {
                case "light":
                    App.Settings.ColorThemeSetting = Settings.ColorTheme.Light;
                    break;

                case "dark":
                    App.Settings.ColorThemeSetting = Settings.ColorTheme.Dark;
                    break;

                case "system":
                    App.Settings.ColorThemeSetting = Settings.ColorTheme.System;
                    break;

                default:
                    throw new ApplicationException("Invalid RadioButon tag");
            }
        }
    }
}
