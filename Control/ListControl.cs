using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		enum ListStatus { All, Artist, Album, Folder, Search }

		SongButton[] SongBtn = new SongButton[33];
		FolderButton[] FoldBtn = new FolderButton[33];
		DetailButton[] DtilBtn = new DetailButton[33];
		SearchButton[] SearBtn = new SearchButton[33];
		int inv = 7;
		private void InitListControl() {
			for (int i = -inv; i <= 9 + inv; i++) {
				SongBtn[i + inv] = new SongButton() {
					Height = 40,
					Margin = new Thickness(0, i * 40, 0, 0),
				};
				FoldBtn[i + inv] = new FolderButton() {
					Height = 40,
					Margin = new Thickness(0, i * 40, 0, 0),
				};
				DtilBtn[i + inv] = new DetailButton() {
					Height = 50,
					Margin = new Thickness(0, i * 50, 0, 0),
				};
				SearBtn[i + inv] = new SearchButton() {
					Height = 50,
					Margin = new Thickness(0, i * 50, 0, 0),
				};

				SongBtn[i + inv].Response += SongButton_Response;
				FoldBtn[i + inv].Response += FolderButton_Response;
				DtilBtn[i + inv].Response += DetailButton_Response;
				SearBtn[i + inv].Response += MainWindow_Response;
			}
		}

		/// <summary>
		/// 리스트에 보여지는 폼의 종류를 폴더를 포함하여 표시합니다.
		/// </summary>
		ListStatus ListMode = ListStatus.All;

		int SongSelected = -1, SongStart;
		List<int> ListSong;

		string ArtistSelected = "<!Artist>";
		int ArtistStart;
		List<FolderData> ListArtist;

		DetailData AlbumSelected = new DetailData("<!Album>", "", 0);
		int AlbumStart;
		List<DetailData> ListAlbum;

		int FolderSelected, FolderStart;
		List<int> ListFolder;

		int ContentHeight;

		private void RefreshListType(ListStatus status) {
			ListMode = status;
			LaunchIndexer(false, 0);

			switch (ListMode) {
				case ListStatus.All:
					InitAll();
					break;
				case ListStatus.Artist:
					InitArtist();
					break;
				case ListStatus.Album:
					InitAlbum();
					break;
				case ListStatus.Folder:
					InitFolder();
					break;
				default:
					return;
			}
		}

		private void RefreshList() {
			switch (ListMode) {
				case ListStatus.All:
					ScrollAll(SongStart, true);
					break;
				case ListStatus.Artist:
					ScrollArtist(ArtistStart, true);
					break;
				case ListStatus.Album:
					ScrollAlbum(AlbumStart, true);
					break;
				case ListStatus.Folder:
					ScrollFolder(FolderStart, true);
					break;
				default:
					return;
			}
		}

		int NowStart = 0;
		private void RefreshContent(int newStart, int gap) {
			try {
				switch (ListMode) {
					case ListStatus.All:
						newStart = GetValidPosition(ListStatus.All, newStart);
						SongSelected = ListSong[newStart];
						ScrollAll(newStart - gap, true);
						break;
					case ListStatus.Artist:
						newStart = GetValidPosition(ListStatus.Artist, newStart);
						ArtistSelected = ListArtist[newStart].Caption;
						ScrollArtist(newStart - gap, true);
						break;
					case ListStatus.Album:
						newStart = GetValidPosition(ListStatus.Album, newStart);
						AlbumSelected = ListAlbum[newStart];
						ScrollAlbum(newStart - gap, true);
						break;
					case ListStatus.Folder:
						newStart = GetValidPosition(ListStatus.Folder, newStart);
						FolderSelected = ListFolder[newStart];
						ScrollFolder(newStart - gap, true);
						break;
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message + "\n" + "ListControl.cs");
			}
		}

		List<string> ListCaption;
		private void RefreshContent() {
			switch (ListMode) {
				case ListStatus.All:
					ListSong = Data.DictSong.Values.OrderBy(x => Data.PosSong[x.ID]).Select(x => x.ID).ToList();
					ListCaption = Data.DictSong.Values.OrderBy(x => Data.PosSong[x.ID]).Select(x => x.Title).ToList();

					RefreshIndexer(ListCaption);
					RefreshShortcut(ListCaption);

					stackInside.Height = ListSong.Count * ContentHeight;
					ScrollAll(SongStart, true);
					break;
				case ListStatus.Artist:
					List<string> dict = Data.DictSong.Select(x => x.Value.Artist)
						.Distinct()
						.OrderBy(y => y, new StringComparer())
						.ToList();

					ListArtist = dict.Select(x => new FolderData(x, Data.DictSong.Count(y => y.Value.Artist == x))).ToList();
					ListCaption = ListArtist.Select(x => x.Caption).ToList();

					RefreshIndexer(ListCaption);
					RefreshShortcut(ListCaption);

					stackInside.Height = ListArtist.Count * ContentHeight;
					ScrollArtist(ArtistStart, true);

					break;
				case ListStatus.Album:
					List<DetailData> dict2 = Data.DictSong.Select(x => new DetailData(x.Value.Album, x.Value.AlbumArtist, -1))
						.Distinct(new DetailComparer())
						.OrderBy(y => y.Caption, new StringComparer())
						.ThenBy(y => y.Detail, new StringComparer())
						.ToList();

					ListAlbum = dict2.Select(x =>
						new DetailData(x.Caption, x.Detail,
							Data.DictSong.Count(y => y.Value.Album == x.Caption && y.Value.AlbumArtist == x.Detail))).ToList();
					ListCaption = ListAlbum.Select(x => x.Caption).ToList();

					RefreshIndexer(ListCaption);
					RefreshShortcut(ListCaption);

					stackInside.Height = ListAlbum.Count * ContentHeight;
					ScrollAlbum(AlbumStart, true);

					break;
				case ListStatus.Folder:
					FolderStart = 0;
					FolderSelected = -1;

					switch (ViewMode) {
						case ViewStatus.Artist:
							ListFolder = Data.DictSong
								.Where(x => x.Value.Artist == ArtistSelected)
								.OrderBy(y => y.Value.Title, new StringComparer())
								.Select(z => z.Key)
								.ToList();
							break;
						case ViewStatus.Album:
							ListFolder = Data.DictSong
								.Where(x => x.Value.Album == AlbumSelected.Caption && x.Value.AlbumArtist == AlbumSelected.Detail)
								.OrderBy(y => y.Value.Title, new StringComparer())
								.Select(z => z.Key)
								.ToList();
							break;
					}

					if (ListFolder == null || ListFolder.Count == 0) {
						ReturnTab();
						return;
					}

					stackInside.Height = ListFolder.Count * ContentHeight;
					ScrollFolder(FolderStart, true);
					break;
				default:
					return;
			}
		}

		#region All Refresh
		private void InitAll() {
			ContentHeight = 40;
			gridContent.Children.Clear();

			for (int i = -inv; i <= 9 + inv; i++) {
				gridContent.Children.Add(SongBtn[i + inv]);
			}

			RefreshContent();
		}

		private void ScrollAll(int newStart, bool moveScroll) {
			if (ListSong == null) { return; }

			newStart = GetStartPosition(ListStatus.All, newStart);

			for (int i = -inv; i <= 9 + inv; i++) {
				if (newStart + i >= 0 && newStart + i < ListSong.Count) {
					SongBtn[i + inv].SetValue(Data.DictSong[ListSong[newStart + i]],
						Data.DictSong[ListSong[newStart + i]].ID == SongSelected, true,
						Data.DictSong[ListSong[newStart + i]].ID == Setting.NowPlaying.ID);
				} else {
					SongBtn[i + inv].SetValue(new SongData() { New = false }, false, false, false);
				}
			}

			int gap = 0;
			if (SongStart < newStart) {
				gap = Math.Min(inv, newStart - SongStart);
			} else {
				gap = Math.Max(-inv, newStart - SongStart);
			}

			SongStart = newStart;
			ScrollTo(SongStart, gap, moveScroll);
		}
		#endregion

		#region Artist Refresh
		private void InitArtist() {
			ContentHeight = 40;
			gridContent.Children.Clear();

			for (int i = -inv; i <= 9 + inv; i++) {
				gridContent.Children.Add(FoldBtn[i + inv]);
			}

			RefreshContent();
		}

		private void ScrollArtist(int newStart, bool moveScroll) {
			if (ListArtist == null) { return; }

			newStart = GetStartPosition(ListStatus.Artist, newStart);

			for (int i = -inv; i <= 9 + inv; i++) {
				if (newStart + i >= 0 && newStart + i < ListArtist.Count) {
					FoldBtn[i + inv].SetValue(ListArtist[newStart + i],
						ListArtist[newStart + i].Caption == ArtistSelected, true);
				} else {
					FoldBtn[i + inv].SetValue(new FolderData("", 0), false, false);
				}
			}

			int gap = 0;
			if (ArtistStart < newStart) {
				gap = Math.Min(inv, newStart - ArtistStart);
			} else {
				gap = Math.Max(-inv, newStart - ArtistStart);
			}

			ArtistStart = newStart;
			ScrollTo(ArtistStart, gap, moveScroll);
		}
		#endregion

		#region Album Refresh
		private void InitAlbum() {
			ContentHeight = 50;
			gridContent.Children.Clear();

			for (int i = -inv; i <= 9 + inv; i++) {
				gridContent.Children.Add(DtilBtn[i + inv]);
			}

			RefreshContent();
		}

		private void ScrollAlbum(int newStart, bool moveScroll) {
			if (ListAlbum == null) { return; }

			newStart = GetStartPosition(ListStatus.Album, newStart);

			for (int i = -inv; i <= 7 + inv; i++) {
				if (newStart + i >= 0 && newStart + i < ListAlbum.Count) {

					DtilBtn[i + inv].SetValue(ListAlbum[newStart + i],
						ListAlbum[newStart + i].Caption == AlbumSelected.Caption
						&& ListAlbum[newStart + i].Detail == AlbumSelected.Detail, true);

				} else {
					DtilBtn[i + inv].SetValue(new DetailData("", "", 0), false, false);
				}
			}

			int gap = 0;
			if (AlbumStart < newStart) {
				gap = Math.Min(inv, newStart - AlbumStart);
			} else {
				gap = Math.Max(-inv, newStart - AlbumStart);
			}

			AlbumStart = newStart;
			ScrollTo(AlbumStart, gap, moveScroll);
		}
		#endregion

		#region Folder Refresh
		private void InitFolder() {
			ContentHeight = 40;
			gridContent.Children.Clear();

			for (int i = -inv; i <= 9 + inv; i++) {
				gridContent.Children.Add(SongBtn[i + inv]);
			}

			RefreshContent();
		}

		private void ScrollFolder(int newStart, bool moveScroll) {
			if (ListFolder == null) { return; }

			newStart = GetStartPosition(ListStatus.Folder, newStart);

			for (int i = -inv; i <= 9 + inv; i++) {
				if (newStart + i >= 0 && newStart + i < ListFolder.Count) {
					SongBtn[i + inv].SetValue(Data.DictSong[ListFolder[newStart + i]],
						Data.DictSong[ListFolder[newStart + i]].ID == FolderSelected, true,
						Data.DictSong[ListFolder[newStart + i]].ID == Setting.NowPlaying.ID);
				} else {
					SongBtn[i + inv].SetValue(new SongData() { New = false }, false, false, false);
				}
			}

			int gap = 0;
			if (FolderStart < newStart) {
				gap = Math.Min(inv, newStart - FolderStart);
			} else {
				gap = Math.Max(-inv, newStart - FolderStart);
			}

			FolderStart = newStart;
			ScrollTo(FolderStart, gap, moveScroll);
		}
		#endregion

		private int GetStartPosition(ListStatus status, int newStart) {
			switch (status) {
				case ListStatus.All:
					newStart = Math.Min(ListSong.Count - 10, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Artist:
					newStart = Math.Min(ListArtist.Count - 10, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Album:
					newStart = Math.Min(ListAlbum.Count - 8, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Folder:
					newStart = Math.Min(ListFolder.Count - 10, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Search:
					newStart = Math.Min(ListSearch.Count - 8, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				default:
					return 0;
			}
		}

		private int GetValidPosition(ListStatus status, int newStart) {
			switch (status) {
				case ListStatus.All:
					newStart = Math.Min(ListSong.Count - 1, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Artist:
					newStart = Math.Min(ListArtist.Count - 1, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Album:
					newStart = Math.Min(ListAlbum.Count - 1, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Folder:
					newStart = Math.Min(ListFolder.Count - 1, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				case ListStatus.Search:
					newStart = Math.Min(ListSearch.Count - 1, newStart);
					newStart = Math.Max(0, newStart);
					return newStart;
				default:
					return 0;
			}
		}

		private void SongButton_Response(object sender, SongEventArgs e) {
			switch (ListMode) {
				case ListStatus.All:
					if (e.PropertyName == "Click") {
						SongSelected = e.ID;

						if (!Setting.PlayDoubleClick) {
							ShuffleAll();
							MusicPrepare(e.ID, 0, false);
						}

						ScrollAll(SongStart, true);
					}
					if (e.PropertyName == "DoubleClick") {
						if (Setting.PlayDoubleClick) {
							ShuffleAll();
							MusicPrepare(e.ID, 0, false);

							ScrollAll(SongStart, true);
						}
					}
					if (e.PropertyName == "MouseDown") {
						MousePressDown(e.ID, e.GetPosition(gridListArea));
					}
					if (e.PropertyName == "Open") {
						OpenSongFile(Data.DictSong[e.ID].FilePath);
					}
					if (e.PropertyName == "Delete") {
						DeleteSong(e.ID);
					}
					break;
				case ListStatus.Folder:
					if (e.PropertyName == "Click") {
						FolderSelected = e.ID;

						if (!Setting.PlayDoubleClick) {
							Setting.NowPlaying.ID = e.ID;

							switch (ViewMode) {
								case ViewStatus.Artist:
									ShuffleArtist(ArtistSelected);
									break;
								case ViewStatus.Album:
									ShuffleAlbum(AlbumSelected.Caption, AlbumSelected.Detail);
									break;
							}

							MusicPrepare(e.ID, 0, false);
						}

						ScrollFolder(FolderStart, true);
					}
					if (e.PropertyName == "DoubleClick") {
						if (Setting.PlayDoubleClick) {
							Setting.NowPlaying.ID = e.ID;

							switch (ViewMode) {
								case ViewStatus.Artist:
									ShuffleArtist(ArtistSelected);
									break;
								case ViewStatus.Album:
									ShuffleAlbum(AlbumSelected.Caption, AlbumSelected.Detail);
									break;
							}

							MusicPrepare(e.ID, 0, false);

							ScrollFolder(FolderStart, true);
						}
					}
					if (e.PropertyName == "Open") {
						OpenSongFile(Data.DictSong[e.ID].FilePath);
					}
					if (e.PropertyName == "Delete") {
						DeleteSong(e.ID);
					}
					break;
			}
		}

		private void OpenSongFile(string path) {
			if (!System.IO.File.Exists(path)) { return; }
			string argument = string.Format("/select, \"{0}\"", path);
			Task.Factory.StartNew(() => Process.Start("explorer.exe", argument));
		}

		private void FolderButton_Response(object sender, FolderEventArgs e) {
			switch (ListMode) {
				case ListStatus.Artist:
					OpenArtist(e.Caption);
					break;

			}
		}

		private void DetailButton_Response(object sender, DetailEventArgs e) {
			switch (ListMode) {
				case ListStatus.Album:
					OpenAlbum(new DetailData(e.Caption, e.Detail, -1));
					break;
			}
		}

		private void OpenArtist(string artist) {
			if (!ListArtist.Exists(x => x.Caption == artist)) { return; }

			ArtistSelected = artist;
			ScrollArtist(ArtistStart, true);
			textTabCaption.Text = ArtistSelected;

			ChangeTab(ViewStatus.Folder);
			RefreshListType(ListStatus.Folder);
		}

		private void OpenAlbum(DetailData album) {
			if (!ListAlbum.Exists(x => x.Caption == album.Caption && x.Detail == album.Detail)) { return; }

			AlbumSelected = album;
			ScrollAlbum(AlbumStart, true);
			textTabCaption.Text = AlbumSelected.Caption;

			ChangeTab(ViewStatus.Folder);
			RefreshListType(ListStatus.Folder);
		}

		// Control event handler

		private void ScrollTo(int to, int gap, bool scrChange) {
			if (ListSong == null) { return; }

			NowStart = to;
			gridContent.BeginAnimation(Grid.MarginProperty,
				GetScrollAnimation(gap * ContentHeight, 0, Math.Abs(gap) * 150));

			if (scrChange) {
				isScrollByCode = true;
				scrollList.ScrollToVerticalOffset(to * ContentHeight);
			}
		}

		private void ScrollToContent(int id, string caption, string detail) {
			int nowStart = 0;

			switch (ListMode) {
				case ListStatus.All:
					nowStart = ListSong.FindIndex(x => x == id);
					break;
				case ListStatus.Artist:
					nowStart = ListArtist.FindIndex(x => x.Caption == caption);
					break;
				case ListStatus.Album:
					nowStart = ListAlbum.FindIndex(x => x.Caption == caption && x.Detail == detail);
					break;
			}

			ScrollToIndex(true, 0, nowStart);
		}

		private void ScrollToIndex(bool scrollChange, int add, int start = -1) {
			int nowStart = 0;
			switch (ListMode) {
				case ListStatus.All:
					nowStart = start < 0 ? SongStart : start;
					ScrollAll(nowStart + add, scrollChange);
					break;
				case ListStatus.Artist:
					nowStart = start < 0 ? ArtistStart : start;
					ScrollArtist(nowStart + add, scrollChange);
					break;
				case ListStatus.Album:
					nowStart = start < 0 ? AlbumStart : start;
					ScrollAlbum(nowStart + add, scrollChange);
					break;
				case ListStatus.Folder:
					nowStart = start < 0 ? FolderStart : start;
					ScrollFolder(nowStart + add, scrollChange);
					break;
				case ListStatus.Search:
					nowStart = start < 0 ? SearchStart : start;
					ScrollSearch(nowStart + add, scrollChange);
					break;
				default:
					return;
			}
		}

		private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e) {
			e.Handled = true;

			ScrollToIndex(true, (int)(e.Delta / -ContentHeight));
		}

		bool isScrollByCode = false;
		private void scrollTest_ScrollChanged(object sender, ScrollChangedEventArgs e) {
			if (isScrollByCode) {
				isScrollByCode = false;
				return;
			}
			ScrollToIndex(false, 0, (int)(scrollList.VerticalOffset / ContentHeight));
		}

		private int GetSelectIndex() {
			try {
				switch (ListMode) {
					case ListStatus.All:
						return ListSong.FindIndex(x => x == SongSelected);
					case ListStatus.Artist:
						return ListArtist.Select(x => x.Caption).ToList().IndexOf(ArtistSelected);
					case ListStatus.Album:
						return ListAlbum.FindIndex(x => x.Caption == AlbumSelected.Caption && x.Detail == AlbumSelected.Detail);
					case ListStatus.Folder:
						return ListFolder.FindIndex(x => x == FolderSelected);
				}
			} catch (Exception ex) {
				//MessageBox.Show(ex.Message + "\n" + "ListControl.cs");
			}
			return -1;
		}

		private void SelectChange(int select) {
			select = GetValidPosition(ListMode, select);

			if (select < NowStart) {
				RefreshContent(select, 0);
			} else if (select >= NowStart + (400 / ContentHeight)) {
				RefreshContent(select, (400 / ContentHeight) - 1);
			} else {
				RefreshContent(select, select - NowStart);
			}
		}

		private void Albumart_MouseDown(object sender, MouseButtonEventArgs e) {
			if (Data.PosSong.ContainsKey(Setting.NowPlaying.ID) && TabMode == ViewStatus.All) {
				RefreshContent(Data.PosSong[Setting.NowPlaying.ID], 1);
			}
		}
	}
}
