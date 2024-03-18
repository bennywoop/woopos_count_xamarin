using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;
using System.Collections.Specialized;
using Android.Preferences;
using System.Threading;
using Android.Content.PM;
using Android;
using Android.Webkit;

namespace WooPosCount
{
    public abstract class HelperMethods
    {
        #region Type Definitions
        //---------------------------------------------------------------------
        public enum TypeCheckReturn
        { Invalid = -1, Empty = 0, Valid = 1, Incomplete = 2 }
        public static Hashtable HT_SKUs = new Hashtable();
        public static Hashtable HT_UPCs = new Hashtable();
        public static Hashtable HT_QTYs = new Hashtable();
        public static ArrayList dimList = new ArrayList();
        public static List<CountedDetail> HT_CountList = new List<CountedDetail>();
        public static TextView txtTotal;
        public static Queue countingQueue = new Queue();
        private static bool addListFlag = false;
        public static bool has_READ_EXTERNAL_STORAGE_PERMISSION = false;
        public static Activity MainActivity;
        public static IMenu MainMenu;
        public static Android.Webkit.WebView webView1;
        public static string productShortName = "WooPOS";
        public static string scanFile = "";
        public static string skuListFile = "";
        public static string snapQtyFile = "";
        public static string batchTag = "";
        public static string QuickEditURL = "";
        //---------------------------------------------------------------------
        #endregion
        public static void refreshListview()
        {
            HelperMethods.txtTotal = MainActivity.FindViewById<TextView>(Resource.Id.ltvTotal);
            AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
            if (appPreferences.getAccessKey("EditProductMode") == "True") return;
            ListView listview1 = MainActivity.FindViewById<ListView>(Resource.Id.listView1);
 
            listview1.Adapter = new MainActivity.CusotmListAdapter(MainActivity, HelperMethods.HT_CountList);
            if (listview1.Count > 0)
            {
                listview1.SetSelection(0);
                ((MainActivity.CusotmListAdapter)listview1.Adapter).expandedPosition = 0;
            }
            listview1.FastScrollEnabled = true;
            listview1.ItemClick += (object sender, AdapterView.ItemClickEventArgs ee) =>
            {
                MainActivity.CusotmListAdapter listAdapter = (MainActivity.CusotmListAdapter)listview1.Adapter;
                listAdapter.expandedPosition = ee.Position;
                listview1.InvalidateViews();
            };
            decimal totals = 0;
            foreach (CountedDetail strLine in HelperMethods.HT_CountList)
            {
                totals += Convert.ToDecimal(strLine.countedQty);
            }
            HelperMethods.txtTotal.Text = "Total " + HelperMethods.HT_CountList.Count.ToString() + " scans, " + Convert.ToInt32(totals).ToString() + " items";
        }
        public static void messageBox(Activity a, String title, String message)
        {
            if (a == null) a = MainActivity;
            AlertDialog.Builder dialog = new AlertDialog.Builder(a);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetPositiveButton("OK", (sender, args) => { });
            dialog.Show();
        }

        public static void QuickEditProductMode(bool editProductMode)
        {
            AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
            ListView listview1 = MainActivity.FindViewById<ListView>(Resource.Id.listView1);
            if (editProductMode)
            {
                appPreferences.saveAccessKey("EditProductMode", "True");
                HelperMethods.QuickEditURL = appPreferences.getAccessKey("QuickEditURL");
                MainMenu.FindItem(Resource.Id.menuAction_Count)?.SetVisible(true);
                MainMenu.FindItem(Resource.Id.menuAction_ClearScanList)?.SetVisible(false);
                MainMenu.FindItem(Resource.Id.menuAction_Export)?.SetVisible(false);
                MainMenu.FindItem(Resource.Id.menuAction_Submit)?.SetVisible(false);
                MainMenu.FindItem(Resource.Id.menuAction_QuickEdit)?.SetVisible(false);
                listview1.Visibility = ViewStates.Invisible;
                HelperMethods.webView1.Visibility = ViewStates.Visible;
                MainActivity.Window.SetTitle("Online Edit Mode");
                HelperMethods.txtTotal.Text = "Tap to Scan Barcode by Camera";
            }
            else
            {
                appPreferences.saveAccessKey("EditProductMode", "");
                HelperMethods.QuickEditURL = "";
                MainMenu.FindItem(Resource.Id.menuAction_Count)?.SetVisible(false);
                MainMenu.FindItem(Resource.Id.menuAction_ClearScanList)?.SetVisible(true);
                MainMenu.FindItem(Resource.Id.menuAction_Export)?.SetVisible(true);
                MainMenu.FindItem(Resource.Id.menuAction_Submit)?.SetVisible(true);
                MainMenu.FindItem(Resource.Id.menuAction_QuickEdit)?.SetVisible(true);
                //MainMenu.FindItem(Resource.Id.menuAction_QuickEdit)?.SetVisible((appPreferences.getAccessKey("QuickEditURL") ?? "").Length > 0);
                listview1.Visibility = ViewStates.Visible;
                HelperMethods.webView1.Visibility = ViewStates.Invisible;
                MainActivity.Window.SetTitle("Offline Count Mode");
                HelperMethods.loadSkus(MainActivity);
                HelperMethods.loadCountList(MainActivity);
                HelperMethods.refreshListview();
            }
            ((FrameLayout)listview1.Parent).RequestLayout();
        }

        public static bool addCode(Activity activity, EditText txtCode)
        {
            if (activity == null) activity = MainActivity;
            if (File.Exists(skuListFile))
                loadSkus(activity);
            bool HasVibrator = false;
            try
            {
                Vibrator vibrator = (Vibrator)activity.GetSystemService(Context.VibratorService);
                if (vibrator.HasVibrator)
                    HasVibrator = true;
            }
            catch { }
            long[] errorVibratorPattern = { 0, 100, 150, 100, 150, 400, 150 };
            if (txtCode.Text.StartsWith("https://mywoopos.com/quickeditproduct") || txtCode.Text == "[[QE]]")
            {
                if (txtCode.Text == "[[QE]]") 
                    txtCode.Text = @"https://mywoopos.com/quickeditproduct/?serverid=DEMO1____57111ecc-db02-4ff2-ad2b-7dd5683ed4ff";
                string url = txtCode.Text;
                txtCode.Text = "";
                AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
                appPreferences.saveAccessKey("QuickEditURL", url);
                QuickEditURL = url;
                QuickEditProductMode(true);
                HelperMethods.webView1.LoadUrl(url); 
                return true;
            }
            if (HelperMethods.QuickEditURL.Length > 0)
            {
                HelperMethods.webView1.LoadUrl(HelperMethods.QuickEditURL + "&scancode=" + System.Web.HttpUtility.UrlEncode(txtCode.Text));
                CookieManager.Instance.SetCookie("https://mywoopos.com/", "cookie_name=cookie_value; path=/");
                txtCode.Text = "";
                return true;
            }
            if (txtCode.Text.StartsWith("[[SCANLIST]]"))
            {
                //txtCode.Text = "[[SCANLIST]]WP010016_1d7f97cc-87a3-4a12-8eef-6d686599b174";

                string filename = txtCode.Text.Replace("[[SCANLIST]]", "SCANLIST_");
                txtCode.Text = "";
                if (HelperMethods.HT_CountList.Count == 0 || !File.Exists(HelperMethods.scanFile))
                {
                    HelperMethods.messageBox(HelperMethods.MainActivity, "Error", "No counted item to upload!");
                    return false;
                }
                var activity2 = new Intent(HelperMethods.MainActivity, typeof(ProgressBarActivity));
                activity2.PutExtra("Title", "Uploading scan list...");
                HelperMethods.MainActivity.StartActivity(activity2);
                ThreadPool.QueueUserWorkItem(o => saveFileToCloudServer(HelperMethods.scanFile, filename));
                return true;
            }
            if (txtCode.Text.StartsWith("[[SKULIST]]"))
            {
                string filename = txtCode.Text.Replace("[[SKULIST]]", "SKULIST_");
                if (txtCode.Text.IndexOf("_") > 0)
                {
                    string dbname = txtCode.Text.Replace("[[SKULIST]]", "");
                    AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
                    appPreferences.saveAccessKey("DATABASENAME", dbname.Substring(0, dbname.IndexOf("_")));
                }
                txtCode.Text = "";
                var activity2 = new Intent(HelperMethods.MainActivity, typeof(ProgressBarActivity));
                activity2.PutExtra("Title", "Downloading Sku list...");
                HelperMethods.MainActivity.StartActivity(activity2);
                ThreadPool.QueueUserWorkItem(o => getFileFromCloudServer(HelperMethods.skuListFile, filename));
                return true;
            }
            if (txtCode.Text.ToUpper().StartsWith("QTY**") && HelperMethods.isInteger(txtCode.Text.ToUpper().Replace("QTY**", "")) == HelperMethods.TypeCheckReturn.Valid)
            {
                int p = ((MainActivity.CusotmListAdapter)(MainActivity.FindViewById<ListView>(Resource.Id.listView1)).Adapter).expandedPosition;
                if (p < 0 || p >= HelperMethods.HT_CountList.Count)
                {
                    txtCode.Text = "";
                    return true;
                }
                ((CountedDetail)HelperMethods.HT_CountList[p]).countedQty = txtCode.Text.ToUpper().Replace("QTY**", "");
                txtCode.Text = "";
                Toast.MakeText(activity, ((CountedDetail)HelperMethods.HT_CountList[p]).code + ": " + ((CountedDetail)HelperMethods.HT_CountList[p]).description + "\r\nQuantity has been changed to " + ((CountedDetail)HelperMethods.HT_CountList[p]).countedQty, ToastLength.Long).Show();
                HelperMethods.writeInventTxt();
                return true;
            }  
            if (txtCode.Text.StartsWith("http:") || txtCode.Text.StartsWith("https:"))
            {
                var uri = Android.Net.Uri.Parse(txtCode.Text);
                txtCode.Text = "";
                var intent = new Intent(Intent.ActionView, uri);
                HelperMethods.MainActivity.StartActivity(intent);
                return true;
            }
            string code = txtCode.Text.ToUpper().Trim();
            if (code.IndexOf(",") >= 0)
            {
                HelperMethods.PlaySound("error");
                if (HasVibrator)
                    try
                    {
                        Vibrator vibrator = (Vibrator)activity.GetSystemService(Context.VibratorService);
                        vibrator.Vibrate(VibrationEffect.CreateWaveform(errorVibratorPattern, -1));
                    }
                    catch { }
                messageBox(activity, "Error", "Invalid Code!");
                return false;
            }
            if (code.StartsWith("TAG:"))
            {
                AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
                if (code == "TAG:")
                {
                    appPreferences.saveAccessKey("TAG", "");
                    Toast.MakeText(HelperMethods.MainActivity, "Tag removed", ToastLength.Long).Show();
                    batchTag = "";
                }
                else
                {
                    appPreferences.saveAccessKey("TAG", code);
                    Toast.MakeText(HelperMethods.MainActivity, "Tag Added " + code, ToastLength.Long).Show();
                    batchTag = code;
                }
                txtCode.Text = "";
                return true;
            }
            string descp = "", price = "";
            decimal packageQty = 1;
            string partialUPC = "";
            if (code.EndsWith(".") && !Search_skuList(code, ref descp, ref price, ref packageQty, ref partialUPC)
                && Search_skuList(code.Substring(0, code.Length - 1), ref descp, ref price, ref packageQty, ref partialUPC))
                code = code.Substring(0, code.Length - 1);
            if (Search_skuList(code, ref descp, ref price, ref packageQty, ref partialUPC))
            {
                if (HT_CountList.Count > 0 && !File.Exists(scanFile))
                {
                    HelperMethods.PlaySound("error");
                    if (HasVibrator)
                        try
                        {
                            Vibrator vibrator = (Vibrator)activity.GetSystemService(Context.VibratorService);
                            vibrator.Vibrate(VibrationEffect.CreateWaveform(errorVibratorPattern, -1));
                        }
                        catch { }
                    messageBox(activity, "Warning", "This item marks the start of a new batch!");
                    HT_CountList.Clear();
                    countingQueue.Clear();
                }

                string countedQty = "";

                HelperMethods.PlaySound("ok");
                if (HasVibrator)
                    try
                    {
                        Vibrator vibrator = (Vibrator)activity.GetSystemService(Context.VibratorService);
                        vibrator.Vibrate(VibrationEffect.CreateOneShot(150, -1));
                    }
                    catch { }
                countedQty = "1";
                CountedDetail cntdl = new CountedDetail();
                if (partialUPC.Length > 0 && code.Contains(partialUPC))
                    cntdl.code = partialUPC;
                else
                    cntdl.code = code;
                cntdl.countedQty = countedQty;
                cntdl.description = descp;
                cntdl.subBatchTag = HelperMethods.batchTag;
                cntdl.price = price;
                countingQueue.Enqueue(cntdl);

                //txtCode.RequestFocus();

                if (!addListFlag)
                {
                    addList(price);
                }
                txtCode.Text = "";
                return true;
            }
            else
            {
                HelperMethods.PlaySound("error");
                if (HasVibrator)
                    try
                    {
                        Vibrator vibrator = (Vibrator)activity.GetSystemService(Context.VibratorService);
                        vibrator.Vibrate(VibrationEffect.CreateWaveform(errorVibratorPattern, -1));
                    }
                    catch { }
                Toast.MakeText(activity, "Invalid Code: " + txtCode.Text, ToastLength.Long).Show();
                System.Threading.Tasks.Task.Delay(500).Wait(); //ab.Wait(1000);
                txtCode.Text = "";
                return false;
            }
        }
        private static void addList(string pprice)
        {
            addListFlag = true;
            while (countingQueue.Count > 0)
            {
                CountedDetail strLine = (CountedDetail)countingQueue.Dequeue();
                HT_CountList.Insert(0, strLine);
                string line = strLine.code + "," + strLine.countedQty;
                if (strLine.subBatchTag != "") line += "," + strLine.subBatchTag;
                appendInventTxt(line);
            }
            addListFlag = false;
        }
        private static bool writingInventTxt = false;
        public static void writeInventTxt()
        {
            try
            {
                while (writingInventTxt)
                    System.Threading.Thread.Sleep(300);
                writingInventTxt = true;
                if (File.Exists(scanFile)) //System.IO.Path.GetDirectoryName(Application.ExecutablePath) + 
                {
                    if (productShortName == "AmberPOS" || productShortName == "HarbortouchPOS")
                        File.Copy(scanFile, scanFile.Replace("invent.txt", "inven_.txt") + "_", true);
                    else
                        File.Copy(scanFile, scanFile.Replace("ScanList", "ScanLis_").Replace(".txt", ".bak_"), true);
                    File.Delete(scanFile);
                }
                FileStream fs = new FileStream(scanFile, FileMode.CreateNew, FileAccess.Write);
                StreamWriter s = new StreamWriter(fs);
                decimal totals = 0;
                for (int i = 0; i < HT_CountList.Count; i++)
                {
                    CountedDetail strLine = (CountedDetail)HT_CountList[HT_CountList.Count - i - 1];
                    string line = strLine.code + "," + strLine.countedQty;
                    if (strLine.subBatchTag != "") line += "," + strLine.subBatchTag;
                    s.Write(line + "\r\n");
                    totals += Convert.ToDecimal(strLine.countedQty);
                }
                fs.Flush();
                s.Close();
                fs.Close();
                writingInventTxt = false;
                HelperMethods.txtTotal.Text = "Total " + HT_CountList.Count.ToString() + " scans, " + Convert.ToInt32(totals).ToString() + " items";
                new MyMediaScannerConnectionClient(scanFile);
            }
            catch (Exception ex)
            {
                messageBox(HelperMethods.MainActivity, "Error", "Cannot write count file " + scanFile + "! \n\n" + ex.Message);
            }
        }
        private static void appendInventTxt(string skurow)
        {
            try
            {
                while (writingInventTxt)
                {
                    System.Threading.Thread.Sleep(300);
                }
                writingInventTxt = true;
                FileStream fs = new FileStream(scanFile, File.Exists(scanFile) ? FileMode.Append : FileMode.CreateNew, FileAccess.Write);
                StreamWriter s = new StreamWriter(fs);
                s.Write(skurow + "\r\n");
                fs.Flush();
                s.Close();
                fs.Close();
                writingInventTxt = false;
                new MyMediaScannerConnectionClient(scanFile);
            }
            catch (Exception ex)
            {
                messageBox(HelperMethods.MainActivity, "Error", "Cannot write count file " + scanFile + "! \n\n" + ex.Message);
            }
        }

        private static DateTime SkuListFileDate = DateTime.Now;
        private static bool isAmber = false;
        public static void loadSkus(Activity activity)
        {
            try
            {
                AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
                if (!File.Exists(skuListFile))
                {
                    if (appPreferences.getAccessKey("EditProductMode") != "True")
                    {
                        if (has_READ_EXTERNAL_STORAGE_PERMISSION)
                            messageBox(activity, "Warning", "SKU List file is not found! \n\nThe SkuList.txt file is a comma delimited CSV file which contains Sku, Description, Price. The file can be uploaded by scanning QR code, or from POS Windows app, or create your own and place it in the root folder of Internal Storage.");
                        else
                            messageBox(activity, "Warning", "SKU List file is not found! \n\nThe SkuList.txt file can be uploaded by scanning QR code from POS Windows app");
                    }
                    return;
                }
                if (SkuListFileDate == File.GetLastWriteTimeUtc(skuListFile) && HT_SKUs.Count > 0) return;
                dimList.Clear();
                HT_SKUs.Clear();
                HT_UPCs.Clear();
                HT_QTYs.Clear();                
                StreamReader fs = new StreamReader(File.OpenRead(skuListFile));
                string data = "";
                int line = 0;
                bool msgshowed = false;
                while ((data = fs.ReadLine()) != null)
                {
                    if (data == "<<Dimension Category List>>") break;
                    line++;
                    if (data.Length > 0)
                    {
                        string delimStr = ",";
                        char[] delimiter = delimStr.ToCharArray();
                        string[] split = data.Split(delimiter);
                        if (line == 1 && (split.Length > 6 || productShortName == "AmberPOS" || productShortName == "HarbortouchPOS"))
                            isAmber = true;
                        if (!isAmber)
                        {
                            if (split.Length < 3)
                            {
                                if (!msgshowed)
                                {
                                    msgshowed = true;
                                    messageBox(activity, "Error", "Invalid SKU List file! \nLine " + line.ToString() + ": " + data + "\nEvery row should contain at least 3 columns: Code,Description,Price");
                                }
                                continue;
                            }
                            if (split.Length == 3)
                            {
                                Array.Resize(ref split, split.Length + 1);
                                split[split.Length - 1] = ";;";
                            }
                            if (split.Length == 4)
                            {
                                Array.Resize(ref split, split.Length + 1);
                                split[split.Length - 1] = "1";
                            }
                        }
                        if (!HT_SKUs.ContainsKey(split[0]))
                        {
                            if (isAmber)
                            {
                                if (split.Length > 8 && HelperMethods.isDecimal(split[8]) == HelperMethods.TypeCheckReturn.Valid)
                                    HT_SKUs.Add(split[0], (new string[] { split[0], split[1], split[2], split[3], split[4], split[5], split[6], split[8] }));
                                else
                                    HT_SKUs.Add(split[0], (new string[] { split[0], split[1], split[2], split[3], split[4], split[5], split[6], "1" }));
                            }
                            else
                            {
                                if (split.Length > 4 && HelperMethods.isDecimal(split[4]) == HelperMethods.TypeCheckReturn.Valid)
                                    HT_SKUs.Add(split[0].ToUpper(), (new string[] { split[0].ToUpper(), split[1], split[2], split[4] }));
                                else
                                    HT_SKUs.Add(split[0].ToUpper(), (new string[] { split[0].ToUpper(), split[1], split[2], "1" }));
                            }
                        }
                        delimStr = ";";
                        delimiter = delimStr.ToCharArray();
                        int iUPC = 3;
                        if (isAmber)
                            iUPC = 7;
                        string[] Usplit = split[iUPC].Split(delimiter);
                        for (int i = 0; i < Usplit.Length; i++)
                        {
                            string UPC = Usplit[i].TrimEnd().ToUpper();
                            if (UPC.Length > 0 && !HT_UPCs.ContainsKey(UPC) && !HT_SKUs.ContainsKey(UPC)) HT_UPCs.Add(UPC, split[0].ToUpper());
                        }

                    }
                }
                while (isAmber && (data = fs.ReadLine()) != null)
                {
                    if (data.Length > 0)
                    {
                        string delimStr = ",";
                        char[] delimiter = delimStr.ToCharArray();
                        string[] split = null;
                        split = data.Split(delimiter);
                        //dimList.Add(new string[]{getDelimitedColumn(data,0,","),getDelimitedColumn(data,1,",")});
                        dimList.Add(new string[] { split[0], split[1] });
                    }
                }
                fs.Close();
                if (File.Exists(snapQtyFile))
                {
                    StreamReader fi = new StreamReader(File.OpenRead(snapQtyFile));
                    while ((data = fi.ReadLine()) != null)
                    {
                        if (data.Length > 0)
                        {
                            string delimStr = ",";
                            char[] delimiter = delimStr.ToCharArray();
                            string[] split = data.Split(delimiter);
                            if (split.Length != 2) continue;
                            string code = split[0];
                            if (code.Length > 0 && !HT_QTYs.ContainsKey(code)) HT_QTYs.Add(code, split[1]);
                        }
                    }
                    fi.Close();
                }
                SkuListFileDate = File.GetLastWriteTimeUtc(skuListFile);
                Toast.MakeText(HelperMethods.MainActivity, "Sku List loaded", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                messageBox(HelperMethods.MainActivity, "Error", "Cannot load SKU list file " + skuListFile + "! \n\n" + ex.Message);
            }
        }
        public static bool Search_skuList_Amber(string code, ref string descp, ref string price, ref decimal packageQty, ref string partialUPC)
        {
            code = code.TrimEnd();
            packageQty = 1;
            string delimStr = "-";
            char[] delimiter = delimStr.ToCharArray();
            string[] split = null;
            split = (code + "----").Split(delimiter);

            bool dimMatch = false;
            string sku = split[0]; //getDelimitedColumn(code,0,"-");
            if (sku == "") return false;
            if (HT_SKUs.Count == 0)
            {
                descp = "N/A, No Preloaded Sku List!";
                price = "0";
                return true;
            }
            if (HT_SKUs.ContainsKey(sku))
            {
                string[] skuList = (string[])HT_SKUs[sku];
                descp = skuList[1];
                price = skuList[2];
                packageQty = Convert.ToDecimal(skuList[7]);
                string dim = skuList[3];  //dim1
                if (HT_QTYs.ContainsKey(code))
                    descp += " Snapshot Qty: " + (string)HT_QTYs[code];
                else
                    descp += " Snapshot Qty: 0";

                if (dim != "0")
                {
                    dimMatch = false;
                    for (int d = 0; d < dimList.Count; d++)
                    {
                        if (((string[])dimList[d])[0] == dim && ((string[])dimList[d])[1] == split[1]) //getDelimitedColumn(code,1,"-"))
                        {
                            dimMatch = true;
                            break;
                        }
                    }
                    if (!dimMatch) return false;
                }

                dim = skuList[4]; //dim2
                if (dim != "0")
                {
                    dimMatch = false;
                    for (int d = 0; d < dimList.Count; d++)
                    {
                        if (((string[])dimList[d])[0] == dim && ((string[])dimList[d])[1] == split[2]) //getDelimitedColumn(code,2,"-"))
                        {
                            dimMatch = true;
                            break;
                        }
                    }
                    if (!dimMatch) return false;
                }

                dim = skuList[5];
                if (dim != "0")
                {
                    dimMatch = false;
                    for (int d = 0; d < dimList.Count; d++)
                    {
                        if (((string[])dimList[d])[0] == dim && ((string[])dimList[d])[1] == split[3]) //getDelimitedColumn(code,3,"-"))
                        {
                            dimMatch = true;
                            break;
                        }
                    }
                    if (!dimMatch) return false;
                }

                dim = skuList[6];   //dim4
                if (dim != "0")
                {
                    dimMatch = false;
                    for (int d = 0; d < dimList.Count; d++)
                    {
                        if (((string[])dimList[d])[0] == dim && ((string[])dimList[d])[1] == split[4]) //getDelimitedColumn(code,4,"-"))
                        {
                            dimMatch = true;
                            break;
                        }
                    }
                    if (!dimMatch) return false;
                }
                return true;
            }
            else if (HT_UPCs.ContainsKey(code))
            {
                sku = (string)HT_UPCs[code];
                string[] skuList = (string[])HT_SKUs[sku];
                descp = skuList[1];
                price = skuList[2];
                packageQty = Convert.ToDecimal(skuList[7]);
                if (HT_QTYs.ContainsKey(code))
                    descp += " Snapshot Qty: " + (string)HT_QTYs[code];
                else
                    descp += " Snapshot Qty: 0";
                return true;
            }
            else if (HelperMethods.isUPCE(code) && HT_UPCs.ContainsKey(HelperMethods.UPCEtoUPCA(code.Substring(1, 6)) + HelperMethods.getUPCcheckDigit(HelperMethods.UPCEtoUPCA(code.Substring(1, 6)))))
            {
                sku = (string)HT_UPCs[HelperMethods.UPCEtoUPCA(code)];
                string[] skuList = (string[])HT_SKUs[sku];
                descp = skuList[1];
                price = skuList[2];
                packageQty = Convert.ToDecimal(skuList[7]);
                if (HT_QTYs.ContainsKey(sku))
                    descp += " Snapshot Qty: " + (string)HT_QTYs[code];
                else
                    descp += " Snapshot Qty: 0";
                return true;
            }
            else if ((code.StartsWith("2") && HelperMethods.isUPCA(code)) || (code.StartsWith("02") && HelperMethods.isEAN13(code)))
            {
                if (code.StartsWith("2")) code = code.Substring(1, 5);
                else code = code.Substring(2, 4);
                bool ret = Search_skuList(code, ref descp, ref price, ref packageQty, ref partialUPC);
                if (HT_QTYs.ContainsKey(code))
                    descp += " Snapshot Qty: " + (string)HT_QTYs[code];
                else
                    descp += " Snapshot Qty: 0";
                return ret;
            }
            return false;
        }
        public static bool Search_skuList(string code, ref string descp, ref string price, ref decimal packageQty, ref string partialUPC)
        {
            if (isAmber)
            {
                return Search_skuList_Amber(code, ref descp, ref price, ref packageQty, ref partialUPC);
            }
            code = code.TrimEnd();
            packageQty = 1;

            if (code == "") return false;
            if (HT_SKUs.Count == 0)
            {
                descp = "N/A, No Preloaded Sku List!";
                price = "0";
                return true;
            }
            if (HT_SKUs.ContainsKey(code))
            {
                string[] skuList = (string[])HT_SKUs[code];
                descp = skuList[1];
                price = skuList[2];
                packageQty = Convert.ToDecimal(skuList[3]);
                return true;
            }
            else if (HT_UPCs.ContainsKey(code))
            {
                string sku = (string)HT_UPCs[code];
                string[] skuList = (string[])HT_SKUs[sku];
                descp = skuList[1];
                price = skuList[2];
                packageQty = Convert.ToDecimal(skuList[3]);
                return true;
            }
            else if (HelperMethods.isUPCE(code) && HT_UPCs.ContainsKey(HelperMethods.UPCEtoUPCA(code.Substring(1, 6)) + HelperMethods.getUPCcheckDigit(HelperMethods.UPCEtoUPCA(code.Substring(1, 6)))))
            {
                string sku = (string)HT_UPCs[HelperMethods.UPCEtoUPCA(code)];
                string[] skuList = (string[])HT_SKUs[sku];
                descp = skuList[1];
                price = skuList[2];
                packageQty = Convert.ToDecimal(skuList[3]);
                return true;
            }
            else if ((code.StartsWith("2") && HelperMethods.isUPCA(code)) || ((code.StartsWith("02") || code.StartsWith("2")) && HelperMethods.isEAN13(code)))
            {
                if (HelperMethods.isUPCA(code)) code = "0" + code;
                string plu = code.Substring(2, 5);
                if (Search_skuList(plu, ref descp, ref price, ref packageQty, ref partialUPC))
                    return true;
                plu = code.Substring(2, 4);
                if (Search_skuList(plu, ref descp, ref price, ref packageQty, ref partialUPC))
                    return true;
            }
            else if (HelperMethods.isUPCA(code) || HelperMethods.isEAN13(code))
            {
                partialUPC = code;
                if (partialUPC.StartsWith("0"))  //try to remove leading zero
                {
                    partialUPC = partialUPC.Substring(1);
                    if (Search_skuList(partialUPC, ref descp, ref price, ref packageQty, ref partialUPC))
                        return true;
                }
                if (partialUPC.StartsWith("0")) //try to remove second leading zero
                {
                    partialUPC = partialUPC.Substring(1);
                    if (Search_skuList(partialUPC, ref descp, ref price, ref packageQty, ref partialUPC))
                        return true;
                }
                partialUPC = code;
                partialUPC = partialUPC.Substring(0, partialUPC.Length - 1); //try to remove check digit
                if (Search_skuList(partialUPC, ref descp, ref price, ref packageQty, ref partialUPC))
                    return true;
                if (partialUPC.StartsWith("0"))
                {
                    partialUPC = partialUPC.Substring(1);
                    if (Search_skuList(partialUPC, ref descp, ref price, ref packageQty, ref partialUPC))
                        return true;
                }
                if (partialUPC.StartsWith("0"))
                {
                    partialUPC = partialUPC.Substring(1);
                    if (Search_skuList(partialUPC, ref descp, ref price, ref packageQty, ref partialUPC))
                        return true;
                }
                partialUPC = "";
            }
            return false;
        }
        public static void loadCountList(Activity activity)
        {
            if (!File.Exists(HelperMethods.scanFile) || HelperMethods.HT_CountList.Count > 0) return;
            try
            {
                StreamReader fi = new StreamReader(File.OpenRead(HelperMethods.scanFile));
                string data = "";
                string descp = "";

                while ((data = fi.ReadLine()) != null)
                {
                    if (data.Length > 0)
                    {
                        string delimStr = ",";
                        char[] delimiter = delimStr.ToCharArray();
                        string[] split = data.Split(delimiter);

                        descp = "";
                        string price = "";
                        decimal packageQty = 1;
                        string partialUPC = "";
                        HelperMethods.Search_skuList(split[0], ref descp, ref price, ref packageQty, ref partialUPC);
                        CountedDetail cntdl = new CountedDetail();
                        if (partialUPC.Length > 0 && split[0].Contains(partialUPC))
                            cntdl.code = partialUPC;
                        else
                            cntdl.code = split[0];
                        cntdl.countedQty = split[1];
                        cntdl.description = descp;
                        cntdl.price = price;
                        if (split.Length > 2)
                            cntdl.subBatchTag = split[2];
                        else
                            cntdl.subBatchTag = "";
                        HelperMethods.HT_CountList.Insert(0, cntdl);
                    }
                }
                fi.Close();
            }
            catch (Exception ex)
            {
                HelperMethods.messageBox(activity, "Error", "Scan list file error:\n" + ex.Message);
            }
        }

        #region Type Checking Methods
        //---------------------------------------------------------------------
        public static TypeCheckReturn isInteger(string str)
        {
            int i = 0;

            //-----------------------------------
            // check if empty
            if (str.Length <= 0)
                return TypeCheckReturn.Valid;

            //-----------------------------------
            // if there is a negative sign (allowed)
            if (str[0] == '-')
            {
                // if there is ONLY a negative sign
                if (str.Length == 1)
                    return TypeCheckReturn.Incomplete;

                // else continue checking
                i++;
            }

            //-----------------------------------
            // check each character ensuring it
            // is valid for an Integer value
            while (i < str.Length)
            {
                if (str[i] < '0' || str[i] > '9')
                    return TypeCheckReturn.Invalid;

                i++;
            }
            try
            {
                int isInt = Convert.ToInt32(str);
            }
            catch
            {
                return TypeCheckReturn.Invalid;
            }
            return TypeCheckReturn.Valid;
        }

        public static TypeCheckReturn isDecimal(string str)
        {
            str = str.Replace(",", "").Replace("$", "").Replace("(", "-").Replace(")", "");
            int i = 0;
            bool dotcount = false;

            //-----------------------------------
            // check if empty
            if (str.Length <= 0)
                return TypeCheckReturn.Valid;
            // if the number is longer than 15 digits then it is not in a decimal format.
            if (str.Length > 15)
                return TypeCheckReturn.Invalid;

            //-----------------------------------
            // if there is a negative sign (allowed)
            if (str[0] == '-')
            {
                // if there is ONLY a negative sign
                if (str.Length == 1)
                    return TypeCheckReturn.Incomplete;

                // else continue checking
                i++;
            }
            if (str[0] == '.') str = "0" + str;

            //-----------------------------------
            // check each character ensuring it
            // is valid for a Decimal value
            while (i < str.Length)
            {
                if (str[i] == '.')
                {
                    // if a dot has already been found
                    // then invalid (only one dot allowed)
                    if (dotcount)
                        return TypeCheckReturn.Invalid;

                    // else dot has been found
                    dotcount = true;
                }
                else if (str[i] < '0' || str[i] > '9')
                {
                    return TypeCheckReturn.Invalid;
                }

                i++;
            }

            // get beggining, less a possible negative sign
            if (str[0] == '-') i = 1; else i = 0;
            // if the first character (less a possible negative sign) is a dot,
            // or the last character is a dot
            if (str[i] == '.' || str[str.Length - 1] == '.')
                return TypeCheckReturn.Incomplete;

            return TypeCheckReturn.Valid;
        }

        public static string fixIncompleteValue(string str)
        {
            bool negvalue = false;

            //-----------------------------------
            // string is empty, just set to zero
            if (str.Length <= 0)
            {
                return "0";
            }

            //-----------------------------------
            // if negative, then set mark,
            // and strip for checking
            if (str[0] == '-')
            {
                negvalue = true;
                str = str.Remove(0, 1);
            }

            //-----------------------------------
            // if the value (less a potential negative sign) is empty or just a dot
            if (str.Length <= 0 || (str.Length == 1 && str[0] == '.'))
                return "0";
            //-----------------------------------
            // check if its a numeric value
            try
            {
                str = Convert.ToDecimal(str).ToString();
                if (negvalue) str = "-" + str;

                return str;
            }
            catch
            {
                return "0";
            }
        }
        //---------------------------------------------------------------------
        #endregion


        #region String Manipulation
        //---------------------------------------------------------------------
        public static string Left(string str, int length)
        {
            try
            {
                if (str.Length <= length)
                    return str;
                return str.Substring(0, length);
            }
            catch
            {
                return str;
            }
        } 
        public static int ConvertToInt(object I)
        {
            try
            {
                return Convert.ToInt32(ConvertToDecimal(I));
            }
            catch
            {
                return 0;
            }
        }
        public static decimal ConvertToDecimal(object Money)
        {
            if (Money == null) return 0;
            string ret = Money.ToString().Replace("$" + " ", "").Replace("$", "").Replace("(", "-").Replace(")", "").Replace(",", "");
            if (ret == "True") return 1;
            ret = HelperMethods.fixIncompleteValue(ret);
            if (ret == "") ret = "0";
            return Convert.ToDecimal(ret);
        }

        //---------------------------------------------------------------------
        #endregion


        #region My Math.Round() Method:
        public static decimal Round(decimal d, int decimals)
        {
            //      Math.Round(4.5m,0)=4
            //      Math.Round(5.5m,0)=6
            //HelperMethods.Round(4.5m,0)=5

            if (d == 0m) return 0;
            decimal r = Math.Abs(d) + 5 * (decimal)Math.Pow(10, -1 * (decimals + 1));
            string str = r.ToString();
            str = str.Substring(0, str.IndexOf(".") + decimals + 1);
            r = Convert.ToDecimal(str);
            return d / Math.Abs(d) * r;
        }
        #endregion

        #region UPC-A UPC-E
        public static bool isUPCE(string upce)
        {
            if (!upce.StartsWith("0")) return false;
            if (upce.Length != 8) return false;
            if (HelperMethods.isInteger(upce) != HelperMethods.TypeCheckReturn.Valid) return false;
            if ("0" + upce.Substring(1, 6) + getUPCcheckDigit(UPCEtoUPCA(upce.Substring(1, 6))) != upce)
                return false;
            return true;
        }

        public static bool isUPCA(string upca)
        {
            if (upca.Length != 12) return false;
            if (HelperMethods.isInteger(upca.Substring(0, 6)) != HelperMethods.TypeCheckReturn.Valid) return false;
            if (HelperMethods.isInteger(upca.Substring(6, 6)) != HelperMethods.TypeCheckReturn.Valid) return false;
            if (upca.Substring(0, 11) + getUPCcheckDigit(upca.Substring(0, 11)) != upca) return false;
            return true;
        }
        public static bool isEAN13(string ean)
        {
            if (ean.Length != 13) return false;
            if (HelperMethods.isInteger(ean.Substring(0, 6)) != HelperMethods.TypeCheckReturn.Valid) return false;
            if (HelperMethods.isInteger(ean.Substring(6, 7)) != HelperMethods.TypeCheckReturn.Valid) return false;
            if (ean.Substring(0, 12) + getUPCcheckDigit(ean.Substring(0, 12)) != ean) return false;
            return true;
        }

        public static string UPCEtoUPCA(string upce)
        {
            //upce must be 6 digits number eg: 050103
            if (upce.Length != 6)
                throw new ApplicationException("Cannot convert UpcE to UpcA. Invalid UpcE " + upce);
            string manufacturer_code, product_code;
            string lastdigit = upce.Substring(5, 1);
            switch (lastdigit)
            {
                case "0":
                case "1":
                case "2":
                    manufacturer_code = upce.Substring(0, 2) + upce.Substring(5, 1) + "00";
                    product_code = "00" + upce.Substring(2, 3);
                    break;
                case "3":
                    manufacturer_code = upce.Substring(0, 3) + "00";
                    product_code = "000" + upce.Substring(3, 2);
                    break;
                case "4":
                    manufacturer_code = upce.Substring(0, 4) + "0";
                    product_code = "0000" + upce.Substring(4, 1);
                    break;
                default:
                    manufacturer_code = upce.Substring(0, 5);
                    product_code = "0000" + upce.Substring(5, 1);
                    break;

            }
            string upca = "0" + manufacturer_code + product_code;
            return upca; //without checkdigit 
        }
        public static string getUPCcheckDigit(string upca)
        {
            //upca must be 11 digits
            //EAN13 must be 12 digits
            //EAN8 must be 7 digits
            int sum = 0;
            bool odd_parity = true;
            for (int i = 0; i < upca.Length; i++)
            {
                int ui = Convert.ToInt16(upca.Substring(upca.Length - i - 1, 1));
                if (odd_parity)
                    sum += 3 * (ui);
                else
                    sum += ui;
                odd_parity = !odd_parity;
            }
            int check_digit = 10 - (sum % 10);
            if (check_digit == 10) check_digit = 0;
            return check_digit.ToString(); //==upca.Substring(11,1);
        }


        #endregion


        private static MediaPlayer player_ok;
        private static MediaPlayer player_pok;
        private static MediaPlayer player_e;

        public static void PlaySound(string sound)
        {
            //if (player_ok == null)
            //{
            //    player_ok = MediaPlayer.Create(MainActivity, Resource.Raw.OK);
            //    player_pok = MediaPlayer.Create(MainActivity, Resource.Raw.Package_OK);
            //    player_e = MediaPlayer.Create(MainActivity, Resource.Raw.Error);
            //}
            //if (sound == "ok")
            //    player_ok.Start();
            //else if (sound == "package_ok")
            //    player_pok.Start();
            //else
            //    player_e.Start();
            ToneGenerator generator = new ToneGenerator(Android.Media.Stream.Notification, 100);
            if (sound == "ok")
            {
                generator.StartTone(Tone.PropBeep);
                SystemClock.Sleep(300);
            }
            else if (sound == "package_ok")
            {
                generator.StartTone(Tone.PropBeep2);
                SystemClock.Sleep(300);
            }
            else
            {
                //generator.StartTone(Tone.SupError);
                generator.StartTone(Tone.CdmaAlertCallGuard);
                SystemClock.Sleep(1000);
            }
            generator.Release();
        }
        public static bool saveFileToCloudServer(string localFileName, string cloudFileName)
        {
            int iRetries = 0;
            int blockNumber = 0;
            int blockSize = 16 * 1024;
            int FileLength = HelperMethods.ConvertToInt(new System.IO.FileInfo(localFileName).Length.ToString());
            while (blockNumber < FileLength * 1m / blockSize)
            {
                try
                {
                    FileStream fs = new FileStream(localFileName, FileMode.Open, FileAccess.ReadWrite);
                    using (fs)
                    {
                        int len = FileLength - blockNumber * blockSize;
                        if (len > blockSize) len = blockSize;
                        Byte[] mybytearray = new Byte[len];
                        fs.Seek(blockNumber * blockSize, System.IO.SeekOrigin.Current);

                        fs.Read(mybytearray, 0, len);
                        com.mywoopos.POSLicenseService lsservice = new com.mywoopos.POSLicenseService();
                        lsservice.Timeout = 20000;
                        lsservice.SaveFile(mybytearray, cloudFileName.Remove(0, cloudFileName.LastIndexOf("\\") + 1), blockNumber, blockSize);
                        fs.Close();
                    }
                    blockNumber++;
                    iRetries = 0;
                    //frmRunning.UpdateProcessBar("Processed " + HelperMethods.ConvertToInt(blockNumber * blockSize * 100m / FileLength).ToString() + " of 100");
                    HelperMethods.MainActivity.RunOnUiThread(() =>
                    {
                        ProgressBarActivity.progress.Progress = HelperMethods.ConvertToInt(blockNumber * blockSize * 100m / FileLength);
                    });
                }
                catch (Exception ex)
                {
                    if (iRetries++ > 10)
                    {
                        HelperMethods.MainActivity.RunOnUiThread(() =>
                        {
                            ProgressBarActivity.progress.Progress = 100;
                            messageBox(HelperMethods.MainActivity, "Error", "Failed to upload scan list!");
                        });
                        return false;
                    }
                    System.Threading.Thread.Sleep(3000);
                }
            }
            HelperMethods.MainActivity.RunOnUiThread(() =>
            {
                ProgressBarActivity.progress.Progress = 100;

                var builder = new AlertDialog.Builder(HelperMethods.MainActivity);
                builder.SetMessage("Successfully uploaded scan list.\nDo you want to clear scan list to start a new batch?");
                builder.SetPositiveButton("Yes", (s, ee) =>
                {
                    File.Copy(scanFile, scanFile.Replace("ScanList", "ScanLis_").Replace(".txt", ".bak_"), true);
                    File.Delete(HelperMethods.scanFile);
                    HelperMethods.HT_CountList.Clear();
                    AppPreferences appPreferences = new AppPreferences(Android.App.Application.Context);
                    batchTag = "";
                    appPreferences.saveAccessKey("TAG", "");
                    HelperMethods.refreshListview();
                });
                builder.SetNegativeButton("No", (s, ee) =>
                {
                    messageBox(HelperMethods.MainActivity, "Warning", "To avoid overlapping during the next scanning session, please clear the scan list from the top-right menu!");
                });
                builder.Create().Show();
            }
            );
            return true;
        }

        public static bool getFileFromCloudServer(string localFileName, string cloudFileName)
        {
            int iRetries = 0;
            int blockNumber = 0;
            int FileLength = 0;
            int blockSize = 16 * 1024;
            cloudFileName = cloudFileName.Remove(0, cloudFileName.LastIndexOf("\\") + 1);
            try
            {
                com.mywoopos.POSLicenseService lsservice = new com.mywoopos.POSLicenseService();
                lsservice.Timeout = 20000;
                FileLength = (int)lsservice.GetFileLen(cloudFileName);
                if (FileLength == 0)
                    return false;
                if (System.IO.File.Exists(localFileName)) File.Delete(localFileName);
            }
            catch (Exception ex)
            {
                return false;
            }
            while (blockNumber < FileLength * 1m / blockSize)
            {
                try
                {
                    com.mywoopos.POSLicenseService lsservice = new com.mywoopos.POSLicenseService();
                    lsservice.Timeout = 20000;
                    long lStartPos = 0;
                    System.IO.FileStream fs;
                    if (System.IO.File.Exists(localFileName))
                    {
                        fs = System.IO.File.OpenWrite(localFileName);
                        lStartPos = blockNumber * blockSize;
                        fs.Seek(lStartPos, System.IO.SeekOrigin.Current);
                    }
                    else
                    {
                        fs = new System.IO.FileStream(localFileName, System.IO.FileMode.Create);
                        lStartPos = 0;
                    }
                    using (fs)
                    {
                        int len = FileLength - blockNumber * blockSize;
                        if (len > blockSize) len = blockSize;
                        Byte[] mybytearray = new Byte[len];
                        mybytearray = lsservice.GetFile(cloudFileName, blockNumber, blockSize);
                        fs.Write(mybytearray, 0, len);
                        fs.Close();
                    }
                    blockNumber++;
                    iRetries = 0;
                    //frmRunning.UpdateProcessBar("Processed " + HelperMethods.ConvertToInt(blockNumber * blockSize * 100m / FileLength).ToString() + " of 100");
                    HelperMethods.MainActivity.RunOnUiThread(() =>
                    {
                        ProgressBarActivity.progress.Progress = HelperMethods.ConvertToInt(blockNumber * blockSize * 100m / FileLength);
                    });
                }
                catch (Exception ex)
                {
                    if (iRetries++ > 10)
                    {
                        HelperMethods.MainActivity.RunOnUiThread(() =>
                        {
                            ProgressBarActivity.progress.Progress = 100;
                            messageBox(HelperMethods.MainActivity, "Error", "Failed to download Sku list!");
                        });
                        return false;
                    }
                    System.Threading.Thread.Sleep(3000);
                }
            }
            new MyMediaScannerConnectionClient(localFileName);
            HelperMethods.MainActivity.RunOnUiThread(() =>
            {
                ProgressBarActivity.progress.Progress = 100;
                loadSkus(HelperMethods.MainActivity);
                messageBox(HelperMethods.MainActivity, "Succeed", "Successfully downloaded Sku list.!");
            }
            );
            return true;
        }
    }//END: HelperMethods
    public class MyMediaScannerConnectionClient : Java.Lang.Object, Android.Media.MediaScannerConnection.IMediaScannerConnectionClient
    {
        private Android.Media.MediaScannerConnection msc;
        private string pathFilename;
        public MyMediaScannerConnectionClient(string pathFilename)
        {
            this.pathFilename = pathFilename;
            this.msc = new Android.Media.MediaScannerConnection(Application.Context, this);
            this.msc.Connect();
        }
        public void OnMediaScannerConnected()
        {
            this.msc.ScanFile(this.pathFilename, null);
        }
        public void OnScanCompleted(string path, Android.Net.Uri uri)
        {
            this.msc.Disconnect();
        }
    }
    public class CountedDetail
    {
        public string code { get; set; }
        public string countedQty { get; set; }
        public string description { get; set; }
        public string subBatchTag { get; set; }
        public string price { get; set; }
    }

    public class AppPreferences
    {
        private ISharedPreferences mSharedPrefs;
        private ISharedPreferencesEditor mPrefsEditor;
        private Context mContext;

        public AppPreferences(Context context)
        {
            this.mContext = context;
            mSharedPrefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            mPrefsEditor = mSharedPrefs.Edit();
        }

        public void saveAccessKey(string key, string value)
        {
            mPrefsEditor.PutString("WooPOS_" + key, value);
            mPrefsEditor.Commit();
        }

        public string getAccessKey(string key)
        {
            return mSharedPrefs.GetString("WooPOS_" + key, "");
        }
    }
}