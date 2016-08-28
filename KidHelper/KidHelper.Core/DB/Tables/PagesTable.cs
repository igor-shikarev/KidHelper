using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace KidHelper.Core.DB.Tables
{
    public class PagesTable
    {
        // query ALL fields
        public static string FIELDS_GET_ALL = "*";
        // query ID
        public static string FIELDS_GET_ID = "ID";
        // query ID, thumb1, thumb2
        public static string FIELDS_GET_THUMB = "ID, thumb1, thumb2";

        private SQLiteConnection _connection;
        private object _locker;

        public PagesTable(SQLiteConnection connection, object locker)
        {
            _connection = connection;
            _locker = locker;

            // создание таблиц
            _connection.CreateTable<PageItem>();
        }

        public IList<PageItem> GetItems(string fields = "*")
        {
            lock (_locker)
            {
                var query = _connection.Query<PageItem>("SELECT " + fields + " FROM [PageItem] ORDER BY [order]");
                return query.ToList();
            }
        }

        public PageItem GetItem(int id)
        {
            lock (_locker)
            {
                return _connection.Table<PageItem>().FirstOrDefault(row => row.ID == id);
            }
        }

        public int Save(PageItem item)
        {
            lock (_locker)
            {
                if (item.ID != 0)
                {
                    _connection.Update(item);
                    return item.ID;
                }
                else
                {
                    return _connection.Insert(item);
                }
            }
        }

        public int Delete(int id)
        {
            lock (_locker)
            {
                return _connection.Delete<PageItem>(id);
            }
        }
    }
}
