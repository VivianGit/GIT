using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings = Kalista.Config.Modes.LaneClear;
using LeagueSharp;
using LeagueSharp.Common;

namespace Kalista.Modes {
	public class LaneClear : ModeBase {
		public override bool ShouldBeExecuted() {
			return Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;
		}

		public override void Execute() {
			if (Player.ManaPercent < Settings.MinMana)
			{
				return;
			}

			this.ActiveExploit(null);

			// Precheck
			if (!(Settings.UseQ && Q.IsReady()) && !(Settings.UseE && E.IsReady()))
			{
				return;
			}


			// Minions around
			var minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

			// TODO: Readd Q logic once Collision is added

			#region E usage

			if (Settings.UseE && E.IsReady())
			{
				// Get minions in E range
				var minionsInRange = minions.Where(m => E.IsInRange(m)).ToArray();

				// Validate available minions
				if (minionsInRange.Length >= Settings.MinNumberE)
				{
					// Check if enough minions die with E
					var killableNum = 0;
					foreach (var minion in minionsInRange.Where(minion => minion.IsRendKillable()))
					{
						// Increase kill number
						killableNum++;

						// Cast on condition met
						if (killableNum >= Settings.MinNumberE)
						{
							E.Cast();
							break;
						}
					}
				}
			}

			#endregion
		}
	}
}
