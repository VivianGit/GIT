﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jhin_As_The_Virtuoso {
	class Program {
		static void Main(string[] args) {
			LeagueSharp.Common.CustomEvents.Game.OnGameLoad += Jhin.OnLoad;
		}
	}
}
