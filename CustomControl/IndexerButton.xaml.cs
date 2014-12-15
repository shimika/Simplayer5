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
	/// IndexerButton.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class IndexerButton : UserControl {
		public IndexerButton(char c) {
			InitializeComponent();
			Header = c.ToString();
		}

		public string Header {
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(IndexerButton), new UIPropertyMetadata(null));

		private int _value;
		public int Value {
			get { return _value; }
			set {
				_value = value;
				SetValue(VisibleProperty, _value < 0 ? Visibility.Collapsed : Visibility.Visible);
			}
		}
		public static readonly DependencyProperty VisibleProperty =
			DependencyProperty.Register("Visible", typeof(Visibility), typeof(IndexerButton), new UIPropertyMetadata(null));

		public void SetValue(int v) {
			this.Value = v;
		}

		public event EventHandler<IndexerEventArgs> Response;
		private void Button_Click(object sender, RoutedEventArgs e) {
			if (Response != null) {
				Response(this, new IndexerEventArgs(Value));
			}
		}
	}

	public class IndexerEventArgs : EventArgs {
		public int Index { get; internal set; }

		public IndexerEventArgs(int index) {
			this.Index = index;
		}
	}
}
