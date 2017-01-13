﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Simplayer5 {
	public class TagLibrary {
		static TagLib.File id3;

		public static bool InsertTagInDatabase(ref SongData sData, bool skipImage = true) {
			string ext = System.IO.Path.GetExtension(sData.FilePath).ToLower();
			if (ext != ".mp3" && ext != ".wma" && ext != ".flac" && ext != ".aac" && ext != ".m4a") { return false; }
			if (!File.Exists(sData.FilePath)) { return false; }
			
			try {
				id3 = TagLib.File.Create(sData.FilePath);
				try {
					try {
						sData.Artist = String.Join(" / ", id3.Tag.Performers);
						if (sData.Artist == null) {
							throw new ArgumentNullException();
						}
					} catch { sData.Artist = ""; }

					try {
						if (id3.Tag.Title.Trim() != null && id3.Tag.Title.Trim() != "") {
							sData.Title = id3.Tag.Title;
						} else {
							sData.Title = System.IO.Path.GetFileName(sData.FilePath);
						}
						if (sData.Title == null) {
							throw new ArgumentNullException();
						}
					} catch {
						sData.Title = System.IO.Path.GetFileName(sData.FilePath);
					}

					try {
						sData.Album = id3.Tag.Album;
						if (sData.Album == null) {
							throw new ArgumentNullException();
						}
					} catch { sData.Album = ""; }

					try {
						sData.AlbumArtist = id3.Tag.AlbumArtists[0];
						if (sData.AlbumArtist == null) {
							throw new ArgumentNullException();
						}
					} catch { sData.AlbumArtist = ""; }

					try {
						//sData.Duration = id3.Properties.Duration.Minutes + ":" + id3.Properties.Duration.streconds.ToString("00");
						sData.Duration = id3.Properties.Duration;
					} catch { sData.Duration = TimeSpan.FromMilliseconds(0); }
				} catch {
					sData.Title = System.IO.Path.GetFileName(sData.FilePath);
					sData.Artist = sData.Album = "";
					sData.Duration = TimeSpan.FromMilliseconds(0);
				}

				sData.AlbumArt = null;
				if (!skipImage) {
					try {
						Bitmap bmp = ByteToImage(id3.Tag.Pictures[0].Data.Data, 100);

						using (MemoryStream memory = new MemoryStream()) {
							bmp.Save(memory, ImageFormat.Bmp);
							memory.Position = 0;
							BitmapImage bitmapImage = new BitmapImage();
							bitmapImage.BeginInit();
							bitmapImage.StreamSource = memory;
							bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
							bitmapImage.EndInit();
							sData.AlbumArt = bitmapImage;
						}
						bmp.Dispose();
					} catch {
						sData.AlbumArt = GetSource("cover.png");
					}
					id3.Dispose();
				}
			} catch (Exception ex) {
				System.Windows.MessageBox.Show("TagLibrary.cs\nInsertTagInDatabase\n\n" + ex.Message);
				return false;
			}

			sData.DurationString = string.Format("{0}:{1:D2}", (int)sData.Duration.TotalMinutes, sData.Duration.Seconds);
			return true;
		}

		public static BitmapImage GetSource(string uriSource) {
			uriSource = "pack://application:,,,/Simplayer5;component/Resources/" + uriSource;
			BitmapImage source = new BitmapImage(new Uri(uriSource));
			return source;
		}

		private static Bitmap ByteToImage(byte[] blob, int hw) {
			using (MemoryStream mStream = new MemoryStream()) {
				mStream.Write(blob, 0, blob.Length);
				mStream.Seek(0, SeekOrigin.Begin);

				Bitmap bm = new Bitmap(mStream);
				Bitmap result = new Bitmap(hw, hw);
				using (Graphics g = Graphics.FromImage(result)) {
					g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					g.FillRectangle(System.Drawing.Brushes.White, 1, 1, hw - 1, hw - 1);
					g.DrawImage(bm, 0, 0, hw, hw);
				}

				bm.Dispose();
				return result;
			}
		}

		public static System.Windows.Media.Color CalculateAverageColor(BitmapSource source) {
			if (source.Format.BitsPerPixel != 32) { return Colors.Black; }

			System.Windows.Size sz = new System.Windows.Size(source.PixelWidth, source.PixelHeight);

			int pixelsSz = (int)sz.Width * (int)sz.Height * (source.Format.BitsPerPixel / 8);
			int stride = ((int)sz.Width * source.Format.BitsPerPixel + 7) / 8;

			byte[] pixels = new byte[pixelsSz];
			source.CopyPixels(pixels, stride, 0);

			const int alphaThershold = 10;
			int[, ,] ck = new int[18, 18, 18];

			for (int y = 0; y < sz.Height; y++) {
				for (int x = 0; x < sz.Width; x++) {
					int index = (int)((y * sz.Width) + x) * 4;
					if (pixels[index + 3] <= alphaThershold) { continue; }

					ck[pixels[index + 2] / 16, pixels[index + 1] / 16, pixels[index] / 16]++;
				}
			}

			List<KeyValuePair<int, int>> klist = new List<KeyValuePair<int, int>>();

			for (int i = 0; i < 16; i++) {
				for (int j = 0; j < 16; j++) {
					for (int k = 0; k < 16; k++) {
						if (i >= 14 && j >= 14 && k >= 14) { continue; }
						klist.Add(new KeyValuePair<int, int>(ck[i, j, k], i * 65536 + j * 256 + k));
					}
				}
			}

			klist.Add(new KeyValuePair<int, int>(1, 0));
			klist.Sort(new Comparison<KeyValuePair<int, int>>(
				(i1, i2) => i2.Key.CompareTo(i1.Key)));

			int r = 0, g = 0, b = 0, pixelCount = 0;
			for (int i = 0; i < Math.Min(klist.Count, 5); i++) {
				r += (klist[i].Value / 65536) * (5 - i) * klist[i].Key;
				g += ((klist[i].Value / 256) % 256) * (5 - i) * klist[i].Key;
				b += (klist[i].Value % 256) * (5 - i) * klist[i].Key;

				pixelCount += (5 - i) * klist[i].Key;
			}

			Console.WriteLine("{0} {1} {2} : {3}\n{4} {5} {6}", r, g, b, pixelCount, r / pixelCount * 16, g / pixelCount * 16, b / pixelCount * 16);

			r = Math.Min(255, r / pixelCount * 16);
			g = Math.Min(255, g / pixelCount * 16);
			b = Math.Min(255, b / pixelCount * 16);

			for (; ; ) {
				if (r < 150 || g < 150 || b < 150) { break; }
				r--; g--; b--;
			}

			return System.Windows.Media.Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
		}
	}
}
