using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using mshtml; 

namespace IPhoneReserve
{ 
    /// <summary>
    /// Interaction logic for WebBrowser.xaml
    /// </summary>
    public partial class WebBrowser : Window
    {
        public string URL { get; set; } 
        public string Shop { get; set; }

        public WebBrowser()
        {
            InitializeComponent();
 
            this.myIE.LoadCompleted += MyIE_LoadCompleted;
        }

        private void MyIE_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Shop))
            { 
                var doc = this.myIE.Document as HTMLDocument;

                var head = doc.getElementsByTagName("head").Cast<HTMLHeadElement>().First();

                var  script = doc.createElement("script") as IHTMLScriptElement;
                script.type = "text/javascript";
                script.src = "https://code.jquery.com/jquery-1.12.4.js";

                head.appendChild((IHTMLDOMNode)script);

                var location = doc.getElementById("selectStore") as HTMLSelectElement;

                //foreach (HTMLOptionElement option in location.getElementsByTagName("option"))
                //{
                //    if (option.getAttribute("value") == this.Shop)
                //    {
                //        option.selected = true; 
                //    }
                //}

                var family = doc.getElementById("selectSubfamily") as HTMLSelectElement;

                family.disabled = false;

            }

            this.Shop = "";
        }
 
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            this.myIE.Navigate(this.txtUrl.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.URL))
            {
                this.myIE.Navigate(this.URL);
            }         
        }

        private void myIE_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            this.txtUrl.Text = e.Uri.ToString();
        } 
    }
}
