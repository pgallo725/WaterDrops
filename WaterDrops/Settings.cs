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


        private bool notificationsEnabled;
        /// <summary>
        /// User setting to specify whether the application is allowed to send
        /// desktop toast notifications as drink or sleep reminders
        /// </summary>
        public bool NotificationsEnabled
        {
            get => notificationsEnabled;
            set
            {
                ApplicationData.Current.LocalSettings.Values["NotificationsEnabled"] = value;
                this.notificationsEnabled = value;

                NotificationsSettingChanged?.Invoke(this, EventArgs.Empty);
            }
        }



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

                this.notificationsEnabled = ApplicationData.Current.LocalSettings.Values
                    .TryGetValue("NotificationsEnabled", out object value) 
                    ? (bool)value : true;
            }
            catch (Exception e)
            {
                this.AutoStartup = false;
                this.CanToggleAutoStartup = false;
                this.NotificationsEnabled = true;

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

                localSettings.Values["NotificationsEnabled"] = this.notificationsEnabled;
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
