namespace WaterDrops
{
    public sealed class UserData
    {
        public Person Person;
        public Water Water;


        public UserData()
        {
            Person = new Person();
            Water = new Water();
        }

        /// <summary>
        /// Loads the user data from the application's LocalSettings
        /// </summary>
        public void Load()
        {
            Person.Load();
            Water.Load();
        }

        /// <summary>
        /// Saves the user data to the application's LocalSettings
        /// </summary>
        public void Save()
        {
            Person.Save();
            Water.Save();
        }
    }
}
