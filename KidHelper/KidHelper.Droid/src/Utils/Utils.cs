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
using System.IO;
using Android.Util;
using System.Threading.Tasks;
using Android.Media;

namespace KidHelper.Droid.Utils
{
    class BitmapInfo
    {
        public static int ORIENTATION_NORMAL = 1;
        public static int ORIENTATION_ROTATE_180 = 3;
        public static int ORIENTATION_ROTATE_270 = 8;
        public static int ORIENTATION_ROTATE_90 = 6;

        public int Width { get; set; }
        public int Height { get; set; }

        public BitmapInfo(int w, int h)
        {
            Width = w;
            Height = h;
        }
    }

    class Util
    {
        /**
         * Вычисление коэффициента для загрузки изображения
         */
        public static int calcInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            float height = options.OutHeight;
            float width = options.OutWidth;
            double inSampleSize = 1.0;

            if (height > reqHeight || width > reqWidth)
            {
                int halfHeight = (int)(height / 2);
                int halfWidth = (int)(width / 2);

                // Calculate a inSampleSize that is a power of 2 - the decoder will use a value that is a power of two anyway.
                while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return (int)inSampleSize;
        }

        public static BitmapInfo getBitmapInfo(ref byte[] bmpData)
        {
            // запрос размеров изображения
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            Bitmap bmp = BitmapFactory.DecodeByteArray(bmpData, 0, bmpData.Length, options);
            Util.FreeAndNil(ref bmp);

            return new BitmapInfo(options.OutWidth, options.OutHeight);
        }

        public static BitmapInfo getBitmapInfo(string fileName)
        {
            // запрос размеров изображения
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            Bitmap bmp = BitmapFactory.DecodeFile(fileName, options);
            Util.FreeAndNil(ref bmp);

            return new BitmapInfo(options.OutWidth, options.OutHeight);
        }

        public static int getBitmapRotation(Activity context, byte[] bmpData)
        {
            // get EXIF info
            int bmp_rotation = 0;
            try
            {
                var cw = new ContextWrapper(context.ApplicationContext);
                var directory = cw.GetDir("testDirectory", FileCreationMode.Private);
                string fileName = string.Concat(directory.AbsolutePath, "/photo.jpg");
                File.WriteAllBytes(fileName, bmpData);

                ExifInterface exif = new ExifInterface(fileName);
                bmp_rotation = exif.GetAttributeInt(ExifInterface.TagOrientation, BitmapInfo.ORIENTATION_NORMAL);

                if (bmp_rotation == BitmapInfo.ORIENTATION_ROTATE_90)
                {
                    bmp_rotation = 90;
                }
                else
                if (bmp_rotation == BitmapInfo.ORIENTATION_ROTATE_180)
                {
                    bmp_rotation = 180;
                }
                else
                if (bmp_rotation == BitmapInfo.ORIENTATION_ROTATE_270)
                {
                    bmp_rotation = 270;
                }
                else
                {
                    bmp_rotation = 0;
                }

                exif.Dispose();
                exif = null;

            }
            catch (Exception ex)
            {
                //
            }

            return bmp_rotation;
        }

        public async static Task<Bitmap> getScaledBitmap(Activity context, byte[] bmpData, int dstW, int dstH)
        {
            Bitmap bmp;

            // запрос размеров изображения
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            bmp = await BitmapFactory.DecodeByteArrayAsync(bmpData, 0, bmpData.Length, options);

            // вычисление желаемого размера фотки
            double koef = Math.Min(1.0 * dstW / options.OutWidth, 1.0 * dstH / options.OutHeight);
            if (koef < 1.0)
            {
                dstW = (int)(dstW * koef);
                dstH = (int)(dstH * koef);
            }

            // меняем размер
            options.InSampleSize = Util.calcInSampleSize(options, dstW, dstH);
            options.InJustDecodeBounds = false;
            bmp = await BitmapFactory.DecodeByteArrayAsync(bmpData, 0, bmpData.Length, options);
            options.Dispose();
            options = null;

            bmpData = null;
            System.GC.Collect(1);

            return bmp;
        }

        public async static Task<byte[]> getScaledBitmapData(Activity context, byte[] bmpData, int dstW, int dstH)
        {
            byte[] result;
            Bitmap bmp;

            // query image size
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            bmp = await BitmapFactory.DecodeByteArrayAsync(bmpData, 0, bmpData.Length, options);

            // calc size
            double koef = Math.Min(1.0 * dstW / options.OutWidth, 1.0 * dstH / options.OutHeight);
            if (koef < 1.0)
            {
                dstW = (int)(dstW * koef);
                dstH = (int)(dstH * koef);
            }

            // calc InSampleSize
            options.InSampleSize = Util.calcInSampleSize(options, dstW, dstH);
            options.InJustDecodeBounds = false;
            bmp = await BitmapFactory.DecodeByteArrayAsync(bmpData, 0, bmpData.Length, options);
            options.Dispose();
            options = null;

            // rotate photo
            int bmp_rotation = Util.getBitmapRotation(context, bmpData);
            if (bmp_rotation != 0)
            {
                Matrix matrix = new Matrix();
                matrix.PostRotate(bmp_rotation);
                bmp = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, matrix, true);
                matrix.Dispose();
                matrix = null;
            }

            // write to stream
            MemoryStream stream = new MemoryStream();
            bmp.Compress(Bitmap.CompressFormat.Jpeg, 80, stream);
            bmp.Recycle();
            Util.FreeAndNil(ref bmp);

            result = stream.ToArray();
            Util.FreeAndNil(ref stream);

            return result;
        }

        public static int dp2px(Activity context, int dp)
        {
            DisplayMetrics displayMetrics = context.Resources.DisplayMetrics;
            int px = (int)(dp * displayMetrics.Density);
            return px;
        }

        public static int px2dp(Activity context, int px)
        {
            DisplayMetrics displayMetrics = context.Resources.DisplayMetrics;
            int dp = (int)(px / displayMetrics.Density);
            return dp;
        }

        public static void FreeAndNil(ref Bitmap obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
                System.GC.Collect(1);
            }
        }

        public static void FreeAndNil(ref MemoryStream obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
                System.GC.Collect(1);
            }
        }

        public static void FreeAndNil(ref System.IO.Stream obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
                System.GC.Collect(1);
            }
        }
    }
}