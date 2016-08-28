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

namespace KidHelper.Droid.Adapters
{
    class ColorArrayAdapter : BaseAdapter<string>, ISpinnerAdapter
    {
        public static readonly string[] COLORS = { "#FFFFFF", "#0000ff", "#ff0000", "#000000", "#00ff00" };
        private Activity _context = null;
        private IList<string> _items = new List<string>();

        public ColorArrayAdapter(Activity context) : base ()
		{
            _context = context;

            for (var i = 0; i < ColorArrayAdapter.COLORS.Length; i++)
            {
                _items.Add(ColorArrayAdapter.COLORS[i]);
            }
        }

        public override string this[int position]
        {
            get { return _items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return _items.Count; }
        }

        private View getView(int position, View convertView, ViewGroup parent, bool isDropDown)
        {
            int layoutId = (isDropDown ? Android.Resource.Layout.SimpleSpinnerDropDownItem : Android.Resource.Layout.SimpleSpinnerItem);

            var view = convertView ?? _context.LayoutInflater.Inflate(layoutId, parent, false);

            TextView lblText = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            Android.Graphics.Color bg = Android.Graphics.Color.ParseColor(_items[position]);
            lblText.SetBackgroundColor(bg);

            return view;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return getView(position, convertView, parent, false);
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            return getView(position, convertView, parent, true);
        }
    }
}