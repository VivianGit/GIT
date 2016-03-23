using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kalista {

	public class Kalista {
		public static bool IsAfterAttack { get; private set; }
		public static AttackableUnit AfterAttackTarget { get; private set; }
		public static Orbwalking.Orbwalker Orbwalker = Config.Orbwalker;

		public static void OnLoad(EventArgs args) {
			// Validate champion
			if (HeroManager.Player.ChampionName != "Kalista")
			{
				return;
			}

			// Initialize classes
			Config.Initialize();
			SoulBoundSaver.Initialize();
			ModeLogic.Initialize();
			SentinelManager.Initialize();

			// Enable E damage indicators
			//DamageIndicator.Initialize(Damages.GetRendDamage);
			CNLib.DamageIndicator.Enabled = true;
			CNLib.DamageIndicator.Fill = true;
			CNLib.DamageIndicator.DamageToUnit = Damages.GetRendDamage;

			// Listen to some required events
			Drawing.OnDraw += OnDraw;
			Spellbook.OnCastSpell += OnCastSpell;
			Orbwalking.OnAttack += OnPostAttack;
			//Game.OnPostTick += delegate { IsAfterAttack = false; };
			
		}

		private static void OnPostAttack(AttackableUnit target, AttackableUnit args) {
			IsAfterAttack = true;
			AfterAttackTarget = target;
		}

		public static void OnTickBalistaCheck(EventArgs args) {
			if (!Config.Specials.UseBalista || !SpellManager.R.IsReady() ||
				(Config.Specials.BalistaComboOnly && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo))
			{
				return;
			}
			
			var target = HeroManager.Enemies.Find(o => o.Buffs.Any(b => b.DisplayName == "RocketGrab" && b.Caster.NetworkId == SoulBoundSaver.SoulBound.NetworkId));
			if (target != null && target.IsValidTarget())
			{
				if ((Config.Specials.BalistaMoreHealthOnly && HeroManager.Player.HealthPercent < target.HealthPercent) ||
					HeroManager.Player.Distance(target) < Config.Specials.BalistaTriggerRange)
				{
					// Remove hook, target too close or has more health
					Game.OnUpdate -= OnTickBalistaCheck;
					return;
				}

				// Cast ult
				SpellManager.R.Cast();
				Game.OnUpdate -= OnTickBalistaCheck;
			}
		}

		private static void OnDraw(EventArgs args) {
			// All circles
			foreach (var spell in SpellManager.AllSpells)
			{
				switch (spell.Slot)
				{
					case SpellSlot.Q:
						if (!Config.Drawing.DrawQ)
						{
							continue;
						}
						break;
					case SpellSlot.W:
						if (!Config.Drawing.DrawW)
						{
							continue;
						}
						break;
					case SpellSlot.E:
						if (Config.Drawing.DrawELeaving)
						{
							Render.Circle.DrawCircle(HeroManager.Player.Position, spell.Range * 0.8f, spell.GetColor());
						}
						if (!Config.Drawing.DrawE)
						{
							continue;
						}
						break;
					case SpellSlot.R:
						if (!Config.Drawing.DrawR)
						{
							continue;
						}
						break;
				}
				Render.Circle.DrawCircle(HeroManager.Player.Position, spell.Range, spell.GetColor());
				
			}

			// E damage on healthbar
			CNLib. DamageIndicator.HealthbarEnabled = Config.Drawing.IndicatorHealthbar;
			CNLib.DamageIndicator.PercentEnabled = Config.Drawing.IndicatorPercent;
		}

		private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args) {
			// Avoid stupid Q casts while jumping in mid air!
			if (sender.Owner.IsMe && args.Slot == SpellSlot.Q && HeroManager.Player.IsDashing())
			{
				// Don't process the packet since we are jumping!
				args.Process = false;
			}
		}
	}

}
