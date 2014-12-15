using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		enum ViewStatus : long { All, Artist, Album, Folder, Search, Playlist, ListModify, Setting, Mini };
		enum PopupStatus { None = 0, SortOption = 3, Volume = 4, }

		/// <summary>
		/// 현재의 전체 뷰의 종류를 표시합니다.
		/// </summary>
		ViewStatus TabMode = ViewStatus.All;

		/// <summary>
		/// 최상단에 보이는 뷰의 종류를 표시합니다.
		/// </summary>
		ViewStatus ViewMode = ViewStatus.All;

		/// <summary>
		/// 팝업의 종류를 표시합니다.
		/// </summary>
		PopupStatus PopupMode = PopupStatus.None;

		Dictionary<ViewStatus, string> DictTabStatus = new Dictionary<ViewStatus, string>();
		UIElement[] TabControl = new UIElement[15];

		private void InitTab() {
			TabControl[0] = buttonSetting;
			TabControl[1] = buttonAdd;
			TabControl[2] = buttonBack;
			TabControl[3] = buttonSearch;
			TabControl[4] = tabSetting;
			TabControl[5] = buttonSortCollection;
			TabControl[6] = buttonPlayRandom;
			TabControl[7] = buttonPlayLoop;
			TabControl[8] = buttonLyrics;
			TabControl[9] = buttonVolume;
			TabControl[10] = textTabCaption;
			TabControl[11] = textboxTabInput;

			DictTabStatus.Add(ViewStatus.All,			"1--1-01111--");
			DictTabStatus.Add(ViewStatus.Artist,		"1--1-11111--");
			DictTabStatus.Add(ViewStatus.Album,			"1--1-21111--");
			DictTabStatus.Add(ViewStatus.Folder,		"--1---11111-");
			DictTabStatus.Add(ViewStatus.Search,		"--1--------1");
			DictTabStatus.Add(ViewStatus.Playlist,		"-1---3-----1");
			DictTabStatus.Add(ViewStatus.ListModify,	"--1--------1");
			DictTabStatus.Add(ViewStatus.Setting,		"--1-1-------");
			DictTabStatus.Add(ViewStatus.Mini,			"-----4111---");
		}

		private void buttonBack_Click(object sender, RoutedEventArgs e) {
			ShowSettingWindow(false);
			ReturnTab();
		}

		private void ReturnTab() {
			ChangeTab(ViewMode);

			switch (ViewMode) {
				case ViewStatus.All:
					RefreshListType(ListStatus.All);
					break;
				case ViewStatus.Artist:
					RefreshListType(ListStatus.Artist);
					break;
				case ViewStatus.Album:
					RefreshListType(ListStatus.Album);
					break;
			}
		}

		private void ChangeTab(ViewStatus mode) {
			TabMode = mode;

			string status = DictTabStatus[TabMode];
			for (int i = 0; i < status.Length; i++) {
				char c = status[i];
				TabControl[i].Visibility = GetVisibility(c != '-');
			}

			try {
				int v = Convert.ToInt32(status[5].ToString());

				foreach (Image img in (buttonSortCollection.Content as Grid).Children) {
					img.Visibility = Visibility.Collapsed;
				}
				for (int i = 2; i <= 6; i++) {
					gridSortOptionSelect.Children[i].IsHitTestVisible = true;
					gridSortOptionSelect.Children[i].Opacity = 0.5;
				}

				gridSortOptionSelect.Children[v + 2].IsHitTestVisible = false;
				gridSortOptionSelect.Children[v + 2].Opacity = 1;
				(buttonSortCollection.Content as Grid).Children[v].Visibility = GetVisibility(true);

				ChangePopupMode(PopupStatus.None);
			} catch { }
		}

		private void ChangePopupMode(PopupStatus mode) {
			PopupMode = mode;

			double duration = 300;

			gridOptionCover.Visibility =
				PopupMode != PopupStatus.None && TabMode != ViewStatus.Mini ? Visibility.Visible : Visibility.Collapsed;
			gridSortOptionSelect.IsHitTestVisible =
				PopupMode == PopupStatus.SortOption ? true : false;
			gridVolume.IsHitTestVisible =
				PopupMode == PopupStatus.Volume ? true : false;

			Storyboard sb = new Storyboard();
			switch (mode) {
				case PopupStatus.SortOption:
					sb.Children.Add(GetThicknessAnimation(duration, 208, 0, gridSortOptionSelect));
					sb.Children.Add(GetThicknessAnimation(duration, 0, -120, gridVolume));
					break;
				case PopupStatus.Volume:
					sb.Children.Add(GetThicknessAnimation(duration, 208, -175, gridSortOptionSelect));
					sb.Children.Add(GetThicknessAnimation(duration, 0, 0, gridVolume));
					break;
				default:
					sb.Children.Add(GetThicknessAnimation(duration, 208, -175, gridSortOptionSelect));
					sb.Children.Add(GetThicknessAnimation(duration, 0, -120, gridVolume));
					break;
			}

			sb.Begin(this);
		}

		private void buttonSortCollection_Click(object sender, RoutedEventArgs e) {
			if (TabMode == ViewStatus.Mini) {
				this.Height = 600;
				ChangeTab(ViewMode);
			} else if (PopupMode == PopupStatus.SortOption) {
				ChangePopupMode(PopupStatus.None);
			} else {
				ChangePopupMode(PopupStatus.SortOption);
			}
		}

		private void gridOptionCover_MouseDown(object sender, MouseButtonEventArgs e) {
			ChangePopupMode(PopupStatus.None);
		}

		private void buttonSortAll_Click(object sender, RoutedEventArgs e) {
			ViewMode = ViewStatus.All;
			ChangeTab(ViewStatus.All);
			RefreshListType(ListStatus.All);
		}
		private void buttonSortArtist_Click(object sender, RoutedEventArgs e) {
			ViewMode = ViewStatus.Artist;
			ChangeTab(ViewStatus.Artist);
			RefreshListType(ListStatus.Artist);
		}
		private void buttonSortAlbum_Click(object sender, RoutedEventArgs e) {
			ViewMode = ViewStatus.Album;
			ChangeTab(ViewStatus.Album);
			RefreshListType(ListStatus.Album);
		}
		private void buttonSortPlaylist_Click(object sender, RoutedEventArgs e) {
			ViewMode = ViewStatus.Playlist;
			ChangeTab(ViewStatus.Playlist);
		}
		private void buttonSortMini_Click(object sender, RoutedEventArgs e) {
			this.Height = 200;
			ChangeTab(ViewStatus.Mini);
		}
	}
}
