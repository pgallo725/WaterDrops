using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;


namespace WaterDrops
{
    class Settings
    {
        // Delegates declaration
        public delegate void NotificationsSettingChangedHandler(Settings settings, EventArgs args);
        public delegate void AutoStartupSettingChangedHandler(bool autoStartupEnabled, EventArgs args);

        // Events declaration
        public event NotificationsSettingChangedHandler NotificationsSettingChanged;
        public event AutoStartupSettingChangedHandler AutoStartupSettingChanged;


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
                if (startupTask == null)
                    return "ERROR";

                switch (startupTask.State)
                {
                    case StartupTaskState.Enabled:
                    case StartupTaskState.Disabled:
                        return "";

                    case StartupTaskState.EnabledByPolicy:
                    case StartupTaskState.DisabledByPolicy:
                        return "AutoStartup setting controlled by admin or group policies";

                    case StartupTaskState.DisabledByUser:
                        return "AutoStartup permission denied, enable it via the Task Manager";

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
            Normal,
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
                // Load other settings from local application settings storage
                this.notificationSetting = ApplicationData.Current.LocalSettings.Values
                    .TryGetValue("NotificationsLevel", out object value)
                    ? (NotificationLevel)value : NotificationLevel.Normal;
            }
            catch (Exception e)
            {
                // Default settings
                this.notificationSetting = NotificationLevel.Normal;

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
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

                localSettings.Values["NotificationsLevel"] = this.notificationSetting;
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
