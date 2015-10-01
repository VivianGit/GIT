using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.UI.IMenu.Abstracts;
using LeagueSharp.SDK.Core.UI.IMenu.Customizer;
using LeagueSharp.SDK.Core.UI.IMenu.Skins;

using Color = System.Drawing.Color;
using Keys = System.Windows.Forms.Keys;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

using SharpDX.Direct3D9;
using SharpDX;


namespace 拉克丝_SDK_ {
	class Program {

		static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		static Menu Config;
		static Spell Q, W, E, R;
		static Vector3 EPos;

		static void Main(string[] args) {
			InitSpell();
			InitMenu();
			InitEvent();

		}

		private static void InitEvent() {
			Game.OnUpdate += Game_OnUpdate;
			Drawing.OnDraw += Drawing_OnDraw;
			Spellbook.OnCastSpell += Spellbook_OnCastSpell;
			Gapcloser.OnGapCloser += Gapcloser_OnGapCloser;
			Dash.OnDash += Dash_OnDash;
			Game.OnWndProc += Game_OnWndProc;	
		}

		private static void Game_OnWndProc(WndEventArgs args) {
			if (args.WParam == (uint)Config["Q技能"]["半手动Q"].GetValue<MenuKeyBind>().Key)
			{
				CastQ(Q.GetTarget());
            }
			if (args.WParam == (uint)Config["E技能"]["半手动E"].GetValue<MenuKeyBind>().Key)
			{
				E.CastOnBestTarget(0,true);
			}
			if (args.WParam == (uint)Config["R技能"]["半手动R"].GetValue<MenuKeyBind>().Key)
			{
				R.CastOnBestTarget(0,true,1);
				//Game.PrintChat("R.CastOnBestTarget(0,true,1);");
            }
		}

		//是否是二段E
		private static bool IsCastingE() {
			return E.Instance.Name == "LuxLightstrikeToggle" ? true : false;
        }

		private static void Dash_OnDash(object sender, Dash.DashArgs e) {
			var hero = sender as Obj_AI_Hero;
			if (hero.IsAlly) return;

			if (Config["E技能"]["自动E"].GetValue<MenuBool>() )
			{
				//如果他脚本下，就马上引爆
				if (e.StartPos.Distance(EPos)<E.Width && IsCastingE())
				{
					E.Cast(e.EndPos);
				}
				
				//如果没放过E，就扔E到位移末端
				if (E.IsInRange(e.EndPos) && !IsCastingE())//e.EndPos.Distance(Player.Position) < E.Range && 
				{
					E.Cast(e.EndPos);
				}
				
			}
		}

		private static void Gapcloser_OnGapCloser(object sender, Gapcloser.GapCloserEventArgs e) {
			if (e.Sender.ChampionName == "MasterYi" && e.Slot == SpellSlot.Q) return;
			if (e.Sender.ChampionName == "Fizz" && e.Slot == SpellSlot.E) return;

			if (Config["防突进"]["Q防突"].GetValue<MenuBool>() && Config["防突进"][e.Sender.ChampionName].GetValue<MenuBool>() && Q.CanCast(e.Sender))
			{
				Q.Cast(e.Sender);
			}

		}

		private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args) {
			if (sender.Owner.IsMe && args.Slot == SpellSlot.E) EPos = args.EndPosition;

			if (sender.Owner.IsAlly
				|| args.Target.Position.Distance(Player.Position) > W.Range
				|| args.EndPosition.Distance(Player.Position) > W.Range
				|| !(W.Instance.State == SpellState.Ready))
				return;

			double dmg = 0;
			Obj_AI_Hero target = null;
            if (args.Target != null)
			{
				dmg = Damage.GetSpellDamage(sender.Owner as Obj_AI_Hero, args.Target as Obj_AI_Base, args.Slot);
				target = args.Target as Obj_AI_Hero;
            }
			else
			{
				foreach (var ally in GameObjects.AllyHeroes.Where(a=>a.IsValid))
				{
					var castArea = ally.Position.Distance(args.EndPosition) * (args.EndPosition - ally.ServerPosition).Normalized() + ally.ServerPosition;
					if (castArea.Distance(ally.ServerPosition) > ally.BoundingRadius / 2)
						continue;
					dmg = Damage.GetSpellDamage(sender.Owner as Obj_AI_Hero,ally,args.Slot);
					target = ally;
				}
			}
			if (dmg >0)
			{
				double shieldValue = 65 + W.Level * 25 + 0.35 * Player.FlatMagicDamageMod;
				if (dmg > target.Health + shieldValue) return;
				if (dmg >=  shieldValue * 0.95 || dmg > 0.1 * target.Health)
				{
					W.Cast(target);
				}
			}
			
		}

		private static void Drawing_OnDraw(EventArgs args) {
			if (Config["显示设置"]["显示Q"].GetValue<MenuBool>())
			{
				//Drawing.DrawCircle(Player.Position,Q.Range, Config["显示设置"]["Q颜色"].GetValue<MenuColor>());
				new LeagueSharp.SDK.Core.Math.Polygons.Circle(Player.Position, Q.Range,100)
					.Draw(Config["显示设置"]["Q颜色"].GetValue<MenuColor>(),4);
			}
		}

		private static void Game_OnUpdate(EventArgs args) {
			

			if (Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk)
			{
				Combo();
			}

			if (Config["抢人头"]["抢人头"].GetValue<MenuBool>())
			{
				KS();
			}


			#region 自动E
			if (Config["E技能"]["自动E"].GetValue<MenuBool>())
			{
				if (!IsCastingE())
				{
					E.CastIfHitchanceMinimum(E.GetTarget(), HitChance.VeryHigh);

					foreach (var enemy in GameObjects.EnemyHeroes.Where(h => E.IsInRange(h)))
					{
						if (enemy.HasBuffOfType(BuffType.Taunt)
							|| enemy.HasBuffOfType(BuffType.Suppression)
							|| enemy.HasBuffOfType(BuffType.Stun)
							|| enemy.HasBuffOfType(BuffType.Snare)
							|| enemy.HasBuffOfType(BuffType.Knockup)
							|| enemy.HasBuffOfType(BuffType.Invulnerability)
							|| enemy.HasBuffOfType(BuffType.Fear)
							|| enemy.HasBuffOfType(BuffType.Charm))
						{
							E.Cast(enemy);
						}
					}
				}
				else
				{
					//自动引爆E。还要判断一下是第二次E，用Buff
					foreach (var enemy in GameObjects.EnemyHeroes.Where(h => EPos.Distance(h.Position) < E.Width))
					{
						//敌人要出E的范围 或者 中了Q(中了Q有Buff)
						if (EPos.Distance(enemy.Position) >= E.Width * 0.6 || enemy.HasBuff("LuxLightBindingMis"))
						{
							E.Cast();
						}
					}
				}
				
			}
			#endregion

			#region 自动Q
			if (Config["Q技能"]["自动Q"].GetValue<MenuBool>())
			{
				Q.CastIfHitchanceMinimum(Q.GetTarget(),HitChance.VeryHigh);

				foreach (var enemy in GameObjects.EnemyHeroes.Where(h => Q.CanCast(h))){
					#region 老版自动Q中了控制的敌人
					//if (enemy.HasBuffOfType(BuffType.Taunt)
					//	|| enemy.HasBuffOfType(BuffType.Suppression)
					//	|| enemy.HasBuffOfType(BuffType.Stun)
					//	|| enemy.HasBuffOfType(BuffType.Snare)
					//	|| enemy.HasBuffOfType(BuffType.Knockup)
					//	|| enemy.HasBuffOfType(BuffType.Invulnerability)
					//	|| enemy.HasBuffOfType(BuffType.Fear)
					//	|| enemy.HasBuffOfType(BuffType.Charm))
					//{
					//	Q.Cast(enemy);
					//}
					#endregion

					foreach (var buffer in enemy.Buffs)
					{
						if ((buffer.Type == BuffType.Taunt ||
							buffer.Type == BuffType.Suppression ||
							buffer.Type == BuffType.Stun ||
							buffer.Type == BuffType.Snare ||
							buffer.Type == BuffType.Knockup ||
							buffer.Type == BuffType.Invulnerability ||
							buffer.Type == BuffType.Fear ||
							buffer.Type == BuffType.Charm)
							&& buffer.EndTime - Game.ClockTime > 500)
						{
							CastQ(enemy);
						}
					}
				}
            }
			#endregion

			#region 自动R
			if (Config["R技能"]["自动R"].GetValue<MenuBool>())
			{
				//R.CastIfWillHit(R.GetTarget(), 4,true);
				//Game.PrintChat("R.CastIfWillHit(R.GetTarget(), 4);");
			}
			#endregion
		}

		private static void KS() {

			if (Config["抢人头"]["E抢人头"].GetValue<MenuBool>())
			{
				var Etarget = E.GetTarget();
				if (Etarget.Health < GetEdmg(Etarget))
				{
					if (!IsCastingE())
					{
						var epr = E.GetPrediction(Etarget);
						if (epr.Hitchance >= HitChance.High) E.Cast(epr.CastPosition);
					}
					if (IsCastingE() && EPos.Distance(Etarget.Position) < E.Width)
					{
						E.Cast();
					}
				}
			}

			if (Config["抢人头"]["Q抢人头"].GetValue<MenuBool>())
			{
				var Qtarget = Q.GetTarget();
				if (Qtarget.Health < GetQdmg(Qtarget))
				{
					CastQ(Qtarget);
				}
			}

			

			if (Config["抢人头"]["R抢人头"].GetValue<MenuBool>())
			{
				var Rtarget = R.GetTarget();
				if (Rtarget.Health < GetRdmg(Rtarget))
				{
					R.CastIfHitchanceMinimum(Rtarget,HitChance.High);
					//Game.PrintChat("R KS");
                }
				
			}
		}

		private static void Combo() {
			Obj_AI_Hero target = null;
            if (Config["辅助模式"]["目标选择"].GetValue<MenuBool>())
			{
				Obj_AI_Hero sup = null;
                foreach (var ally in GameObjects.AllyHeroes.Where(a => TargetSelector.HighestPriority.Contains(a.ChampionName) && a.Position.Distance(Player.Position)<700))
				{
					sup = ally;
                }
				if (sup!= null && sup.Target is Obj_AI_Hero)
				{
					target = sup.Target as Obj_AI_Hero;
					Config["辅助模式"]["辅助目标"].GetValue<MenuSeparator>().DisplayName = sup.ChampionName;
					Logging.Write()(LogLevel.Info, "辅助目标:"+ sup.ChampionName);
                }
				else
				{
					target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
				}
			}
			else
			{
				target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
			}

			
			if (target.IsInvulnerable || !target.IsValid) return;

			if (!IsCastingE())
			{
				E.CastIfHitchanceMinimum(target, HitChance.High);
			}

			CastQ(target);

			
			if (GetRdmg(target) > target.Health)
			{
				R.CastIfHitchanceMinimum(target,HitChance.VeryHigh);
				//R.Cast(target);
			}
		}

		private static void CastQ(Obj_AI_Hero target) {
			if (target.HasBuff("LuxIlluminatingFraulein") &&Orbwalker.CanAttack && target.Position.Distance(Player.Position) < Player.AttackRange)
			{
				Orbwalker.Orbwalk(target);
			}

			var qpred = Q.GetPrediction(target);
			var qcollision = Q.GetCollision(Player.ServerPosition.ToVector2(), new List<Vector2> { qpred.CastPosition.ToVector2() });
			var minioncol = qcollision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
			
			if (Q.CanCast(target)
				&& minioncol <= 1
				&& qpred.Hitchance >= HitChance.High)
			{
				Q.Cast(target);
			}
				
		}

		private static double GetRdmg(Obj_AI_Hero target) {
			if (R.Level < 1 || !R.CanCast(target)) return 0;
			int[] rBaseDamage = { 300,400,599};
			int BaseDamage = rBaseDamage[R.Level-1];
			return Player.CalculateDamage(target, DamageType.Magical, BaseDamage + (0.75 * Player.FlatMagicDamageMod))
				+ (target.HasBuff("luxilluminatingfraulein") ? GetPassiveDmg(target) : 0);
			
        }

		private static double GetEdmg(Obj_AI_Base target) {
			return
			  Player.CalculateDamage(target, DamageType.Magical,
					new[] { 60, 105, 150, 195, 240 }[Program.E.Level - 1] + 0.6 * Player.FlatMagicDamageMod);
		}
		private static double GetQdmg(Obj_AI_Base target) {
			return
			  Player.CalculateDamage(target, DamageType.Magical,
					new[] { 60, 110, 160, 210, 260 }[Program.Q.Level - 1] + 0.7 * Player.FlatMagicDamageMod);
		}

		private static double GetPassiveDmg(Obj_AI_Hero target) {
			return Player.CalculateDamage(target, DamageType.Magical, 10 + (8 * Player.Level) + (0.2 * Player.FlatMagicDamageMod));
		}

		private static void InitMenu() {
			Config = new Menu("光辉女郎", "光辉女郎", true);

			Menu SupMenu = new Menu("辅助模式", "辅助模式");
			SupMenu.Add(new MenuBool("目标选择", "打ADC的目标", true));
			SupMenu.Add(new MenuSeparator("辅助目标","无"));
			Config.Add(SupMenu);

			Menu QMenu = new Menu("Q技能", "Q技能");
			QMenu.Add(new MenuBool("自动Q", "自动Q", true));
			QMenu.Add(new MenuKeyBind("半手动Q", "半手动Q", Keys.Q,KeyBindType.Press));
			Config.Add(QMenu);

			Menu WMenu = new Menu("W技能", "W技能");
			WMenu.Add(new MenuBool("自动W", "自动W", true));
			WMenu.Add(new MenuKeyBind("半手动W", "半手动W",  Keys.W, KeyBindType.Press));
			Config.Add(WMenu);

			Menu EMenu = new Menu("E技能", "E技能");
			EMenu.Add(new MenuBool("自动E", "自动E", true));
			EMenu.Add(new MenuKeyBind("半手动E", "半手动E",  Keys.E, KeyBindType.Press));
			Config.Add(EMenu);

			Menu RMenu = new Menu("R技能", "R技能");
			RMenu.Add(new MenuBool("自动R", "自动R", true));
			RMenu.Add(new MenuKeyBind("半手动R", "半手动R", Keys.R, KeyBindType.Press));
			Config.Add(RMenu);

			Menu KSMenu = new Menu("抢人头", "抢人头");
			KSMenu.Add(new MenuBool("抢人头", "抢人头", true));
			KSMenu.Add(new MenuBool("Q抢人头", "Q抢人头", true));
			KSMenu.Add(new MenuBool("E抢人头", "E抢人头", true));
			KSMenu.Add(new MenuBool("R抢人头", "R抢人头", true));
			Config.Add(KSMenu);

			Menu GapMenu = new Menu("防突进","防突进");
			GapMenu.Add(new MenuBool("Q防突", "Q防突",true));
			GapMenu.Add(new MenuSeparator("防突进名单", "防突进名单"));
			foreach (var enemy in GameObjects.EnemyHeroes)
			{
				GapMenu.Add(new MenuBool(enemy.ChampionName, enemy.ChampionName, true));
            }
			Config.Add(GapMenu);

			Menu DrawMenu = new Menu("显示设置", "显示设置");
			DrawMenu.Add(new MenuBool("显示Q", "显示Q", true));
			DrawMenu.Add(new MenuColor("Q颜色", "显示颜色", new ColorBGRA(225,225,225,225)));
			//DrawMenu.Add(new MenuBool("显示R", "小地图显示R", true));
			//DrawMenu.Add(new MenuColor("R颜色", "显示颜色", new ColorBGRA()));
			Config.Add(DrawMenu);

			Config.Attach();
		}

		private static void InitSpell() {
			Q = new Spell(SpellSlot.Q, 1275);
			W = new Spell(SpellSlot.W, 1075);
			E = new Spell(SpellSlot.E, 1100);
			R = new Spell(SpellSlot.R, 3500);

			Q.SetSkillshot(0.25f, 70f, 1200f, false, SkillshotType.SkillshotLine);
			W.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.SkillshotLine);
			E.SetSkillshot(0.3f, 255f, 1300f, false, SkillshotType.SkillshotCircle);
			R.SetSkillshot(1.0f, 170f, float.MaxValue, false, SkillshotType.SkillshotLine);
		}
	}
}
