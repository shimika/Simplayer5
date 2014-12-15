using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		private void Notice(string message, bool isnew = true, int delay = 3000) {
			this.Dispatcher.BeginInvoke(new Action(() => {
				textNotice.Text = message;

				if (!isnew) { return; }

				Storyboard sb = new Storyboard();
				sb.Children.Add(GetThicknessAnimation(400, 0, 0, gridNotice, 0, 0));
				sb.Children.Add(GetThicknessAnimation(1000, 0, -30, gridNotice, 0, 0, delay));

				sb.Children.Add(GetDoubleAnimation(1, gridNotice, 400, 0));
				sb.Children.Add(GetDoubleAnimation(0, gridNotice, 1000, delay));

				sb.Begin(this);
			}));
		}
	}
}
