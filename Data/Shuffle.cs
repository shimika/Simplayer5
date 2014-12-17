using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		ListStatus PlayMode = ListStatus.All;
		string ArtistTag, AlbumTag, AlbumArtistTag;

		private void ShuffleAll() {
			PlayMode = ListStatus.All;
			ShuffleList(Data.DictSong.Values.OrderBy(x => Data.PosSong[x.ID]).Select(x => x.ID).ToList());
		}

		private void ShuffleArtist(string artist) {
			PlayMode = ListStatus.Artist;
			ArtistTag = artist;

			ShuffleList(Data.DictSong
				.Where(x => x.Value.Artist == ArtistTag)
				.OrderBy(y => y.Value.Title, new StringComparer())
				.Select(z => z.Key)
				.ToList());
		}

		private void ShuffleAlbum(string album, string albumartist) {
			PlayMode = ListStatus.Album;
			AlbumTag = album;
			AlbumArtistTag = albumartist;

			ShuffleList(Data.DictSong
				.Where(x => x.Value.Album == AlbumTag && x.Value.AlbumArtist == AlbumArtistTag)
				.OrderBy(y => y.Value.Title, new StringComparer())
				.Select(z => z.Key)
				.ToList());
		}

		Dictionary<int, ShuffleData> DictShuffle = new Dictionary<int,ShuffleData>();
		private void ShuffleList(List<int> list) {
			DictShuffle.Clear();

			for (int i = 0; i < list.Count; i++) {
				DictShuffle.Add(list[i], new ShuffleData(
					list[i],
					list[(i + list.Count - 1) % list.Count],
					list[(i + 1) % list.Count]));
			}

			Random random = new Random();
			int[] arr = list.OrderBy(x => random.Next()).ToArray();

			for (int i = 0; i < list.Count; i++) {
				DictShuffle[arr[i]].InitRandData(
					arr[(i + list.Count - 1) % list.Count],
					arr[(i + 1) % list.Count]);
			}
		}

		private void ShuffleList() {
			switch (PlayMode) {
				case ListStatus.All:
					ShuffleAll();
					break;
				case ListStatus.Artist:
					ShuffleArtist(ArtistTag);
					break;
				case ListStatus.Album:
					ShuffleAlbum(AlbumTag, AlbumArtistTag);
					break;
				default:
					return;
			}
		}
	}

	public class ShuffleData {
		private int[] data;
		public ShuffleData(int id, int linearPrev, int linearNext) {
			data = new int[5] { -1, linearPrev, id, linearNext, -1 };
		}

		public void InitRandData(int randPrev, int randNext) {
			data[0] = randPrev;
			data[4] = randNext;
		}

		public int GetID(int direction) {
			return data[direction + 2];
		}
	}
}
