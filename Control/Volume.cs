using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		private void buttonVolume_Click(object sender, RoutedEventArgs e) {
			if (PopupMode == PopupStatus.Volume) {
				ChangePopupMode(PopupStatus.None);
			} else {
				ChangePopupMode(PopupStatus.Volume);
			}
		}

		bool isVolumeMouseDown = false;

		private void gridVolume_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			double mouseMovingPixel = e.GetPosition(gridVolume).Y - 10;
			mouseMovingPixel = Math.Min(80, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			mouseMovingPixel = 80 - mouseMovingPixel;

			//MusicPlayer.Volume = mouseMovingPixel / 80;
			rectVolumeBar.Height = mouseMovingPixel;
			textVolume.Text = ((int)((mouseMovingPixel / 80) * 100)).ToString();
			Setting.Volume = mouseMovingPixel / 80;

			isVolumeMouseDown = true;
			gridVolume.CaptureMouse();
		}
		private void gridVolume_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			isVolumeMouseDown = false;
			gridVolume.ReleaseMouseCapture();
			RefreshSettingControls();
		}
		private void gridVolume_PreviewMouseMove(object sender, MouseEventArgs e) {
			if (!isVolumeMouseDown) { return; }
			double mouseMovingPixel = e.GetPosition(gridVolume).Y - 10;
			mouseMovingPixel = Math.Min(80, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			mouseMovingPixel = 80 - mouseMovingPixel;

			//MusicPlayer.Volume = mouseMovingPixel / 80;
			rectVolumeBar.Height = mouseMovingPixel;
			textVolume.Text = ((int)((mouseMovingPixel / 80) * 100)).ToString();
			Setting.Volume = mouseMovingPixel / 80;
		}
	}
}
