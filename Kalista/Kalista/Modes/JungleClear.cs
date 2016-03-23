using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings = Kalista.Config.Modes.JungleClear;
using LeagueSharp;

namespace Kalista.Modes {
	public class JungleClear : ModeBase {
		public override bool ShouldBeExecuted() {
			return Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;
		}

		public override void Execute() {
			this.ActiveExploit(null);

			if (!Settings.UseE || !E.IsReady())
			{
				return;
			}

			// Get a jungle mob that can die with E
			if (MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral).Any(m => m.IsRendKillable()))
			{
				E.Cast();
			}
		}
	}
}
