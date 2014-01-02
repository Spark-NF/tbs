using System.IO;

namespace TBS.Screens
{
	class GameSelectScreen : MenuScreen
	{
		readonly MenuEntry _mapMenuEntry;

		static string[] _maps;
		static int _currentMap;

		public GameSelectScreen()
			: base("Play")
		{
			_maps = Directory.GetFiles("Content/Maps");
			for (var i = 0; i < _maps.Length; ++i)
				_maps[i] = _maps[i].Substring(13, _maps[i].Length - 17);

			// Create our menu entries.
			_mapMenuEntry = new MenuEntry(string.Empty);
			SetMenuEntryText();
			var start = new MenuEntry("Start");
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			_mapMenuEntry.Selected += MapMenuEntrySelected;
			start.Selected += StartMenuEntrySelected;
			back.Selected += OnCancel;

			// Add entries to the menu.
			MenuEntries.Add(_mapMenuEntry);
			MenuEntries.Add(start);
			MenuEntries.Add(back);
		}

		void MapMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_currentMap = (_currentMap + 1) % _maps.Length;
			SetMenuEntryText();
		}
		void StartMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
							   new GameplayScreen(_maps[_currentMap]));
		}

		void SetMenuEntryText()
		{
			_mapMenuEntry.Text = "Map: " + _maps[_currentMap];
		}
	}
}
