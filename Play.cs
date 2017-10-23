﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		MediaPlayer MusicPlayer = new MediaPlayer();
		PreviewWindow PrevWindow;
		LyricsWindow LyricsWindow;
		int PlayingDirection = 1;

		//public static SongData Setting.NowPlaying = new SongData() { ID = -1 };

		public DispatcherTimer TimerDelay = new DispatcherTimer();

		private void InitPlayer() {
			DispatcherTimer PlayingTimer = new DispatcherTimer() {
				Interval = TimeSpan.FromMilliseconds(200),
			};

			TimerDelay.Tick += TimerDelay_Tick;
			PlayingTimer.Tick += PlayingTimer_Tick;
			PlayingTimer.Start();

			MusicPlayer.MediaEnded += MusicPlayer_MediaEnded;
			MusicPlayer.MediaFailed += MusicPlayer_MediaFailed;

			PrevWindow = new PreviewWindow();
			PrevWindow.Show();
			LyricsWindow = new LyricsWindow(Setting.LyricsOn);
			LyricsWindow.Show();

			textPlayArtist.Text = string.Format("ver.{0}", Version.NowVersion);

			if (Data.DictSong.ContainsKey(Setting.NowPlaying.ID)) {
				SongSelected = Setting.NowPlaying.ID;

				TimeSpan curPos = Setting.NowPlayingPosition;
				int min = (int)curPos.TotalMinutes, sec = curPos.Seconds;

				PlayMusic(Setting.NowPlaying.ID, false, false);
				textPlayTimeNow.Text = string.Format("{0}:{1:D2}", min, sec);
				MusicPlayer.Position = curPos;
				double PlayPerTotal = curPos.TotalSeconds / Setting.NowPlaying.Duration.TotalSeconds;
				MoveGauge(gridPlayingGauge.ActualWidth * PlayPerTotal);
			}
		}

		private void TimerDelay_Tick(object sender, EventArgs e) {
			(sender as DispatcherTimer).Stop();
			isDelay = false;
		}

		private void PlayingTimer_Tick(object sender, EventArgs e) {
			if (Setting.PlayMode == 0) { return; }

			if (Setting.NowPlaying.Duration == TimeSpan.FromSeconds(0)) {
				try {
					Setting.NowPlaying.Duration = MusicPlayer.NaturalDuration.TimeSpan;
				} catch { }
				return;
			}

			TimeSpan nowPos = MusicPlayer.Position; int min, sec;
			min = (int)nowPos.TotalMinutes; sec = nowPos.Seconds;

			string strBackup = textPlayTimeNow.Text;
			string strNow = string.Format("{0}:{1:D2}", min, sec);
			string strTot = string.Format("{0}:{1:D2}",
				(int)Setting.NowPlaying.Duration.TotalMinutes,
				Setting.NowPlaying.Duration.Seconds);

			LyricsWindow.lT.Text = string.Format("{0} / {1}", strNow, strTot);

			textPlayTimeNow.Text = strNow;
			textPlayTimeTotal.Text = strTot;

			double PlayPerTotal = MusicPlayer.Position.TotalSeconds / Setting.NowPlaying.Duration.TotalSeconds;

			if (strBackup != strNow) {
				MoveGauge(gridPlayingGauge.ActualWidth * PlayPerTotal);
			}

			LyricsWindow.GetPlayTime(MusicPlayer.Position);

			TimeSpan savedPos = MusicPlayer.Position.Subtract(new TimeSpan(0, 0, 1));
			if (savedPos.Ticks < 0) {
				savedPos = new TimeSpan(0, 0, 0);
			}
			Setting.NowPlayingPosition = savedPos;

		}

		private void MusicPlayer_MediaEnded(object sender, EventArgs e) {
			MusicPrepare(Setting.NowPlaying.ID, Setting.PlayingLoopSeed * Setting.RandomSeed, false);
		}
		private void MusicPlayer_MediaFailed(object sender, ExceptionEventArgs e) {
			if (Data.DictSong.ContainsKey(Setting.NowPlaying.ID)) {
				Data.DictSong[Setting.NowPlaying.ID].Exists = false;
				ScrollToIndex(true, 0);
			}

			if (!CheckPlaybleMusic()) { return; }
			MusicPrepare(Setting.NowPlaying.ID, PlayingDirection * Setting.RandomSeed, false, true);
		}

		public bool MusicPrepare(int id, int playType, bool showPreview, bool isForced = false) {
			if (!isForced) {
				if (isDelay) { return false; }

				// if preview window on, delay 350ms
				// else				     delay 200ms

				isDelay = true;
				TimerDelay.Interval = TimeSpan.FromMilliseconds(showPreview ? 350 : 200);
				TimerDelay.Start();
			}

			// return 0 if song does'nt exists

			if (DictShuffle.Count == 0) {
				StopPlayer();
				return false;
			}

			// if select specific, nID is positive
			// else ID is negative

			if (!DictShuffle.ContainsKey(id)) {
				ShuffleList();

				PlayingDirection = 1;
				int firstID = 0;

				Random random = new Random();
				foreach (int i in DictShuffle.Keys.OrderBy(x => random.Next()).ToList()) {
					firstID = i;
				}

				MusicPrepare(firstID, 0, showPreview, true);
				return true;
			}

			// positive = next, negative = prev
			// linear = 1, random = 2

			if (playType >= 0) {
				PlayingDirection = 1;
			} else {
				PlayingDirection = -1;
			}

			PlayMusic(DictShuffle[id].GetID(playType), showPreview);

			return true;
		}

		private void PlayMusic(int id, bool showPreview, bool play = true) {
			if (!Data.DictSong.ContainsKey(id)) {
				MusicPrepare(-1, 1, true);
				return;
			}

			Data.DictSong[id].Exists = true;
			Setting.NowPlaying.ID = id;
			Setting.NowPlaying.FilePath = Data.DictSong[id].FilePath;

			bool isOK = TagLibrary.InsertTagInDatabase(ref Setting.NowPlaying, false);

			// If file not exists, or etc errors
			if (!isOK) {
				if (Data.DictSong.ContainsKey(Setting.NowPlaying.ID)) {
					Data.DictSong[id].Exists = false;
					if (!CheckPlaybleMusic()) { return; }
				}

				MusicPrepare(Setting.NowPlaying.ID, PlayingDirection * Setting.RandomSeed, false, true);
				return;
			}

			// Set now playing data
			//CheckTagChanged(id, getSongData);

			Data.DictSong[Setting.NowPlaying.ID].New = false;

			// Play music

			MusicPlayer.Open(new Uri(Data.DictSong[Setting.NowPlaying.ID].FilePath));
			if (play) {
				ResumeMusic();
			}

			// set text
			imageAlbumart.Source = imageBackground.Source = Setting.NowPlaying.AlbumArt;
			textPlayTitle.Text = Setting.NowPlaying.Title;
			textPlayArtist.Text = Setting.NowPlaying.Artist;
			textPlayAlbum.Text = Setting.NowPlaying.Album;

			this.Title = Setting.NowPlaying.Title;

			// extract albumart color and set theme
			Color c = TagLibrary.CalculateAverageColor(Setting.NowPlaying.AlbumArt);
			ChangeThemeColor(c);

			Setting.NowPlaying.ID = id;
			RefreshList();

			PrevWindow.AnimateWindow(
				Setting.NowPlaying.Title,
				Setting.NowPlaying.Artist,
				showPreview && Setting.Notification && !Setting.LyricsOn);

			LyricsWindow.InitLyrics(Setting.NowPlaying);
		}

		private void StopPlayer() {
			Setting.NowPlaying.ID = -1;

			textPlayTimeNow.Text = "0:00";
			textPlayTimeTotal.Text = "0:00";
			buttonPlay.Visibility = Visibility.Visible;
			buttonPause.Visibility = Visibility.Collapsed;

			Setting.PlayMode = 0;
			MoveGauge(0);

			textPlayTitle.Text = this.Title = "Simplayer5";
			textPlayArtist.Text = string.Format("ver.{0}", Version.NowVersion);
			textPlayAlbum.Text = "simple is the best";
			imageAlbumart.Source = imageBackground.Source = TagLibrary.GetSource("cover.png");

			LyricsWindow.ResetLyricsWindow();

			RefreshList();
			MusicPlayer.Stop();
		}

		public void PauseMusic() {
			buttonPlay.Visibility = Visibility.Visible;
			buttonPause.Visibility = Visibility.Collapsed;
			Setting.PlayMode = -1;
			MusicPlayer.Pause();
		}

		public void ResumeMusic() {
			buttonPlay.Visibility = Visibility.Collapsed;
			buttonPause.Visibility = Visibility.Visible;
			MusicPlayer.Play();
			Setting.PlayMode = 1;
		}

		private void buttonPlayPause_Click(object sender, RoutedEventArgs e) { TogglePlayingStatus(); }
		private void TogglePlayingStatus() {
			if (Data.DictSong.Count == 0 && Setting.PlayMode == 0) { return; }

			switch (Setting.PlayMode) {
				case -1: ResumeMusic(); break;
				case 0: MusicPrepare(-1, 1, true); break;
				case 1: PauseMusic(); break;
			}
		}

		private void buttonPrev_Click(object sender, RoutedEventArgs e) { MovePlay(-1, false); }
		private void buttonNext_Click(object sender, RoutedEventArgs e) { MovePlay(1, false); }
		private void MovePlay(int direction, bool showPreview) {
			if (Setting.NowPlaying.ID >= 0) {
				MusicPrepare(Setting.NowPlaying.ID, direction * Setting.RandomSeed, showPreview);
			}
		}

		private bool CheckPlaybleMusic() {
			foreach (SongData sData in Data.DictSong.Values) {
				if (sData.Exists) {
					return true;
				}
			}

			StopPlayer();
			Notice("플레이 가능한 노래가 없습니다.");
			return false;
		}

		// Playing delay timer
		public bool isDelay = false;
	}
}
