using System;

namespace TBS
{
	class Building
	{
		public int Type { get; set; }
		private int _previousCaptureStatus;
		public int CaptureStatus { get; private set; }
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
			_previousCaptureStatus = 20;
			CaptureStatus = 20;
		}

		public void Capture(Unit capturing)
		{
			_previousCaptureStatus = CaptureStatus;
			CaptureStatus -= (int)Math.Ceiling((double)capturing.Life / 10);

			if (CaptureStatus > 0)
				return;

			Player = capturing.Player;
			_previousCaptureStatus = 20;
			CaptureStatus = 20;
		}
		public void StopCapture()
		{
			_previousCaptureStatus = 20;
			CaptureStatus = 20;
		}

		public void NextTurn()
		{
			if (CaptureStatus != _previousCaptureStatus)
				return;

			_previousCaptureStatus = 20;
			CaptureStatus = 20;
		}
	}
}
