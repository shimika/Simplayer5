using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace Simplayer5 {
	public partial class MainWindow : Window {
		DispatcherTimer Timer_Keydown = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1), };
		string SearchTag = "";

		List<string> ListShortEng, ListShortKor;
		private void RefreshShortcut(List<string> list) {
			ListShortEng = list.Select(x => Regex.Replace(x.ToLower(), @"[^0-9a-zA-Z가-힣]+", "?")).ToList();
			ListShortKor = list.Select(x => Regex.Replace(KoreanDivide(x.ToLower()), @"[^0-9a-zA-Z]+", "?")).ToList();

			foreach (string str in ListShortEng) {
				Console.WriteLine(str);
			}
		}

		private string KoreanDivide(string origStr) {
			string rtStr = "";
			for (int i = 0; i < origStr.Length; i++) {
				char origChar = origStr[i];
				if (origChar == ' ') { continue; }
				int unicode = Convert.ToInt32(origChar);

				uint jongCode = 0;
				uint jungCode = 0;
				uint choCode = 0;

				if (unicode < 44032 || unicode > 55203) {
					rtStr += origChar;
					continue;
				} else {
					uint uCode = Convert.ToUInt32(origChar - '\xAC00');
					jongCode = uCode % 28;
					jungCode = ((uCode - jongCode) / 28) % 21;
					choCode = ((uCode - jongCode) / 28) / 21;
				}
				string[] choChar = new string[] { "r", "R", "s", "e", "E", "f", "a", "q", "Q", "t", "T", "d", "w", "W", "c", "z", "x", "v", "g" };
				string[] jungChar = new string[] { "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l" };
				string[] jongChar = new string[] { "", "r", "R", "rt", "s", "sw", "sg", "e", "f", "fr", "fa", "fq", "ft", "fx", "fv", "fg", "a", "q", "qt", "t", "T", "d", "w", "c", "z", "x", "v", "g" };
				rtStr += choChar[choCode].ToString() + jungChar[jungCode].ToString() + jongChar[jongCode].ToString();
				rtStr = rtStr.Replace(" ", "");
			}
			return rtStr.ToLower();
		}

		private int GetHead(string tag) {
			if (ListShortEng == null) { return -1; }

			Timer_Keydown.Stop();
			Timer_Keydown.Start();

			SearchTag = Regex.Replace(string.Format("{0}{1}", SearchTag, tag).ToLower(), @"[^0-9a-zA-Z]+", "");

			int idx = ListShortEng.FindIndex(x => x.StartsWith(SearchTag));
			if (idx >= 0) { return idx; }

			return ListShortKor.FindIndex(x => x.StartsWith(SearchTag));
		}

		private string GetSearchTag() { return SearchTag; }
		private void ClearSearchTag() { SearchTag = ""; }
		private void KeydownTimer_Tick(object sender, EventArgs e) {
			(sender as DispatcherTimer).Stop();
			ClearSearchTag();
		}
	}
}
