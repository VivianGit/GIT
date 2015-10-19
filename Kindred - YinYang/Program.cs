using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kindred___YinYang.Spell_Database;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Kindred___YinYang
{
    class Program
    {
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;
        private static readonly Obj_AI_Hero Kindred = ObjectManager.Player;
        public static Menu Config;
        public static Vector3 OrderSpawnPosition = new Vector3(394, 461, 171);
        public static Vector3 ChaosSpawnPosition = new Vector3(14340, 14391, 179);
        public static Orbwalking.Orbwalker Orbwalker;

        public static string[] HighChamps =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs","Kindred"
            };
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Kindred.ChampionName != "Kindred")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 340);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 550);

            Config = new Menu("永猎双子 - 千珏", "Kindred - Yin Yang", true);
            TargetSelector.AddToMenu(Config.SubMenu("目标选择"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("走砍设置"));
            var comboMenu = new Menu("连招设置", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("q.combo.style", "Q 模式").SetValue(new StringList(new[] {"风筝", "100% 命中","安全位置"})));
                comboMenu.AddItem(new MenuItem("q.combo", "使用 Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("w.combo", "使用 W").SetValue(true));
                comboMenu.AddItem(new MenuItem("e.combo", "使用 E").SetValue(true));
                Config.AddSubMenu(comboMenu);
            }
            var eMenu = new Menu("E 设置", "E Settings");
            {
                eMenu.AddItem(new MenuItem("e.whte", "                     E 白名单"));
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsEnemy))
                {
                    eMenu.AddItem(new MenuItem("enemy." + enemy.CharData.BaseSkinName, string.Format("E: {0}", enemy.CharData.BaseSkinName)).SetValue(HighChamps.Contains(enemy.CharData.BaseSkinName)));

                }
                Config.AddSubMenu(eMenu);
            }
            var harassMenu = new Menu("消耗设置", "Harass Settings");
            {
                harassMenu.AddItem(new MenuItem("q.harass", "使用 Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("w.harass", "使用 W").SetValue(true));
                harassMenu.AddItem(new MenuItem("e.harass", "使用 E").SetValue(true));
                harassMenu.AddItem(new MenuItem("harass.mana", "蓝量管理").SetValue(new Slider(20, 1, 99)));
                Config.AddSubMenu(harassMenu);
            }
            var laneClear = new Menu("清线设置", "Clear Settings");
            {
                laneClear.AddItem(new MenuItem("q.clear", "使用 Q").SetValue(true));
                laneClear.AddItem(new MenuItem("q.minion.count", "Q 兵数量").SetValue(new Slider(4, 1, 5)));
                laneClear.AddItem(new MenuItem("clear.mana", "蓝量管理").SetValue(new Slider(20, 1, 99)));
                Config.AddSubMenu(laneClear);
            }
            var jungleClear = new Menu("清野设置", "Jungle Settings");
            {
                jungleClear.AddItem(new MenuItem("q.jungle", "使用 Q").SetValue(true));
                jungleClear.AddItem(new MenuItem("w.jungle", "使用 W").SetValue(true));
                jungleClear.AddItem(new MenuItem("e.jungle", "使用 E").SetValue(true));
                jungleClear.AddItem(new MenuItem("jungle.mana", "蓝量管理").SetValue(new Slider(20, 1, 99)));
                Config.AddSubMenu(jungleClear);
            }
            var ksMenu = new Menu("抢人头设置", "KillSteal Settings");
            {
                ksMenu.AddItem(new MenuItem("q.ks", "使用 Q").SetValue(true));
                ksMenu.AddItem(new MenuItem("q.ks.count", "平A次数").SetValue(new Slider(2, 1, 5)));
                Config.AddSubMenu(ksMenu);
            }
            var miscMenu = new Menu("高级设置", "Miscellaneous");
            {
                miscMenu.AddItem(new MenuItem("q.antigapcloser", "Q防突进").SetValue(true));
                var antiRengar = new Menu("防狮子狗", "Anti Rengar");
                {
                    antiRengar.AddItem(new MenuItem("anti.rengar", "防狮子狗").SetValue(true));
                    antiRengar.AddItem(new MenuItem("hp.percent.for.rengar", "最少血量%").SetValue(new Slider(30, 1, 99)));
                    miscMenu.AddSubMenu(antiRengar);
                }
                var spellMenu = new Menu("R防以下技能", "Spell Breaker");
                {
                    spellMenu.AddItem(new MenuItem("spell.broker", "启用").SetValue(true));
                    spellMenu.AddItem(new MenuItem("katarina.r", "卡特 (R)").SetValue(true));
                    spellMenu.AddItem(new MenuItem("missfortune.r", "好运姐 (R)").SetValue(true));
                    spellMenu.AddItem(new MenuItem("lucian.r", "奥巴马 (R)").SetValue(true));
                    spellMenu.AddItem(new MenuItem("hp.percent.for.broke", "最少血量%").SetValue(new Slider(20, 1, 99)));
                    miscMenu.AddSubMenu(spellMenu);
                }
                var rProtector = new Menu("(R) 保护", "(R) Protector");
                {
                    rProtector.AddItem(new MenuItem("protector","Disable Protector?").SetValue(true));
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsEnemy))
                    {
                        foreach (var skillshot in SpellDatabase.Spells.Where(x => x.charName == enemy.ChampionName)) // 2.5F Protector
                        {
                            rProtector.AddItem(new MenuItem("hero." + skillshot.spellName, ""+skillshot.charName +"("+skillshot.spellKey+")").SetValue(true));
                        }
                    }
                    miscMenu.AddSubMenu(rProtector);
                }
                Config.AddSubMenu(miscMenu);
            }
            var drawMenu = new Menu("显示设置", "Draw Settings");
            {
                var damageDraw = new Menu("伤害预计", "Damage Draw");
                {
                    damageDraw.AddItem(new MenuItem("aa.indicator", "平A伤害").SetValue(new Circle(true, Color.Gold)));
                    drawMenu.AddSubMenu(damageDraw);
                }
                drawMenu.AddItem(new MenuItem("q.drawx", "Q 范围").SetValue(new Circle(false, Color.White)));
                drawMenu.AddItem(new MenuItem("w.draw", "W 范围").SetValue(new Circle(false, Color.Gold)));
                drawMenu.AddItem(new MenuItem("e.draw", "E 范围").SetValue(new Circle(false, Color.DodgerBlue)));
                drawMenu.AddItem(new MenuItem("r.draw", "R 范围").SetValue(new Circle(false, Color.GreenYellow)));
                Config.AddSubMenu(drawMenu);
            }
            Config.AddItem(new MenuItem("e.method", "E 模式").SetValue(new StringList(new[] { "鼠标位置" })));
            Config.AddItem(new MenuItem("use.r", "使用 R").SetValue(true));
            Config.AddItem(new MenuItem("r.whte", "                          R 名单"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsAlly))
            {
                Config.AddItem(new MenuItem("respite." + ally.CharData.BaseSkinName, string.Format("大招: {0}", ally.CharData.BaseSkinName)).SetValue(HighChamps.Contains(ally.CharData.BaseSkinName)));

            }
            Config.AddItem(new MenuItem("min.hp.for.r", "最小生命值%").SetValue(new Slider(20, 1, 99)));
            Config.AddToMainMenu();
            Game.PrintChat("<font color='#ff3232'>永猎双子 - 千钰: </font> <font color='#d4d4d4'>执子之手，与子共生！</font>");
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

       
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!R.IsReady() && Kindred.IsDead && Kindred.IsZombie && sender.IsAlly && !sender.IsMe && !Config.Item("protector").GetValue<bool>())
            {
                return;
            }
            if (sender is Obj_AI_Hero && R.IsReady() && sender.IsEnemy && !spell.SData.IsAutoAttack()
                && !sender.IsDead && !sender.IsZombie && sender.IsValidTarget(1000))
            {
                foreach (var protector in SpellDatabase.Spells.Where(x => x.spellName == spell.SData.Name 
                    && Config.Item("hero." + x.spellName).GetValue<bool>()))
                {
                    if (protector.spellType == SpellType.Circular && Kindred.Distance(spell.End) <= 200 &&
                        sender.GetSpellDamage(Kindred,protector.spellName) > Kindred.Health)
                    {
                        R.Cast(Kindred);
                    }
                    if (protector.spellType == SpellType.Cone && Kindred.Distance(spell.End) <= 200 &&
                        sender.GetSpellDamage(Kindred, protector.spellName) > Kindred.Health)
                    {
                        R.Cast(Kindred);
                    }
                    if (protector.spellType == SpellType.Line && Kindred.Distance(spell.End) <= 200
                        && sender.GetSpellDamage(Kindred,protector.spellName) > Kindred.Health)
                    {
                        Game.PrintChat("Killable");
                        R.Cast(Kindred);
                    }
                }
            }
        }
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (Config.Item("anti.rengar").GetValue<bool>() && R.IsReady() && sender.IsEnemy && !sender.IsAlly && !sender.IsDead
                && sender.Name == "Rengar_LeapSound.troy" && Kindred.HealthPercent < Config.Item("hp.percent.for.rengar").GetValue<Slider>().Value)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x=> x.IsValidTarget(1000) && x.ChampionName == "Rengar"))
                {
                    R.Cast(Kindred);
                }
            }
        }
        private static void SpellBroker()
        {
            if (Config.Item("katarina.r").GetValue<bool>() && R.IsReady() && Kindred.HealthPercent < Config.Item("hp.percent.for.broke").GetValue<Slider>().Value)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.ChampionName == "Katarina" && x.IsValidTarget(R.Range) && x.HasBuff("katarinarsound") && !Kindred.IsDead && !x.IsDead && !x.IsZombie))
                {
                    R.Cast(Kindred);
                }
            }
            if (Config.Item("lucian.r").GetValue<bool>() && R.IsReady() && Kindred.HealthPercent < Config.Item("hp.percent.for.broke").GetValue<Slider>().Value)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.ChampionName == "Lucian" && x.IsValidTarget(R.Range) && x.HasBuff("lucianr") && !Kindred.IsDead && !x.IsDead && !x.IsZombie))
                {
                    R.Cast(Kindred);
                }
            }
            if (Config.Item("missfortune.r").GetValue<bool>() && R.IsReady() && Kindred.HealthPercent < Config.Item("hp.percent.for.broke").GetValue<Slider>().Value)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.ChampionName == "MissFortune" && x.IsValidTarget(R.Range) && x.HasBuff("missfortunebulletsound") && !Kindred.IsDead && !x.IsDead && !x.IsZombie))
                {
                    R.Cast(Kindred);
                }
            }
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 300)
            {
                Q.Cast(gapcloser.End.Extend(ObjectManager.Player.ServerPosition, ObjectManager.Player.Distance(gapcloser.End) + Q.Range));
            }
        }
        private static int AaIndicator(Obj_AI_Hero enemy)
        {
             double aCalculator = ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Physical, Kindred.TotalAttackDamage());
             double killableAaCount = enemy.Health / aCalculator;
             int totalAa = (int)Math.Ceiling(killableAaCount);
             return totalAa;
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    Jungle();
                    break;
            }
            if (Config.Item("use.r").GetValue<bool>() && R.IsReady())
            {
                RLogic();
            }
            if (Config.Item("q.ks").GetValue<bool>() && Q.IsReady())
            {
                KillSteal(Config.Item("q.ks.count").GetValue<Slider>().Value);
            }
            if (Config.Item("spell.broker").GetValue<bool>() && R.IsReady())
            {
                SpellBroker();
            }
           
        }
        private static void CollisionObjectCheckCast(Spell spell, Obj_AI_Hero unit, int count)
        {
            if (spell.GetPrediction(unit).CollisionObjects.Count <= count)
            {
                spell.Cast(Game.CursorPos);
            }        
        }
        private static void CastSafePosition(Spell spell, Obj_AI_Hero hero)
        {
            if (Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.To2D(), Prediction.GetPrediction(hero, 0f, hero.AttackRange).UnitPosition.To2D(), spell.Range, Orbwalking.GetRealAutoAttackRange(hero)).Count() > 0)
            {
                spell.Cast(
                    Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.To2D(),
                        Prediction.GetPrediction(hero, 0f, hero.AttackRange).UnitPosition.To2D(), spell.Range,
                        Orbwalking.GetRealAutoAttackRange(hero)).MinOrDefault(i => i.Distance(Game.CursorPos)));
            }
            else
            {
                spell.Cast(ObjectManager.Player.ServerPosition.Extend(hero.ServerPosition, -E.Range));
            }
        }
        private static void QStyleCast(Spell spell, Obj_AI_Hero unit, int count)
        {
            switch (Config.Item("q.combo.style").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    spell.Cast(Game.CursorPos);
                    break;
                case 1:
                    CollisionObjectCheckCast(spell,unit,count);
                    break;
                case 2:
                    CastSafePosition(spell,unit);
                    break;
            }
        }
        private static void Combo()
        {
            var useQ = Config.Item("q.combo").GetValue<bool>();
            var useW = Config.Item("w.combo").GetValue<bool>();
            var useE = Config.Item("e.combo").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o=> o.IsValidTarget(ObjectManager.Player.AttackRange) && !o.IsDead && !o.IsZombie))
                {
                    QStyleCast(Q,enemy,2);
                }
            }
            if (useW && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && !o.IsDead && !o.IsZombie))
                {
                    W.Cast();
                } 
            }
            if (useE && E.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Config.Item("enemy." + enemy.CharData.BaseSkinName).GetValue<bool>())
                    {
                        E.Cast(enemy);
                    }
                } 
            }
        }
        private static void Harass()
        {
            var useQ = Config.Item("q.combo").GetValue<bool>();
            var useW = Config.Item("w.combo").GetValue<bool>();
            var useE = Config.Item("e.combo").GetValue<bool>();
            var harassMana = Config.Item("harass.mana").GetValue<Slider>().Value;

            if (Kindred.ManaPercent > harassMana)
            {
                if (useQ && Q.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(ObjectManager.Player.AttackRange) && !o.IsDead && !o.IsZombie))
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
                if (useW && W.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && !o.IsDead && !o.IsZombie))
                    {
                        W.Cast();
                    }
                }
                if (useE && E.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                    {
                        if (Config.Item("enemy." + enemy.CharData.BaseSkinName).GetValue<bool>())
                        {
                            E.Cast(enemy);
                        }
                    }
                }
            }
        }
        private static void RLogic()
        {
            var minHP = Config.Item("min.hp.for.r").GetValue<Slider>().Value;  
            foreach (var ally in HeroManager.Allies.Where(o=> o.HealthPercent < minHP && !o.IsRecalling() && !o.IsDead && !o.IsZombie
                && Kindred.Distance(o.Position) < R.Range && !o.InFountain()))
            {
                if (Config.Item("respite."+ally.CharData.BaseSkinName).GetValue<bool>() && Kindred.CountEnemiesInRange(1500) >= 1 
                    && ally.CountEnemiesInRange(1500) >= 1)
                {
                    R.Cast(ally);
                }
            }
        }
        private static void Clear()
        {
            var xMinion = MinionManager.GetMinions(Kindred.ServerPosition,Kindred.AttackRange, MinionTypes.All, MinionTeam.Enemy);
            var useQ = Config.Item("q.clear").GetValue<bool>();
            var manaClear = Config.Item("clear.mana").GetValue<Slider>().Value;
            var minCount = Config.Item("q.minion.count").GetValue<Slider>().Value;
            if (Kindred.ManaPercent >= manaClear)
            {
                if (useQ && Q.IsReady() && xMinion.Count >= minCount)
                {
                    Q.Cast(Game.CursorPos);
                }
            }
        }
        private static void KillSteal(int aacount)
        {
            foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Q.Range)))
            {
                if (enemy.Health < ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Physical, Kindred.TotalAttackDamage()) * aacount)
                {
                    Q.Cast(Game.CursorPos);
                }
            }
        }
        private static void Jungle()
        {
            var useQ = Config.Item("q.jungle").GetValue<bool>();
            var useW = Config.Item("w.jungle").GetValue<bool>();
            var useE = Config.Item("e.jungle").GetValue<bool>();
            var manaSlider = Config.Item("jungle.mana").GetValue<Slider>().Value;
            var mob = MinionManager.GetMinions(Kindred.ServerPosition, Orbwalking.GetRealAutoAttackRange(Kindred) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mob == null ||  mob.Count == 0)
            {
                return;
            }

            if (Kindred.ManaPercent > manaSlider)
            {
                if (Q.IsReady() && useQ)
                {
                    Q.Cast(Game.CursorPos);
                }
                if (W.IsReady() && useW)
                {
                    W.Cast();
                }
                if (E.IsReady() && useE)
                {
                    E.Cast(mob[0]);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.Item("q.drawx").GetValue<Circle>();
            var menuItem2 = Config.Item("w.draw").GetValue<Circle>();
            var menuItem3 = Config.Item("e.draw").GetValue<Circle>();
            var menuItem4 = Config.Item("r.draw").GetValue<Circle>();
            var menuItem5 = Config.Item("aa.indicator").GetValue<Circle>();

            if (menuItem1.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), Q.Range, menuItem1.Color, 5);
            }
            if (menuItem2.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), W.Range, menuItem2.Color, 5);
            }
            if (menuItem3.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), E.Range, menuItem3.Color, 5);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), R.Range, menuItem4.Color, 5);
            }
            if (menuItem4.Active)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(1500)  && x.IsValid && x.IsVisible && !x.IsDead && !x.IsZombie))
                {
                    Drawing.DrawText(enemy.HPBarPosition.X, enemy.HPBarPosition.Y, menuItem5.Color,
                                        string.Format("{0} Basic Attack = Kill", AaIndicator(enemy)));
                }
            }
        }
    }
}
