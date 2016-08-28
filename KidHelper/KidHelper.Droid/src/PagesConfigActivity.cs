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
using KidHelper.Core.DB.Tables;
using KidHelper.Droid.Adapters;

namespace KidHelper.Droid
{
    [Activity(Label = "@string/title_pages_config", Icon = "@drawable/icon", ParentActivity = typeof(MainActivity))]
    public class PagesConfigActivity : ListActivity
    {
        private IList<PageItem> _items;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PagesConfig);

            ListView.ItemLongClick += doListItemLongClick;

            Button btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
            btnAdd.Click += (sender, e) => {
                Intent intent = new Intent(this, typeof(PageEditActivity));
                intent.PutExtra("page_id", 0);
                StartActivity(intent);
            };
        }

        private void loadItems()
        {
            ListAdapter = null;
            _items = App.Current.Database.Pages.GetItems(PagesTable.FIELDS_GET_THUMB);
            ListAdapter = new PageItemAdapter(this, _items);
        }

        protected override void OnResume()
        {
            base.OnResume();

            loadItems();
        }

        private void doListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            e.Handled = true;

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(GetString(Resource.String.dialog_page_delete_title));
            alert.SetMessage(GetString(Resource.String.dialog_page_delete_text));
            alert.SetPositiveButton(GetString(Resource.String.delete), (senderAlert, args) => {
                App.Current.Database.Pages.Delete(_items[e.Position].ID);
                Toast.MakeText(this, GetString(Resource.String.dialog_page_delete_postfix), ToastLength.Short).Show();

                this.loadItems();
            });

            alert.SetNegativeButton(GetString(Resource.String.cancel), (senderAlert, args) => {
                //
            });

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);

            Intent intent = new Intent(this, typeof(PageEditActivity));
            intent.PutExtra("page_id", _items[position].ID);
            StartActivity(intent);
        }
    }
}