using System;
using Microsoft.Xna.Framework;

namespace TBS
{
	class Unit
	{
		public int Type { get; private set; }
		public int MovingDistance { get; private set; }
		public int VisionDistance { get; private set; }
		public Vector2 Position { get; private set; }
		public Player Player { get; private set; }
		public bool Moved { get; set; }

		public Unit(int type, int movingDistance, int visionDistance, Player player, Vector2 position)
		{
			Type = type;
			MovingDistance = movingDistance;
			VisionDistance = visionDistance;
			Position = position;
			Player = player;
			Player.Units.Add(this);
			Moved = false;
		}

		public void Move(Vector2 position)
		{
			if (Moved)
				return;
			Position = position;
			Moved = true;
		}

		public int WeightFromType(int type)
		{
			if (Type == 0 && type == 0
				|| Type == 1 && type == 0
				|| Type == 2 && type == 0)
				return -1;
			var ret = -1;
			if (Type == 3 || Type == 4)
				ret = 1;
			else switch (type)
			{
				case 0: ret = 1; break;
				case 1: ret = 1; break;
				case 2: ret = 1; break;
				case 3: ret = 2; break;
				case 4: ret = 3; break;
			}
			return Math.Min(ret, MovingDistance);
		}
	}
}
