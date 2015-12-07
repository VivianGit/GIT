using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aessmbly {
	public class ItemManager {

	
		private static Menu ItemMenu = Program.Config.SubMenu("道具设置");
		public static readonly Items.Item Cutlass = new Items.Item((int)ItemId.Bilgewater_Cutlass, 550);
		public static readonly Items.Item Botrk = new Items.Item((int)ItemId.Blade_of_the_Ruined_King, 550);

		public static readonly Items.Item Youmuu = new Items.Item((int)ItemId.Youmuus_Ghostblade);

		public static bool UseBotrk(Obj_AI_Hero target) {
			if (ItemMenu.Item("botrk").GetValue<bool>() && Botrk.IsReady() && target.IsValidTarget(Botrk.Range) && Program.Player.Health + Program.Player.GetItemDamage(target, Damage.DamageItems.Botrk) < Program.Player.MaxHealth)
			{
				return Botrk.Cast(target);
			}
			if (ItemMenu.Item("cutlass").GetValue<bool>() && Cutlass.IsReady() && target.IsValidTarget(Cutlass.Range))
			{
				return Cutlass.Cast(target);
			}
			return false;
		}

		public static bool UseYoumuu(Obj_AI_Base target) {
			if (ItemMenu.Item("ghostblade").GetValue<bool>() && Youmuu.IsReady() && target.IsValidTarget(Program.Player.AttackRange + 100))
			{
				return Youmuu.Cast();
			}
			return false;
		}
	}

}
