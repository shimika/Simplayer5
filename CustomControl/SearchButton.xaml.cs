using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	/// SearchButton.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SearchButton : UserControl {
		public SearchButton() {
			InitializeComponent();
		}

		public string Caption {
			get { return (string)GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}
		public static readonly DependencyProperty CaptionProperty =
			DependencyProperty.Register("Caption", typeof(string), typeof(SearchButton), new UIPropertyMetadata(null));

		public string Detail {
			get;
			set;
		}

		public int ID {
			get;set;
		}

		public string Type {
			get { return (string)GetValue(TypeProperty); }
			set { SetValue(TypeProperty, value); }
		}
		public static readonly DependencyProperty TypeProperty =
			DependencyProperty.Register("Type", typeof(string), typeof(SearchButton), new UIPropertyMetadata(null));

		private bool _visible;
		public bool Visible {
			get { return _visible; }
			set {
				_visible = value;
				SetValue(VisibleProperty, _visible ? Visibility.Visible : Visibility.Hidden);
			}
		}
		public static readonly DependencyProperty VisibleProperty =
			DependencyProperty.Register("Visible", typeof(Visibility), typeof(SearchButton), new UIPropertyMetadata(null));

		// Event handler

		public event EventHandler<SearchEventArgs> Response;
		private void Button_Click(object sender, RoutedEventArgs e) {
			if (Response != null) {
				//Response(this, new DetailEventArgs("Click", Caption, Detail));
				Response(this, new SearchEventArgs(this.ID, this.Caption, this.Detail, this.Type));
			}
		}

		public void SetValue(int id, string caption, string detail, string type, bool visible) {
			this.ID = id;
			this.Caption = caption;
			this.Detail = detail;
			this.Type = type;
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

	public class SearchEventArgs : EventArgs {
		public int ID { get; internal set; }
		public string Caption { get; internal set; }
		public string Detail { get; internal set; }
		public string Type { get; internal set; }

		public SearchEventArgs(int id, string caption, string detail, string type) {
			this.ID = id;
			this.Caption = caption;
			this.Detail = detail;
			this.Type = type;
		}
	}
}
