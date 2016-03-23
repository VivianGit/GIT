using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using System.Threading;

namespace CNLib {
	public static class MultiLanguage {
		private static Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();
		private static Dictionary<string, Dictionary<string, string>> DictionaryList = new Dictionary<string, Dictionary<string, string>>();
		public static bool IsCN { get; private set; }
		static MultiLanguage() {
			IsCN = IsChinese();
		}

		public static string TranslatTo(this string textToTranslate,string language) {
			
			if (string.IsNullOrEmpty(textToTranslate))
			{
				return "";
			}
			if (!DictionaryList.Keys.Contains(language) && language!= "Orbwalker")
			{
				return textToTranslate;
			}
			var Translations = DictionaryList[language];
			if (language == "Orbwalker")
			{
				Translations = OrbwalkerDictionary;
			}
			var show = string.Empty;
			var textToTranslateToLower = textToTranslate.ToLower();
			if (Translations.ContainsKey(textToTranslateToLower))
			{
				show = Translations[textToTranslateToLower];
			}
			else if (Translations.ContainsKey(textToTranslate))
			{
				show = Translations[textToTranslate];
			}
			else
			{
				show = textToTranslate;
			}
			return show;
		}

		public static string _(string textToTranslate) {
			var show = string.Empty;
			if (string.IsNullOrEmpty(textToTranslate))
			{
				return "";
			}
			var textToTranslateToLower = textToTranslate.ToLower();
			if (Translations.ContainsKey(textToTranslateToLower))
			{
				show = Translations[textToTranslateToLower];
			}
			else if (Translations.ContainsKey(textToTranslate))
			{
				show = Translations[textToTranslate];
			}
			else
			{
				show = textToTranslate;
			}
			return show;
		}

		public static void AddLanguage(Dictionary<string, Dictionary<string, string>> LanguageDictionary) {
			DictionaryList = LanguageDictionary;
			if (!IsChinese() && LanguageDictionary.Keys.Contains("English"))
			{
				Translations = LanguageDictionary["English"];
			}

		}

		private static bool IsChinese() {
			
			if (!string.IsNullOrEmpty(Config.SelectedLanguage))
			{
				
				if (Config.SelectedLanguage != "Chinese")
				{
					return true;
				}
			}
			else
			{
				var CultureName = System.Globalization.CultureInfo.InstalledUICulture.Name;
				var lid = CultureName.Contains("-")
						? CultureName.Split('-')[0].ToUpperInvariant()
						: CultureName.ToUpperInvariant();
				
				if (lid == "ZH")
				{
					return true;
				}
			}
			return false;
		}

		private static Dictionary<string, string> OrbwalkerDictionary = new Dictionary<string, string>
		{
			{ "Drawings","显示设置"},
			{ "AACircle","平A范围"},
			{ "Enemy AA circle","敌人平A范围"},
			{ "HoldZone","待命区域"},
			{ "Line Width","线圈宽度"},
			{ "Last Hit Helper","补刀标识"},
			{ "Misc","其它设置"},
			{ "Hold Position Radius","待命区域半径"},
			{ "Priorize farm over harass","优先补刀，其次消耗"},
			{ "Auto attack wards","平A眼"},
			{ "Auto attack pets & traps","平A宠物/分身"},
			{ "Auto attack gangplank barrel","平A船长的桶"},
		};
	}
}
