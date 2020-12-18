using System;
using System.Linq;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;      // Notifications library


namespace WaterDrops
{ 
    public class NotificationManager
    {
        // Random number generator
        private readonly Random random;

        // Windows toast notifications manager
        private readonly ToastNotifier notifier;

        // Notification status variables
        private DateTime nextReminderTime = DateTime.Today;
        private string nextReminderTag = "Regular";


        public NotificationManager()
        {
            // Initialize randomizer
            random = new Random();

            // Create a manager for toast notifications
            notifier = ToastNotificationManager.CreateToastNotifier();

            // Register an handler for the WaterAmountChanged event
            App.User.Water.WaterAmountChanged += OnWaterAmountChanged;
        }


        /// <summary>
        /// Schedule the reminders for the rest of the day (initializes notification scheduling at application launch)
        /// More specifically, it schedules the first drink reminder and the sleep reminder at midnight
        /// </summary>
        public void Initialize()
        {
            // Clear any pending notification
            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
            {
                notifier.RemoveFromSchedule(notification);
            }

            if (App.Settings.NotificationsEnabled)
            {
                if (App.User.Water.Amount < App.User.Water.Target)
                {
                    // Schedule the next drink reminder, either at 8:00 in the morning 
                    // or in User.Water.ReminderDelay minutes from now if it's passed that time
                    nextReminderTag = "Regular";
                    nextReminderTime = new[] {
                        DateTime.Today.AddHours(8),
                        DateTime.Now.AddMinutes(App.User.Water.ReminderDelay)
                    }.Max();
                    ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                }

                // Schedule the next sleep reminder at midnight
                ScheduleSleepReminder(DateTime.Today.AddDays(1));
            }
        }


        private void OnWaterAmountChanged(Water water, WaterAmountEventArgs args)
        {
            // If the user just registered a drink that's large enough, the next reminder is rescheduled
            if (args.DeltaAmount > water.GlassSize/2)
            {
                ScheduleNextDrinkReminder();
            }
        }


        /// <summary>
        /// Updates the scheduling and contents of reminders when some settings are changed
        /// </summary>
        /// <param name="rescheduleTime">Whether the scheduled time of previous reminders has to be recalculated
        /// (e.g. when ReminderInterval is changed)</param>
        public void UpdateNotificationSchedule(bool rescheduleTime)
        {
            // Remove previously scheduled notifications
            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
            {
                notifier.RemoveFromSchedule(notification);
            }

            if (App.Settings.NotificationsEnabled)
            {
                if (App.User.Water.Amount < App.User.Water.Target)
                {
                    // If the previous reminder time is no longer valid
                    if (rescheduleTime || DateTime.Now >= nextReminderTime)
                    {
                        // Schedule a new reminder in User.Water.ReminderDelay minutes
                        // or at 8:00 in the morning if the day hasn't yet begun
                        nextReminderTime = new[] {
                            DateTime.Today.AddHours(8),
                            DateTime.Now.AddMinutes(App.User.Water.ReminderInterval)
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


        /// <summary>
        /// Can be run at any time to make sure that drink reminders are scheduled correctly,
        /// and fixes any inconsistency by updating the notification schedule accordingly
        /// </summary>
        public void CheckNotificationSchedule()
        {
            if (App.Settings.NotificationsEnabled)
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
                        nextReminderTime.AddMinutes(App.User.Water.ReminderDelay)
                    }.Max();

                    ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
                }
            }
            else
            {
                // Remove all scheduled reminders
                foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
                {
                    notifier.RemoveFromSchedule(notification);
                }
            }
        }


        /// <summary>
        /// Re-schedules the current drink reminder in ReminderDelay minutes from now
        /// </summary>
        public void PostponeDrinkReminder()
        {
            // Remove any other scheduled drink reminder
            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
            {
                if (notification.Group == "DrinkReminder")
                    notifier.RemoveFromSchedule(notification);
            }

            if (App.Settings.NotificationsEnabled && App.User.Water.Amount < App.User.Water.Target)
            {
                // Postpone the same notification to User.Water.ReminderDelay minutes from now
                nextReminderTag = "Postponed";
                nextReminderTime = DateTime.Now.AddMinutes(App.User.Water.ReminderDelay);
                ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
            }
        }

        /// <summary>
        /// Schedules a new drink reminder in ReminderInterval minutes from now
        /// </summary>
        public void ScheduleNextDrinkReminder()
        {
            // Remove any other scheduled drink reminder
            foreach (ScheduledToastNotification notification in notifier.GetScheduledToastNotifications())
            {
                if (notification.Group == "DrinkReminder")
                    notifier.RemoveFromSchedule(notification);
            }

            if (App.Settings.NotificationsEnabled && App.User.Water.Amount < App.User.Water.Target)
            {
                // And schedule the next one in User.Water.ReminderInterval minutes
                nextReminderTag = "Regular";
                nextReminderTime = DateTime.Now.AddMinutes(App.User.Water.ReminderInterval);
                ScheduleDrinkReminder(nextReminderTime, nextReminderTag);
            }
        }


        /// <summary>
        /// Create and schedule a toast notification to remind the user to drink
        /// </summary>
        /// <param name="when">DateTime representation of the moment when the reminder has to be shown to the user</param>
        /// <param name="tag">Internal notification tag: "Regular" or "Postponed"</param>
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
                                Text = "Psst... hey you! What about drinking a nice glass of fresh water ?"
                            }
                        },

                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///Assets/Images/water_glass.png",
                            HintCrop = ToastGenericAppLogoCrop.None
                        }
                    }
                },

                ActivationType = ToastActivationType.Foreground,

                Scenario = App.Settings.NotificationSetting == Settings.NotificationLevel.Alarm ?
                    ToastScenario.Alarm : ToastScenario.Reminder,

                // Add buttons to the toast body
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Yes (" + App.User.Water.GlassSize.ToString() + " mL)", "confirm")
                        {
                            ActivationType = ToastActivationType.Background
                        },
                        new ToastButton("Not yet (" + App.User.Water.ReminderDelay.ToString() + " mins)", "postpone")
                        {
                            ActivationType = ToastActivationType.Background
                        }
                    }
                },

                // Specify a custom notification sound effect
                Audio = new ToastAudio()
                {
                    Src = (App.Settings.NotificationSetting == Settings.NotificationLevel.Normal) ?
                        new Uri("ms-appx:///Assets/Sounds/waterdrop_sound.wav") :
                        new Uri("ms-appx:///Assets/Sounds/waterdrops_loop.wav"),
                    Loop = (App.Settings.NotificationSetting == Settings.NotificationLevel.Alarm)
                }
            };

            // Create and schedule the toast notification
            ScheduledToastNotification toast =
                new ScheduledToastNotification(toastContent.GetXml(), new DateTimeOffset(when))
                {
                    // Set expiration time
                    ExpirationTime = (tag == "Postponed") ?
                    DateTime.Now.AddMinutes(App.User.Water.ReminderDelay) :
                    DateTime.Now.AddMinutes(App.User.Water.ReminderInterval),

                    // Identify and categorize the toast
                    Id = random.Next(1, int.MaxValue).ToString(),
                    Group = "DrinkReminder",
                    Tag = tag
                };

            notifier.AddToSchedule(toast);
        }


        /// <summary>
        /// Build and schedule a toast notification reminding the user to go to sleep
        /// </summary>
        /// <param name="when">DateTime object that specifies when the reminder has to be shown to the user</param>
        private void ScheduleSleepReminder(DateTime when)
        {
            // Define the appearance and behaviour of the toast notification
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        HeroImage = new ToastGenericHeroImage()
                        {
                            Source = "ms-appx:///Assets/Images/sleep.png"
                        },

                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Sleep Reminder"
                            },

                            new AdaptiveText()
                            {
                                Text = "It's pretty late, you should be going to sleep now. Goodnight ;)"
                            }
                        },

                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///Assets/Images/crescent-moon.png",
                            HintCrop = ToastGenericAppLogoCrop.None
                        }
                    }
                },

                Audio = new ToastAudio()
                {
                    Src = new Uri("ms-appx:///Assets/Sounds/sleep_soothing_sound.wav"),
                    Loop = false
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
                    Id = random.Next(1, int.MaxValue).ToString(),
                    Group = "SleepReminder"
                };

            notifier.AddToSchedule(toast);
        }
    }
}
