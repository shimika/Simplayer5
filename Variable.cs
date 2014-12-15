using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		Color MainColor = Colors.SlateBlue;
		Brush MainBrush = Brushes.SlateBlue;
	}

	class Data {
		public static Dictionary<int, SongData> DictSong = new Dictionary<int, SongData>();
		public static Dictionary<int, int> PosSong = new Dictionary<int, int>();
	}
}
