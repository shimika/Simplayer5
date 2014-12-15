using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		private void RefreshSettingControls() {
			ApplySetting();

			(buttonSettingTopMost.Content as TextBlock).Text = Setting.TopMost ? "Yes" : "No";
			(buttonSettingMinimize.Content as TextBlock).Text = Setting.MinToTray ? "Tray" : "Taskbar";
			(buttonSettingHotkey.Content as TextBlock).Text = Setting.Hotkey ? "Yes" : "No";
			(buttonSettingLyrics.Content as TextBlock).Text = Setting.LyrRight ? "Right" : "Left";
			(buttonSettingNotification.Content as TextBlock).Text = Setting.Notification ? "On" : "Off";
			(buttonSettingSort.Content as TextBlock).Text = Setting.SortAuto ? "Auto" : "Manual";
			(buttonSettingClickCount.Content as TextBlock).Text = Setting.PlayDoubleClick ? "Double click" : "Oneclick";

			imageLinear.Visibility = Setting.RandomSeed == 1 ? Visibility.Visible : Visibility.Collapsed;
			imageRandom.Visibility = Setting.RandomSeed == 2 ? Visibility.Visible : Visibility.Collapsed;
			buttonPlayRandom.Opacity = Setting.RandomSeed == 2 ? 1 : 0.6;

			imageLoopAll.Visibility = Setting.PlayingLoopSeed == 1 ? Visibility.Visible : Visibility.Collapsed;
			imageLoop1.Visibility = Setting.PlayingLoopSeed == 0 ? Visibility.Visible : Visibility.Collapsed;
			buttonPlayLoop.Opacity = Setting.PlayingLoopSeed == 0 ? 1 : 0.6;

			buttonLyrics.Opacity = Setting.LyricsOn ? 1 : 0.6;

			textVolume.Text = ((int)(Setting.Volume * 100)).ToString();
			rectVolumeBar.Height = Setting.Volume * 80;

			textVersion.Text = Setting.Version;

			//if (Setting.Hotkey) { ToggleHotKeyMode(true); }

			//buttonIndexerSort.Visibility = Setting.IsSorted ? Visibility.Collapsed : Visibility.Visible;

			Setting.SaveSetting();
		}

		private void ApplySetting() {
			this.Topmost = Setting.TopMost;
			this.ShowInTaskbar = !Setting.MinToTray;

			//MusicPlayer.Volume = Setting.Volume;
		}

		private void buttonSettingTopMost_Click(object sender, RoutedEventArgs e) {
			Setting.TopMost = !Setting.TopMost;
			RefreshSettingControls();
		}
		private void buttonSettingMinimize_Click(object sender, RoutedEventArgs e) {
			Setting.MinToTray = !Setting.MinToTray;
			RefreshSettingControls();
		}
		private void buttonSettingHotkey_Click(object sender, RoutedEventArgs e) {
			Setting.Hotkey = !Setting.Hotkey;
			//ToggleHotKeyMode(Setting.Hotkey);
			RefreshSettingControls();
		}
		private void buttonSettingLyrics_Click(object sender, RoutedEventArgs e) {
			Setting.LyrRight = !Setting.LyrRight;
			RefreshSettingControls();
		}
		private void buttonSettingNotification_Click(object sender, RoutedEventArgs e) {
			Setting.Notification = !Setting.Notification;
			RefreshSettingControls();
		}
		private void buttonSettingSort_Click(object sender, RoutedEventArgs e) {
			Setting.SortAuto = !Setting.SortAuto;
			RefreshSettingControls();

			if (Setting.SortAuto) {
				Notice("리스트가 정렬되었습니다.");

				RefreshContent();
				SortSongs();
				//ShuffleList(0);
				SaveSongList();
				Setting.SaveSetting();
			}
		}
		private void buttonSettingClickCount_Click(object sender, RoutedEventArgs e) {
			Setting.PlayDoubleClick = !Setting.PlayDoubleClick;
			RefreshSettingControls();
		}

		private void buttonPlayRandom_Click(object sender, RoutedEventArgs e) {
			Setting.RandomSeed = 3 - Setting.RandomSeed;

			if (Setting.RandomSeed == 1) {
				Notice("리스트의 순서로 노래를 재생합니다.");
			} else {
				Notice("임의 순서로 노래를 재생합니다.");
			}
			RefreshSettingControls();
		}
		private void buttonPlayLoop_Click(object sender, RoutedEventArgs e) {
			Setting.PlayingLoopSeed = 1 - Setting.PlayingLoopSeed;

			if (Setting.PlayingLoopSeed == 1) {
				Notice("모든 노래를 재생합니다.");
			} else {
				Notice("현재 곡을 반복 재생합니다.");
			}
			RefreshSettingControls();
		}
		private void buttonLyrics_Click(object sender, RoutedEventArgs e) {
			Setting.LyricsOn = !Setting.LyricsOn;
			//LyricsWindow.ToggleLyrics(Setting.LyricsOn);
			RefreshSettingControls();
		}


		private void buttonSettingGeneral_Click(object sender, RoutedEventArgs e) {
			ChangeSettingTab(true);
		}
		private void buttonSettingInfo_Click(object sender, RoutedEventArgs e) {
			ChangeSettingTab(false);
		}

		private void ShowSettingWindow(bool show) {
			if (show) {
				ChangeSettingTab(true);
			}
			gridSetting.Visibility = GetVisibility(show);
		}

		private void ChangeSettingTab(bool isGeneral) {
			gridSettingGeneral.Visibility = GetVisibility(isGeneral);
			gridSettingInfo.Visibility = GetVisibility(!isGeneral);

			buttonSettingGeneral.Opacity = isGeneral ? 1 : 0.6;
			buttonSettingInfo.Opacity = isGeneral ? 0.6 : 1;
		}

		private void buttonSetting_Click(object sender, RoutedEventArgs e) {
			ShowSettingWindow(true);
			ChangeTab(ViewStatus.Setting);
		}
	}
}
