using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		private void buttonSearch_Click(object sender, RoutedEventArgs e) {
			textboxTabInput.Text = "";

			ChangeTab(ViewStatus.Search);
			ListMode = ListStatus.Search;
			LaunchIndexer(false, 0);

			InitSearch();
			textboxTabInput.Focus();
		}

		List<SearchData> ListSearchLib = new List<SearchData>();
		List<SearchData> ListSearch = new List<SearchData>();
		int SearchStart;

		private void InitSearch() {
			ContentHeight = 50;
			gridContent.Children.Clear();

			for (int i = -inv; i <= 9 + inv; i++) {
				gridContent.Children.Add(SearBtn[i + inv]);
			}

			List<int> listSong = Data.DictSong.Values.OrderBy(x => Data.PosSong[x.ID]).Select(x => x.ID).ToList();
			List<string> listArtist = Data.DictSong.Select(x => x.Value.Artist)
						.Distinct()
						.OrderBy(y => y, new StringComparer())
						.ToList();
			List<DetailData> listAlbum = Data.DictSong.Select(x => new DetailData(x.Value.Album, x.Value.AlbumArtist, -1))
						.Distinct(new DetailComparer())
						.OrderBy(y => y.Caption, new StringComparer())
						.ThenBy(y => y.Detail, new StringComparer())
						.ToList();

			ListSearchLib = listSong.Select(x => new SearchData(x, Data.DictSong[x].Title, "", "Song")).ToList();
			ListSearchLib.AddRange(listArtist.Select(x => new SearchData(-1, x, "", "Artist")));
			ListSearchLib.AddRange(listAlbum.Select(x => new SearchData(-1, x.Caption, x.Detail, "Album")));

			ListSearchLib = ListSearchLib.OrderBy(x => x.Caption, new StringComparer()).ToList();

			RefreshSearchContent("");
		}

		private void RefreshSearchContent(string str) {
			SearchStart = 0;

			if (str == "") {
				ListSearch.Clear();
			} else {
				ListSearch = ListSearchLib.Where(x => x.CaptionLower.IndexOf(str) >= 0).ToList();
			}

			stackInside.Height = ListSearch.Count * ContentHeight;
			ScrollSearch(SearchStart, true);
		}

		private void ScrollSearch(int newStart, bool moveScroll) {
			if (ListSearch == null) { return; }

			newStart = GetStartPosition(ListStatus.Search, newStart);

			for (int i = -inv; i <= 7 + inv; i++) {
				if (newStart + i >= 0 && newStart + i < ListSearch.Count) {

					SearBtn[i + inv].SetValue(ListSearch[newStart + i].ID,
						ListSearch[newStart + i].Caption,
						ListSearch[newStart + i].Detail,
						ListSearch[newStart + i].Type,
						true);

				} else {
					SearBtn[i + inv].SetValue(-1, "", "", "", false);
				}
			}

			int gap = 0;
			if (SearchStart < newStart) {
				gap = Math.Min(inv, newStart - SearchStart);
			} else {
				gap = Math.Max(-inv, newStart - SearchStart);
			}

			SearchStart = newStart;
			ScrollTo(SearchStart, gap, moveScroll);
		}

		private void textboxTabInput_TextChanged(object sender, TextChangedEventArgs e) {
			if (TabMode != ViewStatus.Search) { return; }
			RefreshSearchContent(textboxTabInput.Text.ToLower());
		}

		private void MainWindow_Response(object sender, SearchEventArgs e) {
			switch (e.Type) {
				case "Song":
					SongSelected = e.ID;
					buttonSortAll.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case "Artist":
					ArtistSelected = e.Caption;
					buttonSortArtist.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
				case "Album":
					AlbumSelected = new DetailData(e.Caption, e.Detail, -1);
					buttonSortAlbum.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					break;
			}

			ScrollToContent(e.ID, e.Caption, e.Detail);
		}
	}

	class SearchData {
		public int ID;
		public string Caption, CaptionLower, Detail, Type;

		public SearchData(int i, string c, string d, string t) {
			this.ID = i;
			this.Caption = c;
			this.Detail = d;
			this.Type = t;

			this.CaptionLower = this.Caption.ToLower();
		}
	}
}
