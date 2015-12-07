using LeagueSharp.Common;
using LeagueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aessmbly {
	public class SoulBoundSaver {
		
		public static Obj_AI_Hero SoulBound { get; private set; }

		private static readonly Dictionary<float, float> IncDamage = new Dictionary<float, float>();
		private static readonly Dictionary<float, float> InstDamage = new Dictionary<float, float>();
		public static float IncomingDamage {
			get { return IncDamage.Sum(e => e.Value) + InstDamage.Sum(e => e.Value); }
		}

		public static void Initialize() {
			// Listen to related events
			Game.OnUpdate += OnTick;
			Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
		}

		private static void OnTick(EventArgs args) {
			if (Program.Player.Level>5 && SoulBound == null)
			{
				SoulBound = HeroManager.Allies.Find(h => !h.IsMe && h.Buffs.Any(b => b.Caster.IsMe && b.Name == "kalistacoopstrikeally"));
			}
			else if (Program.Config.Item("saveSoulbound").GetValue<bool>() && Program.R.IsReady())
			{
				if (SoulBound.HealthPercent < 5 && SoulBound.CountEnemiesInRange(500) > 0 ||
					IncomingDamage > SoulBound.Health)
					Program.R.Cast();
			}

			foreach (var entry in IncDamage.Where(entry => entry.Key < Game.Time).ToArray())
			{
				IncDamage.Remove(entry.Key);
			}

			foreach (var entry in InstDamage.Where(entry => entry.Key < Game.Time).ToArray())
			{
				InstDamage.Remove(entry.Key);
			}
		}

		private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
			if (sender.IsEnemy)
			{
				if (SoulBound != null && Program.Config.Item("saveSoulbound").GetValue<bool>())
				{
					if ((!(sender is Obj_AI_Hero) || args.SData.IsAutoAttack()) && args.Target != null && args.Target.NetworkId == SoulBound.NetworkId)
					{
						IncDamage[SoulBound.ServerPosition.Distance(sender.ServerPosition) / args.SData.MissileSpeed + Game.Time] = (float)sender.GetAutoAttackDamage(SoulBound);
					}
					else
					{
						var attacker = sender as Obj_AI_Hero;
						if (attacker != null)
						{
							
							var slot = attacker.GetSpellSlot(args.SData.Name);

							if (slot != SpellSlot.Unknown)
							{
								if (slot == attacker.GetSpellSlot("SummonerDot") && args.Target != null && args.Target.NetworkId == SoulBound.NetworkId)
								{
									InstDamage[Game.Time + 2] = (float)attacker.GetSummonerSpellDamage(SoulBound, Damage.SummonerSpell.Ignite);
									
								}
								else
								{
									switch (slot)
									{
										case SpellSlot.Q:
										case SpellSlot.W:
										case SpellSlot.E:
										case SpellSlot.R:

											if ((args.Target != null && args.Target.NetworkId == SoulBound.NetworkId) || args.End.Distance(SoulBound.ServerPosition) < Math.Pow(args.SData.LineWidth, 2))
											{
												InstDamage[Game.Time + 2] = (float)attacker.GetSpellDamage(SoulBound, slot);
											}

											break;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
