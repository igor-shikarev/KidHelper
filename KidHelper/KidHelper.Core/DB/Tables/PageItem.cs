using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace KidHelper.Core.DB.Tables
{
    public class PageItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        // порядок сортировки
        [Indexed]
        public int order { get; set; }
        // цвет левой стороны #FFFFFF
        public string color1 { get; set; }
        // цвет правой стороны #FFFFFF
        public string color2 { get; set; }
        // фотка левой стороны
        public byte[] image1 { get; set; }
        // фотка правой стороны
        public byte[] image2 { get; set; }
        // звук левой стороны
        public byte[] sound1 { get; set; }
        // звук правой стороны
        public byte[] sound2 { get; set; }
        // превью фотки левой стороны
        public byte[] thumb1 { get; set; }
        // превью фотки правой стороны
        public byte[] thumb2 { get; set; }
    }
}
