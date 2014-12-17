using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplayer5 {
	class DivideString {
		// Divide hangul
		public static string KoreanDivide(string str) {
			string rtStr = "";
			for (int i = 0; i < str.Length; i++) {
				char origChar = str[i];
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
				char[] choChar = new char[] { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
				char[] jungChar = new char[] { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
				char[] jongChar = new char[] { ' ', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
				rtStr += choChar[choCode].ToString() + jungChar[jungCode].ToString() + jongChar[jongCode].ToString();
				rtStr = rtStr.Replace(" ", "");
			}
			return rtStr;
		}

		public static int FinalIndex = 52;

		private static string IndexCaption = "0123456789ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZぁァあアぃィいイぅゥうウぇェえエぉォおオかカがガきキぎギくクぐグけケげゲこコごゴさサざザしシじジすスずズせセぜゼそソぞゾたタだダちチぢヂっッつツづヅてテでデとトどドなナにニぬヌねネのノはハばバぱパひヒびビぴピふフぶブぷプへヘべベぺペほホぼボぽポまマみミむムめメもモゃャやヤゅュゆユょョよヨらラりリるルれレろロゎヮわワをヲんンヴ―";
		public static int GetHeadCharIndex(string str) {
			if (str == null || str.Length == 0) { return 999; }

			string head = KoreanDivide(str.Substring(0, 1).ToUpper());
			int idx = IndexCaption.IndexOf(head[0]);

			if (idx < 0) { return 999; }
			return idx;
		}

		private static string IndexValue = "1111111111ㄱㄱㄴㄷㄷㄹㅁㅂㅂㅅㅅㅇㅈㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZああああああああああああああああああああかかかかかかかかかかかかかかかかかかかかささささささささささささささささささささたたたたたたたたたたたたたたたたたたたたたたななななななななななははははははははははははははははははははははははははははははままままままままままややややややややややややららららららららららわわわわわわわわわわ";
		private static string IndexUnique = "1ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZあかさたなはまやらわ#";
		public static int GetIndexerIndex(string str) {
			int cap = GetHeadCharIndex(str);

			if (cap == 999) { return FinalIndex - 1; }
			char value = IndexValue[cap];

			return IndexUnique.IndexOf(value);
		}
	}
}
