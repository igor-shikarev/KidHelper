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
using Android.Webkit;

namespace KidHelper.Droid
{
    [Activity(Label = "@string/title_guides", Icon = "@drawable/icon", ParentActivity = typeof(MainActivity))]
    public class GuidesActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Guides);

            WebView web = FindViewById<WebView>(Resource.Id.webView);
            web.LoadUrl("file:///android_res/raw/guides.html");
        }
    }
}