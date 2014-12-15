using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		public int nFromIndex, nToIndex, nMouseDownID, nMouseMovingID;
		public bool isMoving, isMouseDown;
		public Point pointMouseDown, pointMouseMove;

		public void MousePressDown(int id, Point pPoint) {
			nPrevMovingIndex = -1;

			isMouseDown = true; isMoving = false;
			nMouseDownID = id; nToIndex = -1;
			pointMouseDown = pPoint;
			nFromIndex = Data.PosSong[id];

			textNowMoving.Text = Data.DictSong[id].Title;
		}

		private void Window_PreviewMouseMove(object sender, MouseEventArgs e) {
			WindowMouseMove(e.GetPosition(gridListArea));
		}
		private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			MouseReleaseUp();
		}

		public void WindowMouseMove(Point point) {
			if (!isMouseDown || nMouseDownID < 0) { return; }

			pointMouseMove = point;

			if (Math.Max(Math.Abs(pointMouseDown.X - pointMouseMove.X), Math.Abs(pointMouseDown.Y - pointMouseMove.Y)) >= 10 && !isMoving) {
				nMouseMovingID = nMouseDownID;
				isMoving = true;

				gridMoveStatus.Visibility = Visibility.Visible;
			}

			if (!isMoving) { return; }
			CalculatePoint();
		}

		private void CalculatePoint() {
			if (!isMouseDown || nMouseDownID < 0 || !isMoving) { return; }

			if (pointMouseMove.X < 0 || pointMouseMove.X > gridListArea.ActualWidth) {
				nToIndex = -1;
				gridMoveStatus.Visibility = Visibility.Collapsed;
				return;
			} else {
				gridMoveStatus.Visibility = Visibility.Visible;
			}

			gridNowMoving.Margin = new Thickness(pointMouseMove.X - 100, pointMouseMove.Y - 20, 0, 0);

			if (pointMouseMove.Y < -10) {
				nToIndex = -1;
				rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");

				ScrollAll(SongStart - 3, true);
			} else if (pointMouseMove.Y > gridListArea.ActualHeight + 10) {
				nToIndex = -1;
				rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");


				ScrollAll(SongStart + 3, true);
			} else {
				rectMovePosition.Visibility = Visibility.Visible;
				double pointAbsolute = scrollList.VerticalOffset + pointMouseMove.Y;

				int nHoverIndex = ((int)pointAbsolute) / 40;
				nHoverIndex = Math.Max(0, nHoverIndex);
				nHoverIndex = Math.Min(Data.DictSong.Count - 1, nHoverIndex);

				if (pointAbsolute < nHoverIndex * 40 + 20) {
					//textTemp.Text = string.Format("{0}번째의 앞에 : {1} {2}", nHoverIndex, pointMouseMove.Y, pointAbsolute);
					nToIndex = nHoverIndex;
				} else if (pointAbsolute >= nHoverIndex * 40 + 20) {
					//textTemp.Text = string.Format("{0}번째의 뒤에 : {1} {2}", nHoverIndex, pointMouseMove.Y, pointAbsolute);
					nToIndex = nHoverIndex + 1;
				}

				rectMovePosition.Margin = new Thickness(0, nToIndex * 40 - scrollList.VerticalOffset, 0, 0);
			}
		}

		public int nPrevMovingIndex = -1;
		public void MouseReleaseUp() {
			try {
				// Mouse down toggle off
				isMouseDown = false;
				if (nMouseDownID < 0 || !isMoving) { return; }

				// Moving toggle off
				isMoving = false;
				gridMoveStatus.Visibility = Visibility.Collapsed;

				nPrevMovingIndex = nMouseDownID;
				if (nToIndex < 0) { return; }

				// If prev position < target position, reduce target index
				if (Data.PosSong[nMouseDownID] < nToIndex) { nToIndex--; }
				if (Data.PosSong[nMouseDownID] == nToIndex) { nMouseDownID = -1; return; }

				// Ordering song list
				List<int> listPosition = Data.PosSong.OrderBy(x => x.Value).Select(x => x.Key).ToList();
				listPosition.Remove(nMouseDownID);
				listPosition.Insert(nToIndex, nMouseDownID);

				Data.PosSong.Clear();
				for (int i = 0; i < listPosition.Count; i++) {
					Data.PosSong.Add(listPosition[i], i);
				}

				nMouseDownID = -1;

				Setting.IsSorted = false;

				RefreshContent();
				SaveSongList();

				if (Setting.SortAuto) {
					Notice("자동 정렬 옵션이 해제되었습니다.");
					Setting.SortAuto = false;
				}
				RefreshSettingControls();

				ShuffleList();
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		bool isCornerScrollingDelay = false;
		private void DelayTimer(double time, string idTag) {
			DispatcherTimer timerDelay = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(time), IsEnabled = true, Tag = idTag };
			timerDelay.Tick += timerScrollDelay_Tick;
		}

		private void timerScrollDelay_Tick(object sender, EventArgs e) {
			string id = (string)((DispatcherTimer)sender).Tag;

			switch (id) {
				case "isCornerScrollingDelay":
					isCornerScrollingDelay = false;
					((DispatcherTimer)sender).Stop();
					break;
			}
			CalculatePoint();
		}
	}
}
