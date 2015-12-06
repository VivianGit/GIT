using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Soraka_As_The_Starchild {
	public static class Utill {
		public static string GetMultiLanguageText(string key) {
			if (LeagueSharp.Common.Config.SelectedLanguage != "Chinese")
			{
				return Properties.Resources.ResourceManager.GetString(key);
			}
			return key;
		}
	}
}
