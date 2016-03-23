using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using static LeagueSharp.Common.Items;
using Settings = Kalista.Config.Items;

namespace Kalista {
	public class ItemManager {
		private static Obj_AI_Hero Player => HeroManager.Player;

		// Offensive items
		public static readonly Item Cutlass = new Item((int)ItemId.Bilgewater_Cutlass, 550);
		public static readonly Item Botrk = new Item((int)ItemId.Blade_of_the_Ruined_King, 550);

		public static readonly Item Youmuu = new Item((int)ItemId.Youmuus_Ghostblade);

		public static bool UseBotrk(Obj_AI_Hero target) {
			if (Settings.UseBotrk && Botrk.IsReady() && target.IsValidTarget(Botrk.Range) && Player.Health + Player.GetItemDamage(target,  Damage.DamageItems.Botrk) < Player.MaxHealth)
			{
				return Botrk.Cast(target);
			}
			if (Settings.UseCutlass && Cutlass.IsReady() && target.IsValidTarget(Cutlass.Range))
			{
				return Cutlass.Cast(target);
			}
			return false;
		}

		public static bool UseYoumuu(Obj_AI_Base target) {
			
			if (Settings.UseGhostblade && Youmuu.IsReady() && target.IsValidTarget(Player.AttackRange + 100))
			{
				return Youmuu.Cast();
			}
			return false;
		}
	}

}
