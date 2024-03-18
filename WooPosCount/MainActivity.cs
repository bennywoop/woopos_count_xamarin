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
using Android;
using Android.Content.PM;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Webkit;

namespace WooPosCount
{
    [Activity(Label = "WooPOS Count V35", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Icon = "@drawable/ic_woopos")]
    //[Activity(Label = "123 Count", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Icon = "@drawable/ic_amber")]
    //AndroidManifest.xml MainMenu.xml Strings.xml
    //productShortName = "AmberPOS";
    public class MainActivity : Activity
    {
        private static bool cameraRunning = false;
        private static bool cameraInitialized = false;
        AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
        const int RC_WRITE_EXTERNAL_STORAGE_PERMISSION = 1000;
        const int RC_READ_EXTERNAL_STORAGE_PERMISSION = 1100;
        const int RC_DELETE_STORAGE_FILE = 1200;
        static readonly string[] PERMISSIONS_TO_REQUEST = { Manifest.Permission.WriteExternalStorage };
        EditText txtCode;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (appPreferences.getAccessKey("ExternalStorageAccess") == "Denied")
            {
                if (HelperMethods.productShortName == "AmberPOS" || HelperMethods.productShortName == "HarbortouchPOS")
                    HelperMethods.scanFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "invent.txt");
                else
                    HelperMethods.scanFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "ScanList.txt");
                HelperMethods.skuListFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "SkuList.txt");
                HelperMethods.snapQtyFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "snapQty.txt");
            }
            else
            {
                if (HelperMethods.productShortName == "AmberPOS" || HelperMethods.productShortName == "HarbortouchPOS")
                    HelperMethods.scanFile = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "invent.txt"); 
                else
                    HelperMethods.scanFile = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "ScanList.txt");
                HelperMethods.skuListFile = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "SkuList.txt");
                HelperMethods.snapQtyFile = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "snapQty.txt");
            }
            HelperMethods.MainActivity = this;
            SetContentView(Resource.Layout.Main);

            txtCode = FindViewById<EditText>(Resource.Id.txtCode);
            ImageButton btnCamera = FindViewById<ImageButton>(Resource.Id.imageButtonCamera);
            ImageButton btnCamera2 = FindViewById<ImageButton>(Resource.Id.imageButtonCamera2);
            ImageButton btnAdd = FindViewById<ImageButton>(Resource.Id.imageButtonAdd);
            TextView totalBar = FindViewById<TextView>(Resource.Id.ltvTotal);

            ListView listview1 = FindViewById<ListView>(Resource.Id.listView1);

            HelperMethods.webView1 = new Android.Webkit.WebView(this);
            Android.Webkit.WebSettings websettings = HelperMethods.webView1.Settings;
            websettings.JavaScriptEnabled = true;
            HelperMethods.webView1.SetWebViewClient(new Android.Webkit.WebViewClient());
            ViewGroup.LayoutParams lp = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            HelperMethods.webView1.Visibility = ViewStates.Invisible;
            ((FrameLayout)listview1.Parent).AddView(HelperMethods.webView1, lp);
            AllowCookies(HelperMethods.webView1);

            txtCode.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    if (txtCode.Text.TrimEnd().Length == 0)
                        return;
                    else
                    {
                        HelperMethods.addCode(null, txtCode);
                        e.Handled = true;
                        HelperMethods.refreshListview();
                        txtCode.NextFocusDownId = txtCode.Id;
                    }
                }
            };

            totalBar.Click += openCamera;
            btnCamera.Click += openCamera;
            btnCamera2.Click += openCamera;

            btnAdd.Click += (object sender, EventArgs e) =>
            {
                if (txtCode.Text.TrimEnd().Length > 0)
                {
                    HelperMethods.addCode(null, txtCode);
                    HelperMethods.refreshListview();
                }
            };
            if (appPreferences.getAccessKey("TAG").StartsWith("TAG:"))
                HelperMethods.batchTag = appPreferences.getAccessKey("TAG");

            //createTestFiles(); 
            if (appPreferences.getAccessKey("EditProductMode") != "True" && RequestExternalStoragePermissionIfNecessary(RC_READ_EXTERNAL_STORAGE_PERMISSION))
            {
                HelperMethods.loadSkus(this);
                HelperMethods.loadCountList(this);
            }
            HelperMethods.refreshListview();
            txtCode.RequestFocus();
        }

        private void AllowCookies(WebView view)
        {           
            CookieManager.Instance.Flush();
            CookieManager.AllowFileSchemeCookies();
            CookieManager.SetAcceptFileSchemeCookies(true);
            CookieManager.Instance.AcceptCookie();
            CookieManager.Instance.AcceptThirdPartyCookies(view);
            CookieManager.Instance.SetAcceptCookie(true);
            CookieManager.Instance.SetAcceptThirdPartyCookies(view, true);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RC_READ_EXTERNAL_STORAGE_PERMISSION:
                    if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                    {
                        HelperMethods.has_READ_EXTERNAL_STORAGE_PERMISSION = true;
                        HelperMethods.loadSkus(this);
                        HelperMethods.loadCountList(this);
                        HelperMethods.refreshListview();
                    }
                    else
                    {
                        appPreferences.saveAccessKey("ExternalStorageAccess", "Denied");
                        HelperMethods.messageBox(this, "Error", "You have denied access the files. Please restart the app and scan QR code to upload and download!");
                        Finish();
                    }
                    break;
                case RC_WRITE_EXTERNAL_STORAGE_PERMISSION:
                    if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                    {
                        HelperMethods.addCode(null, txtCode);
                    }
                    else
                    {
                        appPreferences.saveAccessKey("ExternalStorageAccess", "Denied");
                        HelperMethods.messageBox(this, "Error", "You have denied access the files. Please restart the app and scan QR code to upload and download!");
                        Finish();
                    }
                    break;
                case RC_DELETE_STORAGE_FILE:
                    //await DeleteCountFileAsync();
                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }
        bool RequestExternalStoragePermissionIfNecessary(int requestCode)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                HelperMethods.has_READ_EXTERNAL_STORAGE_PERMISSION = true;
                return true;
                //result = PermissionChecker.checkSelfPermission(context, permission)
                //                 == PermissionChecker.PERMISSION_GRANTED;
            }
            if (Android.OS.Environment.MediaMounted.Equals(Android.OS.Environment.ExternalStorageState))
            {
                if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                {
                    HelperMethods.has_READ_EXTERNAL_STORAGE_PERMISSION = true;
                    return true;
                }

                if (ShouldShowRequestPermissionRationale(Manifest.Permission.WriteExternalStorage))
                {
                    Snackbar.Make(FindViewById(Android.Resource.Id.Content),
                                  "Inventory Count App requires permission to write to the external storage.",
                                  Snackbar.LengthIndefinite)
                            .SetAction("OK", delegate { RequestPermissions(PERMISSIONS_TO_REQUEST, requestCode); });
                }
                else
                {
                    RequestPermissions(PERMISSIONS_TO_REQUEST, requestCode);
                }

                return false;
            }

            Log.Warn("Inventory Count", "External storage is not mounted; cannot request permission");
            return false;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MainMenu, menu);
            HelperMethods.MainMenu = menu;
            //menu.FindItem(Resource.Id.menuAction_QuickEdit)?.SetVisible((appPreferences.getAccessKey("QuickEditURL") ?? "").Length > 0);
            menu.FindItem(Resource.Id.menuAction_Count)?.SetVisible(false);
            if (appPreferences.getAccessKey("EditProductMode") == "True" && appPreferences.getAccessKey("QuickEditURL").Length > 0)
            {
                HelperMethods.QuickEditProductMode(true);
                CookieManager.Instance.GetCookie("https://mywoopos.com/");
                HelperMethods.webView1.LoadUrl(HelperMethods.QuickEditURL);
            }
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menuAction_ClearScanList:
                    if (!File.Exists(HelperMethods.scanFile) || HelperMethods.HT_CountList.Count == 0)
                        HelperMethods.messageBox(this, "Error", "Scan list not found!");
                    else
                        StartActivity(typeof(ConfirmByTyping));
                    return true;
                case Resource.Id.menuAction_QRCode:
                    openCamera();
                    return true;
                case Resource.Id.menuAction_Submit:
                    StartActivity(typeof(SubmitCount));
                    return true;
                case Resource.Id.menuAction_Export:
                    exportCountList();
                    return true;
                //case Resource.Id.menuAction_CameraMode:
                //    switchCameraMode("");
                //    return true;
                case Resource.Id.menuAction_Count:
                    HelperMethods.QuickEditProductMode(false);
                    return true;
                case Resource.Id.menuAction_QuickEdit:
                    if ((appPreferences.getAccessKey("QuickEditURL") ?? "").Length > 0)
                        HelperMethods.QuickEditProductMode(true);
                    else
                    {
                        string url = @"https://support.woopos.com/knowledge-base/accessing-woopos-in-web-browser/";
                        var uri = Android.Net.Uri.Parse(url);
                        var intent = new Intent(Intent.ActionView, uri);
                        StartActivity(intent);
                    }
                    return true;
                case Resource.Id.menuAction_Help:
                    {
                        string url = @"https://support.woopos.com/knowledge-base/using-android-woopos-count-app/";
                        if (!File.Exists(HelperMethods.skuListFile))
                        {
                            url = @"https://support.woopos.com/knowledge-base/uploading-sku-list-inventory-count-scanner/";
                        }
                        var uri = Android.Net.Uri.Parse(url);
                        var intent = new Intent(Intent.ActionView, uri);
                        StartActivity(intent);
                        return true;
                    }
                case Resource.Id.menuAction_Exit:
                    //System.Environment.Exit(0);
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }
        private void exportCountList()
        {
            if (HelperMethods.HT_CountList.Count == 0 || !File.Exists(HelperMethods.scanFile))
            {
                HelperMethods.messageBox(this, "Error", "No counted item to export!");
                return;
            }
            if (!HelperMethods.has_READ_EXTERNAL_STORAGE_PERMISSION)
            {
                HelperMethods.messageBox(this, "Error", "You have denied access to file storage. Please scan QR code to export and upload!");
                return;
            }
            string scanFile1 = HelperMethods.scanFile;
            for (int i = 1; i < 9999; i++)
            {
                if (HelperMethods.productShortName == "AmberPOS" || HelperMethods.productShortName == "HarbortouchPOS")
                    scanFile1 = HelperMethods.scanFile.Replace("invent.txt", "invent" + i.ToString() + ".txt");
                else
                    scanFile1 = HelperMethods.scanFile.Replace("ScanList.txt", "ScanList" + i.ToString() + ".txt");
                if (!File.Exists(scanFile1))
                    break;
            }
            var builder = new Android.App.AlertDialog.Builder(HelperMethods.MainActivity);
            if (HelperMethods.productShortName == "AmberPOS" || HelperMethods.productShortName == "HarbortouchPOS")
                builder.SetMessage("Are you sure to save scan list to " + scanFile1 + "?\n\nPlease import the file(s) manually from " + HelperMethods.productShortName + ".");
            else
                builder.SetMessage("Are you sure to save scan list to " + scanFile1 + "?\n\nPlease import the file(s) manually from " + HelperMethods.productShortName + ". Uploading by QR scanning function is not available for the file(s).");
            builder.SetPositiveButton("Yes", (s, ee) =>
            {
                File.Copy(HelperMethods.scanFile, scanFile1);
                new MyMediaScannerConnectionClient(scanFile1);
                File.Delete(HelperMethods.scanFile);
                HelperMethods.HT_CountList.Clear();
                HelperMethods.batchTag = "";
                appPreferences.saveAccessKey("TAG", ""); HelperMethods.refreshListview();
                Toast.MakeText(HelperMethods.MainActivity, "Scan list saved to " + scanFile1, ToastLength.Long).Show();
            });
            builder.SetNegativeButton("No", (s, ee) => { /* do something on Cancel click */ });
            builder.Create().Show();
        }

        public class CusotmListAdapter : BaseAdapter<CountedDetail>
        {
            Activity context;
            List<CountedDetail> list;
            public int expandedPosition = -1;
            public CusotmListAdapter(Activity _context, List<CountedDetail> _list)
                : base()
            {
                this.context = _context;
                this.list = _list;
            }

            public override int Count
            {
                get { return list.Count; }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override CountedDetail this[int index]
            {
                get { return list[index]; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;
                CountedDetail item = this[position];
                if (true) //view == null)
                {
                    // re-use an existing view, if one is available
                    // otherwise create a new one
                    view = context.LayoutInflater.Inflate(Resource.Layout.ListItemRow, parent, false);

                    EditText txtQty = view.FindViewById<EditText>(Resource.Id.txtQty);
                    Button btnMinus = view.FindViewById<Button>(Resource.Id.btnMinus);
                    Button btnPlus = view.FindViewById<Button>(Resource.Id.btnPlus);
                    Button btnDelete = view.FindViewById<Button>(Resource.Id.btnDelete);
                    txtQty.FocusChange += (object sender, View.FocusChangeEventArgs e) =>
                    {
                        if (e.HasFocus) txtQty.Text = "";
                    };
                    txtQty.KeyPress += (object sender, View.KeyEventArgs e) =>
                    {
                        //if (e.ActionId == Android.Views.InputMethods.ImeAction.Done)
                        if (e.KeyCode == Keycode.Enter)
                        {
                            if (txtQty.Text.TrimEnd().Length > 0 && txtQty.Text.TrimEnd().Length <= 5
                                && (HelperMethods.isDecimal(txtQty.Text) == HelperMethods.TypeCheckReturn.Valid
                                || HelperMethods.ConvertToInt(txtQty.Text) >= 0))
                            {
                                item.countedQty = txtQty.Text;
                                NotifyDataSetChanged();
                                HelperMethods.writeInventTxt();
                                EditText txtCode = HelperMethods.MainActivity.FindViewById<EditText>(Resource.Id.txtCode);
                                txtCode.RequestFocus();
                            }
                            else
                            {
                                HelperMethods.PlaySound("error");
                                Toast.MakeText(HelperMethods.MainActivity, "Invalid Quantity!", ToastLength.Long).Show();
                                NotifyDataSetChanged();
                            }
                        }
                        else
                        {
                            e.Handled = false;
                        }
                    };
                    btnMinus.Click += (object sender, EventArgs e) =>
                    {
                        if (HelperMethods.isDecimal(txtQty.Text) != HelperMethods.TypeCheckReturn.Valid)
                            txtQty.Text = "0";
                        txtQty.Text = (HelperMethods.ConvertToInt(txtQty.Text) - 1).ToString();
                        if (HelperMethods.ConvertToInt(txtQty.Text) < 0)
                            txtQty.Text = "0";
                        item.countedQty = txtQty.Text;
                        NotifyDataSetChanged();
                        HelperMethods.writeInventTxt();
                    };
                    btnPlus.Click += (object sender, EventArgs e) =>
                    {
                        if (HelperMethods.isDecimal(txtQty.Text) != HelperMethods.TypeCheckReturn.Valid)
                            txtQty.Text = "0";
                        txtQty.Text = (HelperMethods.ConvertToInt(txtQty.Text) + 1).ToString();
                        item.countedQty = txtQty.Text;
                        NotifyDataSetChanged();
                        HelperMethods.writeInventTxt();
                    };
                    btnDelete.Click += (object sender, EventArgs e) =>
                    {
                        var builder = new Android.App.AlertDialog.Builder(HelperMethods.MainActivity);
                        builder.SetMessage("Are you sure to delete this line?");
                        builder.SetPositiveButton("Yes", (s, ee) =>
                        {
                            HelperMethods.HT_CountList.RemoveAt(position);
                            NotifyDataSetChanged();
                            HelperMethods.writeInventTxt();
                        });
                        builder.SetNegativeButton("No", (s, ee) => { /* do something on Cancel click */ });
                        builder.Create().Show();
                    };
                }

                view.FindViewById<TextView>(Resource.Id.Title).Text = item.code;
                view.FindViewById<TextView>(Resource.Id.lblQty).Text = item.countedQty;
                view.FindViewById<EditText>(Resource.Id.txtQty).Text = item.countedQty;
                view.FindViewById<TextView>(Resource.Id.Description).Text = item.description;
                view.FindViewById<TextView>(Resource.Id.Price).Text = "Price: " + item.price;


                if (expandedPosition == position)
                {
                    view.FindViewById<LinearLayout>(Resource.Id.ListItemRow).SetBackgroundColor(Android.Graphics.Color.DarkGray);
                    view.FindViewById<LinearLayout>(Resource.Id.qtyButtons).Visibility = ViewStates.Visible;
                    view.FindViewById<TextView>(Resource.Id.Price).Visibility = ViewStates.Visible;
                }
                else
                {
                    view.FindViewById<LinearLayout>(Resource.Id.ListItemRow).SetBackgroundColor(Android.Graphics.Color.Black);
                    view.FindViewById<LinearLayout>(Resource.Id.qtyButtons).Visibility = ViewStates.Gone;
                    view.FindViewById<TextView>(Resource.Id.Price).Visibility = ViewStates.Gone;
                }
                if (item.subBatchTag.Length > 0) view.FindViewById<TextView>(Resource.Id.Description).Text += " (" + item.subBatchTag + ")";
                return view;
            }
        }
        public void createTestFiles()
        {
            File.Delete(HelperMethods.skuListFile);
            if (!File.Exists(HelperMethods.skuListFile))
            {
                FileStream fs = new FileStream(HelperMethods.skuListFile, FileMode.CreateNew, FileAccess.Write);
                StreamWriter s = new StreamWriter(fs);
                if (HelperMethods.productShortName == "AmberPOS" || HelperMethods.productShortName == "HarbortouchPOS")
                {
                    s.WriteLine("1001,Memo Pad-E D,7.50,0,0,0,0,;123;" + "\r\n");
                    s.WriteLine("1002,Memo Pad-E D,7.50,34,0,0,0,;122;" + "\r\n"); //1002-L 1002-01
                    s.WriteLine("<<Dimension Category List>>" + "\r\n");
                    s.WriteLine("" + "\r\n");
                    s.WriteLine("34,01" + "\r\n");
                    s.WriteLine("34,L" + "\r\n");
                }
                else
                {
                    s.WriteLine("1001,Memo Pad-E D,$7.50,;123;" + "\r\n");
                    s.WriteLine("product_id_67,Ship Your Idea QOH:18,15.00" + "\r\n");
                    s.WriteLine("WP00040-Black,Ship Your Idea - Black QOH:38,35.00" + "\r\n");
                }
                    fs.Flush();
                s.Close();
                fs.Close();
                new Android.App.AlertDialog.Builder(this)
                  .SetPositiveButton("OK", (sender, args) =>
                  {
                      // User pressed OK
                  })
                  .SetMessage("SKU List test file created\nuse 1001 as code input!")
                  .SetTitle("New File " + HelperMethods.skuListFile)
                  .Show();
            }
        }

        private void openCamera(object sender, EventArgs e)
        {
            openCamera();
        }
        private async void openCamera()
        {
            if (cameraRunning) return;
            cameraRunning = true;
            EditText txtCode = FindViewById<EditText>(Resource.Id.txtCode);
            if (!cameraInitialized)
            {

                ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);
                cameraInitialized = true;
            }
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            ZXing.Mobile.MobileBarcodeScanningOptions options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            //options.TryHarder = true;
            options.AutoRotate = false;

            scanner.TopText = "Hold the device back about 3 to 6 inches from the barcode. Try to avoid shadows and glare. Avoid objects and light in the background.";
            scanner.BottomText = "Place a barcode in the camera viewfinder. Barcode will scan automatically. Tap the screen to focus if needed.";
            scanner.FlashButtonText = "Torch";
            scanner.AutoFocus();
            var resultX = await scanner.Scan(options);
            if (resultX != null)
            {
                txtCode.Text = resultX.Text;
                HelperMethods.addCode(null, txtCode);
                HelperMethods.refreshListview();
            }
            cameraRunning = false;
        }
    } 
}

