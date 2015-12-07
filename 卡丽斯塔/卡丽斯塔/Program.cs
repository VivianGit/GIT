using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Unit = LeagueSharp.Common.CustomEvents.Unit;

namespace Aessmbly {
	class Program {
		private const int BarWidth = 104;
		private static readonly Vector2 BarOffset = new Vector2(-9, 11);
		private const int LineThickness = 9;
		public static Color DrawingColor = Color.Green;

		#region SentineData
		
		private enum SentinelLocations {
			Baron,
			Dragon,
			Mid,
			Blue,
			Red
		}

		private const float MaxRandomRadius = 15;
		private static readonly Random Random = new Random(DateTime.Now.Millisecond);
		private static readonly Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Vector2>> Locations = new Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Vector2>>
		{
			{
				GameObjectTeam.Order, new Dictionary<SentinelLocations, Vector2>
				{
					{ SentinelLocations.Mid, new Vector2(8428, 6465) },
					{ SentinelLocations.Blue, new Vector2(3871.489f, 7901.054f) },
					{ SentinelLocations.Red, new Vector2(7862.244f, 4111.187f) }
				}
			},
			{
				GameObjectTeam.Chaos, new Dictionary<SentinelLocations, Vector2>
				{
					{ SentinelLocations.Mid, new Vector2(6545, 8361) },
					{ SentinelLocations.Blue, new Vector2(10931.73f, 6990.844f) },
					{ SentinelLocations.Red, new Vector2(7016.869f, 10775.55f) }
				}
			},
			{
				GameObjectTeam.Neutral, new Dictionary<SentinelLocations, Vector2>
				{
					{ SentinelLocations.Baron, new Vector2(5007.124f, 10471.45f) },
					{ SentinelLocations.Dragon, new Vector2(9866.148f, 4414.014f) }
				}
			}
		};

		private static readonly Dictionary<SentinelLocations, Func<bool>> EnabledLocations = new Dictionary<SentinelLocations, Func<bool>>
		{
			{ SentinelLocations.Baron, () => Config.Item("baron").GetValue<bool>() },
			{ SentinelLocations.Dragon, () => Config.Item("dragon").GetValue<bool>() },
			{ SentinelLocations.Mid, () => Config.Item("mid").GetValue<bool>() },
			{ SentinelLocations.Red, () => Config.Item("red").GetValue<bool>() },
			{ SentinelLocations.Blue, () => Config.Item("blue").GetValue<bool>() },
		};

		private static readonly List<Tuple<GameObjectTeam, SentinelLocations>> OpenLocations = new List<Tuple<GameObjectTeam, SentinelLocations>>();
		private static readonly Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Obj_AI_Base>> ActiveSentinels = new Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Obj_AI_Base>>();
		private static Tuple<GameObjectTeam, SentinelLocations> SentLocation { get; set; }
		#endregion

		#region FleeData
		private static Vector3 TargetPosition = Vector3.Zero;
		private static int InitTime { get; set; }
		private static bool IsJumpPossible { get; set; }
		private static Vector3 FleePosition = Vector3.Zero;
		#endregion

		public static Obj_AI_Hero Player = ObjectManager.Player;
		public static Menu Config;
		public static Orbwalking.Orbwalker Orbwalker;
		public static Spell Q, W, E, R;

		static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += Game_OnStart;
		}

		private static void Game_OnStart(EventArgs args) {
			if (Player.ChampionName != "Kalista") return;

			Game.PrintChat(
				"卡丽斯塔".ToHtml("#AAAAFF", FontStlye.Bold)
				+ " - "
				+ "你们无处可逃！我说了，你们无处可逃！".ToHtml(Color.Yellow, FontStlye.Cite));

			LoadMenu();
			LoadSpell();
			LoadSentinel();
			SoulBoundSaver.Initialize();
			DamageIndicator.Initialize(Damages.GetRendDamage);

			Spellbook.OnCastSpell += OnCastSpell;
			Drawing.OnDraw += OnDraw;
			Game.OnUpdate += Game_OnUpdate;
			Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
			Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			Unit.OnDash += Unit_OnDash;

			
			if (Game.MapId == GameMapId.SummonersRift)
			{
				GameObject.OnCreate += GameObject_OnCreate;
			}
		}

		private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args) {
			if (sender.IsMe)
			{
				InitTime = 0;
				TargetPosition = Vector3.Zero;
			}
		}

		private static void GameObject_OnCreate(GameObject sender, EventArgs args) {
			if (SentLocation == null)
			{
				return;
			}

			var sentinel = sender as Obj_AI_Minion;
			if (sentinel != null && sentinel.IsAlly && sentinel.MaxHealth == 2 && sentinel.Name == "RobotBuddy")
			{
				Utility.DelayAction.Add(1000, () => ValidateSentinel(sentinel));
			}
		}

		private static void ValidateSentinel(Obj_AI_Base sentinel) {
			if (sentinel.Health == 2 && sentinel.GetBuffCount("kalistaw") == 1)
			{
				if (!ActiveSentinels.ContainsKey(SentLocation.Item1))
				{
					ActiveSentinels.Add(SentLocation.Item1, new Dictionary<SentinelLocations, Obj_AI_Base>());
				}
				ActiveSentinels[SentLocation.Item1].Remove(SentLocation.Item2);
				ActiveSentinels[SentLocation.Item1].Add(SentLocation.Item2, sentinel);

				SentLocation = null;
				LoadSentinel();
			}
		}

		private static void LoadSentinel() {
			OpenLocations.Clear();
			foreach (var location in Locations)
			{
				if (!ActiveSentinels.ContainsKey(location.Key))
				{
					OpenLocations.AddRange(location.Value.Where(o => EnabledLocations[o.Key]()).Select(loc => new Tuple<GameObjectTeam, SentinelLocations>(location.Key, loc.Key)));
				}
				else
				{
					OpenLocations.AddRange(from loc in location.Value
										   where EnabledLocations[loc.Key]() && !ActiveSentinels[location.Key].ContainsKey(loc.Key)
										   select new Tuple<GameObjectTeam, SentinelLocations>(location.Key, loc.Key));
				}
			}
		}

		private static void Drawing_OnEndScene(EventArgs args) {
			var PercentEnabled = Config.Item("percent").GetValue<bool>();
			var HealthbarEnabled = Config.Item("healthbar").GetValue<bool>();
			if (HealthbarEnabled || PercentEnabled)
			{
				foreach (var unit in HeroManager.Enemies.Where(u => u.IsValidTarget() ))
				{
					var damage = Damages.GetRendDamage(unit);

					if (damage <= 0)
					{
						continue;
					}

					if (HealthbarEnabled)
					{
						var damagePercentage = ((unit.TotalShieldHealth() - damage) > 0 ? (unit.TotalShieldHealth() - damage) : 0) /
											   (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);
						var currentHealthPercentage = unit.TotalShieldHealth() / (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);

						var startPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth), (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
						var endPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1, (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

						Drawing.DrawLine(startPoint, endPoint, LineThickness, DrawingColor);
					}

					if (PercentEnabled)
					{
						Drawing.DrawText(unit.HPBarPosition.X, unit.HPBarPosition.Y,
							Color.MediumVioletRed, 
							string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"), 10);
					}
				}
			}
		}

		private static void DrawDamage() {
			var PercentEnabled = Config.Item("percent").GetValue<bool>();
			var HealthbarEnabled = Config.Item("healthbar").GetValue<bool>();
			if (HealthbarEnabled || PercentEnabled)
			{
				//
				foreach (var unit in HeroManager.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
				{
					// Get damage to unit
					var damage = Damages.GetRendDamage(unit);

					// Continue on 0 damage
					if (damage <= 0)
					{
						continue;
					}

					if (HealthbarEnabled)
					{
						// Get remaining HP after damage applied in percent and the current percent of health
						var damagePercentage = ((unit.TotalShieldHealth() - damage) > 0 ? (unit.TotalShieldHealth() - damage) : 0) /
											   (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);
						var currentHealthPercentage = unit.TotalShieldHealth() / (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);

						// Calculate start and end point of the bar indicator
						var startPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth), (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
						var endPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1, (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

						// Draw the line
						Drawing.DrawLine(startPoint, endPoint, LineThickness, DrawingColor);
					}

					if (PercentEnabled)
					{
						// Get damage in percent and draw next to the health bar
						Drawing.DrawText(unit.HPBarPosition.X, unit.HPBarPosition.Y,
							Color.MediumVioletRed,
							string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"), 10);
					}
				}
			}
		}

		private static void OnDraw(EventArgs args) {
			if (Config.Item("drawQ").GetValue<bool>())
			{
				Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Blue, 2);
			}
			if (Config.Item("drawE").GetValue<bool>())
			{
				Render.Circle.DrawCircle(Player.Position, E.Range, Color.Blue, 2);
			}

			#region FleeDraw
			if (FleePosition != Vector3.Zero)
			{
				Render.Circle.DrawCircle(FleePosition, 50, IsJumpPossible ? Color.Green : Program.Q.IsReady() ? Color.Red : Color.Teal, 10);
			}
			#endregion

			//DrawDamage();
        }

		private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target) {
			if (Config.Item("comboUseQAA").GetValue<bool>() &&
				Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
				Player.ManaPercent > Config.Item("comboMana").GetValue<Slider>().Value &&
				Q.IsReady())
			{
				var hero = target as Obj_AI_Hero;
				if (hero != null && Player.GetAutoAttackDamage(hero) < hero.Health + hero.AllShield + hero.PhysicalShield)
				{
					Q.Cast(hero);
				}
			}
		}

		private static void Orbwalking_OnNonKillableMinion(AttackableUnit minion) {
			var target = minion as Obj_AI_Base;
            if (Config.Item("secureE").GetValue<bool>() && E.IsReady() && target.IsRendKillable())
			{
				E.Cast();
			}
		}

		private static void ActiveExploit(Obj_AI_Base target) {
			if (Config.Item("Exploit").GetValue<bool>())
			{
				if (target == null)
				{
					target = (Obj_AI_Base)Orbwalker.GetTarget();
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

		private static void Game_OnUpdate(EventArgs args) {

			PermaActive();

			switch (Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Mixed:
					Harass();
                    break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					LaneClear();
					JungleClear();
                    break;
				case Orbwalking.OrbwalkingMode.Combo:
					Combo();
                    break;
				default:
					break;
			}

			if (Config.Item("flee").GetValue<KeyBind>().Active)
			{
				Flee();
			}

			if (Game.MapId == GameMapId.SummonersRift)
			{
				#region 哨兵处理
				foreach (var entry in ActiveSentinels.ToArray())
				{
					if (Config.Item("alert").GetValue<bool>() && entry.Value.Any(o => o.Value.Health == 1))
					{
						var activeSentinel = entry.Value.First(o => o.Value.Health == 1);
						var pingstring = string.Format("[卡丽斯塔] 哨兵在{0}被攻击!",
							string.Concat((entry.Key == GameObjectTeam.Order
								? "蓝BUFF处"
								: entry.Key == GameObjectTeam.Chaos
									? "红BUFF处"
									: "河道"), " (", activeSentinel.Key, ")"));
						Utill.Print(pingstring);
                        Game.ShowPing(PingCategory.Fallback, activeSentinel.Value.Position, true);
					}

					var invalid = entry.Value.Where(o => !o.Value.IsValid || o.Value.Health < 2 || o.Value.GetBuffCount("kalistaw") == 0).ToArray();
					if (invalid.Length > 0)
					{
						foreach (var location in invalid)
						{
							ActiveSentinels[entry.Key].Remove(location.Key);
						}
						LoadSentinel();
					}
				}

				if (Config.Item("enabledW").GetValue<bool>()&& !Player.InBase() && W.IsReady() && Player.ManaPercent >= Config.Item("mana").GetValue<Slider>().Value && !Player.IsRecalling())
				{
					if (!Config.Item("noMode").GetValue<bool>() || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
					{
						if (OpenLocations.Count > 0 && SentLocation == null)
						{

							var closestLocation = OpenLocations.Where(o => Locations[o.Item1][o.Item2].Distance(Player) < W.Range - MaxRandomRadius / 2)
								.OrderByDescending(o => Locations[o.Item1][o.Item2].Distance(Player, true))
								.FirstOrDefault();
							if (closestLocation != null)
							{
								var position = Locations[closestLocation.Item1][closestLocation.Item2];
								var randomized = (new Vector2(position.X - MaxRandomRadius / 2 + Random.NextFloat(0, MaxRandomRadius),
									position.Y - MaxRandomRadius / 2 + Random.NextFloat(0, MaxRandomRadius))).To3D();
								SentLocation = closestLocation;
								W.Cast(randomized);
								Utility.DelayAction.Add(2000, () => SentLocation = null);
							}
						}
					}
				}
				#endregion
			}
		}

		private static void Flee() {

			if (TargetPosition != Vector3.Zero)
			{
				Player.IssueOrder(GameObjectOrder.MoveTo, TargetPosition);

				if (Environment.TickCount - InitTime > 500)
				{
					TargetPosition = Vector3.Zero;
					InitTime = 0;
				}
				else
				{
					return;
				}
			}

			if (Config.Item("fleeAutoattack").GetValue<bool>() && !Config.Item("fleeWalljump").GetValue<bool>())
			{
				var dashObjects = VectorHelper.GetDashObjects();
				Orbwalker.ForceTarget(dashObjects.Count > 0 ? dashObjects[0] : null);
			}

			if (Config.Item("fleeWalljump").GetValue<bool>())
			{
				var wallCheck = VectorHelper.GetFirstWallPoint(Player.Position, Game.CursorPos);
				if (wallCheck != null)
				{
					wallCheck = VectorHelper.GetFirstWallPoint((Vector3)wallCheck, Game.CursorPos, 5);
				}

				var movePosition = wallCheck != null ? (Vector3)wallCheck : Game.CursorPos;
				var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
				FleePosition = NavMesh.GridToWorld((short)tempGrid.X, (short)tempGrid.Y);
				Obj_AI_Base target = null;
				if (Config.Item("fleeAutoattack").GetValue<bool>())
				{
					var dashObjects = VectorHelper.GetDashObjects();
					if (dashObjects.Count > 0)
					{
						target = dashObjects[0];
					}
				}
				IsJumpPossible = false;
				if (Q.IsReady() && wallCheck != null)
				{
					var wallPosition = movePosition;
					var direction = (Game.CursorPos.To2D() - wallPosition.To2D()).Normalized();
					const float maxAngle = 80f;
					const float step = maxAngle / 20;
					var currentAngle = 0f;
					var currentStep = 0f;
					var jumpTriggered = false;
					while (true)
					{
						if (currentStep > maxAngle && currentAngle < 0)
						{
							break;
						}
						if ((currentAngle == 0 || currentAngle < 0) && currentStep != 0)
						{
							currentAngle = (currentStep) * (float)Math.PI / 180;
							currentStep += step;
						}
						else if (currentAngle > 0)
						{
							currentAngle = -currentAngle;
						}

						Vector3 checkPoint;
						if (currentStep == 0)
						{
							currentStep = step;
							checkPoint = wallPosition + 300 * direction.To3D();
						}
						else
						{
							checkPoint = wallPosition + 300 * direction.Rotated(currentAngle).To3D();
						}
						if (!checkPoint.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) &&
							!checkPoint.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building))
						{
							wallCheck = VectorHelper.GetFirstWallPoint(checkPoint, wallPosition);
							if (wallCheck != null)
							{
								var wallPositionOpposite = (Vector3)VectorHelper.GetFirstWallPoint((Vector3)wallCheck, wallPosition, 5);

								if (Math.Sqrt(Program.Player.GetPath(wallPositionOpposite).Sum(o => o.To2D().LengthSquared())) - Program.Player.Distance(wallPositionOpposite) > 200)
								{
									if (Program.Player.Distance(wallPositionOpposite, true) < Math.Pow(300 - Program.Player.BoundingRadius / 2, 2))
									{
										InitTime = Environment.TickCount;
										TargetPosition = wallPositionOpposite;
										Q.Cast(wallPositionOpposite);
										jumpTriggered = true;
										break;
									}
									IsJumpPossible = true;
								}
							}
						}
					}
					if (!jumpTriggered)
					{
						Orbwalker.ForceTarget(target);
					}
				}
				else
				{
					Orbwalker.ForceTarget(target);
				}
			}
		}

		private static void PermaActive() {
			Orbwalker.ForceTarget(null);
			if (E.IsReady())
			{
				#region Killsteal
				if (Config.Item("killsteal").GetValue<bool>() && HeroManager.Enemies.Any(h => h.IsValidTarget(E.Range) && h.IsRendKillable()) && E.Cast())
				{
					return;
				}

				#endregion

				#region E on big mobs
				//bigE
				if (Config.Item("bigE").GetValue<bool>())
				{
					if (ObjectManager.Get<Obj_AI_Minion>().Any(m =>
					{
						if (!m.IsAlly && E.IsInRange(m) && m.HasRendBuff())
						{
							var skinName = m.CharData.BaseSkinName.ToLower();
							return (skinName.Contains("siege") ||
									skinName.Contains("super") ||
									skinName.Contains("dragon") ||
									skinName.Contains("baron") ||
									skinName.Contains("spiderboss")) &&
								   m.IsRendKillable();
						}
						return false;
					}) && E.Cast())
					{
						return;
					}
				}

				#endregion

				#region E combo (harass plus)
				//harassPlus
				if (Config.Item("harassPlus").GetValue<bool>())
				{
					if (HeroManager.Enemies.Any(o => o.IsValidTarget() && E.IsInRange(o) && o.HasRendBuff()) &&
						ObjectManager.Get<Obj_AI_Minion>().Any(o => E.IsInRange(o) && o.IsRendKillable()) &&
						E.Cast())
					{
						return;
					}
				}

				#endregion

				#region E before death
				//autoBelowHealthE
				if (Player.HealthPercent < Config.Item("autoBelowHealthE").GetValue<Slider>().Value && HeroManager.Enemies.Any(o => o.IsValidTarget() && o.HasRendBuff() && E.IsInRange(o)) && E.Cast())
				{
					return;
				}

				#endregion
			}
		}

		private static void Combo() {
			// Item usage
			if (Config.Item("comboUseItems").GetValue<bool>() && (Orbwalker.GetTarget() is Obj_AI_Hero))
			{
				ItemManager.UseBotrk((Obj_AI_Hero)Orbwalker.GetTarget());
				ItemManager.UseYoumuu((Obj_AI_Base)Orbwalker.GetTarget());
			}

			var target = TargetSelector.GetTarget((Config.Item("comboUseQ").GetValue<bool>() && Q.IsReady()) ? Q.Range : (E.Range * 1.2f), TargetSelector.DamageType.Physical);
			if (target != null)
			{
				// Q usage
				if (Q.IsReady() && Config.Item("comboUseQ").GetValue<bool>() && (!Config.Item("comboUseQAA").GetValue<bool>() || (Player.GetSpellDamage(target, SpellSlot.Q) > target.TotalShieldHealth() && !target.HasBuffOfType(BuffType.SpellShield))) &&
					Player.ManaPercent >= Config.Item("comboUseQ").GetValue<Slider>().Value && Q.Cast(target)== Spell.CastStates.SuccessfullyCasted)
				{
					return;
				}

				// E usage
				var buff = target.GetRendBuff();
				if (Config.Item("comboUseE").GetValue<bool>() && (E.Level>0 && E.IsReady()) && buff != null && E.IsInRange(target))
				{
                    if (!Config.Item("killsteal").GetValue<bool>() && target.IsRendKillable() && E.Cast())
					{
						return;
					}

					if (buff.Count >= Config.Item("comboNumE").GetValue<Slider>().Value)
					{
						if ((target.Distance(Player) > (E.Range * 0.8) ||
							 buff.EndTime - Game.Time < 0.3) && E.Cast())
						{
							return;
						}
					}

					if (!Config.Item("harassPlus").GetValue<bool>() && Config.Item("comboUseEslow").GetValue<bool>() &&
						ObjectManager.Get<Obj_AI_Minion>().Any(o => E.IsInRange(o) && o.IsRendKillable()) &&
						E.Cast())
					{
						return;
					}
				}

				ActiveExploit(target);

                if (Config.Item("comboUseAA").GetValue<bool>() && Orbwalking.CanAttack() && target.Distance(Player)< Player.AttackRange + Player.BoundingRadius + 20 &&
					Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
				{
					
					var targetMinion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(o => Orbwalker.InAutoAttackRange(o));
                    Orbwalker.ForceTarget(targetMinion);
					ActiveExploit(targetMinion);
				}
			}
		}

		private static void Harass() {
			if (Player.ManaPercent < Config.Item("harassMana").GetValue<Slider>().Value)
			{
				return;
			}
			if (Config.Item("harassUseQ").GetValue<bool>() && Q.IsReady())
			{
				var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
				if (target != null)
				{
					Q.Cast(target);
				}
			}
		}

		private static void LaneClear() {
			if (Player.ManaPercent < Config.Item("laneMana").GetValue<Slider>().Value)
			{
				return;
			}
			if (!(Config.Item("laneUseQ").GetValue<bool>() && Q.IsReady()) && !(Config.Item("laneUseE").GetValue<bool>() && E.IsReady()))
			{
				return;
			}

			var minions = MinionManager.GetMinions(Q.Range);
			#region E usage

			if (Config.Item("laneUseE").GetValue<bool>() && E.IsReady())
			{
				var minionsInRange = minions.Where(m => E.IsInRange(m)).ToArray();
				if (minionsInRange.Length >= Config.Item("laneNumE").GetValue<Slider>().Value)
				{
					var killableNum = 0;
					foreach (var minion in minionsInRange.Where(minion => minion.IsRendKillable()))
					{
						killableNum++;
						if (killableNum >= Config.Item("laneNumE").GetValue<Slider>().Value)
						{
							E.Cast();
							break;
						}
					}
				}
			}
			#endregion

			ActiveExploit(null);
		}

		private static void JungleClear() {
			if (!Config.Item("jungleUseE").GetValue<bool>() || !E.IsReady())
			{
				return;
			}
			var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral);
			if (mobs.Any(m => m.IsRendKillable()))
			{
				E.Cast();
			}
		}

		private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args) {
			if (sender.Owner.IsMe && args.Slot == SpellSlot.Q && Player.IsDashing())
			{
				args.Process = false;
			}
		}

		private static void LoadSpell() {
			Q = new Spell(SpellSlot.Q, 1150, TargetSelector.DamageType.Physical);
			W = new Spell(SpellSlot.W, 5000);
			E = new Spell(SpellSlot.E, 1000, TargetSelector.DamageType.Physical);
			R = new Spell(SpellSlot.R, 1500);

			Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
		}

		private static void LoadMenu() {

			Config = new Menu("卡丽斯塔", "AsKalista", true);
			var targetSelectorMenu = new Menu("目标选择", "目标选择");
			targetSelectorMenu.AddItem(new MenuItem("ts1", "目标选择器设置转换到基本库菜单中"));
            TargetSelector.AddToMenu(targetSelectorMenu);
			Config.AddSubMenu(targetSelectorMenu);

			var OrbMenu = new Menu("走砍设置", "走砍设置");
			Orbwalker = new Orbwalking.Orbwalker(OrbMenu);
			Config.AddSubMenu(OrbMenu);

			var CombMenu = new Menu("连招设置", "连招设置");
			CombMenu.AddItem(new MenuItem("comboUseQ", "使用 Q").SetValue(true));
			CombMenu.AddItem(new MenuItem("comboUseQAA", "只在平A后使用Q").SetValue(true));
			CombMenu.AddItem(new MenuItem("comboUseE", "使用 E").SetValue(true));
			CombMenu.AddItem(new MenuItem("comboUseEslow", "E死小兵以减速有标记敌人").SetValue(true));
			CombMenu.AddItem(new MenuItem("comboUseAA", "A兵追击敌人").SetValue(true));
			CombMenu.AddItem(new MenuItem("comboUseItems", "使用道具").SetValue(true));
			CombMenu.AddItem(new MenuItem("comboNumE", "E人最少层数").SetValue(new Slider(5, 1, 50)));
			CombMenu.AddItem(new MenuItem("comboMana", "使用Q蓝量").SetValue(new Slider(30)));
			Config.AddSubMenu(CombMenu);

			var HarassMenu = new Menu("消耗设置", "消耗设置");
			HarassMenu.AddItem(new MenuItem("harassUseQ", "使用 Q").SetValue(true));
			HarassMenu.AddItem(new MenuItem("harassMana", "蓝量管理").SetValue(new Slider(30)));
			Config.AddSubMenu(HarassMenu);

			var LaneClearMenu = new Menu("清线设置", "清线设置");
			LaneClearMenu.AddItem(new MenuItem("laneUseQ", "使用 Q").SetValue(true));
			LaneClearMenu.AddItem(new MenuItem("laneUseE", "使用 E").SetValue(true));
			LaneClearMenu.AddItem(new MenuItem("laneNumQ", "最少可Q死小兵数量").SetValue(new Slider(3, 1, 10)));
			LaneClearMenu.AddItem(new MenuItem("laneNumE", "最少可E死小兵数量").SetValue(new Slider(2, 1, 10)));
			LaneClearMenu.AddItem(new MenuItem("laneMana", "蓝量管理").SetValue(new Slider(30)));
			Config.AddSubMenu(LaneClearMenu);

			var JungleClearMenu = new Menu("清野设置", "清野设置");
			JungleClearMenu.AddItem(new MenuItem("jungleUseE", "使用 E").SetValue(true));
			Config.AddSubMenu(JungleClearMenu);

			var FleeMenu = new Menu("逃跑设置", "逃跑设置");
			FleeMenu.AddItem(new MenuItem("flee", "逃跑").SetValue(new KeyBind('s', KeyBindType.Press)));
			FleeMenu.AddItem(new MenuItem("fleeWalljump", "跳墙").SetValue(true));
			FleeMenu.AddItem(new MenuItem("fleeAutoattack", "自动平A").SetValue(true));
			Config.AddSubMenu(FleeMenu);

			var MiscMenu = new Menu("其它设置", "其它设置");
			MiscMenu.AddItem(new MenuItem("killsteal", "E抢人头").SetValue(true));
			MiscMenu.AddItem(new MenuItem("bigE", "总是E死大车兵").SetValue(true));
			MiscMenu.AddItem(new MenuItem("saveSoulbound", "R救队友").SetValue(true));
			MiscMenu.AddItem(new MenuItem("secureE", "来不及平A时，用E补刀").SetValue(true));
			MiscMenu.AddItem(new MenuItem("harassPlus", "自动E死小兵消耗有标记的敌人").SetValue(true));
			MiscMenu.AddItem(new MenuItem("autoBelowHealthE", "当血量少于% 自动E").SetValue(new Slider(3)));
			MiscMenu.AddItem(new MenuItem("reductionE", "计算E伤害时少算？点").SetValue(new Slider(20)));
			MiscMenu.AddItem(new MenuItem("Exploit", "使用漏洞").SetValue(false));
			Config.AddSubMenu(MiscMenu);

			var WMenu = new Menu("W设置", "W设置");
			if (Game.MapId!= GameMapId.SummonersRift)
			{
				WMenu.AddItem(new MenuItem("Wstring1", "W设置内容只在召唤师峡谷有效"));
			}
			else
			{
				WMenu.AddItem(new MenuItem("enabledW", "自动W").SetValue(true));
				WMenu.AddItem(new MenuItem("noMode", "只在没有任何攻击模式时自动W").SetValue(true));
				WMenu.AddItem(new MenuItem("alert", "当W哨兵被攻击时警告").SetValue(true));
				WMenu.AddItem(new MenuItem("mana", "释放W时最少蓝量%").SetValue(new Slider(40)));

				WMenu.AddItem(new MenuItem("baron", "大龙").SetValue(true));
				WMenu.AddItem(new MenuItem("dragon", "小龙").SetValue(true));
				WMenu.AddItem(new MenuItem("mid", "中路草丛").SetValue(true));
				WMenu.AddItem(new MenuItem("blue", "蓝buff").SetValue(true));
				WMenu.AddItem(new MenuItem("red", "红buff").SetValue(true));
			}
			Config.AddSubMenu(WMenu);

			var ItemMenu = new Menu("道具设置", "道具设置");
			ItemMenu.AddItem(new MenuItem("cutlass", "使用弯刀").SetValue(true));
			ItemMenu.AddItem(new MenuItem("botrk", "使用破败").SetValue(true));
			ItemMenu.AddItem(new MenuItem("ghostblade", "使用幽梦").SetValue(true));
			Config.AddSubMenu(ItemMenu);

			var DrawMenu = new Menu("显示设置", "显示设置");
			DrawMenu.AddItem(new MenuItem("draw0", "技能范围"));
			DrawMenu.AddItem(new MenuItem("drawQ", "Q 范围").SetValue(true));
			DrawMenu.AddItem(new MenuItem("drawW", "W 范围").SetValue(true));
			DrawMenu.AddItem(new MenuItem("drawE", "E 范围").SetValue(true));
			DrawMenu.AddItem(new MenuItem("drawEleaving", "E 引爆范围").SetValue(true));
			DrawMenu.AddItem(new MenuItem("drawR", "R 范围").SetValue(true));
			DrawMenu.AddItem(new MenuItem("draw1", "伤害计算 (E技能)"));
			DrawMenu.AddItem(new MenuItem("healthbar", "显示在血条上").SetValue(true));
			DrawMenu.AddItem(new MenuItem("percent", "显示百分比").SetValue(true));
			Config.AddSubMenu(DrawMenu);

			Config.AddToMainMenu();
		}
	}
}
