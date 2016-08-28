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
using System.IO;
using Android.Database;
using Android.Graphics;
using Android.Media;
using KidHelper.Droid.Adapters;
using Android.Util;
using KidHelper.Droid.Utils;
using System.Threading.Tasks;
using Android.Provider;

namespace KidHelper.Droid
{
    [Activity(Label = "@string/title_edit", Icon = "@drawable/icon", ParentActivity = typeof(PagesConfigActivity))]
    public class PageEditActivity : Activity
    {
        private PageItem _item;
        private ImageView _imgView1;
        private ImageView _imgView2;
        private Button _btnRecord1;
        private Button _btnRecord2;
        private ImageButton _btnPlay1;
        private ImageButton _btnPlay2;
        private Spinner _spnColor1;
        private Spinner _spnColor2;
        private MediaRecorder _recorder;
        private MediaPlayer _player;

        private void doSelectImageClick(int resId)
        {
            Intent intent = new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri);
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, GetString(Resource.String.select_photo)), resId);
        }

        private string GetPathToImage(Android.Net.Uri uri)
        {
            string path = null;
            // The projection contains the columns we want to return in our query.
            string[] projection = new[] { Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Data };
            using (ICursor cursor = ContentResolver.Query(uri, projection, null, null, null))
            {
                if (cursor != null)
                {
                    int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Data);
                    cursor.MoveToFirst();
                    path = cursor.GetString(columnIndex);
                    cursor.Close();
                }
            }

            return path;
        }

        /**
         * Установка содержимого ImageView
         * 
         */
        private void setImageViewData(ImageView v, byte[] data)
        {
            if (data != null)
            {
                Bitmap bmp = BitmapFactory.DecodeByteArray(data, 0, data.Length);
                v.SetImageBitmap(bmp);
                Util.FreeAndNil(ref bmp);
            } else
            {
                v.SetImageResource(Android.Resource.Drawable.IcMenuGallery);
            }
        }

        /**
         * Запрос временного файла для голоса
         * 
         */
        private string getAudioTmpFileName(string prefix = "")
        {
            var cw = new ContextWrapper(this.ApplicationContext);
            var directory = cw.GetDir("testDirectory", FileCreationMode.Private);
            return string.Concat(directory.AbsolutePath, "/" + prefix + "voice.3gpp");
        }
        
        private void doRecorderStart(object sender, EventArgs e)
        {
            try
            {
                _recorder.SetAudioSource(AudioSource.Mic);
                _recorder.SetOutputFormat(OutputFormat.ThreeGpp);
                _recorder.SetAudioEncoder(AudioEncoder.AmrNb);
                _recorder.SetOutputFile(getAudioTmpFileName());
                //_recorder.SetMaxDuration(2 * 1000);
                _recorder.Prepare();
                _recorder.Start();

                // запрещаем другие кнопки
                _btnRecord1.Enabled = false;
                _btnRecord2.Enabled = false;
                _btnPlay1.Enabled = false;
                _btnPlay2.Enabled = false;

                (sender as Button).Enabled = true;
                (sender as Button).Text = GetString(Resource.String.stop_record);
                (sender as Button).Click -= doRecorderStart;
                (sender as Button).Click += doRecorderStop;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private void doRecorderStop(object sender, EventArgs e)
        {
            try
            {
                _recorder.Stop();
                _recorder.Reset();

                _btnRecord1.Enabled = true;
                _btnRecord2.Enabled = true;
                _btnPlay1.Enabled = true;
                _btnPlay2.Enabled = true;

                (sender as Button).Text = GetString(Resource.String.record_voice);
                (sender as Button).Click -= doRecorderStop;
                (sender as Button).Click += doRecorderStart;

                // запрос бинарных данных
                if ((sender as Button) == _btnRecord1)
                {
                    _item.sound1 = File.ReadAllBytes(getAudioTmpFileName());
                }

                if ((sender as Button) == _btnRecord2)
                {
                    _item.sound2 = File.ReadAllBytes(getAudioTmpFileName());
                }
            } catch (Exception ex)
            {
                Toast.MakeText(this, "Error: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private void doPlay(object sender, EventArgs e)
        {
            try
            {
                byte[] data = null;

                if ((sender as ImageButton) == _btnPlay1)
                {
                    data = _item.sound1;
                }

                if ((sender as ImageButton) == _btnPlay2)
                {
                    data = _item.sound2;
                }

                if (data != null)
                {
                    string fileName = getAudioTmpFileName("play_");
                    File.WriteAllBytes(fileName, data);

                    _player.Reset();
                    _player.SetDataSource(fileName);
                    _player.Prepare();
                    _player.Start();
                } else
                {
                    Toast.MakeText(this, GetString(Resource.String.need_record), ToastLength.Short).Show();
                }
            } catch (Exception ex)
            {
                Toast.MakeText(this, "Error: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private async void doScaledBitmapData(Android.Net.Uri uri, int idx)
        {
            byte[] img_data;
            byte[] thumb_data;

            try
            {
                int thumb_wh = Util.dp2px(this, (int)this.Resources.GetDimension(Resource.Dimension.item_thumb_size));
                DisplayMetrics displayMetrics = this.Resources.DisplayMetrics;

                System.IO.Stream tmp_data = ContentResolver.OpenInputStream(uri);
                MemoryStream data = new MemoryStream();
                tmp_data.CopyTo(data);
                Util.FreeAndNil(ref tmp_data);

                img_data = await Util.getScaledBitmapData(this, data.ToArray(), displayMetrics.WidthPixels, displayMetrics.HeightPixels);
                thumb_data = await Util.getScaledBitmapData(this, data.ToArray(), thumb_wh, thumb_wh);

                displayMetrics.Dispose();
                displayMetrics = null;

                Util.FreeAndNil(ref data);

                switch (idx)
                {
                    case 1:
                        _item.image1 = img_data;
                        _item.thumb1 = thumb_data;
                        setImageViewData(_imgView1, _item.image1);
                        break;
                    case 2:
                        _item.image2 = img_data;
                        _item.thumb2 = thumb_data;
                        setImageViewData(_imgView2, _item.image2);
                        break;
                }
            } catch (Exception ex)
            {
                Toast.MakeText(this, GetString(Resource.String.photo_error) +  " [" + ex.Message + "]", ToastLength.Short).Show();
            }

            img_data = null;
            thumb_data = null;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PageEdit);

            ActionBar.SetDisplayHomeAsUpEnabled(false);

            _recorder = new MediaRecorder();
            _player = new MediaPlayer();

            _imgView1 = FindViewById<ImageView>(Resource.Id.imageView1);
            _imgView2 = FindViewById<ImageView>(Resource.Id.imageView2);

            Button btnSave = FindViewById<Button>(Resource.Id.btnSave);
            btnSave.Click += (sender, e) => {

                _item.color1 = _spnColor1.GetItemAtPosition(_spnColor1.SelectedItemPosition).ToString();
                _item.color2 = _spnColor2.GetItemAtPosition(_spnColor2.SelectedItemPosition).ToString();

                App.Current.Database.Pages.Save(_item);
                Finish();
            };

            Button btnCancel = FindViewById<Button>(Resource.Id.btnCancel);
            btnCancel.Click += (sender, e) => {
                Finish();
            };

            Button btnSelectImage1 = FindViewById<Button>(Resource.Id.btnSelectImage1);
            btnSelectImage1.Click += (sender, e) => {
                doSelectImageClick(Resource.Id.btnSelectImage1);
            };

            Button btnSelectImage2 = FindViewById<Button>(Resource.Id.btnSelectImage2);
            btnSelectImage2.Click += (sender, e) => {
                doSelectImageClick(Resource.Id.btnSelectImage2);
            };

            _btnRecord1 = FindViewById<Button>(Resource.Id.btnRecord1);
            _btnRecord2 = FindViewById<Button>(Resource.Id.btnRecord2);

            _btnRecord1.Click += doRecorderStart;
            _btnRecord2.Click += doRecorderStart;

            _btnPlay1 = FindViewById<ImageButton>(Resource.Id.btnPlay1);
            _btnPlay2 = FindViewById<ImageButton>(Resource.Id.btnPlay2);

            _btnPlay1.Click += doPlay;
            _btnPlay2.Click += doPlay;

            int page_id = Intent.GetIntExtra("page_id", 0);

            if (page_id == 0)
            {
                _item = new PageItem();

                _item.color1 = ColorArrayAdapter.COLORS[0];
                _item.color2 = ColorArrayAdapter.COLORS[0];
            } else
            {
                _item = App.Current.Database.Pages.GetItem(page_id);

            }

            // загрузка фото
            setImageViewData(_imgView1, _item.image1);
            setImageViewData(_imgView2, _item.image2);

            _spnColor1 = FindViewById<Spinner>(Resource.Id.spnColor1);
            _spnColor1.Adapter = new ColorArrayAdapter(this);
            _spnColor1.SetSelection(Array.IndexOf(ColorArrayAdapter.COLORS, _item.color1));
            /*_spnColor1.ItemSelected += (sender, e) => {
                Color bg = Color.ParseColor(_spnColor1.GetItemAtPosition((sender as Spinner).SelectedItemPosition).ToString());
                _imgView1.SetBackgroundColor(bg);
            };*/

            _spnColor2 = FindViewById<Spinner>(Resource.Id.spnColor2);
            _spnColor2.Adapter = new ColorArrayAdapter(this);
            _spnColor2.SetSelection(Array.IndexOf(ColorArrayAdapter.COLORS, _item.color2));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _recorder.Release();
            _recorder.Dispose();
            _recorder = null;

            _player.Release();
            _player.Dispose();
            _player = null;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((resultCode == Result.Ok) && (data != null))
            {
                switch (requestCode)
                {
                    case Resource.Id.btnSelectImage1:
                        doScaledBitmapData(data.Data, 1);
                        break;
                    case Resource.Id.btnSelectImage2:
                        doScaledBitmapData(data.Data, 2);
                        break;
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            // сохранение выбранных фоток
            if (_item.image1 != null)
            {
                outState.PutByteArray("image1", _item.image1);
            }

            if (_item.image2 != null)
            {
                outState.PutByteArray("image2", _item.image2);
            }

            // сохранение голоса
            if (_item.sound1 != null)
            {
                outState.PutByteArray("sound1", _item.sound1);
            }

            if (_item.sound2 != null)
            {
                outState.PutByteArray("sound2", _item.sound2);
            }
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            // восстановление фоток
            _item.image1 = savedInstanceState.GetByteArray("image1");
            _item.image2 = savedInstanceState.GetByteArray("image2");

            setImageViewData(_imgView1, _item.image1);
            setImageViewData(_imgView2, _item.image2);

            // восстановление голоса
            _item.sound1 = savedInstanceState.GetByteArray("sound1");
            _item.sound2 = savedInstanceState.GetByteArray("sound2");
        }
    }
}