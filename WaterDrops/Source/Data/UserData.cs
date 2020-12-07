using System;
using System.Collections.Generic;

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

        public void Load()
        {
            Person.Load();
            Water.Load();
        }

        public void Save()
        {
            Person.Save();
            Water.Save();
        }
    }
}
