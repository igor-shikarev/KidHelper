using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using KidHelper.Core.DB.Tables;

namespace KidHelper.Core.DB
{
    public class Database
    {
        static object locker = new object();

        private SQLiteConnection _connection;
        public PagesTable Pages { get; private set; }

        public Database(SQLiteConnection connection)
        {
            _connection = connection;

            // создание таблиц
            Pages = new PagesTable(connection, locker);
        }
    }
}
