﻿using System;
using System.Collections.Generic;

namespace TBS
{
	class Player
	{
		public int Number { get; private set; }
		public bool IsAI { get; private set; }
		public int Version { get; private set; }
		public int Money { get; set; }
		public List<Building> Buildings { get; private set; }
		public List<Unit> Units { get; private set; }

		public Player(int number, bool isAI, int version, int money = 0)
		{
			Number = number;
			IsAI = isAI;
			Version = version;
			Money = money;
			Buildings = new List<Building>();
			Units = new List<Unit>();
		}

		public void NextTurn()
		{
			Money += Math.Min(Buildings.Count * 1000, 99000);
			foreach (var t in Units)
				t.Moved = false;
		}
	}
}
