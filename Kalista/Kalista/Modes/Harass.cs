using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings = Kalista.Config.Modes.Harass;
using LeagueSharp.Common;

namespace Kalista.Modes {
	public class Harass : ModeBase {
		public override bool ShouldBeExecuted() {
			return Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed;
		}

		public override void Execute() {
			this.ActiveExploit(null);
			// Mana check
			if (Player.ManaPercent < Settings.MinMana)
			{
				return;
			}

			if (Settings.UseQ && Q.IsReady())
			{
				var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
				if (target != null)
				{
					Q.Cast(target);
				}
			}
		}
	}
}
