using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aessmbly {
	public class VectorHelper {
		// Credits to furikuretsu from Stackoverflow (http://stackoverflow.com/a/10772759)
		#region ConeCalculations

		public static bool IsLyingInCone(Vector2 position, Vector2 apexPoint, Vector2 circleCenter, double aperture) {
			var halfAperture = aperture / 2;
			var apexToXVect = apexPoint - position;
			var axisVect = apexPoint - circleCenter;
			var isInInfiniteCone = DotProd(apexToXVect, axisVect) / Magn(apexToXVect) / Magn(axisVect) >
			Math.Cos(halfAperture);

			if (!isInInfiniteCone)
				return false;
			var isUnderRoundCap = DotProd(apexToXVect, axisVect) / Magn(axisVect) < Magn(axisVect);

			return isUnderRoundCap;
		}

		private static float DotProd(Vector2 a, Vector2 b) {
			return a.X * b.X + a.Y * b.Y;
		}

		private static float Magn(Vector2 a) {
			return (float)(Math.Sqrt(a.X * a.X + a.Y * a.Y));
		}

		#endregion

		public static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25) {
			return GetFirstWallPoint(from.To2D(), to.To2D(), step);
		}

		public static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25) {
			var direction = (to - from).Normalized();

			for (float d = 0; d < from.Distance(to); d = d + step)
			{
				var testPoint = from + d * direction;
				var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
				if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
				{
					return from + (d - step) * direction;
				}
			}

			return null;
		}

		public static List<Obj_AI_Base> GetDashObjects(IEnumerable<Obj_AI_Base> predefinedObjectList = null) {
			var objects = predefinedObjectList != null ? predefinedObjectList.ToList() : ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsValidTarget(Program.Player.AttackRange)).ToList();
			var apexPoint = Program.Player.ServerPosition.To2D() + (Program.Player.ServerPosition.To2D() - Game.CursorPos.To2D()).Normalized() * Program.Player.AttackRange;

			return objects.Where(o => IsLyingInCone(o.ServerPosition.To2D(), apexPoint, Program.Player.ServerPosition.To2D(), Math.PI)).OrderBy(o => o.Distance(apexPoint, true)).ToList();
		}
	}
}
