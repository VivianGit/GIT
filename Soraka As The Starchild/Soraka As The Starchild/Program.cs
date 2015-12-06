using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace Soraka_As_The_Starchild {
	class Program {
		static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += Soraka.Load;
		}
	}
}
