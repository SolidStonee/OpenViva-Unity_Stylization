﻿namespace Viva
{


    [System.Serializable]
    public class Language
    {

        [System.Serializable]
        public class ManualEntry
        {
            public string manualTitle;
            public string[] pages;
        }

        public string name;
        public ManualEntry[] manualEntries;
    }

}