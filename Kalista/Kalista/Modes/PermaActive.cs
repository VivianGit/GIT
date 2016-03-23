using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Settings = Kalista.Config.Misc;
using CNLib;

namespace Kalista.Modes {
	public class PermaActive : ModeBase {
		public PermaActive() {
			// Listen to required events
			Orbwalking.OnAttack += OnPostAttack;
			Orbwalking.OnNonKillableMinion += OnUnkillableMinion;
		}

		public override bool ShouldBeExecuted() {
			return true;
		}

		public override void Execute() {
			// Clear the forced target
			Config.Orbwalker.ForceTarget(null);

			//DeBug.WriteConsole("[PermaActive]", "进入Execute", DebugLevel.Info, Config.Menu.GetBool("调试"));
			if (E.IsReady())
			{
				#region Killsteal
				
				if (Settings.UseKillsteal && HeroManager.Enemies.Any(h => h.IsValidTarget(E.Range) && h.IsRendKillable()) && E.Cast())
				{
					return;
				}

				#endregion
				//DeBug.WriteConsole("[PermaActive]", "执行完Killsteal",DebugLevel.Info,Config.Menu.GetBool("调试"));
				#region E on big mobs

				if (Settings.UseEBig)
				{
					
					if (ObjectManager.Get<Obj_AI_Minion>().Any(m =>
					{
						if (!m.IsAlly && E.IsInRange(m) && m.HasRendBuff())
						{
							
							var skinName = m.CharData.BaseSkinName.ToLower();
							return (skinName.Contains("siege") ||
									skinName.Contains("super") ||
									skinName.Contains("dragon") ||
									skinName.Contains("baron") ||
									skinName.Contains("spiderboss")) &&
								   m.IsRendKillable();
						}
						return false;
					}) && E.Cast())
					{
						return;
					}
				}

				#endregion
				//DeBug.WriteConsole("[PermaActive]", "执行完E大车", DebugLevel.Info, Config.Menu.GetBool("调试"));

				#region E combo (harass plus)
				if (Settings.UseHarassPlus)
				{
					//DeBug.WriteConsole("[PermaActive]", "执行E消耗", DebugLevel.Info, Config.Menu.GetBool("调试"));
					if (HeroManager.Enemies.Any(o => o.IsValidTarget(E.Range) && o.HasRendBuff()) &&
						 MinionManager.GetMinions(E.Range).Any(o => o.IsRendKillable()) && E.Cast())
					{
						return;
					}
				}
				#endregion
				//DeBug.WriteConsole("[PermaActive]", "执行完E消耗", DebugLevel.Info, Config.Menu.GetBool("调试"));

				#region E before death

				if (Player.HealthPercent < Settings.AutoEBelowHealth && HeroManager.Enemies.Any(o => o.IsValidTarget() && o.HasRendBuff() && E.IsInRange(o)) && E.Cast())
				{
					return;
				}

				#endregion
				//DeBug.WriteConsole("[PermaActive]", "执行完死前E", DebugLevel.Info, Config.Menu.GetBool("调试"));
			}
			else if (Q.IsReady())
			{
				
			}
		}

		private void OnPostAttack(AttackableUnit target, AttackableUnit args) {
			if (Config.Modes.Combo.UseQAA &&
				Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
				Player.ManaPercent > Config.Modes.Combo.ManaQ &&
				Q.IsReady())
			{
				var hero = target as Obj_AI_Hero;
				if (hero != null && Player.GetAutoAttackDamage(hero) < hero.Health + hero.AllShield + hero.PhysicalShield)
				{
					// Cast Q after auto attack (combo setting)
					Q.Cast(hero);
				}
			}
		}

		private void OnUnkillableMinion(AttackableUnit target) {
			if (Settings.SecureMinionKillsE && E.IsReady() && (target as Obj_AI_Base).IsRendKillable())
			{
				// Cast since it's killable with E
				SpellManager.E.Cast();
			}
		}
	}
}
