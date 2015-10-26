using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Translator {
	public static class Utill {
		public static string Ansi2Utf(string s) {
			var b = Encoding.Convert(Encoding.Default, Encoding.Unicode, Encoding.Default.GetBytes(s));
			b = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, b);
			return Encoding.Default.GetString(b);

		}

		public static string Utf2Ansi(string s) {
			var b = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, Encoding.Default.GetBytes(s));
			b = Encoding.Convert(Encoding.Unicode, Encoding.Default, b);
			return Encoding.Default.GetString(b);
		}

		public static string TranslateByBaidu(string message, string from, string to) {
			
			string url = string.Format("http://openapi.baidu.com/public/2.0/bmt/translate?client_id=ZFP9CoV9s6guCG5z1uuKOAdQ&q={0}&from={1}&to={2}", message, from, to);

			Console.WriteLine(url);

			var wr = WebRequest.Create(url);
			wr.Timeout = 4000;
			wr.Method = "GET";
			var response = wr.GetResponse();
			using (var stream = response.GetResponseStream())
			{
				if (stream != null)
				{
					var ser = new DataContractJsonSerializer(typeof(TransResult));
					var transResult = (TransResult)ser.ReadObject(stream);
					return transResult.trans_result.First().dst;
				}
			}
			return "";
		}

	}

	[DataContract]
	class TransResult {
		[DataMember]
		public string from { get; set; }
		[DataMember]
		public string to { get; set; }
		[DataMember]
		public List<Trans> trans_result { get; set; }
	}
	[DataContract]
	class Trans {
		[DataMember]
		public string src { get; set; }
		[DataMember]
		public string dst { get; set; }
	}
}
