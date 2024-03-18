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
using System.IO;
using System.Threading;

namespace WooPosCount
{
    [Activity(Label = "Submit Counted List")]
    public class SubmitCount : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.InputBox);

            EditText txtConfirm = FindViewById<EditText>(Resource.Id.confirmbox_txtConfirm);
            TextView lblConfirm = FindViewById<TextView>(Resource.Id.confirmbox_lblConfirm);
            lblConfirm.Text = "Submitted list must be loaded to POS within 48 hours.\nPlease type a note, memo, tag or identifier:";
            Button btnOK = FindViewById<Button>(Resource.Id.confirmbox_btnOK);
            Button btnCancel = FindViewById<Button>(Resource.Id.confirmbox_btnCancel);
            btnCancel.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };
            btnOK.Click += (object sender, EventArgs e) =>
            {
                AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
                if(appPreferences.getAccessKey("DATABASENAME").Length==0)
                {
                    Finish();
                    HelperMethods.messageBox(HelperMethods.MainActivity, "Error", "Scan QR code to load SKU list first!");
                    return;
                }
                if (txtConfirm.Text.Trim().Length == 0) txtConfirm.Text = DateTime.Now.ToString("MMddHHmmss");
                txtConfirm.Text = txtConfirm.Text.Replace(@"\"," ");
                txtConfirm.Text = txtConfirm.Text.Replace(@"/", " ");
                txtConfirm.Text = txtConfirm.Text.Replace(@":", " ");
                txtConfirm.Text = txtConfirm.Text.Replace(@"*", " ");
                txtConfirm.Text = txtConfirm.Text.Replace(@"?", " ");
                txtConfirm.Text = txtConfirm.Text.Replace(@"<", " ");
                txtConfirm.Text = txtConfirm.Text.Replace(@">", " ");
                txtConfirm.Text = txtConfirm.Text.Replace(@"|", " ");
                txtConfirm.Text = txtConfirm.Text.Trim();
                string filename = "SUBMIT_" + appPreferences.getAccessKey("DATABASENAME") + "_" + txtConfirm.Text;
                if (HelperMethods.HT_CountList.Count == 0 || !File.Exists(HelperMethods.scanFile))
                {
                    Finish();
                    HelperMethods.messageBox(HelperMethods.MainActivity, "Error", "No counted item to upload!");
                    return;
                }
                com.mywoopos.POSLicenseService lsservice = new com.mywoopos.POSLicenseService();
                lsservice.Timeout = 20000;
                if (lsservice.GetFileLen(filename) > 0)
                {
                    filename += "_" + DateTime.Now.ToString("MMddHHmmss");
                }
                var activity2 = new Intent(HelperMethods.MainActivity, typeof(ProgressBarActivity));
                activity2.PutExtra("Title", "Uploading scan list...");
                HelperMethods.MainActivity.StartActivity(activity2);
                ThreadPool.QueueUserWorkItem(o => HelperMethods.saveFileToCloudServer(HelperMethods.scanFile, filename));
                HelperMethods.refreshListview();
                Finish();
            };
        }
    }
}