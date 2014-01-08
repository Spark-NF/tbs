using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TBS
{
	class Unit : Drawable
	{
		public enum MoveType
		{
			Infantry,
			Bazooka,
			TireA,
			TireB,
			Tank,
			Transport,
			Ship,
			Air
		}
		public enum UnitType
		{
			Infantry,
			Vehicle,
			Ship,
			Sub,
			Helicopter,
			Air
		}
		public enum Weapon
		{
			None,
			MachineGun,
			Bazooka,
			VulcanCannon,
			Cannon,
			Rockets,
			TankGun,
			HeavyTankGun,
			MegaGun,
			AntiShipMissiles,
			Torpedo,
			AntiAirGun,
			AntiAirMissiles,
			AirToGroundMissile,
			Bomb
		}

		public string Type { get; private set; }
		public int MovingDistance { get; private set; }
		public int VisionDistance { get; private set; }
		public Vector2 Position { get; private set; }
		public Player Player { get; private set; }
		public bool Moved { get; set; }
		public bool CanCapture { get; private set; }
		public int Price { get; private set; }
		public int Ammo { get; private set; }
		public int RangeMin { get; private set; }
		public int RangeMax { get; private set; }
		public int Gas { get; private set; }
		public int Life { get; set; }
		public MoveType MovementType { get; private set; }
		public UnitType UType { get; private set; }
		public Weapon MainWeapon { get; private set; }
		public Weapon SecondaryWeapon { get; private set; }
		public Building Capturing { get; private set; }
		public string Building { get; private set; }

		public Unit(string type, Player player, Vector2 position, int movingDistance,
					int visionDistance, bool canCapture, int price, int ammo, int rangeMin,
					int rangeMax, int gas, MoveType moveType, UnitType unitType, Weapon weapon1,
					Weapon weapon2, string building)
		{
			Type = type;
			MovingDistance = movingDistance;
			VisionDistance = visionDistance;
			Position = position;
			Player = player;
			Player.Units.Add(this);
			Moved = false;
			CanCapture = canCapture;
			Price = price;
			Ammo = ammo;
			RangeMin = rangeMin;
			RangeMax = rangeMax;
			Gas = gas;
			Life = 100;
			MovementType = moveType;
			UType = unitType;
			MainWeapon = weapon1;
			SecondaryWeapon = weapon2;
			Capturing = null;
			Building = building;
		}

		public void Move(Vector2 position)
		{
			if (Moved)
				return;
			if (Capturing != null && position != Position)
				Capturing.StopCapture();
			Position = position;
			Moved = true;
		}

		public bool CanMove(Vector2 position, Terrain[,] terrain, List<Unit> allUnits)
		{
			return !(position.X < 0 || position.Y < 0
			    || position.X >= terrain.GetLength(1) || position.Y >= terrain.GetLength(0)
				|| allUnits.Any(u => u.Position == position
				|| terrain[(int)position.Y, (int)position.X].MoveCosts[MovementType] < 0));
		}

		public void Wait()
		{
			if (Moved)
				return;
			if (Capturing != null)
				Capturing.StopCapture();
			Moved = true;
		}

		public bool CanMoveAndAttack()
		{
			return RangeMax == RangeMin && RangeMax == 1;
		}

		public int WeightFromType(Terrain terrain)
		{
			return Math.Min(terrain.MoveCosts[MovementType], MovingDistance);
		}

		public int Damage(Unit other, Terrain[,] map)
		{
			if (MainWeapon != Weapon.None && Ammo > 0 || SecondaryWeapon != Weapon.None)
			{
				var dmg = UnitCreator.Damage(this, other);
				int basedamage;
				if (MainWeapon != Weapon.None && Ammo > 0 && dmg.Item1 > 0)
					basedamage = dmg.Item1;
				else
					basedamage = dmg.Item2;
				if (basedamage > 0)
					return (int)(basedamage
								 * (Math.Ceiling((double)Life / 10f) / 10f)
								 * ((100f + 0f) / (100f + 0f)));
			}
			return 0;
		}

		public void Attack(Unit other, Terrain[,] map, bool counter = true)
		{
			// If the enemy is not in range or we're dead, we don't attack
			if (!InRange(other) || Life <= 0)
				return;

			// We only attack if there is ammo left of if we have a secondary weapon (which has infinite ammo)
			if (MainWeapon != Weapon.None && Ammo > 0 || SecondaryWeapon != Weapon.None)
			{
				var dmg = UnitCreator.Damage(this, other);
				int basedamage;
				if (MainWeapon != Weapon.None && Ammo > 0 && dmg.Item1 > 0)
				{
					Ammo--;
					basedamage = dmg.Item1;
				}
				else
					basedamage = dmg.Item2;
				if (basedamage > 0)
				{
					var hit = (int)(basedamage
									* (Math.Ceiling((double)Life / 10f) / 10f)
									* ((100f + 0f) / (100f + 0f)));
					other.Life -= hit;
					if (other.Life <= 0)
						other.Die();
				}
			}

			// Ennemy's counter-attack (if it's not already one)
			if (counter)
				other.Attack(this, map, false);
		}

		public void Die()
		{
			Player.Units.Remove(this);
			if (Capturing != null)
				Capturing.StopCapture();
		}

		public void Capture(Building target)
		{
			target.Capture(this);
			Capturing = target.Player != Player ? target : null;
			Moved = true;
		}

		public void Heal(int number = 20)
		{
			Life = Math.Min(100, Life + number);
		}

		/// <summary>
		/// Checks if an unit is in immediate range, meaning that no move is required to attack.
		/// </summary>
		/// <param name="other">Unit to check whether in direct range or not.</param>
		/// <returns>Whether the other unit is in direct range or not.</returns>
		public bool InRange(Unit other)
		{
			var diff = (int)Math.Abs(Position.Y - other.Position.Y)
					   + (int)Math.Abs(Position.X - other.Position.X);
			return diff >= RangeMin && diff <= RangeMax;
		}

		/// <summary>
		/// Checks if an unit is in range, taking in account possible displacements of the unit.
		/// </summary>
		/// <param name="other">Unit to check whether in range or not.</param>
		/// <param name="terrain">The map.</param>
		/// <param name="units">All units present on the map.</param>
		/// <returns>The closest point to attack the ennemy on success, (-1,-1) if out of range.</returns>
		public Vector2? InExtandedRange(Unit other, Terrain[,] terrain, List<Unit> units)
		{
			// Where we want to check
			var objX = (int)other.Position.X;
			var objY = (int)other.Position.Y;

			// We check every point the unit could move to
			var pf = new AStar(terrain, units, this);
			for (var y = (int)Math.Max(Position.Y - MovingDistance, 0); y <= (int)Math.Min(Position.Y + MovingDistance, terrain.GetLength(0) - 1); ++y)
				for (var x = (int)Math.Max(Position.X - MovingDistance, 0); x <= (int)Math.Min(Position.X + MovingDistance, terrain.GetLength(1) - 1); ++x)
				{
					// If there is a way to get there
					var nodes = pf.FindPath(new Point((int)Position.X, (int)Position.Y), new Point(x, y));
					if ((nodes == null || !nodes.Any()
						 || !(nodes.Last().DistanceTraveled <= MovingDistance)
						 || !CanMoveAndAttack()) && (y != (int)Position.Y || x != (int)Position.X))
						continue;
					if (nodes != null && nodes.Any() && (nodes.Last().Occupied || nodes.Last().Friendly && nodes.Last().Unit == 2))
						continue;

					// Check every point in range
					var ymin = Math.Max(y - RangeMax, 0);
					var ymax = Math.Min(y + RangeMax, terrain.GetLength(0) - 1);
					var xmin = Math.Max(x - RangeMax, 0);
					var xmax = Math.Min(x + RangeMax, terrain.GetLength(1) - 1);
					for (var iy = ymin; iy <= ymax; ++iy)
						for (var ix = xmin; ix <= xmax; ++ix)
						{
							// If the ennemy is in range
							var diff = Math.Abs(y - iy) + Math.Abs(x - ix);
							if (diff >= RangeMin && diff <= RangeMax && ix == objX && iy == objY)
								return new Vector2(x, y);
						}
				}

			// The ennemy is not in range
			return null;
		}
	}
}
