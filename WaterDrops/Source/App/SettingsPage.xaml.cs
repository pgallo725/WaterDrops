using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WaterDrops
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                // Initialize settings
                StartupToggle.IsEnabled = App.Settings.CanToggleAutoStartup;
                StartupToggle.IsOn = App.Settings.AutoStartupEnabled;
                StartupDescriptionTextBlock.Text = App.Settings.AutoStartupStateDescription;

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

                // Register callbacks for updating UI elements when some settings are changed by the code
                App.Settings.AutoStartupSettingChanged += UpdateStartupSettingToggle;
            };

            this.Unloaded += (sender, e) =>
            {
                // Detach event handlers
                App.Settings.AutoStartupSettingChanged -= UpdateStartupSettingToggle;
            };
        }


        private void StartupSetting_Toggled(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

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
        }


        private void ColorTheme_Changed(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

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

            ElementTheme theme = App.Settings.RequestedApplicationTheme == ApplicationTheme.Light ?
                ElementTheme.Light : ElementTheme.Dark;

            // Apply color theme on-the-fly (without restarting the app)
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = theme;
            }
        }
    }
}
