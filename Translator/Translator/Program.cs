using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using System.Net;
using System.Runtime.Serialization.Json;

namespace Translator {
	class Program {
		
		public static Menu Config;

		static void Main(string[] args) {
			Config = new Menu("翻译", "翻译测试", true);
			Config.AddItem(new MenuItem("enable","启用").SetValue(true));
			Config.AddItem(new MenuItem("API","翻译来源").SetValue(
				new StringList(new[] {"百度","有道","谷歌"},0)));
			Config.AddItem(new MenuItem("src", "来源语言").SetValue(
				new StringList(new[] { "自动", "中文", "英文", "韩文" }, 0)));
			Config.AddItem(new MenuItem("dec", "目标语言").SetValue(
				new StringList(new[] { "自动", "中文", "英文", "韩文" }, 0)));
			Config.AddItem(new MenuItem("DonotProcess", "不显示原话").SetValue(true));

			Config.AddItem(new MenuItem("NameList","翻译以下人员的消息：")
				.SetFontStyle(0, SharpDX.Color.BlanchedAlmond));
			foreach (var hero in HeroManager.AllHeroes)
			{
				Config.AddItem(new MenuItem(hero.Name, Utill.Utf2Ansi(hero.Name)+"("+ hero.ChampionName+")").SetValue(true));
			}
			Config.AddToMainMenu();

			Game.OnChat += Game_OnChat;
			Game.OnInput += Game_OnInput;
        }

		private static void Game_OnInput(GameInputEventArgs args) {
			if (!Config.Item(ObjectManager.Player.Name).GetValue<bool>()) return;
			
			string from, to, TranslatedString = "";
			from = ((Language)Config.Item("src").GetValue<StringList>().SelectedIndex).ToString();
			to = ((Language)Config.Item("dec").GetValue<StringList>().SelectedIndex).ToString();

			string msg = Utill.Utf2Ansi(args.Input);

			switch (Config.Item("API").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					TranslatedString = Utill.TranslateByBaidu(msg, from, to);
					break;
				default:
					TranslatedString = Utill.TranslateByBaidu(msg, from, to);
					break;
			}

			if (!string.IsNullOrEmpty(TranslatedString))
			{
				Game.Say(TranslatedString);
				args.Process = false;
			}
			else
			{
				Game.PrintChat("出错了！没有得到翻译结果");
			}
		}

		private static void Game_OnChat(GameChatEventArgs args) {
			if (args.Sender.IsMe) return;
			if (!Config.Item(args.Sender.Name).GetValue<bool>()) return;
	
			string from, to , TranslatedString = "";
			from = ((Language)Config.Item("src").GetValue<StringList>().SelectedIndex).ToString();
			to = ((Language)Config.Item("dec").GetValue<StringList>().SelectedIndex).ToString();

			switch (Config.Item("API").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					TranslatedString = Utill.TranslateByBaidu(Utill.Utf2Ansi(args.Message), from,to);
                    break;
				default:
					TranslatedString = Utill.TranslateByBaidu(Utill.Utf2Ansi(args.Message), from, to);
					break;
			}
			if (!string.IsNullOrEmpty(TranslatedString))
			{
				Game.PrintChat("[{0}]{1}({2}):{3}",
				new TimeSpan(0, 0, (int)Game.ClockTime),
				args.Sender.Name,
				args.Sender.ChampionName,
				TranslatedString);
				if (Config.Item("DonotProcess").GetValue<bool>())
				{
					args.Process = false;
				}
			}
			else
			{
				Game.PrintChat("出错了！没有得到翻译结果");
            }
		}

		
	}
}
