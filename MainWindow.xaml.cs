using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simplayer5 {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			this.Left = SystemParameters.MaximizedPrimaryScreenWidth - 440;
			this.Top = SystemParameters.PrimaryScreenHeight - SystemParameters.MaximizedPrimaryScreenHeight + 100;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			InitTab();

			try {
				Setting.LoadSetting();
				SetHotkeyEvent();
				RefreshSettingControls();

				InitListControl();
				InitIndexer();
				InitTray();
				InitPlayer();

				LoadSongList();
				RefreshListType(ListStatus.All);
				ShuffleList();

				ResumeWindow();
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
	}
}
