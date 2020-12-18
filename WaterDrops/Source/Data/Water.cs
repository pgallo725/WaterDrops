using System;
using System.Globalization;
using Windows.Storage;


namespace WaterDrops
{

    public class WaterAmountEventArgs : EventArgs
    {
        public int DeltaAmount { get; private set; }

        public WaterAmountEventArgs(int amount)
        {
            this.DeltaAmount = amount;
        }
    }

    public class WaterSettingsEventArgs : EventArgs
    {
        public bool RescheduleTime { get; private set; }

        /// <param name="rescheduleTime">Whether the scheduled time of previous reminders has to be recalculated
        /// (e.g. when ReminderInterval is changed)</param>
        public WaterSettingsEventArgs(bool rescheduleTime)
        {
            this.RescheduleTime = rescheduleTime;
        }
    }


    public class Water
    {
        // Delegate declarations
        public delegate void WaterAmountChangedHandler(Water water, WaterAmountEventArgs args);
        public delegate void WaterSettingsChangedHandler(Water water, WaterSettingsEventArgs args);

        // Event declarations
        public event WaterAmountChangedHandler WaterAmountChanged;
        public event WaterSettingsChangedHandler WaterSettingsChanged;


        private const int DEFAULT_REMINDER_INTERVAL = 30;
        private const int DEFAULT_REMINDER_DELAY = 5;
        private const int DEFAULT_GLASS_SIZE = 250;
        private const int DEFAULT_WATER_AMOUNT = 0;
        private const int DEFAULT_WATER_TARGET = 2000;
        

        /// <summary>
        /// The last time at which the water amount value has been updated
        /// </summary>
        public DateTime Timestamp { get; private set; }


        private int amount = DEFAULT_WATER_AMOUNT;
        /// <summary>
        /// The amount of water drank by the user in the current day, 
        /// written in the application's LocalSettings storage
        /// </summary>
        public int Amount
        {
            get => amount;
            set
            {
                int delta = value - amount;

                amount = value;
                this.Save();

                // Emit the event to inform any listener of the updated value
                WaterAmountChanged?.Invoke(this, new WaterAmountEventArgs(delta));
            }
        }

        private int target = DEFAULT_WATER_TARGET;
        /// <summary>
        /// How much water the user has to drink throughout the day
        /// </summary>
        public int Target 
        { 
            get => target; 
            set
            {
                if (value > 0 && value <= 10000)
                {
                    target = value;
                    this.Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new WaterSettingsEventArgs(false));
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value", "Valid WaterTarget values range from 1 to 10000 mL");
                }
            }
        }

        private int glassSize = DEFAULT_GLASS_SIZE;
        /// <summary>
        /// The amount of water (in mL) contained in a glass
        /// </summary>
        public int GlassSize
        {
            get => glassSize;
            set
            {
                if (value > 0 && value <= 2000)
                {
                    glassSize = value;
                    this.Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new WaterSettingsEventArgs(false));
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value", "Valid GlassSize values range from 1 to 10000 mL");
                }
            }
        }

        private int reminderInterval = DEFAULT_REMINDER_INTERVAL;
        /// <summary>
        /// The time (in minutes) used to schedule periodic drink reminders
        /// </summary>
        public int ReminderInterval
        {
            get => reminderInterval;
            set
            {
                if (value > 0 && value <= 1440)
                {
                    reminderInterval = value;
                    this.Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new WaterSettingsEventArgs(true));
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value", "Valid ReminderInterval values range from 1 to 1440 minutes");
                }
            }
        }

        private int reminderDelay = DEFAULT_REMINDER_DELAY;
        /// <summary>
        /// The time (in minutes) by how much the drink reminder is postponed
        /// </summary>
        public int ReminderDelay
        {
            get => reminderDelay;
            set
            {
                if (value > 0 && value <= 720)
                {
                    reminderDelay = value;
                    this.Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new WaterSettingsEventArgs(true));
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value", "Valid ReminderDelay values range from 1 to 720 minutes");
                }
            }
        }


        /// <summary>
        /// Load the latest water amount (for the current day) and preferences from the application's LocalSettings
        /// </summary>
        public void Load()
        {
            try
            {
                ApplicationDataCompositeValue container = (ApplicationDataCompositeValue)
                    ApplicationData.Current.LocalSettings.Values["Water"];

                // Load values from LocalSettings (if not available default to 0)
                if (container != null)
                {
                    this.reminderInterval = (container["ReminderInterval"] as int?).Value;
                    this.reminderDelay = (container["ReminderDelay"] as int?).Value;
                    this.glassSize = (container["GlassSize"] as int?).Value;
                    this.target = (container["Target"] as int?).Value;
                    this.amount = (container["Amount"] as int?).Value;
                    this.Timestamp = DateTime.ParseExact(container["Timestamp"] as string, "O",
                        CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                    // Reset the values if they refer to a previous day
                    DateTime today = DateTime.UtcNow;
                    if (today.Date > this.Timestamp.Date)
                    {
                        this.amount = DEFAULT_WATER_AMOUNT;
                        this.Timestamp = today;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                // Load default settings and values
                this.reminderInterval = DEFAULT_REMINDER_INTERVAL;
                this.reminderDelay = DEFAULT_REMINDER_DELAY;
                this.glassSize = DEFAULT_GLASS_SIZE;
                this.amount = DEFAULT_WATER_AMOUNT;
                this.target = DEFAULT_WATER_TARGET;
                this.Timestamp = DateTime.UtcNow;
            }

            // Emit events to inform any listener of the updated value and settings
            WaterAmountChanged?.Invoke(this, new WaterAmountEventArgs(amount));
            WaterSettingsChanged?.Invoke(this, new WaterSettingsEventArgs(true));
        }


        /// <summary>
        /// Write the current water amount and preferences (along with a timestamp) to the application's LocalSettings
        /// </summary>
        public void Save()
        {
            // Update the timestamp
            this.Timestamp = DateTime.UtcNow;

            try
            {
                ApplicationDataCompositeValue water = new ApplicationDataCompositeValue()
                {
                    ["ReminderInterval"] = this.reminderInterval,
                    ["ReminderDelay"] = this.reminderDelay,
                    ["GlassSize"] = this.glassSize,
                    ["Target"] = this.target,
                    ["Amount"] = this.amount,
                    ["Timestamp"] = this.Timestamp.ToString("O", CultureInfo.InvariantCulture)
                };

                // Save the value in the local settings storage
                ApplicationData.Current.LocalSettings.Values["Water"] = water;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
