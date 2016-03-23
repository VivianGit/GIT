using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace CNLib {
	public static class AutoLevelUp {

		private static Menu menu{ get; set; }
		private static int lv1, lv2, lv3, lv4;

		public static void Initialize(Menu config) {
			menu = config.AddMenu("自动加点", "自动加点");
			
			menu.AddBool("启用", "启用", true);
			menu.AddStringList("最主", "最主", new[] { "Q", "W", "E", "R" },3);
			menu.AddStringList("优先", "优先", new[] { "Q", "W", "E", "R" },1);
			menu.AddStringList("其次", "其次", new[] { "Q", "W", "E", "R" },1);
			menu.AddStringList("最后", "最后", new[] { "Q", "W", "E", "R" },1);
			menu.AddSlider("升级等级", "升级等级",2,6,1);

			Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
		}

		private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args) {
			if (menu.GetBool("启用"))
			{
				lv1 = menu.GetStringIndex("最主");
				lv2 = menu.GetStringIndex("优先");
				lv3 = menu.GetStringIndex("其次");
				lv4 = menu.GetStringIndex("最后");

				if (lv2 == lv3 || lv2 == lv4 || lv3 == lv4)
				{
					Game.PrintChat("[卡莉丝塔]".ToHtml(Color.BlueViolet,FontStlye.Bold) + $"你开启了自动加点，但没有设置加点方案。".ToHtml(Color.SkyBlue));
					return;
				}

				int delay = 700;
				Utility.DelayAction.Add(delay, () => Up(lv1));
				Utility.DelayAction.Add(delay + 50, () => Up(lv2));
				Utility.DelayAction.Add(delay + 100, () => Up(lv3));
				Utility.DelayAction.Add(delay + 150, () => Up(lv4));
			}
		}

		private static void Up(int indx) {
			if (ObjectManager.Player.Level < 4)
			{
				if (indx == 0 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
					ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
				if (indx == 1 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
					ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
				if (indx == 2 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
					ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
			}
			else
			{
				if (indx == 0)
					ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
				if (indx == 1)
					ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
				if (indx == 2)
					ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
				if (indx == 3)
					ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
			}
		}
	}
}
