using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings = Kalista.Config.Modes.Combo;
using LeagueSharp;
using LeagueSharp.Common;

namespace Kalista.Modes {
	public class Combo : ModeBase {
		public override bool ShouldBeExecuted() {
			return Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
		}

		public override void Execute() {
			// Item usage
			if (Settings.UseItems && Kalista.IsAfterAttack && Kalista.AfterAttackTarget is Obj_AI_Hero)
			{
				ItemManager.UseBotrk((Obj_AI_Hero)Kalista.AfterAttackTarget);
				ItemManager.UseYoumuu((Obj_AI_Base)Kalista.AfterAttackTarget);
			}

			var target = TargetSelector.GetTarget((Settings.UseQ && Q.IsReady()) ? Q.Range : (E.Range * 1.2f),  TargetSelector.DamageType.Physical);
			if (target != null)
			{
				this.ActiveExploit(target);
				// Q usage
				if (Q.IsReady() && Settings.UseQ && 
					(!Settings.UseQAA 
						|| (Player.GetSpellDamage(target, SpellSlot.Q) > target.TotalShieldHealth() && !target.HasBuffOfType(BuffType.SpellShield))
					) && Player.ManaPercent >= Settings.ManaQ && Q.CastIfHitchanceEquals(target,HitChance.High))
				{
					return;
				}

				// E usage
				var buff = target.GetRendBuff();
				if (Settings.UseE && E.IsReady() && buff != null && E.IsInRange(target))
				{
					// Check if the target would die from E
					if (!Config.Misc.UseKillsteal && target.IsRendKillable() && E.Cast())
					{
						return;
					}

					// Check if target has the desired amount of E stacks on
					if (buff.Count >= Settings.MinNumberE)
					{
						// Check if target is about to leave our E range or the buff is about to run out
						if ((target.Distance(Player) > (E.Range * 0.8) ||
							 buff.EndTime - Game.Time < 0.3) && E.Cast())
						{
							return;
						}
					}

					
					// E to slow
					if (!Config.Misc.UseHarassPlus && Settings.UseESlow &&
						ObjectManager.Get<Obj_AI_Minion>().Any(o => E.IsInRange(o) && o.CanAttack && o.IsRendKillable()) &&
						E.Cast())
					{
						return;
					}
				}
				
				// Auto attacks
				if (Settings.UseAA && Orbwalking.CanAttack() && !Orbwalking.InAutoAttackRange(target) &&
					Config.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
				{
					// Force a new target for the Orbwalker
					var t = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(o => Orbwalking.InAutoAttackRange(o) && o.CanAttack);
					this.ActiveExploit(t);
					Config.Orbwalker.ForceTarget(t);
				}
			}
		}
	}
}
