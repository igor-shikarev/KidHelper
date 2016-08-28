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
using System.Threading.Tasks;
using System.Threading;

namespace KidHelper.Droid
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar", NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Splash);

            //startMain();
        }

        private async void startMain()
        {
            await Task<bool>.Run(() => {
                Thread.Sleep(3 * 1000);
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
                Finish();
                return true;
            });
        }

        protected override void OnResume()
        {
            base.OnResume();

            startMain();
        }
    }
}