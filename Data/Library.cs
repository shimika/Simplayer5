using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		public void LoadSongList() {
			if (!File.Exists(Setting.ListFile)) {
				if (File.Exists(Setting.ListFile.Replace("_T.ini", ".ini"))) {
					File.Copy(Setting.ListFile.Replace("_T.ini", ".ini"), Setting.ListFile);
				} else {
					return;
				}
			}

			JsonArrayCollection loadCollection;
			using (StreamReader sr = new StreamReader(Setting.ListFile)) {
				try {
					loadCollection = (JsonArrayCollection)(new JsonTextParser().Parse(sr.ReadToEnd()));
				} catch {
					loadCollection = new JsonArrayCollection();
				}
			}

			Setting.InsertID = 0;
			int id, count = 0;

			foreach (JsonObjectCollection item in loadCollection) {
				id = Convert.ToInt32(item["ID"].GetValue());
				if (id < 0) {
					Setting.InsertID = Convert.ToInt32(item["InsertID"].GetValue());
				} else {
					SongData s = new SongData() {
						ID = id,
						Title = item["Title"].GetValue().ToString(),
						Artist = item["Artist"].GetValue().ToString(),
						Album = item["Album"].GetValue().ToString(),
						AlbumArtist = item["AlbumArtist"].GetValue().ToString(),

						FilePath = item["Path"].GetValue().ToString(),
						DurationString = item["Duration"].GetValue().ToString(),

						AddDate = DateTime.Parse(item["AddDate"].GetValue().ToString())
					};
					Data.DictSong.Add(id, s);
					Data.PosSong.Add(id, count++);

					Setting.InsertID = Math.Max(Setting.InsertID, Convert.ToInt32(item["ID"].GetValue()) + 1);
				}
			}

			RefreshLibraryInfo();
		}

		public void SaveSongList() {
			if (!Directory.Exists(Setting.SettingFolder)) {
				Directory.CreateDirectory(Setting.SettingFolder);
			}

			RefreshLibraryInfo();

			List<SongData> list = Data.DictSong.Values.OrderBy(x => Data.PosSong[x.ID]).ToList();

			JsonArrayCollection saveListCollection = new JsonArrayCollection();
			JsonObjectCollection nodeCollection = new JsonObjectCollection();

			nodeCollection.Add(new JsonStringValue("ID", "-1"));
			nodeCollection.Add(new JsonStringValue("InsertID", Setting.InsertID.ToString()));
			saveListCollection.Add(nodeCollection);

			foreach (SongData sData in list) {
				nodeCollection = new JsonObjectCollection();
				nodeCollection.Add(new JsonStringValue("Title", sData.Title));
				nodeCollection.Add(new JsonStringValue("Path", sData.FilePath));
				nodeCollection.Add(new JsonStringValue("Duration", sData.DurationString));
				nodeCollection.Add(new JsonStringValue("ID", sData.ID.ToString()));

				nodeCollection.Add(new JsonStringValue("Artist", sData.Artist));
				nodeCollection.Add(new JsonStringValue("Album", sData.Album));
				nodeCollection.Add(new JsonStringValue("AlbumArtist", sData.AlbumArtist));

				nodeCollection.Add(new JsonStringValue("AddDate", sData.AddDate.ToString()));

				saveListCollection.Add(nodeCollection);
			}

			using (StreamWriter sw = new StreamWriter(Setting.ListFile)) {
				sw.Write(saveListCollection.ToString());
			}
		}

		private void SortSongs() {
			try {
				Setting.IsSorted = true;
				
				List<int> listPosition = Data.DictSong.OrderBy(x => x.Value.Title, new StringComparer()).Select(y => y.Value.ID).ToList();
				
				Data.PosSong.Clear();
				for (int i = 0; i < listPosition.Count; i++) {
					Data.PosSong.Add(listPosition[i], i);
				}

				RefreshContent();
				Setting.SaveSetting();
				SaveSongList();

				ShuffleList();
			} catch (Exception ex) {
				MessageBox.Show(ex.Message + "\n" + "SortSongs\nLibrary.cs");
			}
		}

		private void Window_PreviewDragLeave(object sender, DragEventArgs e) { }
		private void Window_PreviewDragEnter(object sender, DragEventArgs e) {
			var dropPossible = e.Data != null && ((DataObject)e.Data).ContainsFileDropList();
			if (dropPossible) { }
		}
		private void Window_PreviewDragOver(object sender, DragEventArgs e) {
			e.Handled = true;
		}
		private void Window_PreviewDrop(object sender, DragEventArgs e) {
			if (!(e.Data is DataObject) || !((DataObject)e.Data).ContainsFileDropList()) { return; }

			this.Activate();

			Queue<string> Q = new Queue<string>();
			string strPath;
			List<string> listAdd = new List<string>();

			// BFS
			foreach (string filePath in ((DataObject)e.Data).GetFileDropList()) {
				if (Directory.Exists(filePath)) {
					Q.Enqueue(filePath);

					for (; ; ) {
						if (Q.Count == 0) { break; }
						strPath = Q.Dequeue();
						foreach (string subfile in Directory.GetDirectories(strPath)) {
							if (Directory.Exists(subfile)) { Q.Enqueue(subfile); }
						}

						foreach (string subfile in Directory.GetFiles(strPath)) {
							if (File.Exists(subfile)) {

								listAdd.Add(subfile);
							}
						}
					}
				} else if (File.Exists(filePath)) {
					listAdd.Add(filePath);
				}
			}

			AddFiles(listAdd);
		}

		private void AddFiles(List<string> files) {
			DateTime addDate = DateTime.UtcNow;
			int focusID = -1;
			foreach (string path in files) {
				SongData sData = new SongData() {
					ID = Setting.InsertID,
					FilePath = path, AddDate = addDate,
					New = true,
				};

				try {
					bool isOK = TagLibrary.InsertTagInDatabase(ref sData);
					if (isOK) {
						focusID = Setting.InsertID++;
						Data.PosSong.Add(sData.ID, Data.DictSong.Count);
						Data.DictSong.Add(sData.ID, sData);
					}
				} catch (Exception ex) {
					MessageBox.Show(ex.Message + "\n" + "AddFiles (Top) (Library.cs)");
				}
			}

			try {
				if (focusID >= 0) {
					if (Setting.SortAuto) {
						SortSongs();
					} else {
						Setting.IsSorted = false;
					}
					RefreshContent();
					RefreshContent(Data.PosSong[focusID], 0);

					Setting.SaveSetting();
					SaveSongList();

					ShuffleList();
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message + "\n" + "AddFiles (Library.cs)");
			}
		}

		private void DeleteSong(int id) {
			try {
				Data.PosSong.Remove(id);
				Data.DictSong.Remove(id);

				RefreshContent();

				Setting.SaveSetting();
				SaveSongList();

				ShuffleList();
			} catch (Exception ex) {
				MessageBox.Show(ex.Message + "\n" + "DeleteSong (Library.cs");
			}
		}
	}
}
