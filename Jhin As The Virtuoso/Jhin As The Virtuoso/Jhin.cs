using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SebbyLib;
using SharpDX;
using DXColor = SharpDX.Color;
using Color = System.Drawing.Color;
using OKTWPrediction = SebbyLib.Prediction.Prediction;
using FS = System.Drawing.FontStyle;
using SharpDX.Direct3D9;

namespace Jhin_As_The_Virtuoso {

	class Jhin {
		public static Menu Config {get;set;}
		public static Obj_AI_Hero Player => HeroManager.Player;
		public static Orbwalking.Orbwalker Orbwalker { get; private set; }
		public static Spell Q { get; set; }
		public static Spell W { get; set; }
		public static Spell E { get; set; }
		public static int lastwarded { get; set; }
		public static Spell R { get; set; }
		public static bool IsCastingR => R.Instance.Name == "JhinRShot";
		public static Vector3 REndPos { get; private set; }
		public static Dictionary<int, float> PingList { get; set; } = new Dictionary<int, float>();
		public static List<Obj_AI_Hero> KillableList { get; set; } = new List<Obj_AI_Hero>();
		public static int[] delay => new[] {
				Config.Item("第一次延迟").GetValue<Slider>().Value,
				Config.Item("第二次延迟").GetValue<Slider>().Value,
				Config.Item("第三次延迟").GetValue<Slider>().Value
		};

		public static Items.Item BlueTrinket = new Items.Item(3342, 3500f);
		public static Items.Item ScryingOrb = new Items.Item(3363, 3500f);

		public static Font KillTextFont = new Font(Drawing.Direct3DDevice,new FontDescription {
			 Height = 28,
			 FaceName = "Microsoft YaHei",
		});
		


		internal static void OnLoad(EventArgs args) {
			if (Player.ChampionName!="Jhin")
			{
				return;
			}

			//初始化ping时间
			foreach (var enemy in HeroManager.Enemies)
			{
				PingList.Add(enemy.NetworkId, 0);
			}
			
			LoadSpell();
			LoadMenu();
			LoadEvents();

			LastPosition.Load();
			//DrawHelper.Load();

			DamageIndicator.DamageToUnit = GetRDmg;
		}

		private static void LoadEvents() {
			Game.OnUpdate += Game_OnUpdate;
			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnWndProc += Game_OnWndProc;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
			//Obj_AI_Base.OnDoCast += Obj_AI_Base_OnDoCast;
			AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
			Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
			Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
			//Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
			Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
			Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
			CustomEvents.Unit.OnDash += Unit_OnDash;
			Game.OnChat += Game_OnChat;
			
		}

		private static void Game_OnChat(GameChatEventArgs args) {

			//if (Config.Item("击杀信号提示").GetValue<bool>() && args.Message.Contains(Player.Name) && args.Message.ToGBK().Contains("要求队友"))
			//{
			//	args.Process = false;
			//}
		}

		private static void Orbwalking_OnNonKillableMinion(AttackableUnit minion) {
			if (Q.IsReady() && Config.Item("补刀Q").GetValue<bool>())
			{
				Q.Cast(minion as Obj_AI_Base);
			}
		}

		private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args) {
			if (sender.IsEnemy)
			{
				if (Config.Item("位移E").GetValue<bool>() && E.IsReady() && args.EndPos.Distance(Player)<E.Range && NavMesh.IsWallOfGrass(args.EndPos.To3D(), 10))
				{
					E.Cast(args.EndPos);
				}

				if (Config.Item("位移W").GetValue<bool>() && W.IsReady() && (sender as Obj_AI_Hero).HasWBuff() && args.EndPos.Distance(Player) < W.Range && (!E.IsReady() || args.EndPos.Distance(Player) > E.Range ))
				{
					W.Cast(args.EndPos);
				}
			}
		}

		private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args) {
			if (sender.IsMe 
				&& IsCastingR && Config.Item("禁止移动").GetValue<bool>()
				&& Player.CountEnemiesInRange(Config.Item("禁止距离").GetValue<Slider>().Value) == 0
				&& HeroManager.Enemies.Any(e => e.InRCone() && !e.IsDead && e.IsValid && e.IsVisible)
			)
			{
				args.Process = false;
			}
		}

		private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
			if (sender.IsMe)
			{
				if (args.SData.Name == "JhinRShotMis")
				{
					
					RCharge.Index++;
					RCharge.CastT = Game.ClockTime;
				}
				if (args.SData.Name == "JhinRShotMis4")
				{
					
					RCharge.Index = 0;
					RCharge.CastT = Game.ClockTime;
					RCharge.Target = null;
				}
			}
		}

		private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args) {
			#region Q消耗
			//if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			//{
			//	var t = args.Target;
			//	if (t?.Type == GameObjectType.obj_AI_Minion)
			//	{
			//		var enemy = t as Obj_AI_Base;
			//		if (Q.CanCast(enemy) && enemy.CountEnemiesInRange(200) > 0)
			//		{
			//			args.Process = false;
			//			Q.Cast(enemy);
			//		}
			//	}
			//}
			#endregion

			////Q清兵
			//if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Config.Item("清兵Q").GetValue<bool>())
			//{
			//	var target = Orbwalker.GetTarget() as Obj_AI_Base;
			//	if (target !=null && Q.CanCast(target) && Q.GetDmg(target)>target.Health && MinionManager.GetMinions(target.Position,200)?.Count>=2)
			//	{
			//		args.Process = false;
			//		Q.Cast(target);
			//	}
			//}
		}

		private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target) {
			
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
				&& !target.IsDead && target.IsValidTarget(Q.Range) && Q.IsReady())
			{
				Q.Cast(target as Obj_AI_Base);
			}

			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed 
				&& target?.Type == GameObjectType.obj_AI_Hero
				&& !target.IsDead && target.IsValidTarget(Q.Range) && Q.IsReady()
				&& Config.Item("消耗Q").GetValue<bool>())
			{
				Q.Cast(target as Obj_AI_Base);
			}
		}

		private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args) {
			
			if (Config.Item("打断E").GetValue<bool>() && sender.IsEnemy && E.CanCast(sender))
			{
				if (sender.ChampionName == "Thresh")
				{
					return;
				}
				E.Cast(sender);
			}
		}

		private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args) {
			if (!sender.IsMe) return;
			
			if (Config.Item("自动加点").GetValue<bool>() && Player.Level >= Config.Item("加点等级").GetValue<Slider>().Value)
			{
				int Delay = Config.Item("加点延迟").GetValue<Slider>().Value;

				if (Player.Level == 6 || Player.Level == 11 || Player.Level == 16)
				{
					Player.Spellbook.LevelSpell(SpellSlot.R);
				}

				if (Q.Level == 0)
				{
					Player.Spellbook.LevelSpell(SpellSlot.Q);
				}
				else if (W.Level == 0)
				{
					Player.Spellbook.LevelSpell(SpellSlot.W);
				}
				else if (E.Level == 0)
				{
					Player.Spellbook.LevelSpell(SpellSlot.E);
				}

				if (Config.Item("加点方案").GetValue<StringList>().SelectedIndex == 0)//主Q副W
				{
					DelayLevels(Delay, SpellSlot.Q);
					DelayLevels(Delay + 50, SpellSlot.W);
					DelayLevels(Delay + 100, SpellSlot.E);
				}
				else if (Config.Item("加点方案").GetValue<StringList>().SelectedIndex == 1)//主Q副E
				{
					DelayLevels(Delay, SpellSlot.Q);
					DelayLevels(Delay + 50, SpellSlot.E);
					DelayLevels(Delay + 100, SpellSlot.W);
				}
				else if (Config.Item("加点方案").GetValue<StringList>().SelectedIndex == 2)//主W副Q
				{
					DelayLevels(Delay, SpellSlot.W);
					DelayLevels(Delay + 50, SpellSlot.Q);
					DelayLevels(Delay + 100, SpellSlot.E);
				}
				else if (Config.Item("加点方案").GetValue<StringList>().SelectedIndex == 3)//主W副E
				{
					DelayLevels(Delay, SpellSlot.W);
					DelayLevels(Delay + 50, SpellSlot.E);
					DelayLevels(Delay + 100, SpellSlot.Q);
				}
				else if (Config.Item("加点方案").GetValue<StringList>().SelectedIndex == 4)//主E副Q
				{
					DelayLevels(Delay, SpellSlot.E);
					DelayLevels(Delay + 50, SpellSlot.Q);
					DelayLevels(Delay + 100, SpellSlot.W);
				}
				else if (Config.Item("加点方案").GetValue<StringList>().SelectedIndex == 5)//主E副W
				{
					DelayLevels(Delay, SpellSlot.E);
					DelayLevels(Delay + 50, SpellSlot.W);
					DelayLevels(Delay + 100, SpellSlot.Q);
				}
			}

			if (!Config.Item("自动加点").GetValue<bool>() && Config.Item("自动点大").GetValue<bool>() 
				&& (Player.Level == 6|| Player.Level == 11|| Player.Level == 16))
			{
				Player.Spellbook.LevelSpell(SpellSlot.R);
			}
		}

		public static void DelayLevels(int time, SpellSlot QWER) {
			Utility.DelayAction.Add(time, () => { Player.Spellbook.LevelSpell(QWER); });
		}

		private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) {
			if (Config.Item("防突E").GetValue<bool>())
			{
				E.Cast(gapcloser.End);
			}
			if (Config.Item("防突W").GetValue<bool>() && gapcloser.Sender.HasWBuff())
			{
				W.CastSpell(gapcloser.Sender);
			}
		}

		private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
			if (sender.IsMe)
			{
				if (args.SData.Name == "JhinRShotMis")
				{

					RCharge.Index++;
					RCharge.CastT = Game.ClockTime;
				}
				if (args.SData.Name == "JhinRShotMis4")
				{

					RCharge.Index = 0;
					RCharge.CastT = Game.ClockTime;
					RCharge.Target = null;
				}
			}

			if (sender.IsMe && args.SData.Name == "JhinR")
			{
				REndPos = args.End;
				if (Config.Item("R放眼").GetValue<bool>()
					&& ( ScryingOrb.IsReady())
					&& HeroManager.Enemies.All(e => !e.InRCone() || !e.IsValid || e.IsDead))
				{
					var bushList = VectorHelper.GetBushInRCone();
					var lpl = VectorHelper.GetLastPositionInRCone();
					if (bushList?.Count > 0)
					{
						if (lpl?.Count > 0)
						{
							var lp = lpl.First(p => Game.Time - p.LastSeen > 2 * 1000);
							if (lp!=null)
							{
								var bush = VectorHelper.GetBushNearPosotion(lp.LastPosition, bushList);
								ScryingOrb.Cast(bush);
							}
							
						}
						else
						{
							var bush = VectorHelper.GetBushNearPosotion(REndPos, bushList);
							ScryingOrb.Cast(bush);
						}
						
					}
					else if (lpl?.Count > 0)
					{
						ScryingOrb.Cast(lpl.First().LastPosition);
					}
				}
			}
			
		}

		private static void Game_OnWndProc(WndEventArgs args) {
			if (args.WParam == Config.Item("半手动R自动").GetValue<KeyBind>().Key && IsCastingR)
			{
				args.Process = false;
			}

			if (!MenuGUI.IsChatOpen && args.WParam == Config.Item("半手动R自动").GetValue<KeyBind>().Key && !IsCastingR && R.IsReady() && RCharge.Target == null)
			{
				var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
				if (t != null && t.IsValid && R.CastSpell(t))
				{
					args.Process = false;
					RCharge.Target = t;
				}
				
			}

		}

		private static Obj_AI_Hero GetTargetInR() {
			var ignoredList = new List<Obj_AI_Hero>();
			foreach (var enemy in HeroManager.Enemies.Where(e => !e.IsValid || e.IsDead || !e.InRCone()))
			{
				ignoredList.Add(enemy);
			}
			var target = TargetSelector.GetTarget(R.Range,TargetSelector.DamageType.Physical,true, ignoredList);
			if (target!=null && target.IsValid && !target.IsDead)
			{
				return target;
			}
			return null;
		}

		private static void Game_OnUpdate(EventArgs args) {

			#region 击杀列表 及 击杀信号提示
			foreach (var enemy in HeroManager.Enemies)
			{
				if (R.CanCast(enemy) && !enemy.IsDead && enemy.IsValid && GetRDmg(enemy) >= enemy.Health)
				{
					if (!KillableList.Contains(enemy))
					{
						KillableList.Add(enemy);
					}

					if (Config.Item("击杀信号提示").GetValue<bool>() && Game.Time - PingList[enemy.NetworkId]> 10 * 1000)
					{
						Game.ShowPing(PingCategory.AssistMe, enemy, true);
						Game.ShowPing(PingCategory.AssistMe, enemy, true);
						PingList[enemy.NetworkId] = Game.ClockTime;

						//DeBug.Debug("击杀信号",$"信号目标 {enemy.ChampionName.ToCN()} 信号时间 {Game.ClockTime}", CNLib.DebugLevel.Info,CNLib.Output.Console,Config.Item("调试"));
					}
				}
				else
				{
					if (KillableList.Contains(enemy))
					{
						KillableList.Remove(enemy);
					}
				}
			}
			#endregion

			#region 其它设置，买蓝眼

			if (Config.Item("买蓝眼").GetValue<bool>() && !ScryingOrb.IsOwned() && (Player.InShop()||Player.InFountain()) && Player.Level >= 9)
			{
				Player.BuyItem(ItemId.Farsight_Orb_Trinket);
			}
			#endregion

			#region 提前结束R时 重置大招次数及目标
			if (!IsCastingR && !R.IsReady())
			{
				RCharge.Index = 0;
				RCharge.Target = null;
			}
			#endregion
			
			if (!IsCastingR)
			{
				QLogic();
				WLogic();
				ELogic();
			}
			RLogic();
		}

		private static void ELogic() {
			#region E逻辑
			foreach (var enemy in HeroManager.Enemies)
			{
				#region 硬控E
				if (enemy.IsValidTarget(E.Range + 30) && Config.Item("硬控E").GetValue<bool>() && !OktwCommon.CanMove(enemy))
				{
					E.CastSpell(enemy);
				}
				#endregion

				#region 探草E
				if (enemy.IsDead) continue;
				var path = enemy.GetWaypoints().LastOrDefault().To3D();
				if (!NavMesh.IsWallOfGrass(path, 1)) continue;
				if (enemy.Distance(path) > 200) continue;
				if (NavMesh.IsWallOfGrass(HeroManager.Player.Position, 1) && HeroManager.Player.Distance(path) < 200) continue;

				if (Environment.TickCount - lastwarded > 1000)
				{
					if (E.IsReady() && HeroManager.Player.Distance(path)<E.Range)
					{
						E.Cast(path);
						lastwarded = Environment.TickCount;
					}
				}
				#endregion
			}
			#endregion
			//清兵
			if (Config.Item("清兵E").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
			{
				var minions = MinionManager.GetMinions(E.Range + E.Width,MinionTypes.All,MinionTeam.Enemy,MinionOrderTypes.MaxHealth);
				if (minions?.Count>5)
				{
					var eClear = E.GetCircularFarmLocation(minions, E.Width);
					if (eClear.MinionsHit >= 3)
					{
						E.Cast(eClear.Position);
					}
				}
				
			}
		}

		private static void WLogic() {
			#region W逻辑
			foreach (var enemy in HeroManager.Enemies.Where(e => e.IsValid && !e.IsDead && e.Distance(Player) < W.Range).OrderByDescending(k => k.Distance(Player)).OrderByDescending(k => k.Health))
			{
				if (Config.Item("标记W").GetValue<bool>()
					&& (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
					&& enemy.CountAlliesInRange(650) > 0
					&& enemy.HasWBuff())
				{
					W.CastSpell(enemy);
				}

				if (Config.Item("硬控W").GetValue<bool>() && !OktwCommon.CanMove(enemy) && enemy.HasWBuff())
				{
					W.CastSpell(enemy);
				}

				if (Config.Item("抢人头W").GetValue<bool>() && Player.CountEnemiesInRange(Player.AttackRange+100) == 0 && enemy.Health < OktwCommon.GetIncomingDamage(enemy,W.Delay) + OktwCommon.GetKsDamage(enemy, W)
					&& !Q.CanCast(enemy) && !(Orbwalking.CanAttack() && Orbwalking.InAutoAttackRange(enemy)))
				{
					W.CastSpell(enemy);
				}

			}
			#endregion
		}

		private static void QLogic() {
			#region Q逻辑
			//Q消耗
			if (Config.Item("消耗Q兵").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			{
				var ms = MinionManager.GetMinions(Q.Range);
				if (ms!=null && ms.Count>0)
				{
					var t = ms.Find(m => Q.GetDmg(m) > m.Health && m.CountEnemiesInRange(200) > 0);
					if (t != null)
					{
						Q.Cast(t);
					}
				}
			}

			if (Config.Item("清兵Q").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
			{
				var ms = MinionManager.GetMinions(Q.Range);
				if (ms != null && ms.Count>=3)
				{
					var t = ms.Find(m => Q.GetDmg(m) > m.Health);
					if (t != null)
					{
						Q.Cast(t);
					}
				}
			}

			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
			{
				var target = Orbwalker.GetTarget() as Obj_AI_Hero;
				if (target != null && target.IsValid && !Orbwalking.CanAttack() && !Player.IsWindingUp && target.Health < Q.GetDmg(target) + W.GetDmg(target))
				{
					Q.Cast(target);
				}
			}

			//Q抢人头
			foreach (var enemy in HeroManager.Enemies.Where(e => e.IsValidTarget(Q.Range) && Q.GetDmg(e) + OktwCommon.GetIncomingDamage(e,Q.Delay) > e.Health))
			{
				Q.Cast(enemy);
			}

			#endregion
		}

		private	static void RLogic() {
			#region 自动R逻辑

			/**
			if (Config.Item("半手动R自动").GetValue<KeyBind>().Active && R.IsReady() && RCharge.Target == null)
			{
				var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
				if (t != null && t.IsValid && R.CastSpell(t))
				{
					RCharge.Target = t;
				}
			}
			*/

			if (IsCastingR)
			{
				if (Config.Item("R放眼").GetValue<bool>() && ScryingOrb.IsReady())
				{
					var pistionList = VectorHelper.GetLastPositionInRCone().Where(m => !m.Hero.IsVisible && !m.Hero.IsDead && Game.ClockTime - m.LastSeen < 7 * 1000).OrderByDescending(m => m.LastSeen);

					if (RCharge.Target == null && pistionList.Count() > 0)
					{
						var MissPosition = pistionList.First();
						var MostNearBush = VectorHelper.GetBushNearPosotion(MissPosition.LastPosition);
						if (MostNearBush != Vector3.Zero && MostNearBush.Distance(MissPosition.LastPosition) < 500)
						{
							ScryingOrb.Cast(MostNearBush);
						}
						else
						{
							ScryingOrb.Cast(MissPosition.LastPosition);
						}
					}
					else if (RCharge.Target != null && !RCharge.Target.IsVisible && !RCharge.Target.IsDead)
					{
						var RTargetLastPosition = pistionList?.Find(m => m.Hero == RCharge.Target && Game.ClockTime - m.LastSeen < 3 * 1000);
						if (RTargetLastPosition != null)
						{
							var MostNearBush = VectorHelper.GetBushNearPosotion(RTargetLastPosition.LastPosition);
							if (MostNearBush.Distance(RTargetLastPosition.LastPosition) < 500)
							{
								ScryingOrb.Cast(MostNearBush);
							}
							else
							{
								ScryingOrb.Cast(RTargetLastPosition.LastPosition);
							}
						}
					}
				}
				
				var target = GetTargetInR();
				if (target != null)
				{
					#region 使用R，并记录R目标和施放时间
					if (RCharge.Index == 0)
					{
						if (R.CastSpell(target))
						{
							RCharge.Target = target;
						}
					}
					else
					{
						Utility.DelayAction.Add(delay[RCharge.Index - 1], () =>
						{
							if (R.CastSpell(target))
							{
								RCharge.Target = target;
							}
						});
					}
					#endregion
				}
			}

			#region 旧方法，屏
			//		if (IsCastingR)
			//		{
			//			if (Config.Item("R放眼").GetValue<bool>()
			//				&& ScryingOrb.IsReady()
			//				&& RCharge.Target != null && !RCharge.Target.IsDead && !RCharge.Target.IsVisible)
			//			{
			//				var bushList = VectorHelper.GetBushInRCone();
			//				var pistionList = VectorHelper.GetLastPositionInRCone().OrderBy(m => m.LastSeen);

			//				var RTargetLastPosition = pistionList?.Find(m => m.Hero == RCharge.Target && Game.ClockTime - m.LastSeen < 3 * 1000);
			//				//如果R中有R目标的最后位置
			//				if (RTargetLastPosition!= null)
			//				{
			//					var MostNearBush = VectorHelper.GetBushNearPosotion(RTargetLastPosition.LastPosition, bushList);
			//					if (MostNearBush.Distance(RTargetLastPosition.LastPosition) < 500)
			//					{
			//						ScryingOrb.Cast(MostNearBush);
			//					}
			//					else
			//					{
			//						ScryingOrb.Cast(RTargetLastPosition.LastPosition);
			//					}
			//				}

			//			}
			//			/**
			//			//var lp = VectorHelper.GetLastPositionInRCone().Find(l => l.Hero == RCharge.Target);
			//			//if (bushList?.Count > 0)
			//			//{
			//			//	if (lp != null)
			//			//	{
			//			//		var bush = VectorHelper.GetBushNearPosotion(lp.LastPosition, bushList);
			//			//		ScryingOrb.Cast(bush);
			//			//	}
			//			//	else
			//			//	{
			//			//		var bush = VectorHelper.GetBushNearPosotion(REndPos, bushList);
			//			//		ScryingOrb.Cast(bush);
			//			//	}

			//			//}
			//			//else if (lp != null)
			//			//{
			//			//	ScryingOrb.Cast(lp.LastPosition);
			//			//}
			//			*/

			//			/**
			//			if (Config.Item("R放眼").GetValue<bool>()
			//				&& (BlueTrinket.IsReady() || ScryingOrb.IsReady())
			//				&& RCharge.Target != null && !RCharge.Target.IsDead && !RCharge.Target.IsVisible)
			//			{
			//				Game.PrintChat("进草眼".ToUTF8());
			//				Game.PrintChat("目标位置".ToUTF8() + RCharge.Target.Position);
			//				var bushList = VectorHelper.GetBushInRCone();
			//				if (bushList?.Count > 0 )
			//				{
			//					var bush = VectorHelper.GetBushNearPosotion(RCharge.Target.Position, bushList);
			//					if (bush == Vector3.Zero)
			//					{
			//						BlueTrinket.Cast(RCharge.Target.Position);
			//						ScryingOrb.Cast(RCharge.Target.Position);
			//					}
			//					else
			//					{
			//						BlueTrinket.Cast(bush);
			//						ScryingOrb.Cast(bush);
			//					}

			//				}
			//			}
			//*/
			//			var target = GetTargetInR();
			//			if (target != null)
			//			{
			//				if (RCharge.Index == 0)
			//				{
			//					if (R.CastSpell(target))
			//					{
			//						RCharge.Target = target;
			//					}
			//				}
			//				else
			//				{
			//					Utility.DelayAction.Add(delay[RCharge.Index - 1], () =>
			//					{
			//						if (R.CastSpell(target))
			//						{
			//							RCharge.Target = target;
			//						}
			//					});
			//				}
			//			}
			//			else
			//			{
			//				if (Config.Item("R放眼").GetValue<bool>()
			//					&& (BlueTrinket.IsReady() || ScryingOrb.IsReady())
			//					&& RCharge.Target != null && !RCharge.Target.IsDead)
			//				{
			//					var bushList = VectorHelper.GetBushInRCone();
			//					var lp = VectorHelper.GetLastPositionInRCone().Find(l => l.Hero == RCharge.Target);
			//					if (bushList?.Count > 0)
			//					{
			//						if (lp != null)
			//						{
			//							var bush = VectorHelper.GetBushNearPosotion(lp.LastPosition, bushList);
			//							ScryingOrb.Cast(bush);
			//						}
			//						else
			//						{
			//							var bush = VectorHelper.GetBushNearPosotion(REndPos, bushList);
			//							ScryingOrb.Cast(bush);
			//						}

			//					}
			//					else if (lp != null)
			//					{
			//						ScryingOrb.Cast(lp.LastPosition);
			//					}
			//				}
			//			}
			//		}
			#endregion
			#endregion
		}

		private static float GetRDmg(Obj_AI_Base target) {
			return (IsCastingR || R.IsReady()) ? (5 - RCharge.Index) * (float)R.GetDmg(target) : 0 ;
		}

		private static void Drawing_OnDraw(EventArgs args) {
			#region 范围显示
			var ShowW = Config.Item("W范围").GetValue<Circle>();
			var ShowE = Config.Item("E范围").GetValue<Circle>();
			var ShowR = Config.Item("R范围").GetValue<Circle>();
			var ShowWM = Config.Item("小地图W范围").GetValue<bool>();
			var ShowRM = Config.Item("小地图R范围").GetValue<bool>();

			if (W.IsReady() && ShowW.Active)
			{
				Render.Circle.DrawCircle(Player.ServerPosition, W.Range, ShowW.Color, 2);
			}
			if (W.IsReady() && ShowWM)
			{
				Utility.DrawCircle(Player.ServerPosition, W.Range, ShowW.Color, 2, 30, true);
			}

			if (R.IsReady() && ShowR.Active)
			{
				Render.Circle.DrawCircle(Player.ServerPosition, R.Range, ShowR.Color, 2);
			}
			if (R.IsReady() && ShowRM)
			{
				Utility.DrawCircle(Player.ServerPosition, R.Range, ShowR.Color, 2, 30, true);
			}

			if (E.IsReady() && ShowE.Active)
			{
				Render.Circle.DrawCircle(Player.ServerPosition, E.Range, ShowE.Color, 2);
			}
			#endregion

			var ShowD = Config.Item("大招伤害").GetValue<Circle>();
			DamageIndicator.Enabled = ShowD.Active;
			DamageIndicator.Color = ShowD.Color;

			var ShowT = Config.Item("击杀文本提示").GetValue<Circle>();
			if (ShowT.Active && KillableList?.Count > 0)
			{
				var killname = "R击杀名单\n";
				foreach (var k in KillableList)
				{
					killname += (k.Name + "　").ToGBK() + $"({k.ChampionName})\n";
                }

				var KillTextColor = new ColorBGRA
				{
					A = Config.Item("击杀文本提示").GetValue<Circle>().Color.A,
					B = Config.Item("击杀文本提示").GetValue<Circle>().Color.B,
					G = Config.Item("击杀文本提示").GetValue<Circle>().Color.G,
					R = Config.Item("击杀文本提示").GetValue<Circle>().Color.R,
				};

				KillTextFont.DrawText(null,killname,
					(int)(Drawing.Width * ((float)Config.Item("击杀文本X").GetValue<Slider>().Value / 100)),
					(int)(Drawing.Height * ((float)Config.Item("击杀文本Y").GetValue<Slider>().Value / 100)),
					KillTextColor);
			}

			//var ShowK = Config.Item("击杀目标标识").GetValue<Circle>();
			//if (ShowK.Active)
			//{
			//	foreach (var enemy in KillableList)
			//	{
			//		Render.Circle.DrawCircle(enemy.Position, 40, ShowK.Color, 200, true);
			//		DrawHelper.DrawFillCircle(enemy, ShowK.Color);
			//	}
			//}
		}

		private static void LoadSpell() {
			Q = new Spell(SpellSlot.Q, 600);
			W = new Spell(SpellSlot.W, 2500);
			E = new Spell(SpellSlot.E, 760);
			R = new Spell(SpellSlot.R, 3500);

			W.SetSkillshot(0.75f, 40, float.MaxValue, false, SkillshotType.SkillshotLine);
			E.SetSkillshot(1.3f, 200, 1600, false, SkillshotType.SkillshotCircle);
			R.SetSkillshot(0.2f, 80, 5000, false, SkillshotType.SkillshotLine);
		}

		private static void LoadMenu() {

			Game.PrintChat("戏命师—烬　".ToHtml(25)+"此刻,大美将致!".ToHtml(Color.PowderBlue,FontStlye.Cite));

			Config = new Menu("戏命师 - 烬", "JhinAsTheVirtuoso", true);
			Config.AddToMainMenu();

			//Config.AddItem(new MenuItem("调试", "调试").SetValue(false));

			var OMenu = Config.AddSubMenu(new Menu("走砍设置", "走砍设置"));
			Orbwalker = new Orbwalking.Orbwalker(OMenu);

			//Q菜单
			var QMenu = Config.AddSubMenu(new Menu("Q设置","Q设置"));
			QMenu.AddItem(new MenuItem("消耗Q兵", "可Q死小兵时消耗").SetValue(true));
			QMenu.AddItem(new MenuItem("消耗Q","一直用Q消耗").SetValue(true));
			QMenu.AddItem(new MenuItem("清兵Q", "使用Q清兵").SetValue(true));
			QMenu.AddItem(new MenuItem("补刀Q", "使用Q补刀").SetValue(false));
			QMenu.AddItem(new MenuItem("抢人头Q", "Q抢人头").SetValue(true));

			//W菜单
			var WMenu = Config.AddSubMenu(new Menu("W设置", "W设置"));
			WMenu.AddItem(new MenuItem("硬控W", "自动W硬控敌人").SetValue(true));
			WMenu.AddItem(new MenuItem("标记W","W有标记的敌人").SetValue(true));
			WMenu.AddItem(new MenuItem("抢人头W","W抢人头").SetValue(true));
			WMenu.AddItem(new MenuItem("防突W", "W有标记的突进").SetValue(true));
			WMenu.AddItem(new MenuItem("位移W", "敌人位移W").SetValue(true));

			//E菜单
			var EMenu = Config.AddSubMenu(new Menu("E设置", "E设置"));
			EMenu.AddItem(new MenuItem("连招E", "连招使用E").SetValue(false));
			EMenu.AddItem(new MenuItem("硬控E", "自动E硬控敌人").SetValue(true));
			EMenu.AddItem(new MenuItem("防突E", "自动E防突进").SetValue(true));
			EMenu.AddItem(new MenuItem("打断E", "自动E持续技能敌人").SetValue(true));
			EMenu.AddItem(new MenuItem("探草E", "敌人进草自动E").SetValue(true));
			EMenu.AddItem(new MenuItem("位移E", "敌人位移到看不到的地方E").SetValue(true));
			EMenu.AddItem(new MenuItem("清兵E", "使用E清兵").SetValue(true));

			//R菜单
			var RMenu = Config.AddSubMenu(new Menu("R设置", "R设置"));
			RMenu.AddItem(new MenuItem("S12", "移动设置")).SetFontStyle(FS.Bold, DXColor.Orange);
			RMenu.AddItem(new MenuItem("禁止移动", "R时禁止移动和攻击").SetValue(true));
			RMenu.AddItem(new MenuItem("禁止距离", "当?码敌人靠近解除禁止").SetValue(new Slider(700,0,2000)));
			RMenu.AddItem(new MenuItem("S13", ""));

			RMenu.AddItem(new MenuItem("S1", "击杀提示设置")).SetFontStyle(FS.Bold, DXColor.Orange);
			RMenu.AddItem(new MenuItem("击杀文本提示", "文字提示R可击杀目标").SetValue(new Circle(true, Color.Orange)));
			RMenu.AddItem(new MenuItem("击杀文本X", "文字提示横向位置").SetValue(new Slider(71)));
			RMenu.AddItem(new MenuItem("击杀文本Y", "文字提示纵向位置").SetValue(new Slider(86)));
			RMenu.AddItem(new MenuItem("击杀信号提示", "信号提示R可击杀目标(本地)").SetValue(true));
			RMenu.AddItem(new MenuItem("击杀目标标识", "圆圈标记R可击杀目标").SetValue(new Circle(true, Color.Red)));
			RMenu.AddItem(new MenuItem("S2", ""));

			RMenu.AddItem(new MenuItem("S3", "半手动R设置(自动R)")).SetFontStyle(FS.Bold, DXColor.Orange);
			RMenu.AddItem(new MenuItem("半手动R自动", "半手动R(自动R)").SetValue(new KeyBind('R',KeyBindType.Press)));
			RMenu.AddItem(new MenuItem("第一次延迟", "第一次R后延迟(毫秒)").SetValue(new Slider(0, 0, 1000)));
			RMenu.AddItem(new MenuItem("第二次延迟", "第二次R后延迟(毫秒)").SetValue(new Slider(0, 0, 1000)));
			RMenu.AddItem(new MenuItem("第三次延迟", "第三次R后延迟(毫秒)").SetValue(new Slider(0, 0, 1000)));
			RMenu.AddItem(new MenuItem("S4", ""));

			//RMenu.AddItem(new MenuItem("S5", "半手动R设置(点射)")).SetFontStyle(FS.Bold, DXColor.Orange);
			//RMenu.AddItem(new MenuItem("半手动R点射", "半手动R(点射)").SetValue(new KeyBind('T', KeyBindType.Press)));
			//RMenu.AddItem(new MenuItem("S6", ""));

			RMenu.AddItem(new MenuItem("R放眼","R时无视野放蓝眼").SetValue(true));

			//其它菜单
			var MMenu = Config.AddSubMenu(new Menu("其它设置", "其它设置"));
			MMenu.AddItem(new MenuItem("S10", "自动加点设置")).SetFontStyle(FS.Bold, DXColor.Orange);
			MMenu.AddItem(new MenuItem("自动点大", "只自动学大").SetValue(true));
			MMenu.AddItem(new MenuItem("自动加点", "自动加点").SetValue(false));
			MMenu.AddItem(new MenuItem("加点等级", "从几级开始加点").SetValue(new Slider(2,1,6)));
			MMenu.AddItem(new MenuItem("加点延迟", "加点延迟").SetValue(new Slider(700, 0, 2000)));
			MMenu.AddItem(new MenuItem("加点方案", "加点方案").SetValue(
				new StringList(new[] {"主Q副W","主Q副E","主W副Q","主W副E","主E副Q","主E副W"})));

			MMenu.AddItem(new MenuItem("S11", ""));
			MMenu.AddItem(new MenuItem("买蓝眼","9级时自动买蓝眼").SetValue(true));

			//显示菜单
			var DMenu = Config.AddSubMenu(new Menu("显示设置", "显示设置"));
			DMenu.AddItem(new MenuItem("S7", "范围显示")).SetFontStyle(FS.Bold,DXColor.Orange);
			DMenu.AddItem(new MenuItem("W范围", "显示W范围").SetValue(new Circle(true, Color.Blue, E.Range)));
			DMenu.AddItem(new MenuItem("小地图W范围", "小地图显示W范围").SetValue(false));
			DMenu.AddItem(new MenuItem("E范围", "显示E范围").SetValue(new Circle(true, Color.Yellow, E.Range)));
			DMenu.AddItem(new MenuItem("R范围", "显示R范围").SetValue(new Circle(true, Color.YellowGreen, R.Range)));
			DMenu.AddItem(new MenuItem("小地图R范围", "小地图显示R范围").SetValue(true));
			DMenu.AddItem(new MenuItem("S8", ""));

			DMenu.AddItem(new MenuItem("S9", "伤害提示")).SetFontStyle(FS.Bold, DXColor.Orange);
			DMenu.AddItem(new MenuItem("大招伤害", "显示四次大招后伤害").SetValue(new Circle(true, Color.Red)));
			//DMenu.AddItem(new MenuItem("连招伤害", "显示连招伤害").SetValue(new Circle(true, Color.Green)));
		}
	}
}
