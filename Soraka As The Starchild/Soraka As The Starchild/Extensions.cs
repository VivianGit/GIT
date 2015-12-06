using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;

namespace Soraka_As_The_Starchild {
	public static class Extensions {

		public static bool HasSpellShield(this Obj_AI_Hero target) {
			return target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
		}

		public static bool CastOKTW(this Spell spell, Obj_AI_Base target, OKTWPrediction.HitChance hitChance) {
			var Player = ObjectManager.Player;

			OKTWPrediction.SkillshotType CoreType2 = OKTWPrediction.SkillshotType.SkillshotCircle;
			bool aoe2 = true;

			var predInput2 = new OKTWPrediction.PredictionInput
			{
				Aoe = aoe2,
				Collision = spell.Collision,
				Speed = spell.Speed,
				Delay = spell.Delay,
				Range = spell.Range,
				From = Player.ServerPosition,
				Radius = spell.Width,
				Unit = target,
				Type = CoreType2
			};
			var poutput2 = OKTWPrediction.Prediction.GetPrediction(predInput2);

			if (poutput2.Hitchance >= hitChance)
			{
				return spell.Cast(poutput2.CastPosition);
			}
			return false;
		}

	}
}
