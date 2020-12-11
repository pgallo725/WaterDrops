using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.ExtendedExecution.Foreground;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;      // Notifications library


namespace WaterDrops
{
    /// <summary>
    /// Specific application behaviour in addition to the default Application class
    /// </summary>
    sealed partial class App : Application
    {
        // Random number generator
        private readonly Random rand;

        // Application settings manager
        internal static Settings Settings { get; } = new Settings();

        // User data storage object
        private UserData User { get; } = new UserData();

        // Toast notifications manager
        private readonly ToastNotifier notifier;

        // Notification status variables
        private DateTime nextReminderTime = DateTime.MinValue;
        private string nextReminderTag = "Regular";

        // Extended execution session handle
        ExtendedExecutionForegroundSession session;


        /// <summary>
        /// Initialized the singleton Application object. It's the first line of the generated code
        /// and, as such, it is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // Initialize randomizer
            rand = new Random();

            // Create a manager for toast notifications
            notifier = ToastNotificationManager.CreateToastNotifier();
            Settings.NotificationsSettingChanged += OnNotificationsSettingChanged;
            User.Water.WaterSettingsChanged += OnWaterSettingsChanged;
        }


        /// <summary>
        /// Called when the application is regularly launched by the end user. 
        /// At the application startup other entry points will be used to open a specific file.
        /// </summary>
        /// <param name="e">Details about the request and the startup process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e.PrelaunchActivated)
                return;

            OnLaunchedOrActivated(e);
        }

        /// <summary>
        /// Called when the application is activated by the user clicking on a toast notification body.
        /// </summary>
        /// <param name="e">Details about the request and the activation process.</param>
        protected override void OnActivated(IActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }


        /// <summary>
        /// Initializes the application root frame and tasks, handles different kinds of activation,
        /// loads user data and finally activates the app window
        /// </summary>
        /// <param name="e">Details about the type and arguments of the application's startup</param>
        private void OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            // Initialize the root frame (only once)
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a frame that will act as navigation context
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += OnNavigated;

                // Position the frame in the current window
                Window.Current.Content = rootFrame;
            }

            if (e.Kind == ActivationKind.Launch)
            {
                // Handle normal application launch
                var launchArgs = e as LaunchActivatedEventArgs;

            }
            else if (e.Kind == ActivationKind.StartupTask)
            {
                // Handle automatic startup with Windows
                var startupArgs = e as StartupTaskActivatedEventArgs;

            }
            else if (e.Kind == ActivationKind.ToastNotification)
            {
                // Handle toast activation
                var toastActivationArgs = e as ToastNotificationActivatedEventArgs;

            }

            // Load user data and settings
            Settings.LoadSettings();
            User.Load();

            if (rootFrame.Content == null)
            {
                // When navigation stack is not being resumed, navigate to MainPage 
                // passing it a reference to the Water object as a navigation parameter
                rootFrame.Navigate(typeof(MainPage), User);
            }

            // Make sure that the current window is set as active
            Window.Current.Activate();

            // Define handler for generic BackButton press
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Setup the daily reminder schedule
            SetupDailyNotifications();

            // Register the application's background tasks
            RegisterBackgroundTask("ToastAction", new ToastNotificationActionTrigger());
            RegisterBackgroundTask("ReminderWatchdog", new TimeTrigger(15, false));

            // And finally request extended execution capabilities for the application
            RequestExtendedExecution();
        }


        /// <summary>
        /// Called every time that a new page is displayed (when navigating to it)
        /// </summary>
        /// <param name="sender">Frame that has just been navigated to</param>
        /// <param name="e">Details on the navigation event.</param>
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }


        /// <summary>
        /// Called when the navigation to a specific page has a negative outcome
        /// </summary>
        /// <param name="sender">Frame whose navigation has failed</param>
        /// <param name="e">Details on the navigation error.</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }


        /// <summary>
        /// Called when the UWP BackButton is pressed
        /// </summary>
        /// <param name="sender">Frame whose BackButton has been pressed</param>
        /// <param name="e">Details on the Back navigation request</param>
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }


        /// <summary>
        /// Called when the execution of the application is suspended. The state is saved
        /// without knowing if the application will be terminated or resumed properly.
        /// </summary>
        /// <param name="sender">Suspension request source.</param>
        /// <param name="e">Details about the suspension request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // NOTHING TO DO

            deferral.Complete();
        }



        /// <summary>
        /// Called when the user triggers a background task by clicking on a toast notification button
        /// or when the task background process runs (periodically between every 15 and 30 minutes)
        /// </summary>
        /// <param name="args">Details about the background task activation</param>
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case "ToastAction":

                    if (args.TaskInstance.TriggerDetails is ToastNotificationActionTriggerDetail details)
                    {
                        if (details.Argument == "confirm")
                        {
                            // Register the drink
                            User.Water.Amount += User.Water.GlassSize;

                            // Remove any other scheduled drink reminder
                            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
                            {
                                if (notification.Group == "DrinkReminder")
                                    notifier.RemoveFromSchedule(notification);
                            }

                            if (Settings.NotificationsEnabled && User.Water.Amount < User.Water.Target)
                            {
                                // And schedule the next one in User.Water.ReminderInterval minutes
                                nextReminderTag = "Regular";
                                nextReminderTime = DateTime.Now.AddMinutes(User.Water.ReminderInterval);
                                ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                            }
                        }
                        else if (details.Argument == "postpone")
                        {
                            // Remove any other scheduled drink reminder
                            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
                            {
                                if (notification.Group == "DrinkReminder")
                                    notifier.RemoveFromSchedule(notification);
                            }

                            if (Settings.NotificationsEnabled && User.Water.Amount < User.Water.Target)
                            {
                                // Postpone the same notification to User.Water.ReminderDelay minutes from now
                                nextReminderTag = "Postponed";
                                nextReminderTime = DateTime.Now.AddMinutes(User.Water.ReminderDelay);
                                ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                            }
                        }
                    }
                    break;

                case "ReminderWatchdog":
                    
                    if (DateTime.Now <= DateTime.Today.AddMinutes(30))
                    {
                        // Reset notifications and water progress after midnight
                        User.Water.Amount = 0;
                        SetupDailyNotifications();
                    }

                    if (Settings.NotificationsEnabled)
                    {
                        if (DateTime.Now < nextReminderTime)
                        {
                            // Make sure that the next reminder is properly scheduled
                            if (notifier.GetScheduledToastNotifications().Count(i => i.Group == "DrinkReminder") == 0)
                            {
                                ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                            }
                        }
                        else    /* DateTime.Now > nextReminderTime */
                        {
                            // Schedule the next drink reminder of the same type
                            nextReminderTime = new[] {
                                DateTime.Now.AddSeconds(1),
                                nextReminderTime.AddMinutes(User.Water.ReminderDelay)
                            }.Max();

                            ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                        }
                    }
                    break;

                default:
                    throw new Exception("Unexpected background task activation: " + args.TaskInstance.Task.Name);
            }

            deferral.Complete();
        }


        /// <summary>
        /// Handle the change in the NotificationsEnabled setting change
        /// </summary>
        /// <param name="settings">The settings manager that triggered this event</param>
        /// <param name="args">Ignore this parameter</param>
        private void OnNotificationsSettingChanged(Settings settings, EventArgs args)
        {
            // Remove previously scheduled notifications
            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
            {
                notifier.RemoveFromSchedule(notification);
            }

            if (settings.NotificationsEnabled)
            {
                if (User.Water.Amount < User.Water.Target)
                {
                    // If the previously scheduled reminder cannot be restored
                    if (DateTime.Now >= nextReminderTime)
                    {
                        // Schedule a new reminder in User.Water.ReminderDelay minutes
                        // or at 8:00 in the morning if the day hasn't yet begun
                        nextReminderTime = new[] {
                            DateTime.Today.AddHours(8),
                            DateTime.Now.AddMinutes(User.Water.ReminderInterval)
                        }.Max();

                        if (DateTime.Now < DateTime.Today.AddHours(8))
                            nextReminderTag = "Regular";
                    }

                    ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                }

                // Schedule the next sleep reminder at midnight
                ScheduleSleepReminder(DateTime.Today.AddDays(1));
            }
        }


        private void OnWaterSettingsChanged(Water water, EventArgs args)
        {
            // Remove scheduled drink reminders
            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
            {
                if (notification.Group == "DrinkReminder")
                    notifier.RemoveFromSchedule(notification);
            }

            if (Settings.NotificationsEnabled)
            {
                if (water.Amount < water.Target)
                {
                    // If the previously scheduled reminder cannot be restored
                    if (DateTime.Now >= nextReminderTime)
                    {
                        // Schedule a new reminder in ReminderDelay minutes
                        // or at 8:00 in the morning if the day hasn't yet begun
                        nextReminderTime = new[] {
                            DateTime.Today.AddHours(8),
                            DateTime.Now.AddMinutes(water.ReminderInterval)
                        }.Max();

                        if (DateTime.Now < DateTime.Today.AddHours(8))
                            nextReminderTag = "Regular";
                    }

                    ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                }
            }
        }


        /// <summary>
        /// Schedule the reminders for the rest of the day (initializes notification scheduling at application launch)
        /// More specifically, it schedules the first drink reminder and the sleep reminder at midnight
        /// </summary>
        private void SetupDailyNotifications()
        {
            // Clear all pending notifications
            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
            {
                notifier.RemoveFromSchedule(notification);
            }

            if (Settings.NotificationsEnabled)
            {
                if (User.Water.Amount < User.Water.Target)
                {
                    // Schedule the next drink reminder, either at 8:00 in the morning 
                    // or in User.Water.ReminderDelay minutes from now if it's passed that time
                    nextReminderTag = "Regular";
                    nextReminderTime = new[] {
                        DateTime.Today.AddHours(8),
                        DateTime.Now.AddMinutes(User.Water.ReminderDelay) 
                    }.Max();
                    ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                }

                // Schedule the next sleep reminder at midnight
                ScheduleSleepReminder(DateTime.Today.AddDays(1));
            }
        }


        /// <summary>
        /// Create and schedule a toast notification to remind the user to drink
        /// </summary>
        private void ScheduleDrinkReminder(DateTime when, string tag)
        {
            ToastContent toastContent = new ToastContent()
            {
                // Construct the visuals of the toast
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Drink Reminder"
                            },

                            new AdaptiveText()
                            {
                                Text = "You should have been drinking a glass of water! Have you finished it already ?"
                            }
                        },

                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///Assets/drink.png",
                            HintCrop = ToastGenericAppLogoCrop.None
                        }
                    }
                },

                ActivationType = ToastActivationType.Foreground,

                Scenario = Settings.NotificationSetting == Settings.NotificationLevel.Alarm ?
                    ToastScenario.Alarm : ToastScenario.Reminder,

                // Add buttons to the toast body
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Yes (+" + User.Water.GlassSize.ToString() + " mL)", "confirm")
                        {
                            ActivationType = ToastActivationType.Background
                        },
                        new ToastButton("Not yet (" + User.Water.ReminderDelay.ToString() + " mins)", "postpone")
                        {
                            ActivationType = ToastActivationType.Background
                        }
                    }
                },

                // Specify a custom notification sound effect
                Audio = new ToastAudio() 
                { 
                    //Src = new Uri("ms-appx:///Assets/waterdrop_sound.wav"),
                    Loop = (Settings.NotificationSetting == Settings.NotificationLevel.Alarm)
                }
            };

            // Create and schedule the toast notification
            ScheduledToastNotification toast = 
                new ScheduledToastNotification(toastContent.GetXml(), new DateTimeOffset(when))
            {
                // Set expiration time
                ExpirationTime = (tag == "Postponed") ?
                    DateTime.Now.AddMinutes(User.Water.ReminderDelay) :
                    DateTime.Now.AddMinutes(User.Water.ReminderInterval),

                // Identify and categorize the toast
                Id = rand.Next(1, 100000000).ToString(),
                Group = "DrinkReminder",
                Tag = tag
            };

            notifier.AddToSchedule(toast);
        }


        /// <summary>
        /// Build and schedule a toast notification reminding the user to go to sleep
        /// </summary>
        private void ScheduleSleepReminder(DateTime when)
        {
            ToastContent toastContent = new ToastContent()
            {
                // Construct the visuals of the toast
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        HeroImage = new ToastGenericHeroImage()
                        {
                            Source = "ms-appx:///Assets/sleep.png"
                        },

                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Sleep Reminder"
                            },

                            new AdaptiveText()
                            {
                                Text = "It's pretty late, you should go to sleep now. Goodnight ;)"
                            }
                        },

                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///Assets/crescent-moon.png",
                            HintCrop = ToastGenericAppLogoCrop.None
                        }
                    }
                },

                ActivationType = ToastActivationType.Foreground,

                Scenario = ToastScenario.Reminder
            };


            // Create and schedule the toast notification
            ScheduledToastNotification toast = 
                new ScheduledToastNotification(toastContent.GetXml(), new DateTimeOffset(when))
            {
                // Set expiration time
                ExpirationTime = when.AddHours(8),

                // Identify and categorize the toast
                Id = rand.Next(1, 100000000).ToString(),
                Group = "SleepReminder"
            };

            notifier.AddToSchedule(toast);
        }


        /// <summary>
        /// Register a task to be called when the app is running in the background
        /// </summary>
        /// <param name="taskName">The name (identifier) of the task.</param>
        private async void RegisterBackgroundTask(string taskName, IBackgroundTrigger trigger)
        {
            // If background task is already registered, do nothing
            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(taskName)))
                return;

            // Otherwise request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // Create the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = taskName
            };

            // Assign the specified trigger
            builder.SetTrigger(trigger);

            // And register the task
            BackgroundTaskRegistration registration = builder.Register();
        }


        /// <summary>
        /// Requests extended foreground execution permissions for the application, 
        /// to avoid being suspended by Windows while minimized
        /// </summary>
        private async void RequestExtendedExecution()
        {
            // The previous Extended Execution must be closed before a new one can be requested.
            ClearExtendedExecution();

            session = new ExtendedExecutionForegroundSession
            {
                Reason = ExtendedExecutionForegroundReason.Unconstrained,
                Description = "Background task with periodic reminders"
            };
            session.Revoked += SessionRevoked;
            ExtendedExecutionForegroundResult result = await session.RequestExtensionAsync();
            /*switch (result)
            {
                case ExtendedExecutionForegroundResult.Allowed:
                    break;

                default:
                case ExtendedExecutionForegroundResult.Denied:
                    break;
            }*/
        }


        /// <summary>
        /// Required handler for ExtendedExecutionSession revocation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SessionRevoked(object sender, ExtendedExecutionForegroundRevokedEventArgs args)
        {
            ClearExtendedExecution();
        }


        /// <summary>
        /// Dispose of an extended execution session, releasing resources
        /// </summary>
        private void ClearExtendedExecution()
        {
            if (session != null)
            {
                session.Revoked -= SessionRevoked;
                session.Dispose();
                session = null;
            }
        }

    }

}
