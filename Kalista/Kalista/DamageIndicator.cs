using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using SharpDX.Direct3D9;
using CNLib;

namespace Kalista {
	public static class DamageIndicator {
		private const int BarWidth = 104;
		private const int LineThickness = 9;

		public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

		private static DamageToUnitDelegate DamageToUnit { get; set; }
		
		private static Font font { get; set; }

		private static readonly Vector2 BarOffset = new Vector2(1, 0); // -9, 11

		private static Color _drawingColor;
		public static Color DrawingColor
		{
			get { return _drawingColor; }
			set { _drawingColor = Color.FromArgb(170, value); }
		}

		public static bool HealthbarEnabled { get; set; }
		public static bool PercentEnabled { get; set; }

		public static void Initialize(DamageToUnitDelegate damageToUnit) {
			// Apply needed field delegate for damage calculation
			DamageToUnit = damageToUnit;
			DrawingColor = Color.Green;
			HealthbarEnabled = true;

			font = new Font(Drawing.Direct3DDevice, new FontDescription {
				FaceName = "微软雅黑",
				Height = 26
			});
			

			// Register event handlers
			Drawing.OnEndScene += OnEndScene;
		}

		private static void OnEndScene(EventArgs args) {
			if (HealthbarEnabled || PercentEnabled)
			{
				foreach (var unit in HeroManager.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
				{
					// Get damage to unit
					var damage = DamageToUnit(unit);

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
						var startPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth), (int)(unit.HPBarPosition.Y + BarOffset.Y) - 18);
						var endPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1, (int)(unit.HPBarPosition.Y + BarOffset.Y) - 18);

						// Draw the line
						Drawing.DrawLine(startPoint, endPoint, LineThickness, DrawingColor);
					}

					if (PercentEnabled)
					{
						// Get damage in percent and draw next to the health bar
						var text = string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%");
						font.DrawText(null, text,
							(int)(unit.HPBarPosition.X + BarWidth / 2),
							(int)(unit.HPBarPosition.Y + BarWidth / 2),
							new ColorBGRA(Color.MediumVioletRed.B, Color.MediumVioletRed.G, Color.MediumVioletRed.R, Color.MediumVioletRed.A));
						Drawing.DrawText(unit.HPBarPosition.X, unit.HPBarPosition.Y, Color.MediumVioletRed, string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"), 10);
					}
				}
			}
		}
	}
}
