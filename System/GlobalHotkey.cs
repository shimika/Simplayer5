using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace Simplayer5 {
	public partial class MainWindow : Window {		
		// Global hotkey

		HwndSource HWndSource; 
		private int PlayPauseKey, Stopkey, Prevkey, Nextkey, Lyrkey, SynPrevkey, SynNextkey, LaunchKey;

		private void SetHotkeyEvent() {
			WindowInteropHelper wih = new WindowInteropHelper(this);
			HWndSource = HwndSource.FromHwnd(wih.Handle);
			HWndSource.AddHook(MainWindowProc);

			PlayPauseKey = WinAPI.GlobalAddAtom("ButtonPlayPause");
			Stopkey = WinAPI.GlobalAddAtom("ButtonStop");
			Prevkey = WinAPI.GlobalAddAtom("ButtonPrev");
			Nextkey = WinAPI.GlobalAddAtom("ButtonNext");
			Lyrkey = WinAPI.GlobalAddAtom("LyricsKey");
			SynPrevkey = WinAPI.GlobalAddAtom("SyncPrevKey");
			SynNextkey = WinAPI.GlobalAddAtom("SyncNextKey");
			LaunchKey = WinAPI.GlobalAddAtom("ListShowHide");
		}

		private void ToggleHotKeyMode(bool isON) {
			WindowInteropHelper wih = new WindowInteropHelper(this);

			if (isON) {
				WinAPI.RegisterHotKey(wih.Handle, PlayPauseKey, 3, WinAPI.VK_DOWN);
				WinAPI.RegisterHotKey(wih.Handle, Stopkey, 3, WinAPI.VK_UP);
				WinAPI.RegisterHotKey(wih.Handle, Prevkey, 3, WinAPI.VK_LEFT);
				WinAPI.RegisterHotKey(wih.Handle, Nextkey, 3, WinAPI.VK_RIGHT);
				WinAPI.RegisterHotKey(wih.Handle, Lyrkey, 3, WinAPI.VK_KEY_D);
				WinAPI.RegisterHotKey(wih.Handle, SynPrevkey, 3, WinAPI.VK_OEM_COMMA);
				WinAPI.RegisterHotKey(wih.Handle, SynNextkey, 3, WinAPI.VK_OEM_PERIOD);
				WinAPI.RegisterHotKey(wih.Handle, LaunchKey, 8, WinAPI.VK_KEY_W);
			} else {
				WinAPI.UnregisterHotKey(wih.Handle, PlayPauseKey);
				WinAPI.UnregisterHotKey(wih.Handle, Stopkey);
				WinAPI.UnregisterHotKey(wih.Handle, Prevkey);
				WinAPI.UnregisterHotKey(wih.Handle, Nextkey);
				WinAPI.UnregisterHotKey(wih.Handle, Lyrkey);
				WinAPI.UnregisterHotKey(wih.Handle, SynPrevkey);
				WinAPI.UnregisterHotKey(wih.Handle, SynNextkey);
				WinAPI.UnregisterHotKey(wih.Handle, LaunchKey);
			}
		}

		private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if (msg == WinAPI.WM_HOTKEY) {
				if (wParam.ToString() == PlayPauseKey.ToString()) {
					TogglePlayingStatus();
				} else if (wParam.ToString() == Stopkey.ToString()) {
					StopPlayer();
				} else if (wParam.ToString() == Prevkey.ToString()) {
					MovePlay(-1, true);
				} else if (wParam.ToString() == Nextkey.ToString()) {
					MovePlay(1, true);
				} else if (wParam.ToString() == Lyrkey.ToString()) {
					buttonLyrics.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
				} else if (wParam.ToString() == SynPrevkey.ToString()) {
					if (Setting.PlayMode != 0) {
						LyricsWindow.ChangeOffset(-200);
					}
				} else if (wParam.ToString() == SynNextkey.ToString()) {
					if (Setting.PlayMode != 0) {
						LyricsWindow.ChangeOffset(200);
					}
				} else if (wParam.ToString() == LaunchKey.ToString()) {
					ResumeWindow();
				}
				handled = true;
			}
			return IntPtr.Zero;
		}
	}
}
