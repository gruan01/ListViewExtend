using ListViewExtend.WinPhone.Renders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;
using SC = System.Windows.Controls;
using SM = System.Windows.Media;
using SA = System.Windows.Media.Animation;

[assembly: ExportRenderer(typeof(ListView), typeof(ListViewRender))]
namespace ListViewExtend.WinPhone.Renders {
    public class ListViewRender : ListViewRenderer {

        private SC.Viewbox Viewbox = null;

        private SC.Border Border = null;

        private SC.Grid Grid = null;

        private SA.Storyboard SB = null;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e) {
            base.OnElementChanged(e);

            this.Grid = new SC.Grid() {
                Visibility = System.Windows.Visibility.Collapsed
                //不起作用
                //HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch 
            };

            this.Viewbox = new SC.Viewbox();

            var brush = new SM.SolidColorBrush(SM.Colors.White);
            var rotate = new SM.RotateTransform();
            this.Border = new SC.Border() {
                Background = new SM.ImageBrush() {
                    ImageSource = new BitmapImage(new Uri("loading.jpg", UriKind.Relative))
                },
                BorderBrush = brush,
                BorderThickness = new System.Windows.Thickness(5),
                RenderTransformOrigin = new System.Windows.Point(0.5, 0.5),
                RenderTransform = rotate
            };

            this.Grid.Children.Add(this.Viewbox);
            this.Viewbox.Child = this.Border;
            this.Children.Add(this.Grid);


            this.SB = new SA.Storyboard() {
                Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                AutoReverse = true,
                RepeatBehavior = SA.RepeatBehavior.Forever
            };

            var colorAnimation = new SA.ColorAnimation() {
                AutoReverse = true,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                From = SM.Colors.Green,
                To = SM.Colors.Orange,
                By = SM.Colors.Red
            };
            this.SB.Children.Add(colorAnimation);
            SA.Storyboard.SetTarget(colorAnimation, brush);
            SA.Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(SM.SolidColorBrush.ColorProperty));

            var angelAnimation = new SA.DoubleAnimation() {
                //AutoReverse = true,
                Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                From = 0,
                To = 360
            };

            this.SB.Children.Add(angelAnimation);
            SA.Storyboard.SetTarget(angelAnimation, rotate);
            SA.Storyboard.SetTargetProperty(angelAnimation, new PropertyPath(SM.RotateTransform.AngleProperty));

            this.OverrideEvents();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ListView.IsRefreshingProperty.PropertyName) {
                this.UpdateIsRefreshing();
            }
        }

        private void OverrideEvents() {
            this.ClearEventHandlers(this.Control, "PullToRefreshStarted");
            this.ClearEventHandlers(this.Control, "PullToRefreshCompleted");
            this.ClearEventHandlers(this.Control, "PullToRefreshCanceled");
            this.ClearEventHandlers(this.Control, "PullToRefreshStatusUpdated");

            this.AddEventHandler(this.Control, "PullToRefreshStarted", new EventHandler(OnPullToRefreshStarted));
            this.AddEventHandler(this.Control, "PullToRefreshCompleted", new EventHandler(OnPullToRefreshCompleted));
            this.AddEventHandler(this.Control, "PullToRefreshCanceled", new EventHandler(OnPullToRefreshCanceled));
            this.AddEventHandler(this.Control, "PullToRefreshStatusUpdated", new EventHandler(OnPullToRefreshStatusUpdated));
        }

        protected override void UpdateNativeWidget() {
            base.UpdateNativeWidget();
            if (this.Grid == null)
                return;
            this.Grid.Width = this.Element.Width;
        }


        private void UpdateIsRefreshing() {
            this.Grid.Visibility = this.Element.IsRefreshing ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (this.Element.IsRefreshing)
                this.SB.Begin();
            else
                this.SB.Stop();
        }

        private void OnPullToRefreshStatusUpdated(object sender, EventArgs e) {
            if (!this.Element.IsPullToRefreshEnabled)
                return;

            var s = this.GetProperty<double>(this.Control, "PullToRefreshStatus");

            s = s > 1 ? 1 : s;
            if (s > 0) {
                var w = s * 100;
                this.Viewbox.Width = this.Viewbox.Height = w;
                this.Border.Width = this.Border.Height = w;
                this.Border.CornerRadius = new CornerRadius(w / 2);
            } else {
                this.Viewbox.Width = this.Viewbox.Height = 1;
            }
        }

        private void OnPullToRefreshStarted(object sender, EventArgs args) {
            if (!this.Element.IsPullToRefreshEnabled)
                return;

            this.Grid.Visibility = Visibility.Visible;
            this.Viewbox.Width = this.Viewbox.Height = 1;
        }

        private void OnPullToRefreshCanceled(object sender, EventArgs args) {
            if (!this.Element.IsPullToRefreshEnabled)
                return;
            this.Grid.Visibility = Visibility.Collapsed;
        }

        private void OnPullToRefreshCompleted(object sender, EventArgs args) {
            if (!this.Element.IsPullToRefreshEnabled)
                return;
            this.Grid.Visibility = Visibility.Collapsed;
            //((IListViewController)this.Element).SendRefreshing();
            this.Element.BeginRefresh();
        }

        private void ClearEventHandlers(object obj, string evtName) {
            var t = obj.GetType();
            //var fs = t.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            var evt = t.GetEvent(evtName);
            var f1 = t.GetField(evtName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (evt != null && f1 != null) {
                var handler = (EventHandler)f1.GetValue(obj);
                var ds = handler.GetInvocationList();
                evt.RemoveEventHandler(obj, handler);
            }
        }

        private void AddEventHandler(object obj, string evtName, Delegate handler) {
            var t = obj.GetType();
            var evt = t.GetEvent(evtName);
            evt.AddEventHandler(obj, handler);
        }

        private T GetProperty<T>(object obj, string propName) {
            var t = obj.GetType();
            var prop = t.GetProperty(propName);
            if (prop != null)
                return (T)prop.GetValue(obj);
            else
                return default(T);
        }
    }
}
