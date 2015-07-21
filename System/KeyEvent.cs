using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		string AvailString = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (!Setting.IsVisible) { return; }

			// System Event

			bool SystemEvent = true;
			switch (e.Key) {
				case Key.Escape:
					switch (TabMode) {
						case ViewStatus.Setting:
							ReturnTab();
							break;
						case ViewStatus.Search:
							ReturnTab();
							break;
						default:
							MinimizeWindow();
							break;
					}
					break;
				case Key.Left:
					if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
						try {
							MusicPlayer.Position = new TimeSpan(0, 0, (int)MusicPlayer.Position.TotalSeconds - 3);
						} catch { }
					} else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
						try {
							MusicPlayer.Position = new TimeSpan(0, 0, 0);
						} catch { }
					} else {
						MovePlay(-1, false);
					}
					break;
				case Key.Right:
					if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
						try {
							MusicPlayer.Position = new TimeSpan(0, 0, (int)MusicPlayer.Position.TotalSeconds + 3);
						} catch { }
					} else {
						MovePlay(1, false);
					}
					break;
				case Key.Space:
					TogglePlayingStatus();
					break;
				case Key.Back:
					if (ListMode == ListStatus.Folder) {
						ReturnTab();
					}
					break;
				case Key.OemQuestion: 
					if (TabMode == ViewStatus.Mini) {
						this.Height = 600;
						ChangeTab(ViewMode);
					} else {
						this.Height = 200;
						ChangeTab(ViewStatus.Mini);
					}
					break;
				case Key.OemComma:
					if (Setting.PlayMode != 0) {
						LyricsWindow.ChangeOffset(-200);
					}
					break;
				case Key.OemPeriod:
					if (Setting.PlayMode != 0) {
						LyricsWindow.ChangeOffset(200);
					}
					break;
				default:
					SystemEvent = false;
					break;
			}

			if (SystemEvent) { return; }

			// List Moving

			bool MovingList = true;
			switch (TabMode) {
				case ViewStatus.All: break;
				case ViewStatus.Artist: break;
				case ViewStatus.Album: break;
				case ViewStatus.Folder: break;
				default:
					MovingList = false;
					break;
			}

			if (MovingList) {
				int nowSelectIndex = GetSelectIndex();
				switch (e.Key) {
					case Key.Up:
						SelectChange(nowSelectIndex - 1);
						return;
					case Key.Down:
						SelectChange(nowSelectIndex + 1);
						return;
					case Key.PageUp:
						SelectChange(nowSelectIndex - 10);
						return;
					case Key.PageDown:
						SelectChange(nowSelectIndex + 10);
						return;
					case Key.Home:
						SelectChange(0);
						return;
					case Key.End:
						SelectChange(Data.DictSong.Count);
						return;
					default:
						break;
				}
			}

			switch (e.Key) {
				case Key.Enter:
					switch (TabMode) {
						case ViewStatus.All:
							ShuffleAll();
							MusicPrepare(SongSelected, 0, false);
							return;
						case ViewStatus.Artist:
							OpenArtist(ArtistSelected);
							return;
						case ViewStatus.Album:
							OpenAlbum(AlbumSelected);
							return;
						case ViewStatus.Folder:
							switch (ViewMode) {
								case ViewStatus.Artist:
									ShuffleArtist(ArtistSelected);
									break;
								case ViewStatus.Album:
									ShuffleAlbum(AlbumSelected.Caption, AlbumSelected.Detail);
									break;
							}

							MusicPrepare(FolderSelected, 0, false);
							return;
						default:
							break;
					}
					break;
			}


			if (TabMode != ViewStatus.All && TabMode != ViewStatus.Artist && TabMode != ViewStatus.Album) { return; }

			string input = "?!";
			if (e.Key.ToString()[0] == 'D' && Char.IsNumber(e.Key.ToString(), e.Key.ToString().Length - 1)) {
				input = e.Key.ToString().Substring(1);
			} else {
				if (e.Key.ToString().Length == 1) {
					input = e.Key.ToString();
				}
			}

			if (input.Length == 1 && AvailString.IndexOf(input) >= 0) {
				int idx = GetHead(input);
				if (idx < 0) {
					Notice(string.Format("{0}에 해당하는 노래가 없습니다", GetSearchTag()));
				} else {
					RefreshContent(idx, 0);
				}
			}
		}

		string a = "";
	}
}
