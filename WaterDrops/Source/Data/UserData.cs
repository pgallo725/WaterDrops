using System;
using System.Threading;

namespace WaterDrops
{
    public sealed class UserData
    {
        public Person Person;
        public Water Water;


        // Synchronization primitive that allows threads to wait until the settings are properly loaded
        // before running their initialization logic (e.g. UI controls)
        private readonly ManualResetEventSlim userLoadedSyncEvent = new ManualResetEventSlim();

        /// <summary>
        /// Stops the calling thread until the Person class has been fully initialized,
        /// loading values from the application's LocalSettings
        /// </summary>
        public void WaitUntilLoaded()
        {
            userLoadedSyncEvent.Wait(500);
        }


        public UserData()
        {
            Person = new Person();
            Water = new Water();
        }

        public void Load()
        {
            Person.Load();
            Water.Load();

            // Signal the completion of User data loading to potentially waiting threads
            userLoadedSyncEvent.Set();
        }

        public void Save()
        {
            Person.Save();
            Water.Save();
        }
    }
}
