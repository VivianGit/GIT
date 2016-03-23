﻿using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace CNLib
{
    public static class MenuExtensions
	{

		public static void ToCN(this Menu config,bool IsOrbwalker = false) {
			if (!MultiLanguage.IsCN)
			{
				return;
			}

			var language = IsOrbwalker ? "Orbwalker" : "Chinese";

			foreach (var menu in config.Children)
			{
				if (!string.IsNullOrEmpty(menu.DisplayName))
				{
					menu.DisplayName = menu.DisplayName.TranslatTo(language);
				}
				

				foreach (var item in menu.Items)
				{
					if (!string.IsNullOrEmpty(item.DisplayName))
					{
						item.DisplayName = item.DisplayName.TranslatTo(language);
					}
						
				}
			}

			foreach (var item in config.Items)
			{
				if (!string.IsNullOrEmpty(item.DisplayName))
				{
					item.DisplayName = item.DisplayName.TranslatTo(language);
				}

			}
		}

		public static Orbwalking.Orbwalker AddOrbwalker(this Menu config, string name, string displayName) {
			var OrbMenu = config.AddMenu("走砍设置", "走砍设置");
			var Orbwalker = new Orbwalking.Orbwalker(OrbMenu);
			OrbMenu.ToCN();
			return Orbwalker;
		}

		#region 菜单类方法
		public static int ItemIndex { get; private set; } = 0;


		public static Menu CreatMainMenu(string name, string displayName) {
			var config = new Menu(MultiLanguage._(displayName), name, true);
			config.AddToMainMenu();

			var Menu = config.AddMenu("Credits", "脚本信息");
			Menu.AddLabel($"LSFU - {HeroManager.Player.ChampionName}");
			Menu.AddLabel($"www.lsharp.xyz");
			return config;
		}

		/// <summary>
		/// 添加子菜单
		/// </summary>
		/// <param name="config"></param>
		/// <param name="name"></param>
		/// <param name="displayName"></param>
		/// <returns></returns>
		public static Menu AddMenu(this Menu config, string name, string displayName) {
			if (config.Children.Any(m => m.Name == name))
			{
				DeBug.Write("创建菜单错误",$"已经包含了名为{name}的子菜单!",DebugLevel.Warning);
			}

			return config.AddSubMenu(new Menu(MultiLanguage._(displayName), name));
		}

		public static MenuItem AddLabel(this Menu config, string name, string displayName) {
			if (config.Items.Any(m => m.Name == name))
			{
				config.Item(name).DisplayName = displayName;
				return config.Item(name);
			}
			return config.AddItem(new MenuItem(name, MultiLanguage._(displayName)));
		}

		public static MenuItem AddLabel(this Menu config, string displayName) {
			ItemIndex++;
			return config.AddItem(new MenuItem(ItemIndex.ToString(), MultiLanguage._(displayName)));
		}

		public static string GetLabel(this Menu config, string name) {
			return config.Item(name).DisplayName;
		}

		public static MenuItem AddSeparator(this Menu config, string name) {
			return config.AddItem(new MenuItem(name, ""));
		}

		public static MenuItem AddSeparator(this Menu config) {
			ItemIndex++;
			return config.AddItem(new MenuItem(ItemIndex.ToString(), ""));
		}

		public static MenuItem AddBool(this Menu config, string name, string displayName, bool Defaults = false) {
			if (config.Items.Any(m => m.Name == name))
			{
				DeBug.Write("创建菜单错误", $"已经包含了名为{name}的菜单!", DebugLevel.Warning);
			}

			return config.AddItem(new MenuItem(name, MultiLanguage._(displayName)).SetValue(Defaults));
		}

		public static bool GetBool(this Menu config, string name) {
			return config.Item(name).GetValue<bool>();
		}

		public static MenuItem AddStringList(this Menu config, string name, string displayName, string[] stringList = null, int Index = 0) {
			if (config.Items.Any(m => m.Name == name))
			{
				DeBug.Write("创建菜单错误", $"已经包含了名为{name}的菜单!", DebugLevel.Warning);
			}
			return config.AddItem(new MenuItem(name, MultiLanguage._(displayName)).SetValue(new StringList(stringList.Select(s => MultiLanguage._(s)).ToArray(), Index)));
		}

		public static StringList GetStringList(this Menu config, string name) {
			return config.Item(name).GetValue<StringList>();
		}

		public static int GetStringIndex(this Menu config, string name) {
			return config.Item(name).GetValue<StringList>().SelectedIndex;
		}

		public static MenuItem AddSlider(this Menu config, string name, string displayName, int Defaults = 0, int min = 0, int max = 100) {
			if (config.Items.Any(m => m.Name == name))
			{
				DeBug.Write("创建菜单错误", $"已经包含了名为{name}的菜单!", DebugLevel.Warning);
			}
			return config.AddItem(new MenuItem(name, MultiLanguage._(displayName)).SetValue(new Slider(Defaults, min, max)));
		}

		public static Slider GetSlider(this Menu config, string name) {
			return config.Item(name).GetValue<Slider>();
		}

		public static int GetSliderValue(this Menu config, string name) {
			if (config.Items.All(c=>c.Name!= name))
			{
				return 0;
			}
			return config.Item(name).GetValue<Slider>().Value;
		}

		public static MenuItem AddCircle(this Menu config, string name, string displayName, bool active = false, Color color = new Color()) {
			if (config.Items.Any(m => m.Name == name))
			{
				DeBug.Write("创建菜单错误", $"已经包含了名为{name}的菜单!", DebugLevel.Warning);
			}
			return config.AddItem(new MenuItem(name, MultiLanguage._(displayName)).SetValue(new Circle(active, color)));
		}

		public static Circle GetCircle(this Menu config, string name) {
			return config.Item(name).GetValue<Circle>();
		}

		public static bool GetCircleActive(this Menu config, string name) {
			return config.Item(name).GetValue<Circle>().Active;
		}

		public static Color GetCircleColor(this Menu config, string name) {
			return config.Item(name).GetValue<Circle>().Color;
		}

		public static MenuItem AddKeyBind(this Menu config, string name, string displayName,uint key, KeyBindType type,bool active = false) {
			if (config.Items.Any(m => m.Name == name))
			{
				DeBug.Write("创建菜单错误", $"已经包含了名为{name}的菜单!", DebugLevel.Warning);
			}
			return config.AddItem(new MenuItem(name, MultiLanguage._(displayName)).SetValue(new KeyBind(key, type, active)));
		}

		public static bool GetKeyActive(this Menu config, string name) {
			return config.Item(name).GetValue<KeyBind>().Active;
		}

		public static KeyBind GetKeyBind(this Menu config, string name) {
			return config.Item(name).GetValue<KeyBind>();
		}
		#endregion

	}
}
