using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aessmbly {
	public static class Extensions {
		public static bool HasRendBuff(this Obj_AI_Base target) {
			return target.GetRendBuff() != null;
		}

		public static BuffInstance GetRendBuff(this Obj_AI_Base target) {
			return target.Buffs.FirstOrDefault(b => b.Caster.IsMe && b.IsValid &&b.DisplayName== "KalistaExpungeMarker");
			
		}

		public static bool HasUndyingBuff(this Obj_AI_Hero target) {
			if (target.Buffs.Any(
				b => b.IsValid &&
					 (b.DisplayName == "Chrono Shift" ||
					  b.DisplayName == "JudicatorIntervention" ||
					  b.DisplayName == "Undying Rage" )))
			{
				return true;
			}

			if (target.ChampionName == "Poppy")
			{
				if (HeroManager.Allies.Any(o => !o.IsMe && o.Buffs.Any(b => b.Caster.NetworkId == target.NetworkId && b.IsValid && b.DisplayName == "PoppyDITarget")))
				{
					return true;
				}
			}

			return target.IsInvulnerable;
		}

		public static bool HasSpellShield(this Obj_AI_Hero target) {
			return target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
		}

		public static List<T> MakeUnique<T>(this List<T> list) where T : Obj_AI_Base, new() {
			var uniqueList = new List<T>();

			foreach (var entry in list.Where(entry => uniqueList.All(e => e.NetworkId != entry.NetworkId)))
			{
				uniqueList.Add(entry);
			}

			list.Clear();
			list.AddRange(uniqueList);

			return list;
		}

		public static float TotalShieldHealth(this Obj_AI_Base target) {
			return target.Health + target.AllShield + target.PhysicalShield + target.MagicalShield;
		}
	}

}
