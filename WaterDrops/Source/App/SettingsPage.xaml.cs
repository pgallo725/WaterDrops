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
    }
}
