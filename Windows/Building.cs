namespace TBS
{
	class Building
	{
		public int Type { get; set; }
		private Player _player;
		public Player Player
		{
			get { return _player; }
			set
			{
				if (_player != null)
					_player.Buildings.Remove(this);
				_player = value;
				if (value != null)
					value.Buildings.Add(this);
			}
		}

		public Building(int type, Player player)
		{
			Type = type;
			_player = player;
			if (_player != null)
				_player.Buildings.Add(this);
		}
	}
}
