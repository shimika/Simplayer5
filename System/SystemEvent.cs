using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		[DllImport("user32")]
		static extern IntPtr GetDesktopWindow();
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		private void Window_Activated(object sender, EventArgs e) {
			grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(100)));
		}
		private void Window_Deactivated(object sender, EventArgs e) {
			grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.2, TimeSpan.FromMilliseconds(100)));
		}

		private void MainPlayer_MouseDown(object sender, MouseButtonEventArgs e) {
			try { DragMove(); } catch { }
		}

		private void buttonMin_Click(object sender, RoutedEventArgs e) {
			MinimizeWindow();
		}

		private void MinimizeWindow() {
			if (Setting.IsVisible) {
				Setting.IsVisible = false;

				if (Setting.MinToTray) {
					FocusZOrderWindow();
					AnimateWindow(0);
				} else {
					this.WindowState = WindowState.Minimized;
				}
			}
		}

		private void ResumeWindow() {
			this.Activate();

			if (!Setting.IsVisible) {
				Setting.IsVisible = true;

				if (Setting.MinToTray) {
					AnimateWindow(1, LastY);
				} else {
					this.WindowState = WindowState.Normal;
				}
			}
		}

		private void MainWindow_StateChanged(object sender, EventArgs e) {
			if (this.WindowState == System.Windows.WindowState.Normal) {
				ResumeWindow();
			}
		}

		private void FocusZOrderWindow() {
			IntPtr nextPtr = GetDesktopWindow();
			int zOrder = int.MaxValue;
			foreach (Process p in Process.GetProcesses(".")) {
				try {
					if (p.MainWindowTitle.Length > 0) {
						if (p.MainWindowHandle == Process.GetCurrentProcess().MainWindowHandle) { continue; }

						int z = GetZOrder(p.MainWindowHandle);
						if (zOrder > z) {
							zOrder = z;
							nextPtr = p.MainWindowHandle;
						}
					}
				} catch { }
			}

			SetForegroundWindow(nextPtr);
		}

		private int GetZOrder(IntPtr handle) {
			int z = 0;
			do {
				z++;
				handle = GetWindow(handle, 3);
			} while (handle != IntPtr.Zero);

			return z;
		}

		double LastY = 0;
		private void AnimateWindow(int opacity, double top = 0) {
			if (opacity == 0) {
				this.IsHitTestVisible = false;

				LastY = this.Top;
				top = LastY - 50;
				new AltTab().HideAltTab(this);
			} else {
				this.IsHitTestVisible = true;
				new AltTab().ShowAltTab(this);
			}

			Storyboard sb = new Storyboard();
			sb.Children.Add(GetOpacityAnimation(opacity, this));
			sb.Children.Add(GetTopAnimation(top, this, 250, 50));
			sb.Begin(this);
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			try {
				PrevWindow.Close();
			} catch { }

			Setting.SaveSetting();
			TrayNotify.Dispose();
			System.Windows.Application.Current.Shutdown();
		}

		public bool ProcessCommandLineArgs(IList<string> args) {
			if (args == null || args.Count == 0) { return true; }

			if (args.Count > 1) {
				switch (args[1]) {
					case "--forceclose":
						this.Close();
						break;
				}
			}

			ResumeWindow();
			return true;
		}
	}
}
