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
		private string IndexUnique = "1ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZあかさたなはまやらわ#";

		IndexerButton[] iButton = new IndexerButton[55];

		private void InitIndexer() {
			int count = 0;

			for (int i = 0; i < IndexUnique.Length; i++) {
				if (IndexUnique[i] == 'A') { count = 16; }
				if (IndexUnique[i] == 'あ') { count = 48; }

				iButton[i] = new IndexerButton(IndexUnique[i]);
				iButton[i].Response += IndexerButton_Response;

				Grid.SetRow(iButton[i], count / 8);
				Grid.SetColumn(iButton[i], count % 8);
				gridIndexerInner.Children.Add(iButton[i]);

				count++;
			}

			Timer_Keydown.Tick += KeydownTimer_Tick;
		}

		private void IndexerButton_Response(object sender, IndexerEventArgs e) {
			LaunchIndexer(false, 0);
			RefreshContent(e.Index, 0);
		}

		int[] headIndex = new int[55];
		private void RefreshIndexer(List<string> list) {
			if (ListMode == ListStatus.All && !Setting.IsSorted) {
				gridIndexerInner.Visibility = Visibility.Collapsed;
				buttonIndexerSort.Visibility = Visibility.Visible;
			} else {
				gridIndexerInner.Visibility = Visibility.Visible;
				buttonIndexerSort.Visibility = Visibility.Collapsed;
			}

			headIndex = Enumerable.Repeat(-1, headIndex.Length).ToArray();

			int idx;
			for (int i = 0; i < list.Count; i++) {
				idx = DivideString.GetIndexerIndex(list[i]);

				if (headIndex[idx] < 0) {
					headIndex[idx] = i;
				} else {
					headIndex[idx] = Math.Min(headIndex[idx], i);
				}
			}

			for (int i = 0; i < IndexUnique.Length; i++) {
				iButton[i].SetValue(headIndex[i]);
			}
		}

		private void ListArea_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.MiddleButton == MouseButtonState.Pressed) {
				switch (TabMode) {
					case ViewStatus.All: break;
					case ViewStatus.Artist: break;
					case ViewStatus.Album: break;
					default:
						return;
				}
				LaunchIndexer(true);
			}
		}

		private void gridIndexer_MouseDown(object sender, MouseButtonEventArgs e) {
			LaunchIndexer(false);
		}

		private void LaunchIndexer(bool isVisible, double duration = 150) {
			Setting.isIndexerVisible = isVisible;

			gridIndexer.IsHitTestVisible = Setting.isIndexerVisible;
			gridIndexer.BeginAnimation(Grid.OpacityProperty,
				new DoubleAnimation(Setting.isIndexerVisible ? 1 : 0, TimeSpan.FromMilliseconds(duration)));
		}

		private void buttonIndexerSort_Click(object sender, RoutedEventArgs e) {
			SortSongs();
		}
	}
}
