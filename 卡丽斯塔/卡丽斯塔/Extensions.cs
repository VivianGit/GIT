using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace Aessmbly {
	public static class Extensions {

		//转换为对话框用
		public static string ToUTF8(this string form) {
			var bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(form));
			return Encoding.Default.GetString(bytes);
		}
		//转换为菜单用
		public static string ToGBK(this string form) {
			var bytes = Encoding.Convert(Encoding.UTF8, Encoding.Default, Encoding.Default.GetBytes(form));
			return Encoding.Default.GetString(bytes);
		}


		public static string ToHtml(this string form, Color color, FontStlye fontStlye = FontStlye.Null) {
			string colorhx = "#"+ color.ToArgb().ToString("X6");
            return form.ToHtml(colorhx, fontStlye);
		}

		public static string ToHtml(this string form, string color, FontStlye fontStlye = FontStlye.Null) {
			form = form.ToUTF8();
			form = string.Format("<font color=\"{0}\">{1}</font>", color, form);

			if (fontStlye != FontStlye.Null)
			{
				switch (fontStlye)
				{
					case FontStlye.Bold:
						form = string.Format("<b>{0}</b>", form);
						break;
					case FontStlye.Cite:
						form = string.Format("<i>{0}</i>", form);
						break;
				}
			}
			return form;
		}

		public static bool InBase(this Obj_AI_Hero hero) {
			foreach (var item in ObjectManager.Get<Obj_Shop>())
			{
				if (hero.Distance(item)<5000)
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasRendBuff(this Obj_AI_Base target) {
			return target.GetRendBuff() != null;
		}

		public static BuffInstance GetRendBuff(this Obj_AI_Base target) {
			return target.Buffs.FirstOrDefault(b => b.Caster.IsMe && b.IsValid &&b.DisplayName== "KalistaExpungeMarker");
		}

		public static bool HasUndyingBuff(this Obj_AI_Hero target) {
			if (target.Buffs.Any(
				b => b.IsValid &&
					 (b.DisplayName == "Chrono Shift" ||
					  b.DisplayName == "JudicatorIntervention" ||
					  b.DisplayName == "Undying Rage" )))
			{
				return true;
			}

			if (target.ChampionName == "Poppy")
			{
				if (HeroManager.Allies.Any(o => !o.IsMe && o.Buffs.Any(b => b.Caster.NetworkId == target.NetworkId && b.IsValid && b.DisplayName == "PoppyDITarget")))
				{
					return true;
				}
			}

			return target.IsInvulnerable;
		}

		public static bool HasSpellShield(this Obj_AI_Hero target) {
			return target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
		}

		public static List<T> MakeUnique<T>(this List<T> list) where T : Obj_AI_Base, new() {
			var uniqueList = new List<T>();

			foreach (var entry in list.Where(entry => uniqueList.All(e => e.NetworkId != entry.NetworkId)))
			{
				uniqueList.Add(entry);
			}

			list.Clear();
			list.AddRange(uniqueList);

			return list;
		}

		public static float TotalShieldHealth(this Obj_AI_Base target) {
			return target.Health + target.AllShield + target.PhysicalShield + target.MagicalShield;
		}
	}

}
