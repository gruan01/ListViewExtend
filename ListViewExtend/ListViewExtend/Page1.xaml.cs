using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ListViewExtend {
    public partial class Page1 : ContentPage {

        public List<string> Datas {
            get;
            set;
        }

        public Page1() {
            InitializeComponent();

            this.Datas = new List<string>() {
                "A","B","C","D","E","F","G","A","B","C","D","E","F","G","A","B","C","D","E","F","G","A","B","C","D","E","F","G"
            };

            this.BindingContext = this;
        }

        void RefreshData(object sender, EventArgs e) {
            var lst = (ListView)sender;
            Task.Delay(10000).ContinueWith(t => {
                Device.BeginInvokeOnMainThread(() => {
                    lst.IsRefreshing = false;
                });
            });
        }
    }
}
