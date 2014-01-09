using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TBS
{
	class PlayerAI : Player
	{
		public PlayerAI(int number, int version, int money = 0)
			: base(number, version, money)
		{
			IsAI = true;
		}

		public void Think(Terrain[,] terrain, Building[,] allBuildings, List<Unit> allUnits)
		{
			foreach (var b in Buildings.Where(n => n.Type != "City" && n.Type != "Headquarter"))
			{
				
			}

			for (var i = 0; i < Units.Count; ++i)
			{
				var u = Units[i];

				if (u.CanCapture)
				{
					// Continue capturing if already
					if (u.Capturing != null)
					{
						u.Capture(u.Capturing);
						continue;
					}

					// Search for capturable buildings in range
					Building building = null;
					Vector2? bPos = null;
					for (var y = 0; y < allBuildings.GetLength(0); ++y)
						for (var x = 0; x < allBuildings.GetLength(1); ++x)
							if (allBuildings[y, x] != null
								&& allBuildings[y, x].Player != this
								&& u.CanMove(new Vector2(x, y), terrain, allUnits))
							{
								var pf = new AStar(terrain, allUnits, u);
								var nodes = pf.FindPath(new Point((int)u.Position.X, (int)u.Position.Y), new Point(x, y));
								if (nodes != null && nodes.Any() && nodes.Last().DistanceTraveled <= u.MovingDistance)
								{
									bPos = new Vector2(x, y);
									building = allBuildings[y, x];
								}
							}
					if (building != null)
					{
						u.Move(bPos.Value);
						u.Capture(building);
						continue;
					}
				}

				// Ennemies in range
				Unit foe = null;
				Vector2? go = null;
				var damage = 0;
				foreach (var v in allUnits.Where(v => v.Player != this))
				{
					var d = u.Damage(v, terrain);
					if (u.InRange(v) && d > damage && d >= 10)
					{
						foe = v;
						go = null;
						damage = d;
					}
					else
					{
						var ext = u.InExtandedRange(v, terrain, allUnits);
						if (ext.HasValue && u.CanMoveAndAttack()
							&& u.CanMove(ext.Value, terrain, allUnits)
							&& d > damage && d >= 10)
						{
							foe = v;
							go = ext;
							damage = d;
						}
					}
				}

				// Attack weakest ennemy in range
				if (foe != null)
				{
					if (go.HasValue)
						u.Move(go.Value);
					u.Attack(foe, terrain);
					if (foe.Life <= 0)
						allUnits.Remove(foe);
					if (u.Life <= 0)
					{
						allUnits.Remove(u);
						--i;
					}
					continue;
				}

				// Heal itself if possible and necessary
				if (u.Life <= 90)
				{
					// If already on a friendly building, don't move
					var under = allBuildings[(int)u.Position.Y, (int)u.Position.X];
					if (under != null && under.Player == this)
					{
						u.Wait();
						continue;
					}

					// Search for ally building in range
					Vector2? hPos = null;
					for (var y = 0; y < allBuildings.GetLength(0); ++y)
						for (var x = 0; x < allBuildings.GetLength(1); ++x)
							if (allBuildings[y, x] != null
								&& allBuildings[y, x].Player == this
								&& u.CanMove(new Vector2(x, y), terrain, allUnits))
							{
								var pf = new AStar(terrain, allUnits, u);
								var nodes = pf.FindPath(new Point((int)u.Position.X, (int)u.Position.Y), new Point(x, y));
								if (nodes != null && nodes.Any() && nodes.Last().DistanceTraveled <= u.MovingDistance)
									hPos = new Vector2(x, y);
							}
					if (hPos.HasValue)
					{
						u.Move(hPos.Value);
						continue;
					}
				}

				// Move closer to ennemy base if possible
				Building ennemyBase = null;
				Point? ennemyBasePos = null;
				for (var y = 0; y < allBuildings.GetLength(0); ++y)
					for (var x = 0; x < allBuildings.GetLength(1); ++x)
						if (allBuildings[y, x] != null
							&& allBuildings[y, x].Player != this
							&& allBuildings[y, x].Type == "Headquarter")
						{
							ennemyBasePos = new Point(x, y);
							ennemyBase = allBuildings[y, x];
							break;
						}

				// If the ennemy hasn't been beaten yet
				if (ennemyBase != null)
				{
					var dist = Math.Abs(u.Position.X - ennemyBasePos.Value.X) + Math.Abs(u.Position.Y - ennemyBasePos.Value.Y);

					// Move closer but not on the base itself
					if (ennemyBase != null && dist > 1)
					{
						var pf = new AStar(terrain, allUnits, u);
						var nodes = pf.FindPath(
							new Point((int)u.Position.X, (int)u.Position.Y),
							ennemyBasePos.Value,
							0, true, true);
						while (nodes != null && nodes.Any()
							   && (nodes.Last().DistanceTraveled > u.MovingDistance
								   || !u.CanMove(new Vector2(nodes.Last().Position.X, nodes.Last().Position.Y), terrain, allUnits)
								   || Math.Abs(nodes.Last().Position.X - ennemyBasePos.Value.X)
								   + Math.Abs(nodes.Last().Position.Y - ennemyBasePos.Value.Y) < 0.1))
						{
							nodes.RemoveAt(nodes.Count - 1);
						}
						if (nodes != null && nodes.Any())
						{
							u.Move(new Vector2(nodes.Last().Position.X, nodes.Last().Position.Y));
							continue;
						}
					}
					else if (ennemyBase != null && dist < 0.1)
					{
						var pf = new AStar(terrain, allUnits, u);
						var nodes = pf.FindIntermediatePath(
							new Point((int)u.Position.X, (int)u.Position.Y),
							new Point((int)Headquarter.Position.X, (int)Headquarter.Position.Y),
							u.MovingDistance);
						while (nodes != null && nodes.Any()
							&& nodes.First().DistanceTraveled <= u.MovingDistance
							&& !u.CanMove(new Vector2(nodes.First().Position.X, nodes.First().Position.Y), terrain, allUnits))
						{
							nodes.RemoveAt(0);
						}
						if (nodes != null && nodes.Any() && nodes.First().DistanceTraveled <= u.MovingDistance
							&& u.CanMove(new Vector2(nodes.First().Position.X, nodes.First().Position.Y), terrain, allUnits))
						{
							u.Move(new Vector2(nodes.First().Position.X, nodes.First().Position.Y));
							continue;
						}
					}
				}

				// In other cases do nothing
				u.Wait();
			}
		}
	}
}
