using System;
using System.Globalization;
using Windows.Storage;


namespace WaterDrops
{

    public class WaterEventArgs : EventArgs
    {
        public int DeltaAmount { get; private set; }

        public WaterEventArgs(int amount)
        {
            this.DeltaAmount = amount;
        }
    }


    public class Water
    {
        // Delegate declaration
        public delegate void WaterChangedHandler(Water water, WaterEventArgs args);

        // Event declaration
        public event WaterChangedHandler WaterChanged;


        /// <summary>
        /// The last time at which the water amount value has been updated
        /// </summary>
        public DateTime Timestamp { get; private set; }


        private int amount;
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
                WaterChanged?.Invoke(this, new WaterEventArgs(delta));
            }
        }

        public int PendingAmount { get; set; } = 0;

        /// <summary>
        /// How much water the user has to drink throughout the day
        /// </summary>
        public const int Target = 2000;

        /// <summary>
        /// The amount of water (in mL) contained in a glass
        /// </summary>
        public const int GlassSize = 250;


        /// <summary>
        /// Load the latest water amount (for the current day) from the application's LocalSettings
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
                    this.amount = (int)container["Amount"];
                    this.Timestamp = DateTime.ParseExact(container["Timestamp"] as string, "O",
                        CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                    // Reset the values if they refer to a previous day
                    DateTime today = DateTime.UtcNow;
                    if (today.Date > this.Timestamp.Date)
                    {
                        this.amount = 0;
                        this.Timestamp = today;
                    }
                }
                else
                {
                    this.amount = 0;
                    this.Timestamp = DateTime.UtcNow;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                this.amount = 0;
                this.Timestamp = DateTime.UtcNow;
            }

            // Emit the event to inform any listener of the updated value
            WaterChanged?.Invoke(this, new WaterEventArgs(amount));
        }


        /// <summary>
        /// Write the current water amount (along with a timestamp) to the application's local settings
        /// </summary>
        public void Save()
        {
            // Update the timestamp
            this.Timestamp = DateTime.UtcNow;

            try
            {
                ApplicationDataCompositeValue water = new ApplicationDataCompositeValue()
                {
                    ["Amount"] = this.Amount,
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
