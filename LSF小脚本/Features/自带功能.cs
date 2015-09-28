using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX.Direct3D9;
using SharpDX;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LSF小脚本.Interface;


namespace LSF小脚本.Features {

	class 自带功能 {
		private Menu Config;
		public 自带功能() {
			Config = new Menu("自带功能","自带功能");
			Config.Add(new MenuSeparator("1","以下功能都是L#自带"));
			Config.Add(new MenuBool("无限视距","无限视距"));
			Config.Add(new MenuBool("塔防范围", "显示敌方塔防范围"));
			Config.Add(new MenuBool("调试窗口", "调试窗口"));
			Program.Config.Add(Config);

			Config["无限视距"].GetValue<MenuBool>().Value = Hacks.ZoomHack;
			Config["塔防范围"].GetValue<MenuBool>().Value = Hacks.TowerRanges;
			Config["调试窗口"].GetValue<MenuBool>().Value = Hacks.Console;

			Config["无限视距"].GetValue<MenuBool>().ValueChanged += 无限视距_ValueChanged;
			Config["塔防范围"].GetValue<MenuBool>().ValueChanged += 塔防范围_ValueChanged;
			Config["调试窗口"].GetValue<MenuBool>().ValueChanged += 调度窗口_ValueChanged;
        }

		private void 调度窗口_ValueChanged(object sender, EventArgs e) {
			Hacks.Console = Config["调试窗口"].GetValue<MenuBool>().Value ? true : false;
		}

		private void 塔防范围_ValueChanged(object sender, EventArgs e) {
			Hacks.TowerRanges = Config["塔防范围"].GetValue<MenuBool>().Value ? true : false;
		}

		private void 无限视距_ValueChanged(object sender, EventArgs e) {
			Hacks.ZoomHack = Config["无限视距"].GetValue<MenuBool>().Value ? true : false;
        }
	}
}
