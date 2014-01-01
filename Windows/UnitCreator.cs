using System;
using Microsoft.Xna.Framework;

namespace TBS
{
	class UnitCreator
	{
		public static Unit Unit(string type, Player player, Vector2 position)
		{
			if (type == "Infantry")
				return new Unit(type, 3, 2, player, position, true, 1500, -1, 1, 99, TBS.Unit.MoveType.Infantry, TBS.Unit.UnitType.Infantry, TBS.Unit.Weapon.None, TBS.Unit.Weapon.MachineGun);
			if (type == "Mech")
				return new Unit(type, 2, 2, player, position, true, 2500, 3, 1, 70, TBS.Unit.MoveType.Infantry, TBS.Unit.UnitType.Infantry, TBS.Unit.Weapon.Bazooka, TBS.Unit.Weapon.MachineGun);
			if (type == "Bike")
				return new Unit(type, 5, 2, player, position, true, 2500, -1, 1, 70, TBS.Unit.MoveType.TireB, TBS.Unit.UnitType.Infantry, TBS.Unit.Weapon.None, TBS.Unit.Weapon.MachineGun);
			if (type == "Battle Copter")
				return new Unit(type, 6, 2, player, position, false, 9000, 6, 1, 99, TBS.Unit.MoveType.Air, TBS.Unit.UnitType.Helicopter, TBS.Unit.Weapon.AirToGroundMissile, TBS.Unit.Weapon.MachineGun);
			throw new Exception("Invalid unit type.");
		}
	}
}
