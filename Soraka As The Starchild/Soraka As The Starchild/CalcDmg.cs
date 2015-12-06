using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;

namespace Soraka_As_The_Starchild {
	public class CalcDmg {
		public CalcDmg() {
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
			Spellbook.OnCastSpell += Spellbook_OnCastSpell;
			
		}

		private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args) {
			
		}

		private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
			
		}
	}
}
