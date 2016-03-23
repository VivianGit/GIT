using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CNLib;
using LeagueSharp;
using System.Drawing;

namespace Kalista {
	public static class Config {
		private const string MenuName = "Kalista";
		public static Menu Menu { get; private set; }
		public static Orbwalking.Orbwalker Orbwalker { get; private set; }

		static Config() {

			Menu = MenuExtensions.CreatMainMenu("kalistaMenu", "卡莉丝塔 - 冠军之矛");

			Orbwalker = Menu.AddOrbwalker("走砍设置", "走砍设置");
			Orbwalker.RegisterCustomMode("逃跑", "逃跑",'S');
			Menu.AddSeparator();
			Menu.AddBool("调试", "调试");

			// All modes
			Modes.Initialize();

			// Misc
			Misc.Initialize();

			// Items
			Items.Initialize();

			// Drawing
			Drawing.Initialize();

			// Specials
			Specials.Initialize();

			AutoLevelUp.Initialize(Menu);
		}

		public static void Initialize() {
			Game.PrintChat(MenuName.ToHtml(32)+$"  你们无路可逃，我说了，你们无路可逃！".ToHtml(Color.BlueViolet));
		}

		public static class Modes {
			private static Menu Menu { get; set; }

			static Modes() {
				// Initialize modes menu
				Menu = Config.Menu.AddMenu( "modes","攻击模式");

				// Combo
				Combo.Initialize();

				// Harass
				//Menu.AddSeparator();
				Harass.Initialize();

				// WaveClear
				//Menu.AddSeparator();
				LaneClear.Initialize();

				// JungleClear
				//Menu.AddSeparator();
				JungleClear.Initialize();

				// Flee
				//Menu.AddSeparator();
				Flee.Initialize();
			}

			public static void Initialize() {

			}

			public static class Combo {
				public static bool UseQ => Menu.GetBool("comboUseQ");
				public static bool UseQAA => Menu.GetBool("comboUseQAA");
				public static bool UseE => Menu.GetBool("comboUseE");
				public static int MinNumberE => Menu.GetSliderValue("comboNumE");
				public static bool UseAA => Menu.GetBool("comboUseAA");
				public static bool UseESlow => Menu.GetBool("comboUseEslow");
				public static bool UseItems => Menu.GetBool("comboUseItems");
				public static int ManaQ => Menu.GetSliderValue("comboMana");
				static Combo() {
					var Combo = Menu.AddMenu("Combo", "连招");
					Combo.AddBool("comboUseQ", "使用 Q",true);
					Combo.AddBool("comboUseQAA", "只在平A后使用Q",false);
					Combo.AddSlider("comboMana", "使用Q蓝量%", 30);
					Combo.AddBool("comboUseE", "使用 E",false);
					Combo.AddBool("comboUseEslow", "E死小兵减速敌人", true);
					Combo.AddSlider("comboNumE", "使用E最少层数", 5, 1, 50);
					Combo.AddBool("comboUseAA", "平A小兵追人",true);
					Combo.AddBool("comboUseItems", "使用道具",true);
				}

				public static void Initialize() {
				}
			}

			public static class Harass {
				
				public static bool UseQ => Menu.GetBool("harassUseQ");
				public static int MinMana => Menu.GetSliderValue("harassMana");

				static Harass() {
					var Harass = Menu.AddMenu("Harass", "消耗");

					Harass.AddBool("harassUseQ", "使用 Q",true);
					Harass.AddSlider("harassMana", "使用Q蓝量%", 30);
				}

				public static void Initialize() {
				}
			}

			public static class LaneClear {
				public static bool UseQ => Menu.GetBool("laneUseQ");
				public static int MinNumberQ => Menu.GetSliderValue("laneNumQ");
				public static bool UseE => Menu.GetBool("laneUseE");
				public static int MinNumberE => Menu.GetSliderValue("laneNumE");
				public static int MinMana => Menu.GetSliderValue("laneMana");

				static LaneClear() {
					var LaneClear = Menu.AddMenu("LaneClear", "清兵");

					LaneClear.AddBool("laneUseQ", "使用 Q",true);
					LaneClear.AddBool("laneUseE", "使用 E", true);
					LaneClear.AddSlider("laneNumQ", "可Q死小兵>?使用Q", 3, 1, 10);
					LaneClear.AddSlider("laneNumE", "可E死小兵>?使用E", 2, 1, 10);
					LaneClear.AddSeparator();
					LaneClear.AddSlider("laneMana", "清兵蓝量", 30);
				}

				public static void Initialize() {
				}
			}

			public static class JungleClear {

				public static bool UseE => Menu.GetBool("jungleUseE");

				static JungleClear() {
					var JungleClear = Menu.AddMenu("JungleClear", "清野");
					JungleClear.AddBool("jungleUseE", "使用 E", true);
				}

				public static void Initialize() {
				}
			}

			public static class Flee {

				public static bool UseWallJumps => Menu.GetBool("fleeWalljump");
				public static bool UseAutoAttacks => Menu.GetBool("fleeAutoattack");

				static Flee() {
					var Flee = Menu.AddMenu("Flee", "逃跑");
					Flee.AddLabel("逃跑键位在走砍中设置");
					Flee.AddBool("fleeWalljump", "跳墙",true);
					Flee.AddBool("fleeAutoattack", "使用平A", true);
				}

				public static void Initialize() {
				}
			}
		}
		
		public static class Sentinel {

			public static bool Enabled => Menu.GetBool("enabled");
			public static bool NoModeOnly => Menu.GetBool("noMode");
			public static bool Alert => Menu.GetBool("alert");
			public static int Mana => Menu.GetSliderValue("mana");

			public static bool SendBaron => Menu.GetBool("baron");
			public static bool SendDragon => Menu.GetBool("dragon");
			public static bool SendMid => Menu.GetBool("mid");
			public static bool SendBlue => Menu.GetBool("blue");
			public static bool SendRed => Menu.GetBool("red");

			static Sentinel() {

				if (Game.MapId == GameMapId.SummonersRift)
				{
					var SentinelMenu = Menu.AddMenu("Sentinel", "哨兵选项");
					SentinelMenu.AddLabel("哨兵 (W) 设置");
					SentinelMenu.AddBool("enabled", "启用", true);
					SentinelMenu.AddBool("noMode", "只在没有攻击模式时使用", true);
					SentinelMenu.AddBool("alert", "哨兵被攻击时提示", true);
					SentinelMenu.AddSlider("mana", "使用W最少蓝量%", 40);
					SentinelMenu.AddSeparator();

					SentinelMenu.AddLabel("查看以下位置");
					SentinelMenu.AddBool("baron", "大龙 (卡BUG)", false).ValueChanged += OnValueChange;
					SentinelMenu.AddBool("dragon", "小龙 (卡BUG)",false).ValueChanged += OnValueChange;
					SentinelMenu.AddBool("mid", "中路草丛",true).ValueChanged += OnValueChange;
					SentinelMenu.AddBool("blue", "蓝buff", true).ValueChanged += OnValueChange;
					SentinelMenu.AddBool("red", "红buff", true).ValueChanged += OnValueChange;
					SentinelManager.RecalculateOpenLocations();
				}
			}

			private static void OnValueChange(object sender, OnValueChangeEventArgs e) {
				SentinelManager.RecalculateOpenLocations();
			}

			public static void Initialize() {
			}
		}

		public static class Items {
			private static Menu Menu { get; set; }

			public static bool UseCutlass => Menu.GetBool("cutlass");
			public static bool UseBotrk => Menu.GetBool("botrk");
			public static bool UseGhostblade => Menu.GetBool("ghostblade");

			static Items() {
				Menu = Config.Menu.AddMenu("Items", "道具选项");

				Menu.AddBool("cutlass", "使用锈水弯刀",true);
				Menu.AddBool("botrk", "使用破解", true);
				Menu.AddBool("ghostblade", "使用幽梦", true);
				
			}

			public static void Initialize() {
			}
		}

		public static class Drawing {
			private static Menu Menu { get; set; }

			public static bool DrawQ => Menu.GetBool("drawQ");
			public static bool DrawW => Menu.GetBool("drawW");
			public static bool DrawE => Menu.GetBool("drawE");
			public static bool DrawELeaving => Menu.GetBool("drawEleaving");
			public static bool DrawR => Menu.GetBool("drawR");
			public static bool IndicatorHealthbar => Menu.GetBool("healthbar");
			public static bool IndicatorPercent => Menu.GetBool("percent");

			static Drawing() {
				Menu = Config.Menu.AddMenu("Drawing", "显示选项");

				Menu.AddLabel("技能范围显示设置");
				Menu.AddBool("drawQ", "Q 范围");
				Menu.AddBool("drawW", "W 范围");
				Menu.AddBool("drawE", "E 范围");
				Menu.AddBool("drawEleaving", "E 触发范围");
				Menu.AddBool("drawR", "R 范围", false);
				Menu.AddSeparator();

				Menu.AddLabel("E伤害显示");
				Menu.AddBool("healthbar", "血条上显示");
				Menu.AddBool("percent", "百分比显示");
			}

			public static void Initialize() {
			}
		}

		public static class Misc {
			private static Menu Menu { get; set; }

			public static bool UseKillsteal => Menu.GetBool("killsteal");
			public static bool UseEBig => Menu.GetBool("bigE");
			public static bool SaveSouldBound => Menu.GetBool("saveSoulbound");
			public static bool SecureMinionKillsE => Menu.GetBool("secureE");
			public static bool UseHarassPlus => Menu.GetBool("harassPlus");
			public static int AutoEBelowHealth => Menu.GetSliderValue("autoBelowHealthE");
			public static int DamageReductionE => Menu.GetSliderValue("reductionE");

			static Misc() {
				Menu = Config.Menu.AddMenu("Misc", "高级选项");

				//Menu.AddLabel("最好全开");
				Menu.AddBool("killsteal", "E抢人头", true);
				Menu.AddBool("bigE", "总是E补大车兵", true);
				Menu.AddBool("saveSoulbound", "R救绑定的队友", true);
				Menu.AddBool("secureE", "E死来不及补的小兵", true);
				Menu.AddBool("harassPlus", "当可以E死小兵并且有敌人身上有E标记时自动E",true);
				Menu.AddSlider("autoBelowHealthE", "血量低于<%自动E", 2);
				Menu.AddSlider("reductionE", "E伤害少算?点", 20);
				Menu.AddBool("Exploit","攻速漏洞");

				// Initialize other misc features
				Sentinel.Initialize();
			}

			public static void Initialize() {
			}


		}

		public static class Specials {
			private static Menu Menu { get; set; }

			public static bool UseBalista => Menu.GetBool("useBalista");
			public static bool BalistaComboOnly => Menu.GetBool("balistaComboOnly");
			public static bool BalistaMoreHealthOnly => Menu.GetBool("moreHealth");
			public static int BalistaTriggerRange => Menu.GetSliderValue("balistaTriggerRange");

			static Specials() {
				Menu = Config.Menu.AddMenu("Specials", "特殊选项");

				Menu.AddLabel("Balista");
				if (HeroManager.Allies.Any(o => o.ChampionName == "Blitzcrank"))
				{
					Menu.AddLabel("infoLabel", "没有灵魂绑定!");
					Game.OnUpdate += BalistaCheckSoulBound;
				}
				else
				{
					Menu.AddLabel("队友没有机器人, 所以没有Balista :(");
				}
			}

			private static void BalistaCheckSoulBound(EventArgs args) {
				if (SoulBoundSaver.SoulBound != null)
				{
					Game.OnUpdate -= BalistaCheckSoulBound;
					Menu.Item("infoLabel").Show(false);

					if (SoulBoundSaver.SoulBound.ChampionName != "Blitzcrank")
					{
						Menu.AddLabel("你没有和机器人绑定!");
						Menu.AddLabel("如果你已经重新绑定, 请重新载入脚本");
						Menu.AddLabel("以识别新的绑定。玩的愉快!");
						return;
					}

					Menu.AddBool("useBalista", "启用",true);
					Menu.AddSeparator();
					Menu.AddBool("balistaComboOnly", "只在连招时使用", false);
					Menu.AddBool("moreHealth", "只在我血量较多时使用");

					const int blitzcrankQrange = 925;
					Menu.AddSlider("balistaTriggerRange", "你的被Q敌人距离>?时使用", (int)SpellManager.R.Range, (int)SpellManager.R.Range,
							(int)(SpellManager.R.Range + blitzcrankQrange * 0.8f));

					// Handle Blitzcrank hooks in Kalista.OnTickBalistaCheck
				
					Obj_AI_Base.OnBuffAdd += delegate (Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs eventArgs) {
						if (eventArgs.Buff.DisplayName == "RocketGrab" && eventArgs.Buff.Caster.NetworkId == SoulBoundSaver.SoulBound.NetworkId)
						{
							Game.OnUpdate += Kalista.OnTickBalistaCheck;
						}
					};
					Obj_AI_Base.OnBuffRemove += delegate (Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs eventArgs) {
						if (eventArgs.Buff.DisplayName == "RocketGrab" && eventArgs.Buff.Caster.NetworkId == SoulBoundSaver.SoulBound.NetworkId)
						{
							Game.OnUpdate -= Kalista.OnTickBalistaCheck;
						}
					};
				}
			}

			public static void Initialize() {
			}
		}

		public static class Credits {
			private static Menu Menu { get; set; }

			static Credits() {
				Menu = Config.Menu.AddMenu("Credits", "脚本信息");

				Menu.AddLabel("LSFU - Kalista");
			}

			public static void Initialize() {
			}
		}
	}

}
