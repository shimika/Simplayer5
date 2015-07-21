using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		// Change main theme color
		public void ChangeThemeColor(Color color) {
			MainColor = color;
			grideffectShadow.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(250)));

			Application.Current.Resources["sColor"] = new SolidColorBrush(MainColor);
			Application.Current.Resources["tColor"] = new SolidColorBrush(Color.FromArgb(40, MainColor.R, MainColor.G, MainColor.B));
			Application.Current.Resources["cColor"] = Color.FromArgb(255, MainColor.R, MainColor.G, MainColor.B);
			MainBrush = FindResource("sColor") as SolidColorBrush;
		}

		// Get Animation variables
		private ThicknessAnimation GetThicknessAnimation(double duration, double left, double top, FrameworkElement fe = null, double right = 0, double bottom = 0, double delay = 0) {
			ThicknessAnimation ta = new ThicknessAnimation(
					new Thickness(left, top, right, bottom),
					TimeSpan.FromMilliseconds(duration)) {
						EasingFunction = new PowerEase() {
							Power = 5, EasingMode = EasingMode.EaseOut,
						},
						BeginTime = TimeSpan.FromMilliseconds(delay)
					};

			if (fe != null) {
				Storyboard.SetTarget(ta, fe);
				Storyboard.SetTargetProperty(ta, new PropertyPath(FrameworkElement.MarginProperty));
			}

			return ta;
		}
		private DoubleAnimation GetDoubleAnimation(double opacity, FrameworkElement fe, double duration = 250, double delay = 0) {
			DoubleAnimation da = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(duration)) {
				BeginTime = TimeSpan.FromMilliseconds(delay),
			};
			Storyboard.SetTarget(da, fe);
			Storyboard.SetTargetProperty(da, new PropertyPath(FrameworkElement.OpacityProperty));

			return da;
		}

		private ThicknessAnimation GetScrollAnimation(int start, int end, double duration) {
			duration = Math.Min(400, duration);
			duration = Math.Max(150, duration);

			return new ThicknessAnimation(
				new Thickness(0, start, 0, 0), new Thickness(0, end, 0, 0),
				TimeSpan.FromMilliseconds(duration)) {
					EasingFunction = new PowerEase() {
						Power = 5, EasingMode = EasingMode.EaseOut,
					},
				};
		}


		private static DoubleAnimation GetDoubleAnimation(double val, double duration = 250, double delay = 0) {
			DoubleAnimation da = new DoubleAnimation(val, TimeSpan.FromMilliseconds(duration)) {
				BeginTime = TimeSpan.FromMilliseconds(delay),
			};

			return da;
		}

		public static DoubleAnimation GetOpacityAnimation(double opacity, FrameworkElement fe = null, double duration = 250, double delay = 0) {
			DoubleAnimation da = GetDoubleAnimation(opacity, duration, delay);
			if (fe != null) {
				Storyboard.SetTarget(da, fe);
				Storyboard.SetTargetProperty(da, new PropertyPath(FrameworkElement.OpacityProperty));
			}

			return da;
		}

		public static DoubleAnimation GetTopAnimation(double top, FrameworkElement fe = null, double duration = 250, double delay = 0) {
			DoubleAnimation da = GetDoubleAnimation(top, duration, delay);
			da.EasingFunction = new PowerEase() {
				Power = 3, EasingMode = EasingMode.EaseOut,
			};
			da.FillBehavior = FillBehavior.Stop;
			if (fe != null) {
				Storyboard.SetTarget(da, fe);
				Storyboard.SetTargetProperty(da, new PropertyPath(Window.TopProperty));
			}

			return da;
		}

		DispatcherTimer timerUpdateIndicator;
		int turnUpdate;

		private void StartUpdateIndicator() {
			if (timerUpdateIndicator == null) {
				timerUpdateIndicator = new DispatcherTimer();
				timerUpdateIndicator.Interval = TimeSpan.FromMilliseconds(500);
				timerUpdateIndicator.Tick += timerUpdateIndicator_Tick;
			}
			turnUpdate = 0;
			timerUpdateIndicator.Start();
		}

		private void StopUpdateIndicator() {
			if (timerUpdateIndicator != null) {
				timerUpdateIndicator.Stop();
			}
			buttonUpdate.Opacity = 1;
		}

		private void timerUpdateIndicator_Tick(object sender, EventArgs e) {
			turnUpdate = (turnUpdate + 1) % 2;
			buttonUpdate.Opacity = turnUpdate;

		}
	}
}
