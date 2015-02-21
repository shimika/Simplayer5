using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Xml;

namespace Simplayer5 {
	public class Lyrics {
		private string soapForm = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:SOAP-ENC=\"http://www.w3.org/2003/05/soap-encoding\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns2=\"ALSongWebServer/Service1Soap\" xmlns:ns1=\"ALSongWebServer\" xmlns:ns3=\"ALSongWebServer/Service1Soap12\"><SOAP-ENV:Body><ns1:GetLyric7><ns1:encData>510090e92db352abad44687c57e07504f1cff1e770d4c31a8607404a26c0ae33cbd95b31c52c54c794b0f2d535b2641f7170bcabf279ba900e50098d946f58496e52541ca01c4e16a7ffaef9d13182fae4baf68ea478b7f2a22a92103d822e6c49c9d187b3911ff325d4dab923da8dc50f3824715083d062a43e28f85eb2f2c0</ns1:encData><ns1:stQuery><ns1:strChecksum>#strChecksum#</ns1:strChecksum><ns1:strVersion>3.19</ns1:strVersion><ns1:strMACAddress>e858217d415f52e84df2d807d2045c5a78f008c9bed684f350f2c78b6fceeacb</ns1:strMACAddress><ns1:strIPAddress>192.168.1.1</ns1:strIPAddress></ns1:stQuery></ns1:GetLyric7></SOAP-ENV:Body></SOAP-ENV:Envelope>";
		private string[] lyrList;
		public string[] LyricLists { get { return lyrList; } }

		public bool GetLyrics(string file) {
			bool ok = parseXML(file);

			if (!ok || lyrList == null) {
				return false;
			}
			return true;
		}

		private bool parseXML(string file) {
			string md5 = GetSongMD5(file);
			if (md5 == "") { return false; }

			XmlDocument doc = webRequest(md5);

			if (doc.HasChildNodes) {
				foreach (XmlNode node in doc.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes) {
					if (node.Name == "strLyric") {
						lyrList = new string[1];
						string str = node.InnerText.Replace("<br>", Environment.NewLine);
						lyrList[0] = str;
						break;
					}
				}
			}

			return true;
		}

		private XmlDocument webRequest(string md5) {
			XmlDocument xml = new XmlDocument();
			HttpWebRequest request = null;
			string str = soapForm.Replace("#strChecksum#", md5);

			try {
				request = (HttpWebRequest)WebRequest.Create("http://lyrics.alsong.co.kr/alsongwebservice/service1.asmx");
				request.ContentType = "application/soap+xml; charset=utf-8";
				request.Method = "POST";
				request.UserAgent = "gSOAP/2.7";

				using (StreamWriter sw = new StreamWriter(((WebRequest)request).GetRequestStream())) {
					sw.WriteLine(str);
				}

				Stream response = request.GetResponse().GetResponseStream();
				xml.Load(response);
			} catch (Exception ex) { }

			return xml;
		}

		public static string GetSongMD5(string filePath) {
			try {
				Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				if (stream == null) { return ""; }

				int p = 0;
				stream.Position = 0L;
				byte[] buffer = new byte[27];
				stream.Read(buffer, 0, 27);

				if (buffer.Length < 27 || stream.Position > 50000L) { return ""; }

				long position = stream.Position;

				string tag = Encoding.ASCII.GetString(buffer, 0, 3);

				if (tag == "ID3") {
					stream.Position -= 17L;
					byte[] tagSize = new byte[4];
					Array.Copy(buffer, 6, tagSize, 0, 4);
					movePosition(searchPosition(stream, ref p, tagSize));
				} else {
					stream.Position = 0L;
					movePosition(stream);
				}

				byte[] pureBytes = new byte[163840];
				stream.Read(pureBytes, 0, 163840);

				return getMD5FromByteArray(pureBytes);
			} catch {
				return "";
			}
		}

		private static Stream searchPosition(Stream stream, ref int position, byte[] tagSize) {
			bool flag = false;
			for (int i = 0; i < 500000; i++) {
				position = 10 + ((int)tagSize[0] << 21 | (int)tagSize[1] << 14 | (int)tagSize[2] << 7 | (int)tagSize[3]);

				if (stream.CanSeek) {
					stream.Position = (long)position;
					flag = true;
					break;
				}
			}

			if (!flag) {
				stream.Position = 0L;
			}
			return stream;
		}

		private static Stream movePosition(Stream stream) {
			for (int i = 0; i < 50000; i++) {
				if (stream.ReadByte() == (int)byte.MaxValue && stream.ReadByte() >> 5 == 7) {
					stream.Position += -2L;
					break;
				}
			}

			return stream;
		}

		private static string getMD5FromByteArray(byte[] byteArray) {
			MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider();
			cryptoServiceProvider.ComputeHash(byteArray);

			byte[] hash = cryptoServiceProvider.Hash;
			StringBuilder stringBuilder = new StringBuilder();

			foreach (byte num in hash) {
				stringBuilder.Append(string.Format("{0:X2}", (object)num));
			}
			return stringBuilder.ToString();
		}
	}
}
