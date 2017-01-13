using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Simplayer5 {
	public class SongData {
		public int ID, Position, SortPosition, HeadIndex;
		public string Title, Artist, Album, AlbumArtist, FilePath, SortTag, DurationString;
		public TimeSpan Duration;
		public DateTime AddDate;
		public bool Exists = true, New;

		public BitmapImage AlbumArt;
	}

	public class FolderData {
		public string Caption;
		public int Count;

		public FolderData(string caption, int count) {
			this.Caption = caption;
			this.Count = count;
		}
	}

	public class DetailData : IComparable {
		public string Caption, Detail;
		public int Count;

		public DetailData(string caption, string detail, int count) {
			this.Caption = caption;
			this.Detail = detail;
			this.Count = count;
		}

		public int CompareTo(object obj) {
			if (obj == null) { return 1; }

			DetailData d = obj as DetailData;

			int comp = Caption.CompareTo(d.Caption);
			if (comp == 0) {
				return Detail.CompareTo(d.Detail);
			} else {
				return comp;
			}
		}
	}

	class DetailComparer : IEqualityComparer<DetailData> {
		public bool Equals(DetailData x, DetailData y) {
			if (Object.ReferenceEquals(x, y)) { return true; }
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) {
				return false;
			}

			return x.Caption == y.Caption && x.Detail == y.Detail;
		}

		public int GetHashCode(DetailData obj) {
			return (obj.Caption + "?!?" + obj.Detail).GetHashCode();
		}
	}

	class StringComparer : IComparer<string> {
		public int Compare(string x, string y) {
			try {
				int xHeader = DivideString.GetHeadCharIndex(x);
				int yHeader = DivideString.GetHeadCharIndex(y);
				
				if (xHeader == yHeader) {
					return string.Compare(x, y, true);
				}
				return xHeader.CompareTo(yHeader);
			} catch (Exception ex) {
				MessageBox.Show(ex.Message + "\n" + x + "\n" + y);
				return -1;
			}
		}
	}
}
