using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace TBS
{
	class UnitCreator
	{
		private static readonly Dictionary<string, XElement> Units = new Dictionary<string, XElement>();

		public static void Initialize()
		{
			var doc = XDocument.Load("Content/Units.xml");
			var root = doc.Element("Units");
			if (root == null)
				throw new Exception("Error in units XML file. No <Units> root.");
			var units = root.Elements();
			foreach (var u in units)
				Units.Add(u.Attribute("Name").Value, u);
		}

		public static Unit Unit(string type, Player player, Vector2 position)
		{
			if (!Units.ContainsKey(type))
				throw new Exception("Invalid unit type '" + type + "'. Available types: '" + String.Join("', '", Units.Keys) + "'.");

			var u = Units[type];
			return new Unit(
				type,
				player,
				position,
				Convert.ToInt32(u.Attribute("MovementDistance").Value),
				Convert.ToInt32(u.Attribute("Vision").Value),
				Convert.ToBoolean(u.Attribute("CanCapture").Value),
				Convert.ToInt32(u.Attribute("Price").Value),
				Convert.ToInt32(u.Attribute("Ammo").Value),
				Convert.ToInt32(u.Attribute("AttackRangeMin").Value),
				Convert.ToInt32(u.Attribute("AttackRangeMax").Value),
				Convert.ToInt32(u.Attribute("Gas").Value),
				(Unit.MoveType)Enum.Parse(typeof(Unit.MoveType), u.Attribute("MovementType").Value, true),
				(Unit.UnitType)Enum.Parse(typeof(Unit.UnitType), u.Attribute("UnitType").Value, true),
				u.Attribute("MainWeapon").Value == ""
					? TBS.Unit.Weapon.None
					: (Unit.Weapon)Enum.Parse(typeof(Unit.Weapon), u.Attribute("MainWeapon").Value, true),
				u.Attribute("SecondaryWeapon").Value == ""
					? TBS.Unit.Weapon.None
					: (Unit.Weapon)Enum.Parse(typeof(Unit.Weapon), u.Attribute("SecondaryWeapon").Value, true));
		}
	}
}
