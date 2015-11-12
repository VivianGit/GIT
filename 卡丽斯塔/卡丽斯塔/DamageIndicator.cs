using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
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
		private static FontDescription fontDescription = new FontDescription { FaceName = "微软雅黑",Height = 38 };
        private static Font DamageFont = new Font(Drawing.Direct3DDevice, fontDescription);
		private static Color _color = Color.YellowGreen;
		private static ColorBGRA DamageFontColor = new ColorBGRA(_color.B,_color.G,_color.R,_color.A);

		public delegate float DamageToUnitDelegate(Obj_AI_Base hero);

		private static DamageToUnitDelegate DamageToUnit { get; set; }

		//private static readonly Vector2 BarOffset = new Vector2(-9, 11);
		private static readonly Vector2 BarOffset = new Vector2(10, 20);

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

		private static void DrawHpBar(Obj_AI_Base unit,float damage) {
			const int height = 8;

			var barPos = unit.HPBarPosition;
			var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
			var yPos = barPos.Y + BarOffset.Y;
			var xPosDamage = barPos.X + BarOffset.X + BarWidth * percentHealthAfterDamage;
			var xPosCurrentHp = barPos.X + BarOffset.X + BarWidth * unit.Health / unit.MaxHealth;

			Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + height, 1, Color.Black);

			var differenceInHp = xPosCurrentHp - xPosDamage;
			var pos1 = barPos.X + 9 + (107 * percentHealthAfterDamage);

			for (var i = 0; i < differenceInHp; i++)
			{
				Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + height, 1, DrawingColor);
			}
		}

		private static void OnEndScene(EventArgs args) {
			PercentEnabled = Program.Config.Item("percent").GetValue<bool>();
			HealthbarEnabled = Program.Config.Item("healthbar").GetValue<bool>();
			if (HealthbarEnabled || PercentEnabled)
			{
				//foreach (var unit in HeroManager.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
				foreach (var unit in ObjectManager.Get<Obj_AI_Base>().Where(u => u.IsValid && u.Team!= Program.Player.Team && !u.IsMinion))
				{
					var damage = DamageToUnit(unit);

					if (damage <= 0)
					{
						continue;
					}

					if (HealthbarEnabled)
					{
						DrawHpBar(unit, damage);
						//var damagePercentage = ((unit.TotalShieldHealth() - damage) > 0 ? (unit.TotalShieldHealth() - damage) : 0) /
						//					   (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);
						//var currentHealthPercentage = unit.TotalShieldHealth() / (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);

						//var startPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth), (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
						//var endPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1, (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

						//Drawing.DrawLine(startPoint, endPoint, LineThickness, DrawingColor);
					}

					if (PercentEnabled)
					{
						DamageFont.DrawText(null, string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"),
							(int)(unit.HPBarPosition.X + BarWidth / 2),
							(int)(unit.HPBarPosition.Y + BarWidth / 2),
							 DamageFontColor);
       //                 Drawing.DrawText(unit.HPBarPosition.X, unit.HPBarPosition.Y,
							//Color.MediumVioletRed, string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"), 10);
					}
				}
			}
		}
	}

}
