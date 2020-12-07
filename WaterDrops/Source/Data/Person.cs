using System;
using System.Collections.Generic;
using Windows.Storage;

namespace WaterDrops
{
    public sealed class Person
    {
        // Delegate declaration
        public delegate void PersonChangedHandler(Person person, EventArgs args);

        // Event declaration
        public event PersonChangedHandler PersonChanged;


        public enum HealthStatusType
        {
            Undefined,
            Underweight,
            Healthy,
            Overweight,
            Obese,
            ExtremelyObese
        }

        public enum GenderType
        {
            Male,
            Female
        }

        private GenderType gender;
        /// <summary>
        /// The gender selected by the user in the BMI Calculator form
        /// </summary>
        public GenderType Gender { get => gender; set { gender = value; Save(); } }

        private int age;
        /// <summary>
        /// The age of the user, as specified in the BMI Calculator form
        /// </summary>
        public int Age { get => age; set { age = value; Save(); } }

        private float weight;
        /// <summary>
        /// The user's body weight, as specified in the BMI Calculator form
        /// </summary>
        public float Weight { get => weight; set { weight = value; Save(); } }

        private float height;
        /// <summary>
        /// The user's height, as specified in the BMI Calculator form
        /// </summary>
        public float Height { get => height; set { height = value; Save(); } }

        /// <summary>
        /// A metric representative of the user's body health status, based on the Body Mass Index value
        /// </summary>
        public HealthStatusType HealthStatus
        {
            get
            {
                float bmi = BodyMassIndex;
                if (bmi == 0.0f)
                    return HealthStatusType.Undefined;
                else if (bmi < 18.5)
                    return HealthStatusType.Underweight;
                else if (bmi < 25)
                    return HealthStatusType.Healthy;
                else if (bmi < 30)
                    return HealthStatusType.Overweight;
                else if (bmi < 40)
                    return HealthStatusType.Obese;
                else return HealthStatusType.ExtremelyObese;
            }
        }

        /// <summary>
        /// The Body Mass Index (BMI) value, computed using the user-provided body information
        /// </summary>
        public float BodyMassIndex { get => height > 0 ? weight / (height * height) : 0.0f; }

        /// <summary>
        /// The amount of water that a person should be drinking in a day, based on gender and age
        /// NOTE: not currently used, WaterTarget is hard-coded as 2000 mL
        /// </summary>
        public int WaterTarget
        {
            get
            {
                if (age <= 3)
                {
                    return 1200;
                }
                else if (age <= 6)
                {
                    return 1600;
                }
                else if (age <= 10)
                {
                    return 1800;
                }
                else if (age <= 14)
                {
                    return (gender == GenderType.Male) ? 2100 : 1900;
                }
                else    // Teenagers (14+), adults and elders
                {
                    return (gender == GenderType.Male) ? 2500 : 2000;
                }
            }
        }


        /// <summary>
        /// Load user data from the local application storage.
        /// If one or more fields are not found, default values are loaded
        /// </summary>
        public void Load()
        {
            try
            {
                // Access local ApplicationDataContainer for settings
                ApplicationDataCompositeValue bodyInfo = (ApplicationDataCompositeValue)
                    ApplicationData.Current.LocalSettings.Values["BodyInformation"];

                if (bodyInfo != null)
                {
                    this.gender = (GenderType)bodyInfo["Gender"];
                    this.age = (int)bodyInfo["Age"];
                    this.weight = (float)bodyInfo["Weight"];
                    this.height = (float)bodyInfo["Height"];

                    // Fire the PersonChanged event to inform any listener about the loaded data
                    PersonChanged?.Invoke(this, EventArgs.Empty);

                    return;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            // Load default/null settings
            this.gender = GenderType.Male;
            this.age = 0;
            this.weight = 0.0f;
            this.height = 0.0f;

            // Write the loaded default settings and inform any listener of their values
            Save();
        }


        /// <summary>
        /// Write user data to the application's local storage
        /// </summary>
        public void Save()
        {
            try
            {
                // Save body information to local ApplicationDataContainer
                ApplicationDataCompositeValue bodyInfo = new ApplicationDataCompositeValue
                {
                    ["Gender"] = (int)this.gender,
                    ["Age"] = this.age,
                    ["Weight"] = this.weight,
                    ["Height"] = this.height
                };

                ApplicationData.Current.LocalSettings.Values["BodyInformation"] = bodyInfo;

                // Inform any listener that pending changes have been confirmed
                PersonChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
