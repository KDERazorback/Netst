using System;
using System.IO;
using com.razorsoftware.SettingsLib;

namespace Netst
{
    public static class Settings
    {
        public static string DefaultFilename = @".\Netst.settings.xml";

        private static Persistent _persistent;

        public static VolatileSettings Volatile = new VolatileSettings();

        public static Persistent Persistent
        {
            get
            {
                if (_persistent == null)
                    _persistent = new Persistent();

                return _persistent;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _persistent = value;
                
            }
        }

        public static void Save()
        {
            SaveTo(DefaultFilename);
        }

        public static void Load()
        {
            LoadFrom(DefaultFilename);
        }

        public static void LoadFrom(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                Persistent = (Persistent)Document.LoadFromStream(fs, typeof(Persistent));
        }

        public static void SaveTo(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                Persistent.SaveToStream(fs);
        }
    }
}
