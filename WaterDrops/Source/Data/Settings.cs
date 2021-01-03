using System;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;

namespace WaterDrops
{
    class Settings
    {
        // Delegates declaration
        public delegate void NotificationsSettingChangedHandler(Settings settings, EventArgs args);
        public delegate void AutoStartupSettingChangedHandler(bool autoStartupEnabled, EventArgs args);
        public delegate void ColorThemeChangedHandler(ApplicationTheme theme, EventArgs args);

        // Events declaration
        public event NotificationsSettingChangedHandler NotificationsSettingChanged;
        public event AutoStartupSettingChangedHandler AutoStartupSettingChanged;
        public event ColorThemeChangedHandler ColorThemeChanged;


        private StartupTask startupTask = null;

        /// <summary>
        /// Represents whether the user has control over the StartupTaskState setting,
        /// or if the system has control over it and prevents it to be changed
        /// </summary>
        public bool CanToggleAutoStartup
        {
            get
            {
                return startupTask != null &&
                    (startupTask.State == StartupTaskState.Enabled ||
                    startupTask.State == StartupTaskState.Disabled);
            }
        }

        /// <summary>
        /// Provides additional information about the app's AutoStartup state
        /// </summary>
        public string AutoStartupStateDescription
        {
            get
            {
                ResourceLoader resources = ResourceLoader.GetForCurrentView();

                if (startupTask == null)
                    return resources.GetString("ErrorString");

                switch (startupTask.State)
                {
                    case StartupTaskState.Enabled:
                    case StartupTaskState.Disabled:
                        return string.Empty;

                    case StartupTaskState.EnabledByPolicy:
                    case StartupTaskState.DisabledByPolicy:
                        return resources.GetString("StartupPolicyControlledString");

                    case StartupTaskState.DisabledByUser:
                        return resources.GetString("NoStartupPermissionString");

                    default:
                        throw new ApplicationException("Invalid startup task state");
                }
            }
        }

        /// <summary>
        /// Check if the application has been set up to start automatically with Windows (on user logon)
        /// </summary>
        public bool AutoStartupEnabled { get => startupTask != null && startupTask.State.ToString().Contains("Enabled"); }

        /// <summary>
        /// Attempts to apply the requested AutoStartup setting to the StartupTask object,
        /// this operation may fail due to Windows policies and permission settings
        /// </summary>
        /// <param name="autoStartupEnabled">Specifies whether the AutoStartup setting has to be enabled or disabled</param>
        /// <returns>The AutoStartup setting value after the operation,
        /// it may differ from the autoStartupEnabled parameter if the operation has not been successful</returns>
        public void TryChangeAutoStartupSetting(bool autoStartupEnabled)
        {
            if (startupTask != null)
            {
                if (autoStartupEnabled)             // Enable automatic startup with Windows
                    TryEnableStartupTask();
                else TryDisableStartupTask();       // Disable scheduled execution at Windows startup
            }
        }


        public enum NotificationLevel
        {
            Disabled,
            Standard,
            Alarm
        }

        private NotificationLevel notificationSetting;
        /// <summary>
        /// User setting to specify which type of desktop toast notifications, if any,
        /// the application is allowed to send as drink or sleep reminders
        /// </summary>
        public NotificationLevel NotificationSetting
        {
            get => notificationSetting;
            set
            {
                // Update setting only if different from the current value
                if (notificationSetting != value)
                {
                    ApplicationData.Current.LocalSettings.Values["NotificationsLevel"] = (int)value;
                    this.notificationSetting = value;

                    NotificationsSettingChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Specifies whether toast notification reminders are enabled for the application
        /// </summary>
        public bool NotificationsEnabled { get => notificationSetting != NotificationLevel.Disabled; }


        private readonly UISettings uiSettings = new UISettings();

        public enum ColorTheme
        {
            Light,
            Dark,
            System
        }

        private ColorTheme colorThemeSetting;
        /// <summary>
        /// User setting to specify whether the light or dark application theme has to be used,
        /// or if the app will simply follow the system theme (set by the user in Windows settings)
        /// </summary>
        public ColorTheme ColorThemeSetting
        {
            get => colorThemeSetting;
            set
            {
                // Update setting only if different from the current value
                if (colorThemeSetting != value)
                {
                    ApplicationData.Current.LocalSettings.Values["ColorTheme"] = (int)value;
                    this.colorThemeSetting = value;

                    ColorThemeChanged?.Invoke(RequestedApplicationTheme, EventArgs.Empty);
                }
            }
        }

        private ApplicationTheme systemTheme;
        /// <summary>
        /// The requested ApplicationTheme (Light or Dark) which has been selected by the
        /// user (either in the app or via Windows settings)
        /// </summary>
        public ApplicationTheme RequestedApplicationTheme
        {
            get
            {
                switch (colorThemeSetting)
                {
                    case ColorTheme.Light: return ApplicationTheme.Light;
                    case ColorTheme.Dark: return ApplicationTheme.Dark;
                    case ColorTheme.System: return systemTheme;
                    default: throw new ApplicationException("Invalid color theme setting");
                }
            }
        }

        /// <summary>
        /// Handles color theme changes applied outside of the application (e.g. Windows settings)
        /// </summary>
        private async void SystemColorSettingsChanged(UISettings sender, object args)
        {
            Color backgroundColor = sender.GetColorValue(UIColorType.Background);
            systemTheme = (backgroundColor == Colors.Black) ? 
                ApplicationTheme.Dark : ApplicationTheme.Light;

            // Update the current application colors if it's using the system theme
            if (ColorThemeSetting == ColorTheme.System)
                await CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.Normal, InvokeColorThemeChangedHandlers);
        }

        /// <summary>
        /// Utility method for invoking the ColorThemeChanged event handler as a dispatched call
        /// </summary>
        private void InvokeColorThemeChangedHandlers()
        {
            ColorThemeChanged?.Invoke(RequestedApplicationTheme, EventArgs.Empty);
        }


        /// <summary>
        /// Load previously saved application settings locally from the device
        /// If one or more settings are not found, the default value is loaded
        /// </summary>
        public void LoadSettings()
        {
            // Get the application's StartupTask object
            try
            {
                this.startupTask = StartupTask.GetAsync("WaterDropsStartupId").AsTask().Result;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            // Store the initial application theme (which reflects the system theme)
            systemTheme = Application.Current.RequestedTheme;

            try
            {
                ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

                // Load other settings from local application settings storage
                this.notificationSetting = LocalSettings.Values.TryGetValue("NotificationsLevel", out object value)
                    ? (NotificationLevel)value : NotificationLevel.Standard;

                this.colorThemeSetting = LocalSettings.Values.TryGetValue("ColorTheme", out value)
                    ? (ColorTheme)value : ColorTheme.System;
            }
            catch (Exception e)
            {
                // Default settings
                this.notificationSetting = NotificationLevel.Standard;
                this.colorThemeSetting = ColorTheme.System;

                Console.Error.WriteLine(e.Message);
            }

            // Attach SystemColorSettingsChanged handler to the UISettings event
            uiSettings.ColorValuesChanged += SystemColorSettingsChanged;
        }


        /// <summary>
        /// Write application settings to Windows.Storage.ApplicationData.Current.LocalSettings
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

                LocalSettings.Values["NotificationsLevel"] = this.notificationSetting;
                LocalSettings.Values["ColorTheme"] = this.colorThemeSetting;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }


        private async void TryEnableStartupTask()
        {
            if (startupTask.State == StartupTaskState.Disabled)
            {
                // Task is disabled but can be enabled.
                await startupTask.RequestEnableAsync();

                AutoStartupSettingChanged?.Invoke(AutoStartupEnabled, EventArgs.Empty);
            }
        }

        private void TryDisableStartupTask()
        {
            if (startupTask.State == StartupTaskState.Enabled)
            {
                // Task is enabled but can be disabled.
                startupTask.Disable();

                AutoStartupSettingChanged?.Invoke(false, EventArgs.Empty);
            }
        }
    }
}
