using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		private void CheckUpdate() {
			WebClient web = new WebClient();
			web.DownloadStringCompleted+=web_DownloadStringCompleted;
			web.DownloadStringAsync(new Uri("https://dl.dropboxusercontent.com/u/95054900/Simplayer.txt"));
		}

		string ver = "", url = "";
		bool isForce = false;
		private void web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			if (e.Error != null) {
				textNewVersion.Text = "???";
				ver = url = "";
				textNewVersion.Foreground = Brushes.Black;
				return;
			}

			try {
				string[] res = e.Result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				ver = res[0];
				url = res[1];

				textNewVersion.Text = ver;

				if (ver == Setting.Version) {
					textNewVersion.Foreground = Brushes.Green;
					if (isForce) {
						Notice("Simplayer가 최신입니다.");
					}
				} else {
					textNewVersion.Foreground = Brushes.Crimson;
				}
			} catch {
				textNewVersion.Foreground = Brushes.Black;
			}
		}

		private void CheckUpdate_MouseDown(object sender, MouseButtonEventArgs e) {
			if (ver == "") {
				CheckUpdate();
			} else if (ver == Setting.Version) {
				isForce = true;
				CheckUpdate();
			} else {
				try {
					Process.Start(url);
				} catch { }
			}
		}
	}
}
