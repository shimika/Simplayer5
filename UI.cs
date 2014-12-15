using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		private Visibility GetVisibility(bool b) {
			return b ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
