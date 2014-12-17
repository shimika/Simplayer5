using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;

namespace Simplayer5 {
	class Setting {
		// Property

		public static string ListFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer5\Simplayer.ini";
		public static string SettingFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer5\SimplayerPref.ini";
		public static string ffFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer5";

		public static bool TopMost = false, MinToTray = false, Hotkey = false, LyrRight = true, Notification = false, SortAuto = false, PlayDoubleClick = true;
		public static bool LyricsOn = false;
		public static double Volume = 0.5;

		public static bool IsSorted = false, IsVisible = true;
		public static string Version = "5.1";

		public static int InsertID;

		public static bool isIndexerVisible = false;

		public static int PlayMode = 0;
		public static int RandomSeed, PlayingLoopSeed;

		// Method

		public static void LoadSetting() {
			if (!File.Exists(SettingFile)) { SaveSetting(); }

			JsonObjectCollection loadCollection;
			using (StreamReader sr = new StreamReader(SettingFile)) {
				loadCollection = (JsonObjectCollection)(new JsonTextParser().Parse(sr.ReadToEnd()));
			}

			foreach (JsonObject obj in loadCollection) {
				switch (obj.Name) {
					case "TopMost":
						Setting.TopMost = Convert.ToBoolean(obj.GetValue());
						break;
					case "MinToTray":
						Setting.MinToTray = Convert.ToBoolean(obj.GetValue());
						break;
					case "Hotkey":
						Setting.Hotkey = Convert.ToBoolean(obj.GetValue());
						break;
					case "LyrRight":
						Setting.LyrRight = Convert.ToBoolean(obj.GetValue());
						break;
					case "Notification":
						Setting.Notification = Convert.ToBoolean(obj.GetValue());
						break;
					case "SortAuto":
						Setting.SortAuto = Convert.ToBoolean(obj.GetValue());
						break;
					case "PlayDoubleClick":
						Setting.PlayDoubleClick = Convert.ToBoolean(obj.GetValue());
						break;
					case "PlayAll":
						Setting.PlayingLoopSeed = Convert.ToBoolean(obj.GetValue()) ? 1 : 0;
						break;
					case "PlayLinear":
						Setting.RandomSeed = Convert.ToBoolean(obj.GetValue()) ? 1 : 2;
						break;
					case "LyricsOn":
						Setting.LyricsOn = Convert.ToBoolean(obj.GetValue());
						break;
					case "Volume":
						Setting.Volume = Convert.ToDouble(obj.GetValue().ToString());
						break;
					case "IsSorted":
						Setting.IsSorted = Convert.ToBoolean(obj.GetValue());
						break;
				}
			}
		}

		public static void SaveSetting() {

			JsonObjectCollection saveCollection = new JsonObjectCollection();

			saveCollection.Add(new JsonStringValue("TopMost", Setting.TopMost.ToString()));
			saveCollection.Add(new JsonStringValue("MinToTray", Setting.MinToTray.ToString()));
			saveCollection.Add(new JsonStringValue("Hotkey", Setting.Hotkey.ToString()));
			saveCollection.Add(new JsonStringValue("LyrRight", Setting.LyrRight.ToString()));
			saveCollection.Add(new JsonStringValue("Notification", Setting.Notification.ToString()));
			saveCollection.Add(new JsonStringValue("SortAuto", Setting.SortAuto.ToString()));
			saveCollection.Add(new JsonStringValue("PlayDoubleClick", Setting.PlayDoubleClick.ToString()));
			saveCollection.Add(new JsonStringValue("PlayAll", Setting.PlayingLoopSeed == 1 ? "True" : "False"));
			saveCollection.Add(new JsonStringValue("PlayLinear", Setting.RandomSeed == 1 ? "True" : "False"));
			saveCollection.Add(new JsonStringValue("LyricsOn", Setting.LyricsOn.ToString()));
			saveCollection.Add(new JsonStringValue("Volume", Setting.Volume.ToString()));
			saveCollection.Add(new JsonStringValue("IsSorted", Setting.IsSorted.ToString()));

			using (StreamWriter sw = new StreamWriter(SettingFile)) {
				sw.Write(saveCollection.ToString());
			}
		}
	}
}
