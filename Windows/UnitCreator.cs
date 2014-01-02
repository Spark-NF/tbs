using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace TBS
{
	class UnitCreator
	{
		private static readonly Dictionary<string, XElement> Units = new Dictionary<string, XElement>();
		private static readonly Dictionary<Tuple<string, string>, int> DamageChart = new Dictionary<Tuple<string, string>, int>();

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
			DamageChart.Add(new Tuple<string, string>("Infantry", "Infantry"), 55);
			DamageChart.Add(new Tuple<string, string>("Infantry", "Mech"), 45);
			DamageChart.Add(new Tuple<string, string>("Infantry", "Bike"), 45);
			DamageChart.Add(new Tuple<string, string>("Infantry", "Artillery"), 10);
			DamageChart.Add(new Tuple<string, string>("Infantry", "Battle Copter"), 8);
			DamageChart.Add(new Tuple<string, string>("Mech", "Infantry"), 65);
			DamageChart.Add(new Tuple<string, string>("Mech", "Mech"), 55);
			DamageChart.Add(new Tuple<string, string>("Mech", "Bike"), 55);
			DamageChart.Add(new Tuple<string, string>("Mech", "Artillery"), 70);
			DamageChart.Add(new Tuple<string, string>("Mech", "Battle Copter"), 12);
			DamageChart.Add(new Tuple<string, string>("Bike", "Infantry"), 65);
			DamageChart.Add(new Tuple<string, string>("Bike", "Mech"), 55);
			DamageChart.Add(new Tuple<string, string>("Bike", "Bike"), 55);
			DamageChart.Add(new Tuple<string, string>("Bike", "Artillery"), 15);
			DamageChart.Add(new Tuple<string, string>("Bike", "Battle Copter"), 12);
			DamageChart.Add(new Tuple<string, string>("Artillery", "Infantry"), 90);
			DamageChart.Add(new Tuple<string, string>("Artillery", "Mech"), 85);
			DamageChart.Add(new Tuple<string, string>("Artillery", "Bike"), 85);
			DamageChart.Add(new Tuple<string, string>("Artillery", "Artillery"), 75);
			DamageChart.Add(new Tuple<string, string>("Artillery", "Battle Copter"), -1);
			DamageChart.Add(new Tuple<string, string>("Battle Copter", "Infantry"), 75);
			DamageChart.Add(new Tuple<string, string>("Battle Copter", "Mech"), 65);
			DamageChart.Add(new Tuple<string, string>("Battle Copter", "Bike"), 65);
			DamageChart.Add(new Tuple<string, string>("Battle Copter", "Artillery"), 64);
			DamageChart.Add(new Tuple<string, string>("Battle Copter", "Battle Copter"), 65);
		}

		public static Dictionary<string, int> GetPrices(string building)
		{
			return Units
				.Where(u => u.Value.Attribute("Building").Value == building)
				.ToDictionary(
					u => u.Key,
					u => Convert.ToInt32(u.Value.Attribute("Price").Value));
		}

		public static int Damage(Unit u1, Unit u2)
		{
			return DamageChart[new Tuple<string, string>(u1.Type, u2.Type)];
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
				player.Money -= price;

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
