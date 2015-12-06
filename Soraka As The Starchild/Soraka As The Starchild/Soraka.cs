using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Soraka_As_The_Starchild {
	public class Soraka {

		public static Menu Config;
		public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		public static Orbwalking.Orbwalker Orbwalker;
		public static Spell Q, W, E, R;
		public static Obj_AI_Base DrwaTarget =null;

		public static void Load(EventArgs args) {
			if (Player.ChampionName != "Soraka")
			{
				return;
			}

			LoadSpell();
			LoadMenu();
			

			Game.OnUpdate += Game_OnUpdate;
			Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
			Drawing.OnDraw += Drawing_OnDraw;
        }

		private static void Drawing_OnDraw(EventArgs args) {
			var QShow = Config.Item("显示Q").GetValue<Circle>();
			if (QShow.Active)
			{
				if (Config.Item("技能可用才显示").GetValue<bool>())
				{
					if (Q.IsReady())
					{
						Render.Circle.DrawCircle(Player.Position, Q.Range, QShow.Color, 2);
					}
						
				}
				else
				{
					Render.Circle.DrawCircle(Player.Position, Q.Range, QShow.Color, 2);
				}
			}

			var WShow = Config.Item("显示W").GetValue<Circle>();
			if (WShow.Active)
			{
				if (Config.Item("技能可用才显示").GetValue<bool>())
				{
					if (W.IsReady())
						Render.Circle.DrawCircle(Player.Position, W.Range, WShow.Color, 2);
				}
				else
				{
					Render.Circle.DrawCircle(Player.Position, W.Range, WShow.Color, 2);
				}
			}

			var EShow = Config.Item("显示E").GetValue<Circle>();
			if (EShow.Active)
			{
				if (Config.Item("技能可用才显示").GetValue<bool>())
				{
					if (E.IsReady())
						Render.Circle.DrawCircle(Player.Position, E.Range, EShow.Color, 2);
				}
				else
				{
					Render.Circle.DrawCircle(Player.Position, E.Range, EShow.Color, 2);
				}
			}

			if (Config.Item("辅助目标").GetValue<bool>())
			{
				var target = DrwaTarget;
                if (target != null)
				{
					Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Red, 5);
				}
			}
		}

		private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
			if ((!W.IsReady() && !R.IsReady()) || !sender.IsEnemy)
				return;

			foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid))
			{
				double dmg = 0;

				if (args.Target != null && args.Target.NetworkId == ally.NetworkId)
				{
					dmg = sender.GetSpellDamage(Player, args.SData.Name);
				}
				else
				{
					var castArea = ally.Distance(args.End) * (args.End - ally.ServerPosition).Normalized() + ally.ServerPosition;
					if (castArea.Distance(ally.ServerPosition) > ally.BoundingRadius / 2)
						continue;
					dmg = sender.GetSpellDamage(Player, args.SData.Name);
				}

				if (dmg > 0)
				{
					
					if (dmg>GetWHeal()+GetRHeal()+ally.Health)
					{
						return;
					}
					if (dmg < GetRHeal() + ally.Health && dmg > ally.Health && !W.CanCast(ally))
					{
						if (R.Cast())
						{
							return;
						}
					}
					
					double HpPercentage = (dmg * 100) / ally.Health;
					if (dmg > GetWHeal() && W.Cast(ally)==Spell.CastStates.SuccessfullyCasted)
						return;

						//if (HpPercentage >= 10)
						//	W.Cast(ally);
						//else if (dmg > GetWHeal())
						//	W.Cast(ally);
				}
			}
		}

		private static void Game_OnUpdate(EventArgs args) {

			AutoW();

			switch (Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.LastHit:
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					Mixed();
                    break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					LaneClear();
                    break;
				case Orbwalking.OrbwalkingMode.Combo:
					Combo();
                    break;
				
				default:
					break;
			}
			
        }

		private static void LaneClear() {
			var minionList = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,MinionTeam.NotAlly);
			var farmPosition = Q.GetCircularFarmLocation(minionList, Q.Width);
			if (farmPosition.MinionsHit>=3)
			{
				Q.Cast(farmPosition.Position);
			}
			
		}

		private static void Mixed() {
			var target = GetTarget();
			DrwaTarget = target;

			if (target != null && target.IsValid)
			{
				if (Config.Item("连招Q").GetValue<bool>() && Q.CanCast(target) && Cast(Q, target))
				{
					return;
				}
			}
		}

		private static void AutoW() {
			if (!W.IsReady() && Player.HealthPercent < Config.Item("自己血量").GetValue<Slider>().Value)
			{
				return;
			}
			Obj_AI_Base MostLowHero = null;
			foreach (var ally in HeroManager.Allies.Where(a=>!a.IsMe && a.Distance(Player)<W.Range+100 && !a.IsDead && a.HealthPercent < Config.Item("队友血量").GetValue<Slider>().Value))
			{
				if (MostLowHero == null)
				{
					MostLowHero = ally;
                }
				else if (MostLowHero.HealthPercent > ally.HealthPercent)
				{
					MostLowHero = ally;
				}
			}
			if (MostLowHero!=null && W.IsInRange(MostLowHero))
			{
				W.Cast(MostLowHero);
            }
		}

		private static void Combo() {
			var target = GetTarget();
			DrwaTarget = target;

			if (target != null && target.IsValid)
			{
				if (Config.Item("连招Q").GetValue<bool>() && Q.CanCast(target) && Cast(Q,target))
				{
					return;
				}
				if (Config.Item("连招E").GetValue<bool>() && E.CanCast(target) && Cast(E, target))
				{
					return;
				}
			}
	
        }

		private static Obj_AI_Base GetTarget() {
			var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, false);
			var adc = GetAdc();
			if (adc != null && Config.Item("辅助目标").GetValue<bool>())
			{
				if (adc.Target != null && adc.Target is Obj_AI_Hero && adc.Target.IsValid)
				{
					target = adc.Target as Obj_AI_Hero;
				}
			}
			return target;
        }

		private static Obj_AI_Base GetAdc(float range = 970) {
			Obj_AI_Base Adc = null;
			foreach (var ally in HeroManager.Allies.Where(a => !a.IsDead))
			{
				if (Adc == null)
				{
					Adc = ally;
				}
				else if(Adc.TotalAttackDamage < ally.TotalAttackDamage)
				{
					Adc = ally;
				}
			}
			return Adc;
		}

		private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args) {
			if (sender.IsEnemy && E.CanCast(sender))
			{
				E.Cast(sender);
			}
		}

		private static void LoadSpell() {
			Q = new Spell(SpellSlot.Q, 970);
			W = new Spell(SpellSlot.W, 550);
			E = new Spell(SpellSlot.E, 925);
			R = new Spell(SpellSlot.R);

			Q.SetSkillshot(0.283f, 210, 1100, false, SkillshotType.SkillshotCircle);
			E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);
		}

		private static void LoadMenu() {
			Config = new Menu("星娘", "星娘", true);
			Config.AddToMainMenu();

			var SupportConfig = Config.AddSubMenu(new Menu("辅助模式", "辅助模式"));
			SupportConfig.AddItem(new MenuItem("辅助目标", "攻击ADC的目标[测试]").SetValue(false));
			SupportConfig.AddItem(new MenuItem("辅助模式", "不补刀").SetValue(true));
			SupportConfig.AddItem(new MenuItem("辅助模式距离", "不补刀距离").SetValue(new Slider((int)Player.AttackRange, (int)Player.AttackRange, 2000)));

			var PredictConfig = Config.AddSubMenu(new Menu("预判设置", "预判设置"));
			PredictConfig.AddItem(new MenuItem("预判模式", "预判选择").SetValue(new StringList(new[] { "基本库", "OKTW" })));
			PredictConfig.AddItem(new MenuItem("命中率", "命中率").SetValue(new StringList(new[] { "非常高", "高", "一般" })));

			var OrbMenu = Config.AddSubMenu(new Menu("走砍设置", "走砍设置"));
			Orbwalker = new Orbwalking.Orbwalker(OrbMenu);

			var DrawConfig = Config.AddSubMenu(new Menu("显示设置", "显示设置"));
			DrawConfig.AddItem(new MenuItem("技能可用才显示", "技能可用才显示").SetValue(true));
			DrawConfig.AddItem(new MenuItem("显示Q", "显示 Q 范围").SetValue(new Circle(true, Color.YellowGreen)));
			DrawConfig.AddItem(new MenuItem("显示W", "显示 W 范围").SetValue(new Circle(true, Color.Yellow)));
			DrawConfig.AddItem(new MenuItem("显示E", "显示 E 范围").SetValue(new Circle(true, Color.GreenYellow)));
			DrawConfig.AddItem(new MenuItem("辅助目标", "标识ADC的目标").SetValue(new Circle(true, Color.GreenYellow)));

			var QMenu = Config.AddSubMenu(new Menu("Q技能", "Q技能"));
			QMenu.AddItem(new MenuItem("连招Q", "连招使用Q").SetValue(true));
			QMenu.AddItem(new MenuItem("清线Q", "清线使用Q").SetValue(true));
			QMenu.AddItem(new MenuItem("补刀Q", "补刀使用Q").SetValue(true));

			var WMenu = Config.AddSubMenu(new Menu("W技能", "W技能"));
			WMenu.AddItem(new MenuItem("自动W", "自动W").SetValue(true));
			WMenu.AddItem(new MenuItem("队友血量", "自动W队友血量").SetValue(new Slider(80)));
			WMenu.AddItem(new MenuItem("自己血量", "自动W自己血量").SetValue(new Slider(5)));

			var EMenu = Config.AddSubMenu(new Menu("E技能", "E技能"));
			EMenu.AddItem(new MenuItem("连招E", "连招使用E").SetValue(true));
		}

		private static float GetWHeal() {
			if (!W.IsReady() || W.Level < 1) return 0;
			return new[] { 120, 150, 180, 210, 240 }[W.Level] + Player.FlatMagicDamageMod * 0.6f;
		}

		private static float GetRHeal() {
			if (!R.IsReady() || R.Level < 1) return 0;
			return new[] { 150,250,350 }[R.Level] + Player.FlatMagicDamageMod * 0.55f;
		}

		private static bool Cast(Spell spell, Obj_AI_Base target) {

			if (Config.Item("预判模式").GetValue<StringList>().SelectedIndex == 0)
			{
				var hitChangceList = new[] { HitChance.VeryHigh, HitChance.High, HitChance.Medium };
				return spell.CastIfHitchanceEquals(target, hitChangceList[Config.Item("命中率").GetValue<StringList>().SelectedIndex]);
			}
			if (Config.Item("预判模式").GetValue<StringList>().SelectedIndex == 1)
			{
				var hitChangceList = new[] { OKTWPrediction.HitChance.VeryHigh, OKTWPrediction.HitChance.High, OKTWPrediction.HitChance.Medium };
				return spell.CastOKTW(target, hitChangceList[Config.Item("命中率").GetValue<StringList>().SelectedIndex]);
			}
			return false;
		}
	}
}