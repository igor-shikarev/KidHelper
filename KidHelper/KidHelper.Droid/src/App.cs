using System;
using System.IO;

using Android.App;
using Android.Util;
using SQLite;
using KidHelper.Core.DB;

namespace KidHelper.Droid
{
    [Application]
    class App : Application
    {
        public static App Current { get; private set; }
        public Database Database { get; private set; }

        private SQLiteConnection _connection;

        public App(IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
            Current = this;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            var sqliteFilename = "data.db3";
            string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(libraryPath, sqliteFilename);
            _connection = new SQLiteConnection(path);

            Database = new Database(_connection);

            //Log.Debug("ish", "app_creat");
        }
    }
}