using System.Collections.Generic;

namespace TBS
{
	class Player
	{
		public int Number { get; private set; }
		public bool IsAI { get; private set; }
		public int Money { get; private set; }
		public List<Building> Buildings { get; private set; }
		public List<Unit> Units { get; private set; }

		public Player(int number, bool isAI, int money = 3000)
		{
			Number = number;
			IsAI = isAI;
			Money = money;
			Buildings = new List<Building>();
			Units = new List<Unit>();
		}

		public void NextTurn()
		{
			Money += Buildings.Count * 1000;
			foreach (var t in Units)
				t.Moved = false;
		}
	}
}
