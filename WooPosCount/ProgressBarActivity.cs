using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Android.Views.InputMethods;
using Android.Hardware;
using System.Threading;
using System.Timers;

namespace WooPosCount
{
    [Activity(Label = "Progressing...")]
    public class ProgressBarActivity : Activity
    {
        public static ProgressBar progress;
        System.Timers.Timer _timer;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ProgressBar);

            EditText txtCode = FindViewById<EditText>(Resource.Id.txtCode);
            progress = FindViewById<ProgressBar>(Resource.Id.myprogressBar);
            progress.Max = 100;
            progress.Progress = 0;
            FindViewById<TextView>(Resource.Id.lblProgress).Text = Intent.GetStringExtra("Title") ?? "Loading...";
            _timer = new System.Timers.Timer();
            _timer.Enabled = true;
            _timer.Interval = 200;
            _timer.Elapsed += OnTimeEvent;
            _timer.Start();
        }
        public static void setProgress(int _percent)
        {
            HelperMethods.MainActivity.RunOnUiThread(() =>
                    {
                        progress.Progress = _percent;
                    });
        }
        private void OnTimeEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (progress.Progress >= 100)
                {
                    _timer.Dispose();
                    Finish();
                }
            });
        }
    }
}