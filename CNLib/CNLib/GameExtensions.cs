using LeagueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNLib {
	public static class GameExtensions {
		public static float TotalShieldHealth(this Obj_AI_Base target) {
			return target.Health + target.AllShield + target.PhysicalShield + target.MagicalShield;
		}
	}
}
