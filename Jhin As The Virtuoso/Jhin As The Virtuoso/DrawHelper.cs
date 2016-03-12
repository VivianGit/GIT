using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace Jhin_As_The_Virtuoso {
	class DrawHelper {
		public static IntPtr DesktopHandle { get; private set; }
		public static Graphics graphics { get; private set; }
		public static SolidBrush myBrush { get; private set; }

		[DllImport("User32.dll")]
		public extern static IntPtr GetDC(IntPtr hWnd);

		public static void Load() {
			DesktopHandle = GetDC(IntPtr.Zero);
			graphics = Graphics.FromHdc(DesktopHandle);
			myBrush = new SolidBrush(Color.Red);
		}

		public static void DrawFillCircle(Obj_AI_Base target,Color color) {
			myBrush.Color = color;
			var ScreenPosition = new SharpDX.Vector2();
			Drawing.WorldToScreen(target.Position,out ScreenPosition);
			graphics.FillEllipse(myBrush, ScreenPosition.X, ScreenPosition.Y, target.BoundingRadius, target.BoundingRadius);
		}

		public static void DrawFillCircle(float x, float y, float w = 60, float h = 60) {
			graphics.FillEllipse(myBrush,x,y,w,h);
		}
	}
}
