﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace MagicalGirlLux
{

	internal class Program
	{

		public const string ChampName = "Lux";
		public static Menu Config;
		public static Orbwalking.Orbwalker Orbwalker;
		public static Spell Q, W, E, R;
		public static HpBarIndicator Hpi = new HpBarIndicator();
		private static SpellSlot Ignite;
		public static GameObject LuxEGameObject;
		private static readonly Obj_AI_Hero player = ObjectManager.Player;

		private static void Main(string[] args)
		{

			CustomEvents.Game.OnGameLoad += OnLoad;
		}



		private static void OnLoad(EventArgs args)
		{

			if (player.ChampionName != ChampName)
			return;

			Notifications.AddNotification("光辉载入~ ", 10000);

			Q = new Spell(SpellSlot.Q, 1175);

			W = new Spell(SpellSlot.W, 1075);

			E = new Spell(SpellSlot.E, 1100);

			R = new Spell(SpellSlot.R, 3000);

			Q.SetSkillshot(0.25f, 70f, 1300f, false, SkillshotType.SkillshotLine);

			W.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.SkillshotLine);

			E.SetSkillshot(0.25f, 280f, 1300f, false, SkillshotType.SkillshotCircle);

			R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);

			Config = new Menu("奥术光辉", "MLGLux", true);
			Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("[光辉]: 走砍设置", "Orbwalker")));
			TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("[光辉]: 目标选择", "Target Selector")));

			var combo = Config.AddSubMenu(new Menu("[光辉]: 连招设置", "Combo Settings"));
			var harass = Config.AddSubMenu(new Menu("[光辉]: 消耗设置", "Harass Settings"));
			var killsteal = Config.AddSubMenu(new Menu("[光辉]: 抢人头设置", "Killsteal Settings"));
			var laneclear = Config.AddSubMenu(new Menu("[光辉]: 清线设置", "Laneclear Settings"));
			var jungleclear = Config.AddSubMenu(new Menu("[光辉]: 清野设置", "Jungle Settings"));
			var misc = Config.AddSubMenu(new Menu("[光辉]: 高级设置", "Misc Settings"));
			var drawing = Config.AddSubMenu(new Menu("[光辉]: 显示设置", "Draw Settings"));
			var debug = Config.AddSubMenu(new Menu("[光辉]: 调试输出", "debug"));
			Config.AddItem(new MenuItem("Science", "作者：ScienceARK"));
			Config.AddItem(new MenuItem("Science2", "翻译：晴依"));

            combo.SubMenu("[连招]蓝量管理")
                .AddItem(new MenuItem("qmana", "[Q] 蓝量 %").SetValue(new Slider(10, 100, 0)));
            combo.SubMenu("[连招]蓝量管理")
                .AddItem(new MenuItem("wmana", "[W] 蓝量 %").SetValue(new Slider(20, 100, 0)));
            combo.SubMenu("[连招]蓝量管理")
                .AddItem(new MenuItem("emana", "[E] 蓝量 %").SetValue(new Slider(5, 100, 0)));
            combo.SubMenu("[连招]蓝量管理")
                .AddItem(new MenuItem("rmana", "[R] 蓝量 %").SetValue(new Slider(10, 100, 0)));

            combo.SubMenu("[技能] Q 光之束缚").AddItem(new MenuItem("UseQ", "使用Q").SetValue(true));
            combo.SubMenu("[技能] Q 光之束缚").AddItem(new MenuItem("AutoQ", "自动Q被控敌人").SetValue(true));

            combo.SubMenu("[技能] W 曲光屏障").AddItem(new MenuItem("UseW", "使用W").SetValue(true));
            combo.SubMenu("[技能] W 曲光屏障").AddItem(new MenuItem("UseWP", "血量少于生命值?%放W").SetValue(true));
            combo.SubMenu("[技能] W 曲光屏障").AddItem(new MenuItem("UseWHP", "生命值?%").SetValue(new Slider(80, 100, 1)));
            combo.SubMenu("[技能] W 曲光屏障").AddItem(new MenuItem("UseWA", "W队友").SetValue(true));
            combo.SubMenu("[技能] W 曲光屏障").AddItem(new MenuItem("UseWAHP", "队友血量%").SetValue(new Slider(30, 100, 1)));

            combo.SubMenu("[技能] E 透光奇点").AddItem(new MenuItem("UseE", "使用E").SetValue(true));

            combo.SubMenu("[技能] R 终极闪光")
                .AddItem(new MenuItem("semir", "半手动R").SetValue(new KeyBind('R', KeyBindType.Press)));
            combo.SubMenu("[技能] R 终极闪光").AddItem(new MenuItem("UseR", "使用R").SetValue(true));
            combo.SubMenu("[技能] R 终极闪光").AddItem(new MenuItem("UseRA", "使用R打AOE伤害").SetValue(false));
            combo.SubMenu("[技能] R 终极闪光").AddItem(new MenuItem("RA", "AOE敌人个数").SetValue(new Slider(3, 5, 1)));
            combo.SubMenu("[技能] R 终极闪光").AddItem(new MenuItem("RQ", "Q中敌人自动R").SetValue(false));

            combo.SubMenu("[技能] 召唤师技能").AddItem(new MenuItem("UseIgnite", "使用点燃").SetValue(true));

            killsteal.AddItem(new MenuItem("SmartKS", "智能抢人头").SetValue(true));
            killsteal.AddItem(new MenuItem("KSI", "点燃抢人头").SetValue(true));
            killsteal.AddItem(new MenuItem("KSQ", "使用 Q").SetValue(true));
            killsteal.AddItem(new MenuItem("KSE", "使用 E").SetValue(true));
            killsteal.AddItem(new MenuItem("RKS", "使用 R").SetValue(true));

            drawing.AddItem(new MenuItem("Draw_Disabled", "禁用所有显示").SetValue(false));
            drawing.AddItem(new MenuItem("Qdraw", "显示 Q 范围").SetValue(new Circle(true, System.Drawing.Color.Orange)));
            drawing.AddItem(new MenuItem("Wdraw", "显示 W 范围").SetValue(new Circle(true, System.Drawing.Color.DarkOrange)));
            drawing.AddItem(new MenuItem("Edraw", "显示 E 范围").SetValue(new Circle(true, System.Drawing.Color.AntiqueWhite)));
            drawing.AddItem(new MenuItem("Rdraw", "显示 R 范围").SetValue(new Circle(true, System.Drawing.Color.CornflowerBlue)));
            drawing.AddItem(new MenuItem("RLine", "显示 [R] 预判").SetValue(new Circle(true, System.Drawing.Color.SkyBlue)));


            harass.AddItem(new MenuItem("autoharass", "自动消耗开关").SetValue(new KeyBind('L', KeyBindType.Toggle)));
            harass.AddItem(new MenuItem("Qharass", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("Qharassslowed", "只有当敌人被减速/击晕/禁锢时才放Q").SetValue(false));
            harass.AddItem(new MenuItem("Eharass", "使用 E").SetValue(true));
            harass.AddItem(new MenuItem("harassmana", "蓝量管理").SetValue(new Slider(30, 100, 0)));

            misc
                .AddItem(new MenuItem("DrawD", "血量显示伤害").SetValue(true));
            misc
                .AddItem(new MenuItem("AntiGapQ", "[Q]防突进").SetValue(true));
            misc
                .AddItem(new MenuItem("AntiGapE", "[E]防突进").SetValue(true));
            misc
                .AddItem(new MenuItem("packetcast", "封包丢技能").SetValue(true));


            laneclear
                .AddItem(new MenuItem("laneQ", "使用 Q").SetValue(false));
            laneclear
                .AddItem(new MenuItem("laneE", "使用 E").SetValue(true));
            laneclear
                .AddItem(new MenuItem("lanemana", "蓝量管理").SetValue(new Slider(30, 100, 0)));


            jungleclear
                .AddItem(new MenuItem("jungleQ", "使用 Q").SetValue(true));
            jungleclear
                .AddItem(new MenuItem("jungleE", "使用 E").SetValue(true));
            jungleclear
                .AddItem(new MenuItem("junglemana", "蓝量管理").SetValue(new Slider(30, 100, 0)));
            jungleclear
                .AddItem(new MenuItem("blank", "                                        "));

            jungleclear.AddItem(
                new MenuItem("jungleks", "抢野开关").SetValue(new KeyBind('K', KeyBindType.Toggle)));

			jungleclear.AddItem(new MenuItem("DoNotStellAlly","不抢自己家野怪").SetValue(true));

            jungleclear
                .AddItem(new MenuItem("Blue", "抢蓝buff").SetValue(true));
            jungleclear
                .AddItem(new MenuItem("Red", "抢红buff").SetValue(true));
            jungleclear
                .AddItem(new MenuItem("Dragon", "抢小龙").SetValue(true));
            jungleclear
                .AddItem(new MenuItem("Baron", "抢大龙").SetValue(true));

            debug.AddItem(new MenuItem("Debug", "显示预判位置").SetValue(false));
            debug.AddItem(new MenuItem("qdebug", "[Q] 调试").SetValue(new Circle(true, System.Drawing.Color.Orange)));
            debug.AddItem(new MenuItem("edebug", "[E] 调试").SetValue(new Circle(true, System.Drawing.Color.Purple)));
            debug.AddItem(new MenuItem("rdebug", "[R] 调试").SetValue(new Circle(true, System.Drawing.Color.CadetBlue)));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnDelete += LuxEgone;
            GameObject.OnCreate += GameObject_OnCreate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), System.Drawing.Color.Green);
                }
            }
        }

        private static int CalcDamage(Obj_AI_Base target)
        {
            //Calculate Combo Damage
            var aa = player.GetAutoAttackDamage(target, Config.Item("packetcast").GetValue<bool>());
            var damage = aa;
            Ignite = player.GetSpellSlot("summonerdot");

            if (Ignite.IsReady())
                damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Config.Item("UseE").GetValue<bool>()) // edamage
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target);
                }
            }
            if (target.HasBuff("luxilluminatingfraulein"))
            {
                damage += aa + 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2 - target.FlatMagicReduction;
            }
            if (player.HasBuff("lichbane"))
            {
                damage += player.CalcDamage(target, Damage.DamageType.Magical,
                    (player.BaseAttackDamage * 0.75) + ((player.BaseAbilityDamage + player.FlatMagicDamageMod) * 0.5));
            }
            if (R.IsReady() && Config.Item("UseR").GetValue<bool>()) // rdamage
            {
                damage += R.GetDamage(target);
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>())
            {
                damage += Q.GetDamage(target);
            }
            return (int)damage;
        }

        private static void Autospells()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;
            if (target.IsInvulnerable)
                return;
            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
            {
                if (target.HasBuffOfType(BuffType.Snare)
                    || target.HasBuffOfType(BuffType.Suppression)
                    || target.HasBuffOfType(BuffType.Taunt)
                    || target.HasBuffOfType(BuffType.Stun)
                    || target.HasBuffOfType(BuffType.Charm)
                    || target.HasBuffOfType(BuffType.Fear))

                    Q.Cast(target, Config.Item("packetcast").GetValue<bool>());
            }
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (player.IsDead || gapcloser.Sender.IsInvulnerable)
                return;

            var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
            if (gapcloser.Sender.IsValidTarget(Q.Range) && Config.Item("AntiGap").GetValue<bool>())
            {
                Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, System.Drawing.Color.DeepPink);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, System.Drawing.Color.MediumPurple, "GAPCLOSER!");
            }

            if (Q.IsReady() && gapcloser.Sender.IsValidTarget(Q.Range) &&
                Config.Item("AntiGapQ").GetValue<bool>())
                Q.Cast(gapcloser.Sender, Config.Item("packetcast").GetValue<bool>());

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range) &&
                Config.Item("AntiGapE").GetValue<bool>())
                E.Cast(gapcloser.End, Config.Item("packetcast").GetValue<bool>());
        }

        private static void LuxEgone(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Lux_Base_E_tar_aoe_green" || sender.Name == "Lux_Base_E_mis.troy")

                LuxEGameObject = null;
        }


        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Lux_Base_E_tar_aoe_green" || sender.Name == "Lux_Base_E_mis.troy")

                LuxEGameObject = sender;

        }
        private static void OnDraw(EventArgs args)
        {
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            if (Config.Item("jungleks").GetValue<KeyBind>().Active)
                Drawing.DrawText(pos.X - 50, pos.Y + 50, System.Drawing.Color.HotPink, "[R] Junglesteal Enabled");
            if (Config.Item("autoharass").GetValue<KeyBind>().Active)
                Drawing.DrawText(pos.X - 50, pos.Y + 30, System.Drawing.Color.Plum, "AutoHarass Enabled");
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;
            {
                if (Config.Item("Qdraw").GetValue<Circle>().Active)
                    if (Q.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range,
                            Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : System.Drawing.Color.Red);

                if (Config.Item("Wdraw").GetValue<Circle>().Active)
                    if (W.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range,
                            W.IsReady() ? Config.Item("Wdraw").GetValue<Circle>().Color : System.Drawing.Color.Red);

                if (Config.Item("Edraw").GetValue<Circle>().Active)
                    if (E.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                            E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : System.Drawing.Color.Red);

                if (Config.Item("Rdraw").GetValue<Circle>().Active)
                    if (R.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range - 2,
                            R.IsReady() ? Config.Item("Rdraw").GetValue<Circle>().Color : System.Drawing.Color.Red);

                {
                    var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                    if (target == null)
                        return;
                    if (target.IsInvulnerable)
                        return;

                    var rpredl = R.GetPrediction(target).CastPosition;
                    var rdmg = R.GetDamage(target);
                    var rpdmg = R.GetDamage(target) + 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2;
                    var rpred = R.GetPrediction(target);
                    var rdraw = new Geometry.Polygon.Line(player.Position, rpredl, R.Range);

                    if (Config.Item("RLine").GetValue<Circle>().Active
                        && target.IsValidTarget(R.Range)
                        && R.IsReady()
                        && rpred.Hitchance >= HitChance.VeryHigh
                        && Config.Item("UseR").GetValue<bool>()
                        && target.HasBuff("luxilluminatingfraulein")
                        && target.Health < rpdmg
                        && target.IsValidTarget(R.Range)
                        && Config.Item("UseR").GetValue<bool>()
                        && R.IsReady()
                        && rpred.Hitchance >= HitChance.VeryHigh
                        && target.Health < rdmg)

                        rdraw.Draw(Config.Item("RLine").GetValue<Circle>().Color, 4);

                    var orbwalkert = Orbwalker.GetTarget();
                    if (orbwalkert.IsValidTarget(R.Range))
                        Render.Circle.DrawCircle(orbwalkert.Position, 80, System.Drawing.Color.DeepSkyBlue, 7);


                    var rdebugp = R.GetPrediction(target);
                    var qdebugp = Q.GetPrediction(target);
                    var edebugp = E.GetPrediction(target);
                    var rdebug = new Geometry.Polygon.Line(player.Position, rpredl, R.Range);
                    var qdebug = new Geometry.Polygon.Line(player.Position, qdebugp.CastPosition, Q.Range + 200);
                    var edebug = new Geometry.Polygon.Line(player.Position, edebugp.CastPosition, E.Range + 200);

                    if (!Config.Item("Debug").GetValue<bool>())
                        return;

                    if (qdebugp.Hitchance >= HitChance.VeryHigh && Config.Item("qdebug").GetValue<Circle>().Active)
                        qdebug.Draw(Config.Item("qdebug").GetValue<Circle>().Color, 2);
                    if (edebugp.Hitchance >= HitChance.VeryHigh && Config.Item("edebug").GetValue<Circle>().Active)
                        edebug.Draw(Config.Item("edebug").GetValue<Circle>().Color, 3);
                    if (rdebugp.Hitchance >= HitChance.VeryHigh && Config.Item("rdebug").GetValue<Circle>().Active)
                        rdebug.Draw(Config.Item("rdebug").GetValue<Circle>().Color, 4);
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;
            if (target.IsInvulnerable)
                return;
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var qpred = Q.GetPrediction(target, Config.Item("packetcast").GetValue<bool>());
            var qcollision = Q.GetCollision(player.ServerPosition.To2D(), new List<Vector2> { qpred.CastPosition.To2D() });
            var minioncol = qcollision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if (E.IsReady() && target.IsValidTarget(R.Range) && Config.Item("Eharass").GetValue<bool>() &&
                player.ManaPercent >= harassmana)
                Elogic();

            if (target.IsValidTarget(Q.Range)
                && minioncol <= 1
                && Config.Item("Qharass").GetValue<bool>()
                && qpred.Hitchance >= HitChance.VeryHigh && player.ManaPercent >= harassmana
                && target.HasBuffOfType(BuffType.Slow) || target.IsValidTarget(Q.Range)
                && minioncol <= 1
                && Config.Item("Qharass").GetValue<bool>()
                && qpred.Hitchance >= HitChance.VeryHigh && player.ManaPercent >= harassmana
                && target.HasBuffOfType(BuffType.Stun) || target.IsValidTarget(Q.Range)
                && minioncol <= 1
                && Config.Item("Qharass").GetValue<bool>()
                && qpred.Hitchance >= HitChance.VeryHigh && player.ManaPercent >= harassmana
                && target.HasBuffOfType(BuffType.Snare))

                Q.Cast(target, Config.Item("packetcast").GetValue<bool>());

            if (Config.Item("Qharassslowed").GetValue<bool>())
                return;

            if (target.IsValidTarget(Q.Range)
                && minioncol <= 1
                && Config.Item("Qharass").GetValue<bool>()
                && qpred.Hitchance >= HitChance.VeryHigh && player.ManaPercent >= harassmana)
                Q.Cast(target, Config.Item("packetcast").GetValue<bool>());
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void Killsteal()
        {
            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(R.Range))
                    .Where(x => !x.IsZombie)
                    .Where(x => !x.IsDead))
            {
                Ignite = player.GetSpellSlot("summonerdot");
                var edmg = E.GetDamage(enemy);
                var qdmg = Q.GetDamage(enemy);
                var rdmg = R.GetDamage(enemy);

                var rpdmg = R.GetDamage(enemy) + 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2;
                var rpred = R.GetPrediction(enemy);
                var qpred = Q.GetPrediction(enemy);
                var epred = E.GetPrediction(enemy);

                var qcollision = Q.GetCollision(player.ServerPosition.To2D(),
                new List<Vector2> { qpred.CastPosition.To2D() });
                var minioncol = qcollision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

                if (player.Distance(enemy.Position) <= 600 && IgniteDamage(enemy) >= enemy.Health &&
                    player.HealthPercent <= 25 &&
                    Config.Item("KSI").GetValue<bool>())
                    player.Spellbook.CastSpell(Ignite, enemy);


                if (enemy.Health < edmg && E.IsReady() && epred.Hitchance >= HitChance.VeryHigh)
                    E.Cast(epred.CastPosition, Config.Item("packetcast").GetValue<bool>());
                else if (enemy.Health < edmg && E.IsReady() && epred.Hitchance >= HitChance.VeryHigh)
                    E.Cast(epred.CastPosition, Config.Item("packetcast").GetValue<bool>());

                if (enemy.Health < qdmg && qpred.Hitchance >= HitChance.VeryHigh && minioncol <= 1 && Q.IsReady() &&
                    Config.Item("KSQ").GetValue<bool>())
                    Q.Cast(enemy, Config.Item("packetcast").GetValue<bool>());

                var ripdmg = R.GetDamage(enemy) + 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2 +
                             IgniteDamage(enemy);
                var passivedmg = 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2 - enemy.FlatMagicReduction;
                var passiveaa = player.GetAutoAttackDamage(player) + passivedmg;
                var lichdmg = player.CalcDamage(enemy, Damage.DamageType.Magical,
                    (player.BaseAttackDamage * 0.75) + ((player.BaseAbilityDamage + player.FlatMagicDamageMod) * 0.5));

                if (enemy.IsValidTarget(Q.Range) && Q.GetPrediction(enemy).Hitchance >= HitChance.VeryHigh && Q.IsReady() &&
                    E.IsReady() && enemy.Health < E.GetDamage(enemy) + Q.GetDamage(enemy))
                    return;

                if (player.HasBuff("lichbane") && enemy.Health < lichdmg &&
                    player.Distance(enemy.Position) < Orbwalking.GetRealAutoAttackRange(player))
                    return;


                if (player.HasBuff("lichbane") && enemy.HasBuff("luxilluminatingfraulein") &&
                    enemy.Health < lichdmg + passivedmg && //LIchbane AA PASSIVE check
                    player.Distance(enemy.Position) < Orbwalking.GetRealAutoAttackRange(player))
                    return;

                if (player.Distance(enemy.Position) < E.Range - 200 && E.GetDamage(enemy) > enemy.Health && E.IsReady() ||
                    LuxEGameObject != null && enemy.Distance(LuxEGameObject.Position) <= E.Width &&
                    enemy.Health < E.GetDamage(enemy) ||
                    player.Distance(enemy.Position) < E.Range - 200 && Q.GetDamage(enemy) > enemy.Health && Q.IsReady() &&
                    Q.GetPrediction(enemy).Hitchance >= HitChance.VeryHigh ||
                    player.Distance(enemy.Position) < Orbwalking.GetRealAutoAttackRange(player) &&
                    player.GetAutoAttackDamage(enemy) * 2 > enemy.Health)
                    return;

                if (LuxEGameObject != null && enemy.Distance(LuxEGameObject.Position) <= E.Width &&
                    enemy.Health < E.GetDamage(enemy) ||
                    enemy.HasBuff("LuxLightBindingMis")
                    && enemy.Health < passiveaa && player.Distance(enemy.Position) <= Orbwalking.GetRealAutoAttackRange(player) &&
                    enemy.HasBuff("luxilluminatingfraulein"))
                    return;

                if (enemy.Health < rdmg - 100 + (3 * player.Level) && enemy.Position.CountAlliesInRange(650) >= 1)
                    return;

                if (enemy.Health <= passiveaa && enemy.HasBuff("luxilluminatingfraulein") &&
                    player.Distance(enemy.Position) <= Orbwalking.GetRealAutoAttackRange(player))
                    return;

                if (enemy.Health <= Q.GetDamage(enemy) && Q.GetPrediction(enemy).Hitchance >= HitChance.VeryHigh &&
                    Q.IsReady())
                    return;

                if (enemy.HasBuff("caitlynaceinthehole"))
                    return;

                if (enemy.Health < rdmg - 100 + (3 * player.Level) && enemy.Position.CountAlliesInRange(650) >= 1)
                    return;

                if (enemy.IsValidTarget(R.Range)
                    && rpred.Hitchance >= HitChance.VeryHigh
                    && Config.Item("RKS").GetValue<bool>()
                    && enemy.HasBuff("luxilluminatingfraulein")
                    && enemy.Health < rpdmg
                    && player.Distance(enemy.Position) >= 100
                    || enemy.IsValidTarget(R.Range)
                    && rpred.Hitchance >= HitChance.VeryHigh
                    && Config.Item("RKS").GetValue<bool>()
                    && enemy.HasBuff("luxilluminatingfraulein")
                    && enemy.Health < rpdmg
                    && enemy.HasBuff("LuxLightBindingMis"))
                    R.Cast(enemy, Config.Item("packetcast").GetValue<bool>());

                if (enemy.IsValidTarget(R.Range)
                    && Config.Item("RKS").GetValue<bool>()
                    && rpred.Hitchance >= HitChance.VeryHigh
                    && enemy.Health < rdmg
                    && player.Distance(enemy.Position) >= 100
                    || enemy.IsValidTarget(R.Range)
                    && Config.Item("RKS").GetValue<bool>()
                    && rpred.Hitchance >= HitChance.VeryHigh
                    && enemy.Health < rdmg
                    && enemy.HasBuff("LuxLightBindingMis"))
                    R.Cast(enemy, Config.Item("packetcast").GetValue<bool>());

                if (player.Distance(enemy.Position) < 600 && E.GetDamage(enemy) > enemy.Health && E.IsReady()
                    ||
                    LuxEGameObject != null && enemy.Distance(LuxEGameObject.Position) <= E.Width &&
                    enemy.Health < E.GetDamage(enemy) ||
                    player.Distance(enemy.Position) < 600 && Q.GetDamage(enemy) > enemy.Health && Q.IsReady() ||
                    player.Distance(enemy.Position) < 600 && enemy.Health < player.GetAutoAttackDamage(enemy) * 2)
                    return;

                if (player.Distance(enemy.Position) <= 600 && ripdmg >= enemy.Health &&
                    Config.Item("UseIgnite").GetValue<bool>() && R.IsReady() && Ignite.IsReady())
                    player.Spellbook.CastSpell(Ignite, enemy);
            }
        }

        private static void Junglesteal()
        {
            if (!R.IsReady())
                return;

            if (Config.Item("Blue").GetValue<bool>())//
            {
                var blueBuff =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.CharData.BaseSkinName == "SRU_Blue")
                        .Where(x => player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));


                if (blueBuff != null)
				{
					var BuffIsAlly = ((blueBuff.Position.Distance(new Vector3(3800.99f, 7883.53f, 52.18f)) < 200 && player.Team == GameObjectTeam.Order)
						|| (blueBuff.Position.Distance(new Vector3(10984.11f, 6960.31f, 51.72f)) < 200 && player.Team == GameObjectTeam.Chaos)) 
						? true : false;
					//不抢队友
					if ((!Config.Item("DoNotStellAlly").GetValue<bool>() && BuffIsAlly) || !BuffIsAlly)
					{
						R.Cast(blueBuff, Config.Item("packetcast").GetValue<bool>());
					}
					
				}
                    
            }

            if (Config.Item("Red").GetValue<bool>())//
            {
                var redBuff =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.CharData.BaseSkinName == "SRU_Red")
                        .Where(x => player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));

				if (redBuff != null) {
					var BuffIsAlly = ((redBuff.Position.Distance(new Vector3(7813.07f, 4051.33f, 53.81f)) < 200 && player.Team == GameObjectTeam.Order)
						|| (redBuff.Position.Distance(new Vector3(139.29f, 10779.34f, 56.38f)) < 200 && player.Team == GameObjectTeam.Chaos))
						? true : false;
					//不抢队友
					if ((!Config.Item("DoNotStellAlly").GetValue<bool>() && BuffIsAlly) || !BuffIsAlly)
					{
						R.Cast(redBuff, Config.Item("packetcast").GetValue<bool>());
					}
				}
                   
            }

            if (Config.Item("Baron").GetValue<bool>())//
            {
                var Baron =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.CharData.BaseSkinName == "SRU_Baron")
                        .Where(x => player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));
				
                if (Baron != null)
                    R.Cast(Baron, Config.Item("packetcast").GetValue<bool>());
            }

            if (Config.Item("Dragon").GetValue<bool>()) //
            {
                var Dragon =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.CharData.BaseSkinName == "SRU_Dragon")
                        .Where(x => player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));

                if (Dragon != null)
                    R.Cast(Dragon, Config.Item("packetcast").GetValue<bool>());
            }
        }


        private static void Jungleclear()
        {
            var junglem = Config.Item("junglemana").GetValue<Slider>().Value;
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            var Qfarmpos = W.GetLineFarmLocation(allMinionsQ, Q.Width);
            var Efarmpos = E.GetCircularFarmLocation(allMinionsE, E.Width);
            if (Qfarmpos.MinionsHit >= 1 && Config.Item("jungleQ").GetValue<bool>() &&
                player.ManaPercent >= junglem)
            {
                Q.Cast(Qfarmpos.Position, Config.Item("packetcast").GetValue<bool>());
            }
            if (Efarmpos.MinionsHit >= 1 && allMinionsE.Count >= 1 && Config.Item("jungleE").GetValue<bool>() &&
                player.ManaPercent >= junglem)
                E.Cast(Efarmpos.Position, Config.Item("packetcast").GetValue<bool>());

            if (LuxEGameObject != null)
                E.Cast();
        }

        private static void Laneclear()
        {
            var lanem = Config.Item("lanemana").GetValue<Slider>().Value;
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width);

            var Qfarmpos = W.GetLineFarmLocation(allMinionsQ, Q.Width);
            var Efarmpos = E.GetCircularFarmLocation(allMinionsE, E.Width);
            if (Qfarmpos.MinionsHit >= 1 && Config.Item("laneQ").GetValue<bool>() && player.ManaPercent >= lanem)
            {
                Q.Cast(Qfarmpos.Position, Config.Item("packetcast").GetValue<bool>());
            }
            if (Efarmpos.MinionsHit >= 3 && allMinionsE.Count >= 2 && Config.Item("laneE").GetValue<bool>() &&
                player.ManaPercent >= lanem)
                E.Cast(Efarmpos.Position, Config.Item("packetcast").GetValue<bool>());

            if (LuxEGameObject != null)
                E.Cast();
        }

        private static void forceR()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;
            if (R.IsReady() && target.IsValidTarget(R.Range))
                R.Cast(target, Config.Item("packetcast").GetValue<bool>());
        }

        public static bool qcasted { get; set; }

        private static void Rlogic()
        {
            {
                if (!Q.IsReady())
                    qcasted = true;
                Utility.DelayAction.Add(1000, () => qcasted = false);
            }

            Ignite = player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

			//var shield = target.AttackShield - target.MagicShield;
			var shield = target.AllShield;

			var rdmg = R.GetDamage(target);
            var rpdmg = R.GetDamage(target) + 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2;
            var rpred = R.GetPrediction(target);
            var ripdmg = R.GetDamage(target) + 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2 +
                         IgniteDamage(target);
            var lichdmg = player.CalcDamage(target, Damage.DamageType.Magical,
                (player.BaseAttackDamage * 0.75) + ((player.BaseAbilityDamage + player.FlatMagicDamageMod) * 0.5));

            var cdmg = R.GetDamage(target) + E.GetDamage(target);
            var passivedmg = 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2 - target.FlatMagicReduction;
            var passiveaa = player.GetAutoAttackDamage(player) + passivedmg;

            if (target.IsInvulnerable)
                return;

            if (target.HasBuff("caitlynaceinthehole"))
                return;



            if (target.Health <= passiveaa && target.HasBuff("luxilluminatingfraulein") &&
                player.Distance(target.Position) <= Orbwalking.GetRealAutoAttackRange(player))
                return;

            if (target.Health <= Q.GetDamage(target) && Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh &&
                Q.IsReady())
                return;

            if (target.Health <= Q.GetDamage(target) && Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh &&
                qcasted)
                return;

            if (LuxEGameObject != null && target.IsValidTarget(R.Range) &&
                target.Position.Distance(LuxEGameObject.Position) <= E.Width && R.IsReady() //EPR combo
                && target.IsValidTarget(R.Range) && rpred.Hitchance >= HitChance.VeryHigh &&
                target.Health < cdmg + (passivedmg * 1) && target.HasBuff("LuxLightBindingMis")
                ||
                LuxEGameObject != null && target.IsValidTarget(R.Range) &&
                target.Position.Distance(LuxEGameObject.Position) <= E.Width && R.IsReady() && Ignite.IsReady()
                //ERQPI combo
                && target.IsValidTarget(R.Range) && rpred.Hitchance >= HitChance.VeryHigh &&
                target.Health < cdmg + (passivedmg * 1) + IgniteDamage(target) && target.HasBuff("LuxLightBindingMis")
                ||
                LuxEGameObject != null && target.IsValidTarget(R.Range) && player.HasBuff("lichbane") &&
                player.Distance(target.Position) <= Orbwalking.GetRealAutoAttackRange(player) && //ERPB combo
                target.Position.Distance(LuxEGameObject.Position) <= E.Width && R.IsReady()
                && target.IsValidTarget(R.Range) && rpred.Hitchance >= HitChance.VeryHigh &&
                target.Health < cdmg + (passivedmg * 1) + lichdmg && target.HasBuff("LuxLightBindingMis")
                ||
                LuxEGameObject != null && target.IsValidTarget(R.Range) && player.HasBuff("lichbane") &&
                player.Distance(target.Position) <= Orbwalking.GetRealAutoAttackRange(player) && //ERLIP combo
                target.Position.Distance(LuxEGameObject.Position) <= E.Width && R.IsReady() && Ignite.IsReady()
                && target.IsValidTarget(R.Range) && rpred.Hitchance >= HitChance.VeryHigh &&
                target.Health < cdmg + (passivedmg * 1) + IgniteDamage(target) + lichdmg &&
                target.HasBuff("LuxLightBindingMis"))
                R.Cast(target, Config.Item("packetcast").GetValue<bool>());


            if (target.HasBuff("LuxLightBindingMis") && Config.Item("RQ").GetValue<bool>() && //RQ config
                rpred.Hitchance >= HitChance.VeryHigh)
                R.Cast(target, Config.Item("packetcast").GetValue<bool>());

            if (player.HasBuff("lichbane") && target.Health < lichdmg && //LIchbane AA check
                player.Distance(target.Position) < Orbwalking.GetRealAutoAttackRange(player))
                return;

            if (player.HasBuff("lichbane") && target.HasBuff("luxilluminatingfraulein") &&
                target.Health < lichdmg + passivedmg && //LIchbane AA PASSIVE check
                player.Distance(target.Position) < Orbwalking.GetRealAutoAttackRange(player))
                return;

            if (target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh && Q.IsReady() &&
                //QE overkill check
                E.IsReady() && target.Health < E.GetDamage(target) + Q.GetDamage(target))
                return;

            if (player.Distance(target.Position) < E.Range - 200 && E.GetDamage(target) > target.Health && E.IsReady() ||
                LuxEGameObject != null && target.Distance(LuxEGameObject.Position) <= E.Width &&
                target.Health < E.GetDamage(target) || //ECHECK
                player.Distance(target.Position) < E.Range - 200 && Q.GetDamage(target) > target.Health && Q.IsReady() &&
                Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh ||
                player.Distance(target.Position) < Orbwalking.GetRealAutoAttackRange(player) &&
                player.GetAutoAttackDamage(target) * 2 > target.Health)
                return;

            if (LuxEGameObject != null && target.Distance(LuxEGameObject.Position) <= E.Width &&
                target.Health < E.GetDamage(target) ||
                target.HasBuff("LuxLightBindingMis")
                && target.Health < passiveaa && player.Distance(target.Position) <= Orbwalking.GetRealAutoAttackRange(player) &&
                target.HasBuff("luxilluminatingfraulein"))
                return;

            if (target.IsValidTarget(R.Range)
                && R.IsReady()
                && Config.Item("UseRA").GetValue<bool>()
                && rpred.Hitchance >= HitChance.VeryHigh && player.Distance(target.Position) <= E.Range &&
                target.IsValidTarget(R.Range) && !E.IsReady())

                R.CastIfWillHit(target, Config.Item("RA").GetValue<Slider>().Value, Config.Item("packetcast").GetValue<bool>());

            else if (target.IsValidTarget(R.Range)
                     && R.IsReady()
                     && Config.Item("UseRA").GetValue<bool>()
                     && rpred.Hitchance >= HitChance.VeryHigh && player.Distance(target.Position) >= E.Range &&
                     target.IsValidTarget(R.Range))

                R.CastIfWillHit(target, Config.Item("RA").GetValue<Slider>().Value, Config.Item("packetcast").GetValue<bool>());

            if (target.Health < rdmg - 100 + (3 * player.Level) && target.Position.CountAlliesInRange(650) >= 1)
                return;

            if (target.IsValidTarget(R.Range)
                && rpred.Hitchance >= HitChance.VeryHigh
                && Config.Item("UseR").GetValue<bool>()
                && target.HasBuff("luxilluminatingfraulein")
                && target.Health < rpdmg
                && player.Distance(target.Position) >= 100
                || target.IsValidTarget(R.Range)
                && rpred.Hitchance >= HitChance.VeryHigh
                && Config.Item("UseR").GetValue<bool>()
                && target.HasBuff("luxilluminatingfraulein")
                && target.Health < rpdmg
                && target.HasBuff("LuxLightBindingMis"))
                R.Cast(target, Config.Item("packetcast").GetValue<bool>());

            if (target.IsValidTarget(R.Range)
                && Config.Item("UseR").GetValue<bool>()
                && rpred.Hitchance >= HitChance.VeryHigh
                && target.Health < rdmg
                && player.Distance(target.Position) >= 100
                || target.IsValidTarget(R.Range)
                && Config.Item("UseR").GetValue<bool>()
                && rpred.Hitchance >= HitChance.VeryHigh
                && target.Health < rdmg
                && target.HasBuff("LuxLightBindingMis"))
                R.Cast(target, Config.Item("packetcast").GetValue<bool>());

            if (player.Distance(target.Position) < 600 && E.GetDamage(target) > target.Health && E.IsReady()
                ||
                LuxEGameObject != null && target.Distance(LuxEGameObject.Position) <= E.Width &&
                target.Health < E.GetDamage(target) ||
                player.Distance(target.Position) < 600 && Q.GetDamage(target) > target.Health && Q.IsReady() ||
                player.Distance(target.Position) < 600 && target.Health < player.GetAutoAttackDamage(target) * 2)
                return;

            if (player.Distance(target.Position) <= 600 && ripdmg >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>() && R.IsReady() && Ignite.IsReady())
                player.Spellbook.CastSpell(Ignite, target);
        }

        private static void Wlogic()
        {
            if (player.HasBuff("zedulttargetmark")
                || player.HasBuff("soulshackles")
                || player.HasBuff("summonerdot")
                || player.HasBuff("vladimirhemoplage")
                || player.HasBuff("fallenonetarget")
                || player.HasBuff("caitlynaceinthehole")
                || player.HasBuff("fizzmarinerdoombomb")
                || player.HasBuff("leblancsoulshackle")
                || player.HasBuff("mordekaiserchildrenofthegrave"))
            {
                W.Cast(player);
            }

            if (player.HasBuff("Recall") || player.InFountain() || player.IsDead)
                return;

            if (Config.Item("UseWP").GetValue<bool>()
                && (player.HealthPercent <= 25
                && W.IsReady() && player.Position.CountEnemiesInRange(W.Range) >= 1
                && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                player.HasBuffOfType(BuffType.Poison)
                || player.HasBuffOfType(BuffType.Snare)))
            {
                W.Cast(player);
            }

            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(R.Range))
                    .Where(x => !x.IsZombie)
                    .Where(x => !x.IsDead))
            {

                if (Config.Item("UseWP").GetValue<bool>()
                    && (player.HealthPercent <= Config.Item("UseWHP").GetValue<Slider>().Value
                        && W.IsReady() && player.Position.CountEnemiesInRange(W.Range) >= 1
                        && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && enemy.IsFacing(player)) ||
                    player.HasBuffOfType(BuffType.Poison)
                    || player.HasBuffOfType(BuffType.Snare))
                {
                    W.Cast(player);
                }
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                var wpred = W.GetPrediction(hero);
                if (player.Distance(hero.Position) > W.Range) return;
                if (Config.Item("UseWA").GetValue<bool>() &&
                    (hero.HealthPercent <= Config.Item("UseWAHP").GetValue<Slider>().Value && W.IsReady() &&
                     hero.Distance(player.ServerPosition) <= W.Range && wpred.Hitchance >= HitChance.VeryHigh) &&
                    hero.Position.CountEnemiesInRange(W.Range) >= 1
                    || hero.HasBuffOfType(BuffType.Poison)
                    || hero.HasBuffOfType(BuffType.Snare))
                {
                    W.Cast(hero);
                }

                if (hero.HasBuff("zedulttargetmark")
                    || hero.HasBuff("summonerdot")
                    || hero.HasBuff("soulshackles")
                    || hero.HasBuff("vladimirhemoplage")
                    || hero.HasBuff("fallenonetarget")
                    || hero.HasBuff("caitlynaceinthehole")
                    || hero.HasBuff("fizzmarinerdoombomb")
                    || hero.HasBuff("leblancsoulshackle")
                    || hero.HasBuff("mordekaiserchildrenofthegrave"))
                {
                    W.Cast(hero);
                }
            }


        }


        private static void Edetonate()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            if (LuxEGameObject != null && LuxEGameObject.Position.CountEnemiesInRange(E.Width) >= 2)
                E.Cast();

            if (LuxEGameObject != null && LuxEGameObject.Position.CountEnemiesInRange(E.Width) >= 1 &&
                target.Position.CountEnemiesInRange(500) >= 2)
                E.Cast();

            if (LuxEGameObject != null && LuxEGameObject.Position.CountEnemiesInRange(E.Width) >= 1 &&
                player.Distance(target.Position) >= Orbwalking.GetRealAutoAttackRange(player))
                E.Cast();

            if (target.HasBuff("luxilluminatingfraulein") && target.HasBuff("LuxLightBindingMis") &&
                player.Distance(target.Position) <= Orbwalking.GetRealAutoAttackRange(player))
                return;

            if (LuxEGameObject != null && LuxEGameObject.Position.CountEnemiesInRange(E.Width) >= 1)
                E.Cast();
        }

        private static void Elogic()
        {
            var target = TargetSelector.GetTarget(E.Range + 500, TargetSelector.DamageType.Magical);
            var epred = E.GetPrediction(target);
            var emana = Config.Item("emana").GetValue<Slider>().Value;

            if (LuxEGameObject != null && E.IsReady() && LuxEGameObject.Position.CountEnemiesInRange(E.Width) < 1)
                Utility.DelayAction.Add(2000, () => E.Cast());

            if (target.IsInvulnerable)
                return;

            if (target.HasBuff("luxilluminatingfraulein") && target.HasBuff("LuxLightBindingMis") &&
                player.Distance(target.Position) <= Orbwalking.GetRealAutoAttackRange(player))
                return;

            if (LuxEGameObject != null
                && LuxEGameObject.Position.CountEnemiesInRange(300) >= 1)
                E.Cast();

            if (LuxEGameObject != null
                && target.HasBuffOfType(BuffType.Slow))
                E.Cast();

            if (LuxEGameObject != null)
                return;

            if (player.ManaPercent >= emana && epred.Hitchance >= HitChance.VeryHigh)
                E.Cast(epred.CastPosition, Config.Item("packetcast").GetValue<bool>());
            else if (player.ManaPercent >= emana && epred.Hitchance >= HitChance.VeryHigh)
                E.Cast(epred.CastPosition, Config.Item("packetcast").GetValue<bool>());
        }

        private static void Combo()
        {
            Ignite = player.GetSpellSlot("summonerdot");
            var qmana = Config.Item("qmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            if (target.IsInvulnerable)
                return;

            var qpred = Q.GetPrediction(target, Config.Item("packetcast").GetValue<bool>());
            var qcollision = Q.GetCollision(player.ServerPosition.To2D(), new List<Vector2> { qpred.CastPosition.To2D() });
            var minioncol = qcollision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
            var lichdmg = player.CalcDamage(target, Damage.DamageType.Magical,
                (player.BaseAttackDamage * 0.75) + ((player.BaseAbilityDamage + player.FlatMagicDamageMod) * 0.5));

            if (target.IsValidTarget(Q.Range)
                && minioncol <= 1
                && Q.IsReady()
                && qpred.Hitchance >= HitChance.VeryHigh && player.ManaPercent >= qmana &&
                Config.Item("UseQ").GetValue<bool>())
                Q.Cast(target, Config.Item("packetcast").GetValue<bool>());

            var passivedmg = 10 + (8 * player.Level) + player.FlatMagicDamageMod * 0.2 - target.FlatMagicReduction;
            var passiveaa = player.GetAutoAttackDamage(player) + passivedmg;

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>() && target.IsValidTarget(E.Range))
                Elogic();

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health &&
                player.HealthPercent <= 25 &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) + E.GetDamage(target) >= target.Health &&
            player.HealthPercent <= 25 && E.IsReady() && LuxEGameObject == null &&
            Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);


            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) + E.GetDamage(target) >= target.Health &&
            player.HealthPercent <= 25 && E.IsReady() && LuxEGameObject != null && target.Distance(LuxEGameObject.Position) <= LuxEGameObject.BoundingRadius &&
            Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) + E.GetDamage(target) >= target.Health &&
            player.HealthPercent <= 25 && Q.IsReady() && Q.GetPrediction(target).Hitchance >= HitChance.High &&
            Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);



            if (player.HasBuff("lichbane") && target.Health < lichdmg &&
                player.Distance(target.Position) < Orbwalking.GetRealAutoAttackRange(player))
                return;

            if (LuxEGameObject != null && target.Distance(LuxEGameObject.Position) <= E.Width &&
                target.Health < E.GetDamage(target) ||
                target.HasBuff("LuxLightBindingMis")
                && target.Health < passiveaa && player.Distance(target.Position) <= Orbwalking.GetRealAutoAttackRange(player) &&
                target.HasBuff("luxilluminatingfraulein"))
                return;

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var wmana = Config.Item("wmana").GetValue<Slider>().Value;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    Rlogic();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    Jungleclear();
                    break;
            }
            if (Config.Item("UseW").GetValue<bool>() && player.ManaPercent >= wmana)
            {
                Wlogic();
            }
            if (Config.Item("jungleks").GetValue<KeyBind>().Active)
            {
                Junglesteal();
            }
            if (E.IsReady())
            {
                Edetonate();
            }
            if (Config.Item("SmartKS").GetValue<bool>())
            {
                Killsteal();
            }
            if (Config.Item("autoharass").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (Config.Item("semir").GetValue<KeyBind>().Active)
            {
                forceR();
            }
            if (Config.Item("AutoQ").GetValue<bool>())
            {
                Autospells();
            }
        }
    }
}
