using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;


namespace WaterDrops
{
    class Settings
    {
        // Delegate declaration
        public delegate void NotificationsSettingChangedHandler(Settings settings, EventArgs args);

        // Event declaration
        public event NotificationsSettingChangedHandler NotificationsSettingChanged;


        private bool autoStartup;
        /// <summary>
        /// Setting that determines whether the application will launch
        /// automatically when the user logins to Windows
        /// </summary>
        public bool AutoStartup
        {
            get => autoStartup;
            set
            {
                this.autoStartup = value;

                if (autoStartup)            // Enable automatic startup with Windows
                    EnableStartupTask();
                else DisableStartupTask();      // Disable scheduled execution at Windows startup
            }
        }

        /// <summary>
        /// Represents whether the user has control over the AutoStartup setting,
        /// or if the system has control over it and prevents it to changed
        /// </summary>
        public bool CanToggleAutoStartup { get; private set; }


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
        public async Task LoadSettings()
        {
            try
            {
                // Check StartupTask status in order to load the AutoStartup value
                StartupTaskState startup = (await StartupTask.GetAsync("WaterDropsStartupId")).State;

                this.CanToggleAutoStartup = (startup == StartupTaskState.Enabled || startup == StartupTaskState.Disabled);

                if (startup == StartupTaskState.Enabled || startup == StartupTaskState.EnabledByPolicy)
                    this.autoStartup = true;
                else this.autoStartup = false;

                this.notificationSetting = ApplicationData.Current.LocalSettings.Values
                    .TryGetValue("NotificationsLevel", out object value)
                    ? (NotificationLevel)value : NotificationLevel.Normal;
            }
            catch (Exception e)
            {
                this.AutoStartup = false;
                this.CanToggleAutoStartup = false;
                this.NotificationSetting = NotificationLevel.Normal;

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


        async private void EnableStartupTask()
        {
            StartupTask startupTask = await StartupTask.GetAsync("WaterDropsStartupId");
            if (startupTask.State == StartupTaskState.Disabled)
            {
                // Task is disabled but can be enabled.
                await startupTask.RequestEnableAsync();
            }
        }

        async private void DisableStartupTask()
        {
            try
            {
                StartupTask startupTask = await StartupTask.GetAsync("WaterDropsStartupId");
                if (startupTask.State == StartupTaskState.Enabled)
                {
                    // Task is enabled but can be disabled.
                    startupTask.Disable();
                }
            }
            catch (ArgumentException)
            {
                // The StartupTask does not exist, nothing has to be done
            }
        }
    }
}
