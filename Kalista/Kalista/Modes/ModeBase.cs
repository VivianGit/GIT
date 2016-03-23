using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using CNLib;

namespace Kalista.Modes {
	public abstract class ModeBase {
		protected static readonly Obj_AI_Hero Player = HeroManager.Player;

		protected Spell Q
		{
			get { return SpellManager.Q; }
		}
		protected Spell W
		{
			get { return SpellManager.W; }
		}
		protected Spell E
		{
			get { return SpellManager.E; }
		}
		protected Spell R
		{
			get { return SpellManager.R; }
		}

		public abstract bool ShouldBeExecuted();
		public abstract void Execute();

		public void ActiveExploit(Obj_AI_Base target) {
			if (Config.Menu.GetBool("Exploit"))
			{
				if (target == null)
				{
					target = (Obj_AI_Base)Config.Orbwalker.GetTarget();
				}
				if (target.IsValidTarget())
				{
					if (Game.Time * 1000 >= Orbwalking.LastAATick + 1)
					{
						Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
					}
					if (Game.Time * 1000 > Orbwalking.LastAATick + Player.AttackDelay * 1000 - 150f)
					{
						Player.IssueOrder(GameObjectOrder.AttackUnit, target);
					}
				}
				else
				{
					Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
				}
			}
		}
	}
}
