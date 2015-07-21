using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ShimiKore;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		Updater updater = null;

		private void InitUpdater() {
			updater = new Updater("Simplayer5", Version.NowVersion);
			updater.UpdateAvailable += UpdateAvailable;
			updater.UpdateComplete += UpdateComplete;

			updater.UpdateCheck();
		}

		private void UpdateCheck() {
			buttonUpdate.Visibility = Visibility.Collapsed;
			textNewVersion.Text = "???";
			textNewVersion.Foreground = Brushes.Black;

			updater.UpdateCheck();
		}

		private void UpdateAvailable(object sender, UpdateArgs e) {
			textNewVersion.Text = e.NewVersion;

			if (e.IsOld) {
				buttonUpdate.Visibility = Visibility.Visible;
				textNewVersion.Foreground = Brushes.Crimson;
			}
			else {
				textNewVersion.Foreground = Brushes.Green;
				//Notice("Simplayer가 최신입니다.");
			}
		}

		private void UpdateDownload() {
			Notice("업데이트를 시작합니다");
			StartUpdateIndicator();

			updater.UpdateApplication();
		}

		private void UpdateComplete(object sender, UpdateCompleteArgs e) {
			if (e.Complete) {
				try { PrevWindow.Close(); } catch { }
				try { LyricsWindow.Close(); } catch { }

				Setting.SaveSetting();
				TrayNotify.Dispose();
			}
			else {
				StopUpdateIndicator();
				Notice("업데이트를 실패했습니다.");
			}
		}

		private void CheckUpdate_MouseDown(object sender, MouseButtonEventArgs e) {
			UpdateCheck();
		}

		private void buttonUpdate_Click(object sender, RoutedEventArgs e) {
			UpdateDownload();
		}
	}
}
