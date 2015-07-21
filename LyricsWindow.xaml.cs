using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simplayer5 {
	/// <summary>
	/// LyricsWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class LyricsWindow : Window {
		public const int WS_EX_TRANSPARENT = 0x00000020;
		public const int GWL_EXSTYLE = (-20);
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hwnd, int index);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
		}

		public bool isShowLyrics = true;
		public LyricsWindow(bool isFirstShow) {
			InitializeComponent();
			if (!Directory.Exists(saveFolder)) { Directory.CreateDirectory(saveFolder); }

			isShowLyrics = !isFirstShow;
			this.Top = SystemParameters.PrimaryScreenHeight - 110;
			this.Left = Setting.LyrRight ? SystemParameters.FullPrimaryScreenWidth - 700 :
										   SystemParameters.WorkArea.Left;

			
			this.Loaded += delegate(object sender, RoutedEventArgs e) { new AltTab().HideAltTab(this); };

			DispatcherTimer dtm2 = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
			dtm2.Tick += delegate(object sender2, EventArgs e2) {
				this.Top = SystemParameters.PrimaryScreenHeight - 110;
				this.Left = Setting.LyrRight ? SystemParameters.FullPrimaryScreenWidth - 700 :
											   SystemParameters.WorkArea.Left;
				this.Topmost = false; this.Topmost = true;
			};
			dtm2.Start();
			ToggleLyrics(isFirstShow);
		}

		public void ToggleLyrics(bool isVisible) {
			double dOpacity = isVisible ? 0.85 : 0;
			this.Topmost = false;
			this.Topmost = true;
			gridBack.BeginAnimation(Grid.OpacityProperty,
				new DoubleAnimation(dOpacity, TimeSpan.FromMilliseconds(500)) {
					EasingFunction = new PowerEase() { Power = 7, EasingMode = EasingMode.EaseOut }
				});
		}

		public void ResetLyricsWindow() {
			lS.Text = "";
			lO.Text = "Offset:0ms";
			lT.Text = "00:00 / 00:00";
			ChangeLabels("Simplayer5", "", "");
		}

		Dictionary<string, LString[]> dictLyrics = new Dictionary<string, LString[]>();
		Dictionary<string, int> dictOffset = new Dictionary<string, int>();
		public string strMD5 = "", strFilePath;
		int nOffset = 0;

		public void InitLyrics(SongData nowPlayingData) {
			lS.Text = string.Format("{0}{1}{2}", nowPlayingData.Title, nowPlayingData.Artist == "" ? "" : " - ", nowPlayingData.Artist);
			lT.Text = string.Format("0:00 / {0}:{1:D2}", (int)nowPlayingData.Duration.TotalMinutes, nowPlayingData.Duration.Seconds);
			ChangeLabels(nowPlayingData.Title, nowPlayingData.Artist, nowPlayingData.Album);
			lO.Text = "Offset: 0ms";

			strFilePath = nowPlayingData.FilePath;
			strMD5 = Lyrics.GetSongMD5(strFilePath);

			if (dictOffset.ContainsKey(strMD5)) {
				nOffset = dictOffset[strMD5];
				ChangeOffset(0);
			}
			Task.Factory.StartNew(() => GetLyricsByFilePath(nowPlayingData.FilePath));
		}

		private void GetLyricsByFilePath(string file) {
			Lyrics lyr = new Lyrics();
			LString[] lsLyrics;

			string md5 = Lyrics.GetSongMD5(file);
			string fLyr = string.Format("{0}{1}.txt", saveFolder, md5);
			string fSyn = string.Format("{0}{1}_offset.txt", saveFolder, md5);

			if (!dictLyrics.ContainsKey(md5)) {
				string strLyrics;

				if (lyr.GetLyrics(file)) {
					strLyrics = lyr.LyricLists[0];

					string strBefore = "";
					if (File.Exists(fLyr)) {
						using (StreamReader sr = new StreamReader(fLyr)) { strBefore = sr.ReadToEnd(); }
					}

					if (strBefore != strLyrics) {
						using (StreamWriter sw = new StreamWriter(fLyr)) { sw.Write(strLyrics); }
						using (StreamWriter sw = new StreamWriter(fSyn)) { sw.Write("0"); }
					}

				} else {
					if (File.Exists(fLyr)) {
						using (StreamReader sr = new StreamReader(fLyr)) { strLyrics = sr.ReadToEnd(); }
					} else {
						strLyrics = "";
					}
				}
				lyr = null;
				if (strLyrics == "") { return; }
				try {
					using (StreamReader sr = new StreamReader(fSyn)) {
						nOffset = Convert.ToInt32(sr.ReadLine());
					}
				} catch (Exception ex) { }

				string[] SplitLyr = strLyrics.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				if (SplitLyr.Length == 0) { return; }
				lsLyrics = new LString[SplitLyr.Length];

				for (int i = 0; i < SplitLyr.Length; i++) {
					lsLyrics[i].strTime = string.Format("00:{0}", SplitLyr[i].Substring(1, 8));
					lsLyrics[i].tTime = TimeSpan.Parse(lsLyrics[i].strTime);
					lsLyrics[i].strLyrics = SplitLyr[i].Substring(10, SplitLyr[i].Length - 10).Trim();
				}
				try {
					dictLyrics.Add(md5, lsLyrics);
				} catch { }
				this.Dispatcher.BeginInvoke(new Action(() => {
					ChangeOffset(0);
				}));
			} 
		}

		string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimplayerLyrics\";
		public void ChangeOffset(int add) {
			if (strMD5 != "") {
				nOffset += add;
				dictOffset[strMD5] = nOffset;
				try {
					StreamWriter swLyrics2 = new StreamWriter(saveFolder + strMD5 + "_offset.txt");
					swLyrics2.Write(nOffset); swLyrics2.Flush(); swLyrics2.Close();
				} catch { }
			}
			lO.Text = string.Format("Offset: {0}ms", nOffset);
		}

		public void GetPlayTime(TimeSpan timePlay) {
			if (!dictLyrics.ContainsKey(strMD5)) { return; }
			if (dictLyrics[strMD5].Length == 0) { return; }

			TimeSpan nowTime, itTime, lastTime;
			string a1, a2, a3; a1 = a2 = a3 = "";
			int b1, b2, b3; b1 = b2 = b3 = 0;

			nowTime = TimeSpan.Parse(string.Format("00:{0:D2}:{1:D2}.{2:D2}", (int)timePlay.TotalMinutes, timePlay.Seconds, timePlay.Milliseconds / 10));
			int findIndex = -1; int lLen = dictLyrics[strMD5].Length;

			for (int lIndex = 0; lIndex < lLen; ) {
				try {
					if (lIndex > 2 && dictLyrics[strMD5][lIndex].strTime == "00:00:00.00") { lIndex++; continue; }

					itTime = dictLyrics[strMD5][lIndex].tTime;
					itTime = itTime.Add(new TimeSpan(0, 0, 0, 0, nOffset));
					if (TimeSpan.Compare(nowTime, itTime) < 0) { break; }

					findIndex = lIndex; lIndex++;
					lastTime = itTime;
				} catch (Exception ex) {
				}
			}

			for (int i = 0; i <= findIndex; i++) {
				if (dictLyrics[strMD5][i].strTime != dictLyrics[strMD5][findIndex].strTime) { continue; }

				if (b1 == 0) {
					b1 = 1; a1 = dictLyrics[strMD5][i].strLyrics;
				} else if (b2 == 0) {
					b2 = 1; a2 = dictLyrics[strMD5][i].strLyrics;
				} else if (b3 == 0) {
					b3 = 1; a3 = dictLyrics[strMD5][i].strLyrics;
				}
			}

			if (b1 == 0) { return; }
			if (CompareLabels(a1, a2, a3)) { return; }
			ChangeLabels(a1, a2, a3);
		}

		string str1, str2, str3;
		public bool CompareLabels(string t1, string t2, string t3) { return (str1 == t1 && str2 == t2 && str3 == t3); }
		public void ChangeLabels(string t1, string t2, string t3) {
			str1 = t1; str2 = t2; str3 = t3;
			labelFirsts = !labelFirsts;
			if (!labelFirsts) {
				R1.Text = t1; R2.Text = t2; R3.Text = t3;
				FlipLayout(L1, L2, L3, R1, R2, R3);
			} else {
				L1.Text = t1; L2.Text = t2; L3.Text = t3;
				FlipLayout(R1, R2, R3, L1, L2, L3);
			}
		}

		bool labelFirsts = true;
		int FlipTime = 200;
		public void FlipLayout(UIElement Src1, UIElement Src2, UIElement Src3, UIElement Des1, UIElement Des2, UIElement Des3) {
			UIElement[] uie = new UIElement[] { Src1, Src2, Src3, Des1, Des2, Des3 };
			Storyboard sb = new Storyboard();
			for (int i = 0; i < 6; i++) {
				DoubleAnimation da = new DoubleAnimation(1 - i / 3, i / 3, TimeSpan.FromMilliseconds(FlipTime - 50 + 100 * (i / 3)));
				Storyboard.SetTargetName(da, (uie[i] as FrameworkElement).Name);
				Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
				sb.Children.Add(da);
			}
			sb.Begin(this);
		}
	}
	public struct LString {
		public string strLyrics, strTime;
		public TimeSpan tTime;
	}
}
