using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

using ssTuple = System.Tuple<string, string>;
using iiTuple = System.Tuple<int, int>;

namespace TBS
{
	class UnitCreator
	{
		private static readonly Dictionary<string, XElement> Units = new Dictionary<string, XElement>();
		private static readonly Dictionary<ssTuple, iiTuple> DamageChart = new Dictionary<ssTuple, iiTuple>();

		public static void Initialize()
		{
			var doc = XDocument.Load("Content/Units.xml");
			var root = doc.Element("Units");
			if (root == null)
				throw new Exception("Error in units XML file. No <Units> root.");

			var units = root.Elements();
			Units.Clear();
			foreach (var u in units)
				Units.Add(u.Attribute("Name").Value, u);

			DamageChart.Clear();
			var info = new[] { "Infantry", "Mech", "Bike", "Recon", "Flare", "Anti-Air", "Tank", "Medium Tank", "War Tank", "Artillery", "Anti-Tank", "Rockets", "Missiles", "Rig", "Fighter", "Bomber", "Seaplane", "Duster", "Battle Copter", "Transport Copter", "Gunboat", "Cruiser", "Submarine", "Carrier", "Battleship", "Lander" };
			var data = new[]
			{
				new[] {   0,55, 0,45, 0,45, 0,12, 0,10, 0, 3, 0, 5, 0, 5, 0, 1, 0,10, 0,30, 0,20, 0,20, 0,14,-1,-1,-1,-1,-1,-1,-1,-1, 0, 8, 0,30,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {   0,65, 0,55, 0,55,85,18,80,15,55, 5,55, 8,25, 5,15, 1,70,15,55,35,85,35,85,35,75,20,-1,-1,-1,-1,-1,-1,-1,-1, 0,12, 0,35,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {   0,65, 0,55, 0,55, 0,18, 0,15, 0, 5, 0, 8, 0, 5, 0, 1, 0,15, 0,35, 0,35, 0,35, 0,20,-1,-1,-1,-1,-1,-1,-1,-1, 0,12, 0,35,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {   0,75, 0,65, 0,65, 0,35, 0,30, 0, 8, 0, 8, 0, 5, 0, 1, 0,45, 0,25, 0,55, 0,55, 0,45,-1,-1,-1,-1,-1,-1,-1,-1, 0,18, 0,35,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {   0,80, 0,70, 0,70, 0,60, 0,50, 0,45, 0,10, 0, 5, 0, 1, 0,45, 0,25, 0,55, 0,55, 0,45,-1,-1,-1,-1,-1,-1,-1,-1, 0,18, 0,35,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {  105,0,105,0,105,0,60, 0,50, 0,45, 0,15, 0,10, 0, 5, 0,50, 0,25, 0,55, 0,55, 0,50, 0,70,-1,70,-1,75,-1,75,-1,105,0,120,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {   0,75, 0,70, 0,70,85,40,80,35,75, 8,55, 8,35, 5,20, 1,70,15,55,35,85,35,85,35,75,20,-1,-1,-1,-1,-1,-1,-1,-1, 0,18, 0,40,55, 0, 9, 0, 9, 0, 8, 0, 8, 0,18, 0 },
				new[] {   0,90, 0,80, 0,80,95,40,90,35,90, 8,70, 8,55, 5,35, 1,70,15,55,35,85,35,85,35,75,20,-1,-1,-1,-1,-1,-1,-1,-1, 0,24, 0,40,55, 0,12, 0,12, 0,10, 0,10, 0,22, 0 },
				new[] { 0,105,0,95,0,95,105,45,105,40,105,10,85,10,75,10,55, 1,70,15,55,35,85,35,85,35,75,20,-1,-1,-1,-1,-1,-1,-1,-1, 0,35, 0,45,65, 0,14, 0,14, 0,12, 0,12, 0,28, 0 },
				new[] {  90, 0,85, 0,85, 0,80, 0,75, 0,65, 0,60, 0,45, 0,35, 0,75, 0,55, 0,80, 0,80, 0,70, 0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,100,0,55, 0,55, 0,45, 0,45, 0,65, 0 },
				new[] {  75, 0,65, 0,65, 0,75, 0,75, 0,75, 0,75, 0,65, 0,55, 0,65, 0,55, 0,70, 0,70, 0,65, 0,-1,-1,-1,-1,-1,-1,-1,-1,45, 0,55, 0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {  95, 0,90, 0,90, 0,90, 0,85, 0,75, 0,70, 0,55, 0,45, 0,80, 0,65, 0,85, 0,85, 0,80, 0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,105,0,65, 0,65, 0,55, 0,55, 0,75, 0 },
				new[] {  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,100,0,100,0,100,0,100,0,120,0,120,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,55, 0,65, 0,65, 0,80, 0,120,0,120,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {  115,0,110,0,110,0,105,0,105,0,85, 0,105,0,95, 0,75, 0,105,0,80, 0,105,0,95, 0,105,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,120,0,50, 0,95, 0,85, 0,85, 0,95, 0 },
				new[] {  90, 0,85, 0,85, 0,80, 0,80, 0,45, 0,75, 0,65, 0,55, 0,70, 0,50, 0,80, 0,70, 0,75, 0,45, 0,55, 0,55, 0,65, 0,85, 0,95, 0,105,0,40, 0,55, 0,65, 0,45, 0,85, 0 },
				new[] {  55, 0,45, 0,45, 0,18, 0,15, 0, 5, 0, 8, 0, 5, 0, 1, 0,15, 0, 5, 0,20, 0,20, 0,15, 0,40, 0,45, 0,45, 0,55, 0,75, 0,90, 0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {   0,75, 0,65, 0,65,75,30,75,30,10, 1,70, 8,45, 8,35, 1,65,25,20, 1,75,35,55,25,70,20,-1,-1,-1,-1,-1,-1,-1,-1, 0,65, 0,85,85, 0, 5, 0,25, 0,25, 0,25, 0,25, 0 },
				new[] {  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {  115,0,110,0,110,0,105,0,105,0,85, 0,105,0,95, 0,75, 0,105,0,80, 0,105,0,95, 0,105,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,75, 0,40, 0,40, 0,40, 0,40, 0,55, 0 },
				new[] {  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,105,0,105,0,105,0,105,0,120,0,120,85, 0,28, 0,95, 0,38, 0,38, 0,40, 0 },
				new[] {  115,0,110,0,110,0,105,0,105,0,85, 0,105,0,95, 0,75, 0,105,0,80, 0,105,0,95, 0,105,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,120,0,20, 0,55, 0,110,0,80, 0,85, 0 },
				new[] {  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 0,35, 0,35, 0,40, 0,40, 0,45, 0,55,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
				new[] {  75, 0,70, 0,70, 0,70, 0,70, 0,65, 0,65, 0,50, 0,40, 0,70, 0,55, 0,75, 0,75, 0,65, 0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,95, 0,65, 0,65, 0,50, 0,45, 0,75, 0 },
				new[] {  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 }
			};
			for (var atk = 0; atk < info.Length; ++atk)
				for (var def = 0; def < info.Length; ++def)
					DamageChart.Add(new ssTuple(info[atk], info[def]), new iiTuple(data[atk][def * 2], data[atk][def * 2 + 1]));
		}

		public static Dictionary<string, int> GetPrices(string building)
		{
			return Units
				.Where(u => u.Value.Attribute("Building").Value == building)
				.ToDictionary(
					u => u.Key,
					u => Convert.ToInt32(u.Value.Attribute("Price").Value));
		}

		public static Tuple<int, int> Damage(Unit u1, Unit u2)
		{
			return DamageChart[new ssTuple(u1.Type, u2.Type)];
		}

		public static int Price(string type)
		{
			if (!Units.ContainsKey(type))
				return -1;
			return Convert.ToInt32(Units[type].Attribute("Price").Value);
		}

		public static Unit Unit(string type, Player player, Vector2 position, bool pay = false)
		{
			if (!Units.ContainsKey(type))
				return null;

			var u = Units[type];
			var price = Convert.ToInt32(u.Attribute("Price").Value);

			if (pay)
			{
				if (player.Money < price)
					return null;
				player.Money -= price;
			}

			return new Unit(
				type,
				player,
				position,
				Convert.ToInt32(u.Attribute("MovementDistance").Value),
				Convert.ToInt32(u.Attribute("Vision").Value),
				Convert.ToBoolean(u.Attribute("CanCapture").Value),
				price,
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
					: (Unit.Weapon)Enum.Parse(typeof(Unit.Weapon), u.Attribute("SecondaryWeapon").Value, true),
				u.Attribute("Building").Value);
		}
	}
}
