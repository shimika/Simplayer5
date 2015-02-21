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

namespace Simplayer5 {
	public partial class MainWindow : Window {
		DispatcherTimer UpdateTimer = null;

		private void CheckUpdate(bool force) {
			if (UpdateTimer == null) {
				UpdateTimer = new DispatcherTimer();
				UpdateTimer.Interval = TimeSpan.FromHours(1);
				UpdateTimer.Tick += UpdateTimer_Tick;
				UpdateTimer.Start();
			}

			buttonUpdate.Visibility = Visibility.Collapsed;
			textNewVersion.Text = "???";
			ver = NewUrl = "";
			textNewVersion.Foreground = Brushes.Black;

			WebClient web = new WebClient();
			web.DownloadStringCompleted += web_DownloadStringCompleted;
			web.DownloadStringAsync(new Uri("https://dl.dropboxusercontent.com/u/95054900/Simplayer.txt"), force);
		}

		private void UpdateTimer_Tick(object sender, EventArgs e) {
			CheckUpdate(false);
		}

		string ver = "", NewUrl = "";
		private void web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			lock (ver) {
				bool force = (bool)e.UserState;

				if (e.Error != null) {
					textNewVersion.Text = "???";
					ver = NewUrl = "";
					textNewVersion.Foreground = Brushes.Black;
					return;
				}

				try {
					string[] res = e.Result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
					ver = res[0];
					NewUrl = res[1];

					textNewVersion.Text = ver;

					if (ver == Version.NowVersion) {
						textNewVersion.Foreground = Brushes.Green;
						NewUrl = "";

						if (force) {
							Notice("Simplayer가 최신입니다.");
						}
					} else {
						buttonUpdate.Visibility = Visibility.Visible;
						textNewVersion.Foreground = Brushes.Crimson;
					}
				} catch {
					ver = NewUrl = "";
					textNewVersion.Foreground = Brushes.Black;
				}
			}
		}

		private void CheckUpdate_MouseDown(object sender, MouseButtonEventArgs e) {
			if (NewUrl == "") {
				CheckUpdate(true);
			} else {
				try {
					Process.Start(NewUrl);
				} catch { }
			}
		}

		private void buttonUpdate_Click(object sender, RoutedEventArgs e) {
			try {
				UpdateDownload();
			} catch { }
		}

		public static string UpdateFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Shimika\";

		BackgroundWorker BwUpdate;
		bool isUpdating = false;
		private void UpdateDownload() {
			if (isUpdating) { return; }
			if (NewUrl == "") { return; }

			if (!Directory.Exists(UpdateFolder)) {
				Directory.CreateDirectory(UpdateFolder);
			}

			Notice("업데이트를 시작합니다");
			StartUpdateIndicator();

			BwUpdate = new BackgroundWorker() {
				WorkerSupportsCancellation = true,
			};
			BwUpdate.DoWork += BwUpdate_DoWork;
			BwUpdate.RunWorkerCompleted += BwUpdate_RunWorkerCompleted;

			List<string> list = new List<string>();
			// Path, project, 

			list.Add(string.Format("{0}updater.exe", UpdateFolder));
			list.Add(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
			list.Add(Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName));
			list.Add(NewUrl);

			BwUpdate.RunWorkerAsync(list);
		}

		private void BwUpdate_DoWork(object sender, DoWorkEventArgs e) {
			List<string> list = (List<string>)e.Argument;

			string updater = list[0];
			string path = list[1];
			string project = list[2];
			string url = list[3];

			WebClient web = new WebClient();
			web.DownloadFile("https://dl.dropboxusercontent.com/u/95054900/Shimika/Updater.exe", updater);
			web.DownloadFile(url, string.Format("{0}\\{1}_update.exe", path, project));

			e.Result = list;
		}

		private void BwUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) {
				StopUpdateIndicator();
				isUpdating = false;
				return;
			}

			List<string> list = (List<string>)e.Result;
			string updater = list[0];
			string path = list[1];
			string project = list[2];
			int id = Process.GetCurrentProcess().Id;

			try {
				if (!File.Exists(updater)) {
					throw new Exception("Can't get updater");
				}

				if (!File.Exists(string.Format("{0}\\{1}_update.exe", path, project))) {
					throw new Exception("Update error");
				}

				Process pro = new Process();
				pro.StartInfo = new ProcessStartInfo();
				pro.StartInfo.FileName = updater;
				pro.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" {2}", project, path, id);
				pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				pro.Start();

				this.Close();
			} catch (Exception ex) {
				Notice("업데이트를 실패했습니다.");
			}

			StopUpdateIndicator();
			isUpdating = false;
		}

		DispatcherTimer timerUpdateIndicator;
		int turnUpdate;

		// Torrent

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
