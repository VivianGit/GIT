﻿namespace Valvrave_Sharp.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Events;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Utils;
    using LeagueSharp.SDK.Core.Wrappers;

    using SharpDX;

    using Valvrave_Sharp.Core;
    using Valvrave_Sharp.Evade;

    using Color = System.Drawing.Color;
    using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

    internal class Yasuo : Program
    {
        #region Constants

        private const int QCirWidth = 250, RWidth = 400;

        #endregion

        #region Constructors and Destructors

        public Yasuo()
        {
            Q = new Spell(SpellSlot.Q, 510);
            Q2 = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 1300);
            Q.SetSkillshot(GetQ12Delay, 20, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(GetQ3Delay, 90, 1500, false, SkillshotType.SkillshotLine);
            E.SetTargetted(0, GetESpeed);
            Q.DamageType = Q2.DamageType = R.DamageType = DamageType.Physical;
            E.DamageType = DamageType.Magical;
            Q.MinHitChance = Q2.MinHitChance = HitChance.VeryHigh;

            var orbwalkMenu = new Menu("Orbwalk", "走砍设置");
            {
                Config.Separator(orbwalkMenu, "blank0", "Q/点燃/物品: 总是使用");
                Config.Separator(orbwalkMenu, "blank1", "E突进设置");
                Config.Bool(orbwalkMenu, "EGap", "使用 E");
                Config.Slider(orbwalkMenu, "ERange", "如果距离 >", 300, 0, (int)E.Range);
                Config.Bool(orbwalkMenu, "ETower", "E进塔");
                Config.Bool(orbwalkMenu, "EStackQ", "突进时攒Q", false);
                Config.Separator(orbwalkMenu, "blank2", "R 设置");
                Config.Bool(orbwalkMenu, "R", "使用 R");
                Config.Bool(orbwalkMenu, "RDelay", "敌人将落地时R");
                Config.Slider(orbwalkMenu, "RHpU", "当敌人血量 < (%)", 60);
                Config.Slider(orbwalkMenu, "RCountA", "或者当敌人数量 >=", 2, 1, 5);
                MainMenu.Add(orbwalkMenu);
            }
            var hybridMenu = new Menu("Hybrid", "消耗设置");
            {
                Config.Separator(hybridMenu, "blank3", "Q: 总是使用");
                Config.Bool(hybridMenu, "Q3", "可以用 Q3");
                Config.Bool(hybridMenu, "QLastHit", "用(Q1/2)补刀");
                Config.Separator(hybridMenu, "blank4", "自动Q设置");
                Config.KeyBind(hybridMenu, "AutoQ", "开关", Keys.T, KeyBindType.Toggle);
                Config.Bool(hybridMenu, "AutoQ3", "可以用 Q3", false);
                MainMenu.Add(hybridMenu);
            }
            var lcMenu = new Menu("LaneClear", "清线设置");
            {
                Config.Separator(lcMenu, "blank5", "Q: 总是使用");
                Config.Bool(lcMenu, "Q3", "可以用 Q3");
                Config.Separator(lcMenu, "blank6", "E 设置");
                Config.Bool(lcMenu, "E", "使用 E");
                Config.Bool(lcMenu, "ELastHit", "只用来补刀", false);
                Config.Bool(lcMenu, "ETower", "可以E下塔", false);
                MainMenu.Add(lcMenu);
            }
            var farmMenu = new Menu("Farm", "补刀设置");
            {
                Config.Separator(farmMenu, "blank7", "Q 设置");
                Config.Bool(farmMenu, "Q", "使用 Q");
                Config.Bool(farmMenu, "Q3", "可以使用Q3", false);
                Config.Separator(farmMenu, "blank8", "E 设置");
                Config.Bool(farmMenu, "E", "使用 E");
                Config.Bool(farmMenu, "ETower", "可以E下塔", false);
                MainMenu.Add(farmMenu);
            }
            var ksMenu = new Menu("KillSteal", "抢人头设置");
            {
                Config.Bool(ksMenu, "Q", "使用 Q");
                Config.Bool(ksMenu, "E", "使用 E");
                Config.Bool(ksMenu, "R", "使用 R");
                Config.Separator(ksMenu, "blank7", "额外 R 设置");
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    Config.Bool(ksMenu, "RCast" + enemy.ChampionName, "施放向" + enemy.ChampionName, false);
                }
                MainMenu.Add(ksMenu);
            }
            var fleeMenu = new Menu("Flee", "逃跑设置");
            {
                Config.KeyBind(fleeMenu, "E", "使用 E", Keys.S);
                Config.Bool(fleeMenu, "Q", "E突时攒Q");
                MainMenu.Add(fleeMenu);
            }
            if (GameObjects.EnemyHeroes.Any())
            {
                Evade.Init();
            }
            var drawMenu = new Menu("Draw", "显示设置");
            {
                Config.Bool(drawMenu, "Q", "Q 范围",false);
                Config.Bool(drawMenu, "E", "E Range", true);
                Config.Bool(drawMenu, "R", "R Range", false);
                Config.Bool(drawMenu, "StackQ", "显示 自动攒Q 的状态");
                MainMenu.Add(drawMenu);
            }
            Config.KeyBind(MainMenu, "StackQ", "自动攒Q", Keys.Z, KeyBindType.Toggle);

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdateEvade;
        }

        #endregion

        #region Properties

        private static float GetESpeed
        {
            get
            {
                return 700 + Player.MoveSpeed;
            }
        }

        private static float GetQ12Delay
        {
            get
            {
                return 0.4f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.66f));
            }
        }

        private static float GetQ3Delay
        {
            get
            {
                return 0.5f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.66f));
            }
        }

        private static List<Obj_AI_Base> GetQCirObj
        {
            get
            {
                var pos = Player.GetDashInfo().EndPos;
                var obj = new List<Obj_AI_Base>();
                obj.AddRange(GameObjects.EnemyHeroes.Where(i => i.IsValidTarget() && i.Distance(pos) < QCirWidth));
                obj.AddRange(GameObjects.EnemyMinions.Where(i => i.IsValidTarget() && i.Distance(pos) < QCirWidth));
                obj.AddRange(GameObjects.Jungle.Where(i => i.IsValidTarget() && i.Distance(pos) < QCirWidth));
                return obj.Count > 0 && Player.GetDashInfo().EndTick - Variables.TickCount < 100 + Game.Ping * 2
                           ? obj
                           : new List<Obj_AI_Base>();
            }
        }

        private static Obj_AI_Hero GetQCirTarget
        {
            get
            {
                var pos = Player.GetDashInfo().EndPos.ToVector3();
                var target = TargetSelector.GetTarget(QCirWidth, DamageType.Physical, null, pos);
                return target != null && Player.GetDashInfo().EndTick - Variables.TickCount < 100 + Game.Ping * 2
                           ? target
                           : null;
            }
        }

        private static List<Obj_AI_Hero> GetRTarget
        {
            get
            {
                return
                    GameObjects.EnemyHeroes.Where(i => R.IsInRange(i) && CanCastR(i))
                        .OrderByDescending(TargetSelector.GetPriority)
                        .ToList();
            }
        }

        private static bool HaveQ3
        {
            get
            {
                return Player.HasBuff("YasuoQ3W");
            }
        }

        #endregion

        #region Methods

        private static void AutoQ()
        {
            if (!MainMenu["Hybrid"]["AutoQ"].GetValue<MenuKeyBind>().Active || !Q.IsReady() || Player.IsDashing()
                || (HaveQ3 && !MainMenu["Hybrid"]["AutoQ3"]))
            {
                return;
            }
            if (!HaveQ3)
            {
                var target = Q.GetTarget(30);
                if (target != null)
                {
                    Common.Cast(Q, target, true);
                }
            }
            else
            {
                CastQ3();
            }
        }

        private static bool CanCastDelayR(Obj_AI_Hero target)
        {
            var buff = target.Buffs.FirstOrDefault(i => i.Type == BuffType.Knockback || i.Type == BuffType.Knockup);
            return buff != null && buff.EndTime - Game.Time < (buff.EndTime - buff.StartTime) / 3;
        }

        private static bool CanCastE(Obj_AI_Base target)
        {
            return !target.HasBuff("YasuoDashWrapper");
        }

        private static bool CanCastR(Obj_AI_Hero target)
        {
            return target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Knockup);
        }

        private static bool CastQ3(Obj_AI_Hero target = null)
        {
            var spellQ = new Spell(SpellSlot.Q, Q2.Range);
            spellQ.SetSkillshot(Q2.Delay, Q2.Width, Q2.Speed, true, Q2.Type);
            if (target != null)
            {
                var pred = Common.GetPrediction(spellQ, target, true, new[] { CollisionableObjects.YasuoWall });
                if (pred.Hitchance >= Q2.MinHitChance && Q.Cast(pred.CastPosition))
                {
                    return true;
                }
            }
            else
            {
                var hit = -1;
                var predPos = new Vector3();
                foreach (var hero in GameObjects.EnemyHeroes.Where(i => i.IsValidTarget(Q2.Range)))
                {
                    var pred = Common.GetPrediction(spellQ, hero, true, new[] { CollisionableObjects.YasuoWall });
                    if (pred.Hitchance >= Q2.MinHitChance && pred.AoeTargetsHitCount > hit)
                    {
                        hit = pred.AoeTargetsHitCount;
                        predPos = pred.CastPosition;
                    }
                }
                if (predPos.IsValid() && Q.Cast(predPos))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CastQCir(Obj_AI_Base target)
        {
            return target.Distance(Player.GetDashInfo().EndPos) < QCirWidth - target.BoundingRadius
                   && Q.Cast(target.Position);
        }

        private static void Farm()
        {
            if (MainMenu["Farm"]["Q"] && Q.IsReady() && !Player.IsDashing() && (!HaveQ3 || MainMenu["Farm"]["Q3"]))
            {
                var minion =
                    GameObjects.EnemyMinions.Where(
                        i =>
                        i.IsValidTarget((!HaveQ3 ? Q : Q2).Range - 25) && (!HaveQ3 ? Q : Q2).GetHealthPrediction(i) > 0
                        && (!HaveQ3 ? Q : Q2).GetHealthPrediction(i) <= GetQDmg(i)).MaxOrDefault(i => i.MaxHealth);
                if (minion != null && Common.Cast(!HaveQ3 ? Q : Q2, minion, true) == CastStates.SuccessfullyCasted)
                {
                    return;
                }
            }
            if (MainMenu["Farm"]["E"] && E.IsReady())
            {
                var minion =
                    GameObjects.EnemyMinions.Where(
                        i =>
                        i.IsValidTarget(E.Range) && CanCastE(i) && Evader.IsSafePoint(PosAfterE(i).ToVector2()).IsSafe
                        && (!UnderTower(PosAfterE(i)) || MainMenu["Farm"]["ETower"]) && E.GetHealthPrediction(i) > 0
                        && E.GetHealthPrediction(i) <= GetEDmg(i)).MaxOrDefault(i => i.MaxHealth);
                if (minion != null)
                {
                    E.CastOnUnit(minion);
                }
            }
        }

        private static void Flee()
        {
            if (MainMenu["Flee"]["Q"] && Q.IsReady() && !HaveQ3 && Player.IsDashing() && GetQCirObj.Count > 0
                && CastQCir(GetQCirObj.MinOrDefault(i => i.Distance(Player))))
            {
                return;
            }
            if (Player.IsDashing() || !E.IsReady())
            {
                return;
            }
            var obj = GetNearObj();
            if (obj != null)
            {
                E.CastOnUnit(obj);
            }
        }

        private static double GetEDmg(Obj_AI_Base target)
        {
            return Player.CalculateDamage(
                target,
                DamageType.Magical,
                (50 + 20 * E.Level) * (1 + Math.Max(0, Player.GetBuffCount("YasuoDashScalar") * 0.25))
                + 0.6 * Player.FlatMagicDamageMod);
        }

        private static Obj_AI_Base GetNearObj(Obj_AI_Base target = null, bool inQCir = false)
        {
            var pos = target != null
                          ? Prediction.GetPrediction(target, E.Delay, 0, E.Speed).UnitPosition
                          : Game.CursorPos;
            var obj = new List<Obj_AI_Base>();
            obj.AddRange(GameObjects.EnemyHeroes.Where(i => i.IsValidTarget(E.Range) && CanCastE(i)));
            obj.AddRange(GameObjects.EnemyMinions.Where(i => i.IsValidTarget(E.Range) && CanCastE(i)));
            obj.AddRange(GameObjects.Jungle.Where(i => i.IsValidTarget(E.Range) && CanCastE(i)));
            return
                obj.Where(
                    i =>
                    PosAfterE(i).Distance(pos) < (inQCir ? QCirWidth : Player.Distance(pos))
                    && Evader.IsSafePoint(PosAfterE(i).ToVector2()).IsSafe)
                    .MinOrDefault(i => PosAfterE(i).Distance(pos));
        }

        private static double GetQDmg(Obj_AI_Base target)
        {
            var dmgItem = 0d;
            if (Items.HasItem(3057) && (Items.CanUseItem(3057) || Player.HasBuff("Sheen")))
            {
                dmgItem = Player.BaseAttackDamage;
            }
            if (Items.HasItem(3078) && (Items.CanUseItem(3078) || Player.HasBuff("Sheen")))
            {
                dmgItem = Player.BaseAttackDamage * 2;
            }
            var k = 1d;
            var reduction = 0d;
            var dmgBonus = dmgItem
                           + Player.TotalAttackDamage * (Player.Crit >= 0.85f ? (Items.HasItem(3031) ? 1.875 : 1.5) : 1);
            if (Items.HasItem(3153))
            {
                var dmgBotrk = Math.Max(0.08 * target.Health, 10);
                var minion = target as Obj_AI_Minion;
                if (minion != null)
                {
                    dmgBotrk = Math.Min(dmgBotrk, 60);
                }
                dmgBonus += dmgBotrk;
            }
            var hero = target as Obj_AI_Hero;
            if (hero != null)
            {
                if (Items.HasItem(3047, hero))
                {
                    k *= 0.9d;
                }
                if (hero.ChampionName == "Fizz")
                {
                    reduction += hero.Level > 15
                                     ? 14
                                     : (hero.Level > 12
                                            ? 12
                                            : (hero.Level > 9 ? 10 : (hero.Level > 6 ? 8 : (hero.Level > 3 ? 6 : 4))));
                }
                var mastery = hero.Masteries.FirstOrDefault(i => i.Page == MasteryPage.Defense && i.Id == 68);
                if (mastery != null && mastery.Points > 0)
                {
                    reduction += 1 * mastery.Points;
                }
            }
            return Player.CalculateMixedDamage(
                target,
                20 * Q.Level + (dmgBonus - reduction) * k,
                Player.GetBuffCount("ItemStatikShankCharge") == 100
                    ? 100 * (Player.Crit >= 0.85f ? (Items.HasItem(3031) ? 2.25 : 1.8) : 1)
                    : 0);
        }

        private static double GetRDmg(Obj_AI_Hero target)
        {
            return Player.CalculateDamage(
                target,
                DamageType.Physical,
                new[] { 200, 300, 400 }[R.Level - 1] + 1.5f * Player.FlatPhysicalDamageMod);
        }

        private static void Hybrid()
        {
            if (!Q.IsReady() || Player.IsDashing())
            {
                return;
            }
            if (!HaveQ3)
            {
                var target = Q.GetTarget(30);
                if (target != null && Common.Cast(Q, target, true) == CastStates.SuccessfullyCasted)
                {
                    return;
                }
                if (MainMenu["Hybrid"]["QLastHit"] && Q.GetTarget(100) == null)
                {
                    var minion =
                        GameObjects.EnemyMinions.Where(
                            i =>
                            i.IsValidTarget(Q.Range - 25) && Q.GetHealthPrediction(i) > 0
                            && Q.GetHealthPrediction(i) <= GetQDmg(i)).MaxOrDefault(i => i.MaxHealth);
                    if (minion != null)
                    {
                        Common.Cast(Q, minion, true);
                    }
                }
            }
            else if (MainMenu["Hybrid"]["Q3"])
            {
                CastQ3();
            }
        }

        private static void KillSteal()
        {
            if (MainMenu["KillSteal"]["Q"] && Q.IsReady())
            {
                if (Player.IsDashing())
                {
                    var target = GetQCirTarget;
                    if (target != null && target.Health <= GetQDmg(target) && CastQCir(target))
                    {
                        return;
                    }
                }
                else
                {
                    var target = (!HaveQ3 ? Q : Q2).GetTarget(!HaveQ3 ? 30 : 0);
                    if (target != null && target.Health <= GetQDmg(target))
                    {
                        if (!HaveQ3)
                        {
                            if (Common.Cast(Q, target, true) == CastStates.SuccessfullyCasted)
                            {
                                return;
                            }
                        }
                        else if (CastQ3(target))
                        {
                            return;
                        }
                    }
                }
            }
            if (MainMenu["KillSteal"]["E"] && E.IsReady())
            {
                var target = E.GetTarget(0, false, GameObjects.EnemyHeroes.Where(i => !CanCastE(i)));
                if (target != null
                    && (target.Health <= GetEDmg(target)
                        || (MainMenu["KillSteal"]["Q"] && Q.IsReady(30)
                            && target.Health - GetEDmg(target) <= GetQDmg(target))) && E.CastOnUnit(target))
                {
                    return;
                }
            }
            if (MainMenu["KillSteal"]["R"] && R.IsReady() && GetRTarget.Count > 0)
            {
                var target =
                    GetRTarget.FirstOrDefault(
                        i => MainMenu["KillSteal"]["RCast" + i.ChampionName] && i.Health <= GetRDmg(i));
                if (target != null)
                {
                    R.CastOnUnit(target);
                }
            }
        }

        private static void LaneClear()
        {
            if (MainMenu["LaneClear"]["E"] && E.IsReady())
            {
                var minion = new List<Obj_AI_Minion>();
                minion.AddRange(GameObjects.EnemyMinions.Where(i => i.IsValidTarget(E.Range) && CanCastE(i)));
                minion.AddRange(GameObjects.Jungle.Where(i => i.IsValidTarget(E.Range) && CanCastE(i)));
                minion =
                    minion.Where(i => !UnderTower(PosAfterE(i)) || MainMenu["LaneClear"]["ETower"])
                        .OrderByDescending(i => i.MaxHealth)
                        .ToList();
                if (minion.Count > 0)
                {
                    var obj =
                        minion.FirstOrDefault(
                            i => E.GetHealthPrediction(i) > 0 && E.GetHealthPrediction(i) <= GetEDmg(i));
                    if (!MainMenu["LaneClear"]["ELastHit"] && obj == null && Q.IsReady(30)
                        && (!HaveQ3 || MainMenu["LaneClear"]["Q3"]))
                    {
                        var sub = new List<Obj_AI_Minion>();
                        foreach (var mob in minion)
                        {
                            if (((E.GetHealthPrediction(mob) > 0
                                  && E.GetHealthPrediction(mob) <= GetEDmg(mob) + GetQDmg(mob))
                                 || mob.Team == GameObjectTeam.Neutral) && mob.Distance(PosAfterE(mob)) < QCirWidth)
                            {
                                sub.Add(mob);
                            }
                            var nearMinion = new List<Obj_AI_Minion>();
                            nearMinion.AddRange(
                                GameObjects.EnemyMinions.Where(
                                    i => i.IsValidTarget() && i.Distance(PosAfterE(mob)) < QCirWidth));
                            nearMinion.AddRange(
                                GameObjects.Jungle.Where(
                                    i => i.IsValidTarget() && i.Distance(PosAfterE(mob)) < QCirWidth));
                            if (nearMinion.Count > 2
                                || nearMinion.Any(
                                    i => E.GetHealthPrediction(mob) > 0 && E.GetHealthPrediction(mob) <= GetQDmg(mob)))
                            {
                                sub.Add(mob);
                            }
                        }
                        if (sub.Count > 0)
                        {
                            obj = sub.FirstOrDefault();
                        }
                    }
                    if (obj != null && E.CastOnUnit(obj))
                    {
                        return;
                    }
                }
            }
            if (Q.IsReady() && (!HaveQ3 || MainMenu["LaneClear"]["Q3"]))
            {
                if (Player.IsDashing())
                {
                    var minion = GetQCirObj.Select(i => i as Obj_AI_Minion).Where(i => i.IsValid()).ToList();
                    if (minion.Any(i => i.Health <= GetQDmg(i) || i.Team == GameObjectTeam.Neutral) || minion.Count > 2)
                    {
                        CastQCir(minion.MinOrDefault(i => i.Distance(Player)));
                    }
                }
                else
                {
                    var minion = new List<Obj_AI_Minion>();
                    minion.AddRange(GameObjects.EnemyMinions.Where(i => i.IsValidTarget((!HaveQ3 ? Q : Q2).Range - 25)));
                    minion.AddRange(GameObjects.Jungle.Where(i => i.IsValidTarget((!HaveQ3 ? Q : Q2).Range - 25)));
                    minion = minion.OrderByDescending(i => i.MaxHealth).ToList();
                    if (minion.Count > 0)
                    {
                        if (!HaveQ3)
                        {
                            var obj =
                                minion.FirstOrDefault(
                                    i => Q.GetHealthPrediction(i) > 0 && Q.GetHealthPrediction(i) <= GetQDmg(i));
                            if (obj != null && Common.Cast(Q, obj, true) == CastStates.SuccessfullyCasted)
                            {
                                return;
                            }
                        }
                        var pos =
                            (!HaveQ3 ? Q : Q2).GetLineFarmLocation(
                                minion.Select(i => Common.GetPrediction(!HaveQ3 ? Q : Q2, i).UnitPosition.ToVector2())
                                    .ToList());
                        if (pos.MinionsHit > 0)
                        {
                            Q.Cast(pos.Position);
                        }
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (MainMenu["Draw"]["Q"] && Q.Level > 0)
            {
                Drawing.DrawCircle(
                    Player.Position,
                    (!HaveQ3 ? Q : Q2).Range,
                    Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (MainMenu["Draw"]["E"] && E.Level > 0)
            {
                Drawing.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (MainMenu["Draw"]["R"] && R.Level > 0)
            {
                Drawing.DrawCircle(
                    Player.Position,
                    R.Range,
                    R.IsReady() && GetRTarget.Count > 0 ? Color.LimeGreen : Color.IndianRed);
            }
            if (MainMenu["Draw"]["StackQ"] && Q.Level > 0)
            {
                var text = string.Format(
                    "Auto Stack Q: {0}",
                    MainMenu["StackQ"].GetValue<MenuKeyBind>().Active
                        ? (HaveQ3 ? "Full" : (Q.IsReady() ? "Ready" : "Not Ready"))
                        : "Off");
                var pos = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(
                    pos.X - (float)Drawing.GetTextExtent(text).Width / 2,
                    pos.Y + 20,
                    MainMenu["StackQ"].GetValue<MenuKeyBind>().Active && Q.IsReady() && !HaveQ3
                        ? Color.White
                        : Color.Gray,
                    text);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (!Equals(Q.Delay, GetQ12Delay))
            {
                Q.Delay = GetQ12Delay;
            }
            if (!Equals(Q2.Delay, GetQ3Delay))
            {
                Q2.Delay = GetQ3Delay;
            }
            if (!Equals(E.Speed, GetESpeed))
            {
                E.Speed = GetESpeed;
            }
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling())
            {
                return;
            }
            KillSteal();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    Orbwalk();
                    break;
                case OrbwalkerMode.Hybrid:
                    Hybrid();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    Farm();
                    break;
            }
            if (Orbwalker.ActiveMode != OrbwalkerMode.Orbwalk && Orbwalker.ActiveMode != OrbwalkerMode.Hybrid)
            {
                AutoQ();
            }
            StackQ();
            if (MainMenu["Flee"]["E"].GetValue<MenuKeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                Flee();
            }
        }

        private static void OnUpdateEvade(EventArgs args)
        {
            if (Player.IsDead || !MainMenu["Evade"]["Enabled"].GetValue<MenuKeyBind>().Active)
            {
                return;
            }
            if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
            {
                return;
            }
            var safePoint = Evader.IsSafePoint(Player.ServerPosition.ToVector2());
            var safePath = Evader.IsSafePath(Player.GetWaypoints(), 100);
            if (!safePath.IsSafe && !safePoint.IsSafe)
            {
                TryToEvade(safePoint.SkillshotList, Game.CursorPos.ToVector2());
            }
        }

        private static void Orbwalk()
        {
            if (MainMenu["Orbwalk"]["R"] && R.IsReady() && GetRTarget.Count > 0)
            {
                var hero = (from enemy in GetRTarget
                            let nearEnemy =
                                GameObjects.EnemyHeroes.Where(i => i.Distance(enemy) < RWidth && CanCastR(i)).ToList()
                            where
                                (nearEnemy.Count > 1 && enemy.Health <= GetRDmg(enemy))
                                || nearEnemy.Any(i => i.HealthPercent < MainMenu["Orbwalk"]["RHpU"])
                                || nearEnemy.Count >= MainMenu["Orbwalk"]["RCountA"]
                            orderby nearEnemy.Count descending
                            select enemy).ToList();
                if (hero.Count > 0)
                {
                    var target = !MainMenu["Orbwalk"]["RDelay"]
                                     ? hero.FirstOrDefault()
                                     : hero.FirstOrDefault(CanCastDelayR);
                    if (target != null && R.CastOnUnit(target))
                    {
                        return;
                    }
                }
            }
            if (MainMenu["Orbwalk"]["EGap"] && E.IsReady())
            {
                var target = Q.GetTarget() ?? Q2.GetTarget();
                if (target != null)
                {
                    var nearObj = GetNearObj(target, true) ?? GetNearObj(target);
                    if (nearObj != null && (!UnderTower(PosAfterE(nearObj)) || MainMenu["Orbwalk"]["ETower"])
                        && (target.Compare(nearObj)
                                ? (target.Distance(PosAfterE(target)) < target.GetRealAutoAttackRange()
                                   || (HaveQ3 && target.HasBuffOfType(BuffType.SpellShield)))
                                : Player.Distance(target) > MainMenu["Orbwalk"]["ERange"]) && E.CastOnUnit(nearObj))
                    {
                        return;
                    }
                }
            }
            if (Q.IsReady())
            {
                if (Player.IsDashing())
                {
                    var target = GetQCirTarget;
                    if (target != null && CastQCir(target))
                    {
                        return;
                    }
                    if (!HaveQ3 && MainMenu["Orbwalk"]["EGap"] && MainMenu["Orbwalk"]["EStackQ"]
                        && Q.GetTarget(70) == null)
                    {
                        var obj = GetQCirObj.MinOrDefault(i => i.Distance(Player));
                        if (obj != null && CastQCir(obj))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (!HaveQ3)
                    {
                        var target = Q.GetTarget(30);
                        if (target != null && Common.Cast(Q, target, true) == CastStates.SuccessfullyCasted)
                        {
                            return;
                        }
                    }
                    else if (CastQ3())
                    {
                        return;
                    }
                }
            }
            var itemTarget = Q.GetTarget() ?? Q2.GetTarget();
            UseItem(itemTarget);
            if (itemTarget == null)
            {
                return;
            }
            if (Ignite.IsReady() && itemTarget.HealthPercent < 30 && Player.Distance(itemTarget) <= 600)
            {
                Player.Spellbook.CastSpell(Ignite, itemTarget);
            }
        }

        private static Vector3 PosAfterE(Obj_AI_Base target)
        {
            return Player.ServerPosition.Extend(
                target.ServerPosition,
                Player.Distance(target) < 410 ? E.Range : Player.Distance(target) + 65);
        }

        private static void StackQ()
        {
            if (!MainMenu["StackQ"].GetValue<MenuKeyBind>().Active || !Q.IsReady() || Player.IsDashing() || HaveQ3)
            {
                return;
            }
            var target = Q.GetTarget(30);
            if (target != null && Common.Cast(Q, target, true) == CastStates.SuccessfullyCasted)
            {
                return;
            }
            var minion = new List<Obj_AI_Minion>();
            minion.AddRange(GameObjects.EnemyMinions.Where(i => i.IsValidTarget(Q.Range - 25)));
            minion.AddRange(GameObjects.Jungle.Where(i => i.IsValidTarget(Q.Range - 25)));
            minion = minion.OrderByDescending(i => i.MaxHealth).ToList();
            if (minion.Count == 0)
            {
                return;
            }
            var obj = minion.FirstOrDefault(i => Q.GetHealthPrediction(i) > 0 && Q.GetHealthPrediction(i) <= GetQDmg(i));
            if (obj != null && Common.Cast(Q, obj, true) == CastStates.SuccessfullyCasted)
            {
                return;
            }
            var pos =
                Q.GetLineFarmLocation(minion.Select(i => Common.GetPrediction(Q, i).UnitPosition.ToVector2()).ToList());
            if (pos.MinionsHit > 0)
            {
                Q.Cast(pos.Position);
            }
        }

        private static void TryToEvade(List<Skillshot> hitBy, Vector2 to)
        {
            var dangerLevel =
                hitBy.Select(
                    i =>
                    MainMenu["Evade"][i.SpellData.ChampionName.ToLowerInvariant()][i.SpellData.SpellName]["DangerLevel"]
                        .GetValue<MenuSlider>().Value).Concat(new[] { 0 }).Max();
            foreach (var evadeSpell in
                EvadeSpellDatabase.Spells.Where(i => i.Enabled && i.DangerLevel <= dangerLevel && i.IsReady)
                    .OrderBy(i => i.DangerLevel))
            {
                if (evadeSpell.EvadeType == EvadeTypes.Dash && evadeSpell.CastType == CastTypes.Target)
                {
                    var targets =
                        Evader.GetEvadeTargets(evadeSpell)
                            .Where(
                                i =>
                                Evader.IsSafePoint(PosAfterE(i).ToVector2()).IsSafe
                                && (!UnderTower(PosAfterE(i)) || MainMenu["Evade"]["Spells"][evadeSpell.Name]["ETower"]))
                            .ToList();
                    if (targets.Count > 0)
                    {
                        var closestTarget = targets.MinOrDefault(i => to.Distance(PosAfterE(i)));
                        Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                        return;
                    }
                }
                if (evadeSpell.EvadeType == EvadeTypes.WindWall)
                {
                    var skillshots =
                        Evade.DetectedSkillshots.Where(
                            i =>
                            i.Enabled && i.SpellData.CollisionObjects.Contains(CollisionObjectTypes.YasuoWall)
                            && i.IsAboutToHit(
                                150 + evadeSpell.Delay - MainMenu["Evade"]["Spells"][evadeSpell.Name]["WDelay"],
                                Player)).ToList();
                    if (skillshots.Count > 0)
                    {
                        var dangerousSkillshot =
                            skillshots.MaxOrDefault(
                                i =>
                                MainMenu["Evade"][i.SpellData.ChampionName.ToLowerInvariant()][i.SpellData.SpellName][
                                    "DangerLevel"].GetValue<MenuSlider>().Value);
                        Player.Spellbook.CastSpell(
                            evadeSpell.Slot,
                            Player.ServerPosition.Extend(dangerousSkillshot.Start, 100));
                    }
                }
            }
        }

        private static bool UnderTower(Vector3 pos)
        {
            return GameObjects.EnemyTurrets.Any(i => !i.IsDead && i.Distance(pos) <= 850 + Player.BoundingRadius);
        }

        private static void UseItem(Obj_AI_Hero target)
        {
            if (target != null && (target.HealthPercent < 40 || Player.HealthPercent < 50))
            {
                if (Bilgewater.IsReady)
                {
                    Bilgewater.Cast(target);
                }
                if (BotRuinedKing.IsReady)
                {
                    BotRuinedKing.Cast(target);
                }
            }
            if (Youmuu.IsReady && Common.CountEnemy(Q.Range + E.Range) > 0)
            {
                Youmuu.Cast();
            }
            if (Tiamat.IsReady && Common.CountEnemy(Tiamat.Range) > 0)
            {
                Tiamat.Cast();
            }
            if (Hydra.IsReady && Common.CountEnemy(Hydra.Range) > 0)
            {
                Hydra.Cast();
            }
            if (Titanic.IsReady && Common.CountEnemy(Titanic.Range) > 0)
            {
                Titanic.Cast();
            }
        }

        #endregion
    }
}