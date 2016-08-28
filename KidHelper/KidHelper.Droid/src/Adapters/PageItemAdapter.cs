using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using KidHelper.Core.DB.Tables;
using Android.Util;
using KidHelper.Droid.Utils;
using System.Threading.Tasks;

namespace KidHelper.Droid.Adapters
{
    public class PageItemAdapter : BaseAdapter<PageItem>
    {
        private Activity _context = null;
        private IList<PageItem> _items = new List<PageItem>();

        public PageItemAdapter(Activity context, IList<PageItem> items) : base()
        {
            _context = context;
            _items = items;
        }

        public override PageItem this[int position]
        {
            get
            {
                return _items[position];
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return _items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.PageListViewItem, parent, false);

            ImageView imgView1 = view.FindViewById<ImageView>(Resource.Id.imgImage1);
            ImageView imgView2 = view.FindViewById<ImageView>(Resource.Id.imgImage2);
            TextView lblPos = view.FindViewById<TextView>(Resource.Id.lblPos);

            Bitmap bmp;

            if (this[position].thumb1 != null)
            {
                bmp = BitmapFactory.DecodeByteArray(this[position].thumb1, 0, this[position].thumb1.Length);
                imgView1.SetImageBitmap(bmp);
                Util.FreeAndNil(ref bmp);
            }

            if (this[position].thumb2 != null)
            {
                bmp = BitmapFactory.DecodeByteArray(this[position].thumb2, 0, this[position].thumb2.Length);
                imgView2.SetImageBitmap(bmp);
                Util.FreeAndNil(ref bmp);
            }

            lblPos.Text = (position + 1).ToString() + ".";

            return view;
        }
    }
}