using System;
using System.Linq;
using Windows.Foundation;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.ExtendedExecution.Foreground;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace WaterDrops
{
    /// <summary>
    /// Specific application behaviour in addition to the default Application class
    /// </summary>
    sealed partial class App : Application
    {
        // User data storage object
        internal static UserData User { get; } = new UserData();

        // Application settings manager
        internal static Settings Settings { get; } = new Settings();

        // Notifications manager
        private readonly NotificationManager notificationManager = new NotificationManager();

        // Extended execution session handle
        private ExtendedExecutionForegroundSession session;


        /// <summary>
        /// Initializes the singleton Application object. It's the first line of the generated code
        /// and, as such, it is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        /// <summary>
        /// Called when the application is regularly launched by the end user. 
        /// At the application startup other entry points will be used to open a specific file.
        /// </summary>
        /// <param name="e">Details about the request and the startup process</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e.PrelaunchActivated)
                return;

            OnLaunchedOrActivated(e);
        }

        /// <summary>
        /// Called when the application is activated by the user clicking on a toast notification body
        /// </summary>
        /// <param name="e">Details about the request and the activation process</param>
        protected override void OnActivated(IActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        /// <summary>
        /// Initializes the application root frame and tasks, handles different kinds of activation,
        /// loads user data and finally activates the app window.
        /// </summary>
        /// <param name="e">Details about the type and arguments of the application's startup</param>
        private void OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            // Initialize the root frame (only once)
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a frame that will act as navigation context
                rootFrame = new Frame();

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


            // Set the user's preferred application color theme at startup
            this.RequestedTheme = Settings.RequestedApplicationTheme;

            // Set ApplicationView properties to define title bar look and window size
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            applicationView.SetPreferredMinSize(new Size(700, 420));

            // Make sure that the current window is set as active
            Window.Current.Activate();

            // Initialize notifications manager and setup daily reminders
            notificationManager.Initialize();

            // Hook the user/settings update events to the NotificationManager scheduling function
            Settings.NotificationsSettingChanged += (sender, args) => notificationManager.UpdateNotificationSchedule(false);
            User.Water.WaterSettingsChanged += (sender, args) => notificationManager.UpdateNotificationSchedule(args.RescheduleTime);

            // Register the application's background tasks
            RegisterBackgroundTask("ToastAction", new ToastNotificationActionTrigger());
            RegisterBackgroundTask("ReminderWatchdog", new TimeTrigger(15, false));

            // Request extended execution capabilities for the application
            RequestExtendedExecution();

            // And finally, if the navigation stack is not being resumed, load the MainPage 
            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }
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
                        }
                        else if (details.Argument == "postpone")
                        {
                            // Postpone the same notification to a few minutes from now
                            notificationManager.PostponeDrinkReminder();
                        }
                    }
                    break;

                case "ReminderWatchdog":

                    if (DateTime.Now <= DateTime.Today.AddMinutes(30))
                    {
                        // Reset notifications and water progress after midnight
                        User.Water.Amount = 0;
                        notificationManager.UpdateNotificationSchedule(true);
                    }
                    else
                    {
                        // Periodically check that reminders are being properly scheduled
                        notificationManager.CheckNotificationSchedule();
                    }
                    break;

                default:
                    throw new Exception("Unexpected background task activation: " + args.TaskInstance.Task.Name);
            }

            deferral.Complete();
        }

        /// <summary>
        /// Called when the execution of the application is suspended. The state is saved
        /// without knowing if the application will be terminated or resumed properly.
        /// </summary>
        /// <param name="sender">Source of the suspension request</param>
        /// <param name="e">Details about the suspension request</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // NOTHING TO DO

            deferral.Complete();
        }


        /// <summary>
        /// Register a task to be called when the app is running in the background
        /// </summary>
        /// <param name="taskName">The name (identifier) of the task</param>
        /// <param name="trigger">The type of trigger that has to be registered for the background task</param>
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
        }

        /// <summary>
        /// Required handler for ExtendedExecutionSession revocation
        /// </summary>
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
