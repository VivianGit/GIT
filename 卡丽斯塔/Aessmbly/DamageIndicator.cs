using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace Aessmbly {
	public static class DamageIndicator {
		private const int BarWidth = 104;
		private const int LineThickness = 9;

		public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

		private static DamageToUnitDelegate DamageToUnit { get; set; }

		private static readonly Vector2 BarOffset = new Vector2(-9, 11);

		private static Color _drawingColor;
		public static Color DrawingColor {
			get { return _drawingColor; }
			set { _drawingColor = Color.FromArgb(170, value); }
		}

		public static bool HealthbarEnabled { get; set; }
		public static bool PercentEnabled { get; set; }

		public static void Initialize(DamageToUnitDelegate damageToUnit) {
			DamageToUnit = damageToUnit;
			DrawingColor = Color.Green;
			HealthbarEnabled = true;

			Drawing.OnDraw += OnEndScene;
		}

		private static void OnEndScene(EventArgs args) {
			PercentEnabled = Program.Config.Item("percent").GetValue<bool>();
			HealthbarEnabled = Program.Config.Item("healthbar").GetValue<bool>();
			if (HealthbarEnabled || PercentEnabled)
			{
				foreach (var unit in HeroManager.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
				{
					var damage = DamageToUnit(unit);

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
							Color.MediumVioletRed, string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"), 10);
					}
				}
			}
		}
	}

}
