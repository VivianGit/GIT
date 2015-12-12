using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aessmbly {
	public static class Damages {
		public static float QDamage;

		private static readonly float[] RawRendDamage = { 20, 30, 40, 50, 60 };
		private static readonly float[] RawRendDamageMultiplier = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
		private static readonly float[] RawRendDamagePerSpear = { 10, 14, 19, 25, 32 };
		private static readonly float[] RawRendDamagePerSpearMultiplier = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };

		static Damages() {
			QDamage =new float[] { 10, 70, 130, 190, 250 }[Program.Q.Level] + Program.Player.BaseAttackDamage; 
		}

		public static bool IsRendKillable(this Obj_AI_Base target) {
			if (target == null || !target.IsValidTarget() || !target.HasRendBuff())
			{
				return false;
			}

			var totalHealth = target.TotalShieldHealth();

			var hero = target as Obj_AI_Hero;
			if (hero != null)
			{
				if (hero.HasUndyingBuff() || hero.HasSpellShield())
				{
					return false;
				}
				if (hero.ChampionName == "Blitzcrank" && !target.HasBuff("BlitzcrankManaBarrierCD") && !target.HasBuff("ManaBarrier"))
				{
					totalHealth += target.Mana / 2;
				}
			}

			return GetRendDamage(target) > totalHealth;
		}

		public static float GetRendDamage(Obj_AI_Base target) {
			return GetRendDamage(target, -1);
		}

		public static float GetRendDamage(Obj_AI_Base target, int customStacks = -1) {
		
            return (float)Program.Player.CalcDamage(target, Damage.DamageType.Physical, 
				GetRawRendDamage(target, customStacks) 
				* (Program.Player.HasBuff("SummonerExhaustSlow") ? 0.6f : 1)
				- Program.Config.Item("reductionE").GetValue<Slider>().Value) ; 
		}

		public static float GetRawRendDamage(Obj_AI_Base target, int customStacks = -1) {
			var stacks = (customStacks > -1  ? customStacks :
				target.HasRendBuff() ? target.GetRendBuff().Count : 0) - 1;
			if (stacks > -1)
			{
				var index = Program.E.Level - 1;
				return RawRendDamage[index] + stacks * RawRendDamagePerSpear[index] +
					   Program.Player.TotalAttackDamage * (RawRendDamageMultiplier[index] + stacks * RawRendDamagePerSpearMultiplier[index]);
			}

			return 0;
		}
	}
}
