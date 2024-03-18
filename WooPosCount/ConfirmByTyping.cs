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

namespace WooPosCount
{
    [Activity(Label = "Confirmation")]
    public class ConfirmByTyping : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.InputBox);

            EditText txtConfirm = FindViewById<EditText>(Resource.Id.confirmbox_txtConfirm);
            Button btnOK = FindViewById<Button>(Resource.Id.confirmbox_btnOK);
            Button btnCancel = FindViewById<Button>(Resource.Id.confirmbox_btnCancel);
            btnCancel.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };
            btnOK.Click += (object sender, EventArgs e) =>
            {
                if (txtConfirm.Text.ToUpper() == "DELETEALL")
                {
                    File.Delete(HelperMethods.scanFile);
                    HelperMethods.HT_CountList.Clear();
                    AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
                    HelperMethods.batchTag = "";
                    appPreferences.saveAccessKey("TAG", "");
                    HelperMethods.refreshListview();
                    Finish();
                }
                else
                    HelperMethods.messageBox(this, "Error", "Please type the word DELETEALL!");
            };
        }
    }
}