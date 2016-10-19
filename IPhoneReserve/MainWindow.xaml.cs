using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Net;
using Microsoft.Win32;
using Newtonsoft.Json;
using IR.Common;

namespace IPhoneReserve
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string reserveUrl = "https://reserve.cdn-apple.com/HK/zh_HK/reserve/iPhone/availability";
        //private readonly string reserveUrl = "https://reserve.cdn-apple.com/CN/zh_CN/reserve/iPhone/availability";

        private Dictionary<string, string> models = new Dictionary<string, string>()
        {
            {"MN8G2ZP/A" , "i7-32-Black" },
            {"MN492ZP/A" , "i7-Plus-32-Sliver" },
            {"MNQK2ZP/A" , "i7-Plus-32-Gold" },
            {"MN8N2ZP/A" , "i7-128-Gold" },
            {"MN8Q2ZP/A" , "i7-128-Jet" },
            {"MN4L2ZP/A" , "i7-Plus-256-Jet" },
            {"MN8K2ZP/A" , "i7-32-Rose" },
            {"MN4F2ZP/A" , "i7-Plus-256-Sliver" },
            {"MN8U2ZP/A" , "i7-256-Gold" },
            {"MN8R2ZP/A" , "i7-256-Black" },
            {"MNQJ2ZP/A" , "i7-Plus-32-Sliver" },
            {"MN8H2ZP/A" , "i7-32-Silver" },
            {"MN4C2ZP/A" , "i7-Plus-128-Rose" },
            {"MN4D2ZP/A" , "i7-Plus-128-Jet" },
            {"MN4J2ZP/A" , "i7-Plus-256-Gold" },
            {"MN4A2ZP/A" , "i7-Plus-128-Gold" },
            {"MN8L2ZP/A" , "i7-128-Black" },
            {"MN8T2ZP/A" , "i7-256-Sliver" },
            {"MN8V2ZP/A" , "i7-256-Rose" },
            {"MNQH2ZP/A" , "i7-Plus-32-Black" }, 
            {"MN8W2ZP/A" , "i7-256-Jet" },
            {"MN4E2ZP/A" , "i7-Plus-256-Black" },
            {"MN8J2ZP/A" , "i7-32-Gold" },
            {"MN8M2ZP/A" , "i7-128-Sliver" },
            {"MN4K2ZP/A" , "i7-Plus-256-Rose" },
            {"MN482ZP/A" , "i7-Plus-128-Black" },
            {"MNQL2ZP/A" , "i7-Plus-32-Rose" },
            {"MN8P2ZP/A" , "i7-128-Rose" }
        };

        private NotifyIcon notify;

        private volatile bool _stop = true;

        private ObservableCollection<Product> cache;

        private WebClient client;

        private int _available;

        public MainWindow()
        {
            InitializeComponent();
         
            this.notify = new NotifyIcon();
            this.notify.Icon = IPhoneReserve.Properties.Resources.monitor;
             
            this.notify.Click += Notify_Click;
            
            this.client = new WebClient();
            this.client.Encoding = UTF8Encoding.UTF8; 
        }
 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.notify.Visible = true;
        }

        private void Notify_Click(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Activate();
                           
            this.Show();
        }         

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Uri uri;

            string json = this.reserveUrl + ".json";
            
            if (Uri.TryCreate(json, UriKind.Absolute, out uri))
            {
                this.AddLog("Start ...");

                this._available = 0;
                this._stop = false;

                Task.Run(new Action(() => {
                    this.checkIR(uri);
                }));
            }
        }

        private async void checkIR(Uri uri)
        { 
            string str = "";
            
            while (!this._stop)
            {
                try
                {
                    str = await client.DownloadStringTaskAsync(uri);

                    if (!string.IsNullOrEmpty(str) && str != "{ }")
                    {
                        this.bindData(str);
                    }
                                        
                    await Task.Delay(3000);
                }
                catch (Exception ex)
                {
                    this.AddLog(ex.Message);

                    this._stop = true;

                    break;
                }
            }

            this.Dispatcher.Invoke(new Action(() => {
                if (this.cache != null)
                    this.cache.Clear();
            }));
            
            this.AddLog("Stop.");
        }

        private void bindData(string str)
        {
            var definition = new
            {
                R485 = new Dictionary<string, object>(),
                R409 = new Dictionary<string, object>(),
                R428 = new Dictionary<string, object>(),
                R610 = new Dictionary<string, object>(),
                R499 = new Dictionary<string, object>(),
                updated = ""
            };

            var jsonAttr = JsonConvert.DeserializeAnonymousType(str, definition);

            //Causeway Bay R409
            var qw1 = from a in jsonAttr.R409
                      where a.Key != "timeSlot"
                      select new Product() { Name = models[a.Key], ModelNumber = a.Key, Shop = "R409", Status = a.Value.ToString() };

            //Festival Walk
            var qw2 = from a in jsonAttr.R485
                      where a.Key != "timeSlot"
                      select new Product() { Name = models[a.Key], ModelNumber = a.Key, Shop = "R485", Status = a.Value.ToString() };

            //IFC
            var qw3 = from a in jsonAttr.R428
                      where a.Key != "timeSlot"
                      select new Product() { Name = models[a.Key], ModelNumber = a.Key, Shop = "R428", Status = a.Value.ToString() };

            //TST
            var qw4 = from a in jsonAttr.R499
                      where a.Key != "timeSlot"
                      select new Product() { Name = models[a.Key], ModelNumber = a.Key, Shop = "R499", Status = a.Value.ToString() };

            //ShaTin
            var qw5 = from a in jsonAttr.R610
                      where a.Key != "timeSlot"
                      select new Product() { Name = models[a.Key], ModelNumber = a.Key, Shop = "R610", Status = a.Value.ToString() };

            List<Product> list = new List<Product>();

            list.AddRange(qw1);
            list.AddRange(qw2);
            list.AddRange(qw3);
            list.AddRange(qw4);
            list.AddRange(qw5);

            var q = list.Where(p => p.Available).ToList();

            var dt = this.ToDatetime(jsonAttr.updated);

            this.cache = new ObservableCollection<Product>(q);

            this.Dispatcher.Invoke(new Action(() => {

                this.listView.ItemsSource = this.cache;
                this.txtStatus.Text = "Update: " + dt.ToString();

            }));

            int n = q.Count;

            if (n != this._available)
            {
                if (n > 0)
                {
                    this._available = n;

                    this.Dispatcher.Invoke(new Action( () => {
                        this.WindowState = WindowState.Normal;
                        this.Activate();
                    }));                    
                }

                this.AddLog(DateTime.Now.ToString() + ", find " + n + ".");
            } 

            if (n == 0 && this._available > 0)
            {
                this._available = 0;

                this.AddLog(DateTime.Now.ToString() + ", sold out.");                
            }            
        }

        private DateTime ToDatetime(string str)
        {
            long unixDate = long.Parse(str);
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return start.AddMilliseconds(unixDate).ToLocalTime();
        }

        private void AddLog(string text)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.logText.AppendText(text + "\r\n");
            }));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.logText.Document.Blocks.Clear();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this._stop = true;
        }
 
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();

                this.notify.Visible = true;
            } 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this._stop)
            {
                e.Cancel = false;
            }
            else
            { 
                e.Cancel = true;
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string fmt = "partNumber={0}&channel=1&rv=&path=&sourceID=&iPP=false&appleCare=&iUID=&iuToken=&carrier=&store={1}";

            string postUrl = this.reserveUrl;
             
            var item = this.listView.SelectedItem as Product;
            
            if (item != null)
            {
                WebBrowser browser = new WebBrowser();

                browser.URL = this.reserveUrl + "?channel=1";
                browser.Shop = item.Shop;

                browser.Show();

                this.listView.SelectedIndex = -1;
            }           
        }
 
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser browser = new WebBrowser();
            browser.URL = this.reserveUrl + "?channel=1";
           // browser.Shop = "R401";

            browser.Show();
        }

        private void SetIE()
        {
            int RegVal = 11001;
            
            // set the actual key
            RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);

            try
            {
                Key.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);
                Key.Close();
            }
            catch(Exception ex)
            {
                this.AddLog(ex.Message);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.SetIE();
        }
    }
}
