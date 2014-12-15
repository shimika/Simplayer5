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
	/// FolderButton.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class FolderButton : UserControl {
		public FolderButton() {
			InitializeComponent();

			this.Caption = "";
		}

		// Property

		public string Caption {
			get { return (string)GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}
		public static readonly DependencyProperty CaptionProperty =
			DependencyProperty.Register("Caption", typeof(string), typeof(FolderButton), new UIPropertyMetadata(null));

		public string Count {
			get { return (string)GetValue(CountProperty); }
			set { SetValue(CountProperty, value); }
		}
		public static readonly DependencyProperty CountProperty =
			DependencyProperty.Register("Count", typeof(string), typeof(FolderButton), new UIPropertyMetadata(null));

		private bool _select;
		public bool Select {
			get { return _select; }
			set {
				_select = value;
				SetValue(SelectedProperty, _select ? Visibility.Visible : Visibility.Collapsed);
			}
		}
		public static readonly DependencyProperty SelectedProperty =
			DependencyProperty.Register("Selected", typeof(Visibility), typeof(FolderButton), new UIPropertyMetadata(null));


		private bool _visible;
		public bool Visible {
			get { return _visible; }
			set {
				_visible = value;
				SetValue(VisibleProperty, _visible ? Visibility.Visible : Visibility.Hidden);
			}
		}
		public static readonly DependencyProperty VisibleProperty =
			DependencyProperty.Register("Visible", typeof(Visibility), typeof(FolderButton), new UIPropertyMetadata(null));

		// Event handler

		public event EventHandler<FolderEventArgs> Response;
		private void Button_Click(object sender, RoutedEventArgs e) {
			if (Response != null) {
				Response(this, new FolderEventArgs("Click", Caption));
			}
		}

		public void SetValue(FolderData data, bool select, bool visible) {
			this.Caption = data.Caption;
			this.Count = data.Count > 0 ? data.Count.ToString() : "";
			this.Select = select;
			this.Visible = visible;

			innerRect.Opacity = 0;
		}

		private void Button_MouseEnter(object sender, MouseEventArgs e) {
			innerRect.Opacity = 1;
		}

		private void Button_MouseLeave(object sender, MouseEventArgs e) {
			innerRect.Opacity = 0;
		}
	}

	public class FolderEventArgs : EventArgs {
		public string PropertyName { get; internal set; }
		public string Caption { get; internal set; }

		public FolderEventArgs(string propertyName, string caption) {
			this.PropertyName = propertyName;
			this.Caption = caption;
		}
	}
}
