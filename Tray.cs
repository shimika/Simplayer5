using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		public NotifyIcon TrayNotify = new System.Windows.Forms.NotifyIcon();
		private ToolStripMenuItem CloseMenuItem = new System.Windows.Forms.ToolStripMenuItem("종료");

		private void InitTray() {
			IntPtr iconHandle = Simplayer5.Properties.Resources.icon_tray.Handle;
			TrayNotify.Icon = System.Drawing.Icon.FromHandle(iconHandle);
			TrayNotify.Visible = true; TrayNotify.Text = "Simplayer5";

			ContextMenuStrip contextStrip = new ContextMenuStrip();
			contextStrip.Items.Add(CloseMenuItem);
			TrayNotify.ContextMenuStrip = contextStrip;

			TrayNotify.MouseDoubleClick += TrayNotify_MouseDoubleClick;
			CloseMenuItem.Click += MenuShutdown_Click;
		}

		private void TrayNotify_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				ResumeWindow();
			}
		}
		private void MenuShutdown_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}
