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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simplayer5 {
	/// <summary>
	/// SongButton.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SongButton : UserControl {
		public SongButton() {
			InitializeComponent();
		}

		// Property

		public string Title {
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}
		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register("Title", typeof(string), typeof(SongButton), new UIPropertyMetadata(null));

		public string Count {
			get { return (string)GetValue(CountProperty); }
			set { SetValue(CountProperty, value); }
		}
		public static readonly DependencyProperty CountProperty =
			DependencyProperty.Register("Count", typeof(string), typeof(SongButton), new UIPropertyMetadata(null));

		public int ID {
			get { return (int)GetValue(IDProperty); }
			set { SetValue(IDProperty, value); }
		}
		public static readonly DependencyProperty IDProperty =
			DependencyProperty.Register("ID", typeof(int), typeof(SongButton), new UIPropertyMetadata(null));

		private bool _select;
		public bool Select {
			get { return _select; }
			set {
				_select = value;
				SetValue(SelectedProperty, _select ? Visibility.Visible : Visibility.Collapsed);
			}
		}
		public static readonly DependencyProperty SelectedProperty =
			DependencyProperty.Register("Selected", typeof(Visibility), typeof(SongButton), new UIPropertyMetadata(null));

		private bool _new = false;
		public bool New {
			get { return _new; }
			set {
				_new = value;
				SetValue(NewProperty, _new ? Visibility.Visible : Visibility.Collapsed);
			}
		}
		public static readonly DependencyProperty NewProperty =
			DependencyProperty.Register("New", typeof(Visibility), typeof(SongButton), new UIPropertyMetadata(null));

		private bool _visible = false;
		public bool Visible {
			get { return _visible; }
			set {
				_visible = value;
				SetValue(VisibleProperty, _visible ? Visibility.Visible : Visibility.Hidden);
			}
		}
		public static readonly DependencyProperty VisibleProperty =
			DependencyProperty.Register("Visible", typeof(Visibility), typeof(SongButton), new UIPropertyMetadata(null));

		private bool _playing = false;
		public bool Playing {
			get { return _playing; }
			set {
				_playing = value;
				SetValue(PlayingProperty, _playing ? Visibility.Visible : Visibility.Hidden);
			}
		}
		public static readonly DependencyProperty PlayingProperty =
			DependencyProperty.Register("Playing", typeof(Visibility), typeof(SongButton), new UIPropertyMetadata(null));

		private bool _error = false;
		public bool Error {
			get { return _error; }
			set {
				_error = value;
				SetValue(ErrorProperty, _error ? Visibility.Visible : Visibility.Hidden);
			}
		}
		public static readonly DependencyProperty ErrorProperty =
			DependencyProperty.Register("Error", typeof(Visibility), typeof(SongButton), new UIPropertyMetadata(null));

		// Event handler

		public event EventHandler<SongEventArgs> Response;


		private void Button_MouseDown(object sender, MouseButtonEventArgs e) {
			if (Response != null) {
				Response(this, new SongEventArgs("MouseDown", ID));
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			if (Response != null) {
				Response(this, new SongEventArgs("Click", ID));
			}

			if (!Setting.PlayDoubleClick) {
				RectAnimate();
			}
		}

		private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (Response != null) {
				Response(this, new SongEventArgs("DoubleClick", ID));
			}

			if (Setting.PlayDoubleClick) {
				RectAnimate();
			}
		}

		private void MenuItem_Open(object sender, RoutedEventArgs e) {
			if (Response != null) {
				Response(this, new SongEventArgs("Open", ID));
			}
		}

		private void MenuItem_Delete(object sender, RoutedEventArgs e) {
			if (Response != null) {
				Response(this, new SongEventArgs("Delete", ID));
			}
		}

		public void SetValue(SongData data, bool select, bool visible, bool playing) {
			this.ID = data.ID;
			this.Title = data.Title;
			this.Count = data.DurationString;
			this.New = data.New;
			this.Select = select;
			this.Visible = visible;
			this.Playing = playing;
			this.Error = !data.Exists;

			innerRect.Opacity = 0;
		}

		private void Button_MouseEnter(object sender, MouseEventArgs e) {
			innerRect.Opacity = 1;
		}

		private void Button_MouseLeave(object sender, MouseEventArgs e) {
			innerRect.Opacity = 0;
		}

		private void RectAnimate() {
			return;
			rectSelect.BeginAnimation(Rectangle.WidthProperty,
				new DoubleAnimation(50, 5, TimeSpan.FromMilliseconds(500)) {
					EasingFunction = new PowerEase() {
						Power = 4, EasingMode = EasingMode.EaseOut,
					}
				});
		}
	}

	public class SongEventArgs : MouseButtonEventArgs {
		public string PropertyName { get; internal set; }
		public int ID { get; internal set; }

		public SongEventArgs(string propertyName, int id)
			: base(Mouse.PrimaryDevice, new TimeSpan(DateTime.Now.Ticks).Milliseconds, MouseButton.Left) {

			this.PropertyName = propertyName;
			this.ID = id;
		}
	}
}
