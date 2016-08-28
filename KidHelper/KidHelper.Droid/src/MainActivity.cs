using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using KidHelper.Core.DB.Tables;
using Android.Graphics;
using KidHelper.Droid.Utils;
using Android.Media;
using System.IO;

namespace KidHelper.Droid
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = false,  Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private int _position = 0;
        private IList<PageItem> _idsList;
        private PageItem _current_item;
        private ImageView _imageView1;
        private ImageView _imageView2;
        private ImageButton _btnPrev;
        private ImageButton _btnNext;
        private MediaPlayer _player;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _idsList = App.Current.Database.Pages.GetItems(PagesTable.FIELDS_GET_ID);

            if (_idsList.Count == 0)
            {
                SetContentView(Resource.Layout.MainEmpty);
                Button btnPageConfig = FindViewById<Button>(Resource.Id.btnPageConfig);
                btnPageConfig.Click += (sender, e) => {
                    Intent intent = new Intent(this, typeof(PagesConfigActivity));
                    StartActivity(intent);
                };
            }
            else
            {
                SetContentView(Resource.Layout.Main);

                _player = new MediaPlayer();

                _imageView1 = FindViewById<ImageView>(Resource.Id.imageView1);
                _imageView2 = FindViewById<ImageView>(Resource.Id.imageView2);

                _btnPrev = FindViewById<ImageButton>(Resource.Id.btnPrev);
                _btnNext = FindViewById<ImageButton>(Resource.Id.btnNext);

                _btnPrev.Click += (sender, e) => {
                    doSlidePage(true);
                };

                _btnNext.Click += (sender, e) => {
                    doSlidePage(false);
                };

                _imageView1.Click += (sender, e) => {
                    if (_current_item != null)
                    {
                        doPlay(_current_item.sound1);
                    }
                };

                _imageView2.Click += (sender, e) => {
                    if (_current_item != null)
                    {
                        doPlay(_current_item.sound2);
                    }
                };

                // read saved data
                if (bundle != null)
                {
                    _position = bundle.GetInt("current_position", 0);
                    _position++;
                }

                doSlidePage(true);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_idsList.Count != 0)
            {
                _imageView1.SetImageBitmap(null);
                _imageView2.SetImageBitmap(null);

                _player.Release();
                _player.Dispose();
                _player = null;
            }

            _idsList.Clear();
            _idsList = null;


            System.GC.Collect(1);
        }

        private void doPlay(byte[] data)
        {
            try
            {
                var cw = new ContextWrapper(this.ApplicationContext);
                var directory = cw.GetDir("testDirectory", FileCreationMode.Private);
                string fileName = string.Concat(directory.AbsolutePath, "/voice.3gpp");


                if (data != null)
                {
                    File.WriteAllBytes(fileName, data);

                    _player.Reset();
                    _player.SetDataSource(fileName);
                    _player.Prepare();
                    _player.Start();
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.need_record), ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private void doSlidePage(bool isBack = false)
        {
            // change position
            if (isBack)
            {
                _position--;
                _position = Math.Max(0, _position);
            } else
            {
                _position++;
                _position = Math.Min(_position, _idsList.Count - 1);
            }

            if (_idsList.Count != 0)
            {
                _current_item = null;
                _current_item = App.Current.Database.Pages.GetItem(_idsList[_position].ID);
                Bitmap bmp = null;

                // stop sound
                _player.Stop();

                // set preview photo
                if (_current_item.image1 != null)
                {
                    bmp = BitmapFactory.DecodeByteArray(_current_item.image1, 0, _current_item.image1.Length);
                    _imageView1.SetImageBitmap(bmp);
                    Util.FreeAndNil(ref bmp);
                }
                else
                {
                    _imageView1.SetImageResource(Android.Resource.Drawable.IcMenuGallery);
                }

                if (_current_item.image2 != null)
                {
                    bmp = BitmapFactory.DecodeByteArray(_current_item.image2, 0, _current_item.image2.Length);
                    _imageView2.SetImageBitmap(bmp);
                    Util.FreeAndNil(ref bmp);
                }
                else
                {
                    _imageView2.SetImageResource(Android.Resource.Drawable.IcMenuGallery);
                }
                //--------------------------------------------------------------------------

                // set border color
                Color bg;
                if (_current_item.color1 != null)
                {
                    bg = Color.ParseColor(_current_item.color1);
                    _imageView1.SetBackgroundColor(bg);
                }

                if (_current_item.color2 != null)
                {
                    bg = Color.ParseColor(_current_item.color2);
                    _imageView2.SetBackgroundColor(bg);
                }
                //--------------------------------------------------------------------------

                System.GC.Collect();
            }

            _btnPrev.Enabled = (_idsList.Count != 0) && !(_position == 0);
            _btnNext.Enabled = (_idsList.Count != 0) && !(_position == (_idsList.Count - 1));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;
            switch (item.ItemId)
            {
                case Resource.Id.mi_config_pages:
                    intent = new Intent(this, typeof(PagesConfigActivity));
                    StartActivity(intent);
                    return true;
                case Resource.Id.mi_guides:
                    intent = new Intent(this, typeof(GuidesActivity));
                    StartActivity(intent);
                    return true;
                case Resource.Id.mi_about:
                    intent = new Intent(this, typeof(AboutActivity));
                    StartActivity(intent);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            // save current position
            outState.PutInt("current_position", _position);
        }
    }
}

