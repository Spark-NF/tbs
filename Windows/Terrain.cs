﻿using System.Collections.Generic;

namespace TBS
{
	class Terrain
	{
		public string Type { get; private set; }
		public int Defense { get; private set; }
		public Sprite Texture { get; set; }
		public Dictionary<Unit.MoveType, int> MoveCosts { get; private set; }

		public Terrain(string type, int defense, params int[] costs)
		{
			Type = type;
			Defense = defense;
			MoveCosts = new Dictionary<Unit.MoveType, int>
			{
				{ Unit.MoveType.Infantry, costs[0] },
				{ Unit.MoveType.Bazooka, costs[1] },
				{ Unit.MoveType.TireA, costs[2] },
				{ Unit.MoveType.TireB, costs[3] },
				{ Unit.MoveType.Tank, costs[4] },
				{ Unit.MoveType.Air, costs[5] },
				{ Unit.MoveType.Ship, costs[6] },
				{ Unit.MoveType.Transport, costs[7] }
			};
		}
	}
}
