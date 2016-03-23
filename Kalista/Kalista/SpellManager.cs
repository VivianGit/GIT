using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kalista {
	public static class SpellManager {
		public static Spell Q { get; private set; }
		public static Spell W { get; private set; }
		public static Spell E { get; private set; }
		public static Spell R { get; private set; }

		public static List<Spell> AllSpells { get; private set; }

		public static Dictionary<SpellSlot, Color> ColorTranslation { get; private set; }

		static SpellManager() {
			// Initialize spells
			Q = new Spell(SpellSlot.Q, 1150 );
			W = new Spell(SpellSlot.W, 5000);
			E = new Spell(SpellSlot.E, 1000);
			R = new Spell(SpellSlot.R, 1500);

			Q.SetSkillshot(250, 1200, 40,true,SkillshotType.SkillshotLine);

			AllSpells = new List<Spell>(new Spell[] { Q, W, E, R });
			ColorTranslation = new Dictionary<SpellSlot, Color>
			{
				{ SpellSlot.Q, Color.IndianRed },
				{ SpellSlot.W, Color.MediumPurple},
				{ SpellSlot.E, Color.DarkRed},
				{ SpellSlot.R, Color.Red }
			};
		}

		public static Color GetColor(this Spell spell) {
			return ColorTranslation.ContainsKey(spell.Slot) ? ColorTranslation[spell.Slot] : Color.Wheat;
		}
	}
}
