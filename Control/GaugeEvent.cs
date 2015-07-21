using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		bool isPlayingGaugeDown = false;

		private void gridPlayingGauge_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			if (Setting.PlayMode == 0) { return; }

			e.Handled = true;
			double mouseMovingPixel = e.GetPosition(gridPlayingGauge).X;
			mouseMovingPixel = Math.Min(370, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			MoveGauge(mouseMovingPixel, false);

			isPlayingGaugeDown = true;
			gridPlayingGauge.CaptureMouse();
		}
		private void gridPlayingGauge_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			isPlayingGaugeDown = false;
			gridPlayingGauge.ReleaseMouseCapture();
		}
		private void gridPlayingGauge_PreviewMouseMove(object sender, MouseEventArgs e) {
			if (Setting.PlayMode == 0) { return; }
			if (!isPlayingGaugeDown) { return; }

			double mouseMovingPixel = e.GetPosition(gridPlayingGauge).X;
			mouseMovingPixel = Math.Min(370, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			MoveGauge(mouseMovingPixel, false);
		}

		private void MoveGauge(double mouseMovingPixel, bool isSystemCommand = true) {
			rectPlayRatio.Width = mouseMovingPixel;
			circlePlayRatio.Margin = new Thickness(mouseMovingPixel - 6, 0, 0, 0);

			if (!isSystemCommand) {
				try {
					MusicPlayer.Position = new TimeSpan(0, 0,
							(int)((mouseMovingPixel / gridPlayingGauge.ActualWidth) * MusicPlayer.NaturalDuration.TimeSpan.TotalSeconds));

				}
				catch(Exception ex) {
					//Notice(ex.Message);
				}
			}
		}
	}
}
