using System;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.ApplicationModel.Resources;


namespace WaterDrops
{
    class Settings
    {
        // Delegates declaration
        public delegate void NotificationsSettingChangedHandler(Settings settings, EventArgs args);
        public delegate void AutoStartupSettingChangedHandler(bool autoStartupEnabled, EventArgs args);
        public delegate void ColorThemeSettingChangedHandler(ColorTheme currentTheme, EventArgs args);

        // Events declaration
        public event NotificationsSettingChangedHandler NotificationsSettingChanged;
        public event AutoStartupSettingChangedHandler AutoStartupSettingChanged;
        public event ColorThemeSettingChangedHandler ColorThemeSettingChanged;


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
                        return "";

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

                    ColorThemeSettingChanged?.Invoke(colorThemeSetting, EventArgs.Empty);
                }
            }
        }


        /// <summary>
        /// Load previously saved application settings locally from the device
        /// If one or more settings are not found, the default value is loaded
        /// </summary>
        public async void LoadSettings()
        {
            // Get the application's StartupTask object
            try
            {
                this.startupTask = await StartupTask.GetAsync("WaterDropsStartupId");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

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
