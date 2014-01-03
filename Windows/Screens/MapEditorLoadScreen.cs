using System.IO;

namespace TBS.Screens
{
	class MapEditorLoadScreen : MenuScreen
	{
		readonly MenuEntry _mapMenuEntry;

		static string[] _maps;
		static int _currentMap;

		public MapEditorLoadScreen()
			: base("Load map")
		{
			_maps = Directory.GetFiles("Content/Maps");
			for (var i = 0; i < _maps.Length; ++i)
				_maps[i] = _maps[i].Substring(13, _maps[i].Length - 17);

			// Create our menu entries.
			_mapMenuEntry = new MenuEntry(string.Empty);
			SetMenuEntryText();
			var load = new MenuEntry("Load");
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			_mapMenuEntry.Selected += MapMenuEntrySelected;
			load.Selected += LoadMenuEntrySelected;
			back.Selected += OnCancel;

			// Add entries to the menu.
			MenuEntries.Add(_mapMenuEntry);
			MenuEntries.Add(load);
			MenuEntries.Add(back);
		}

		void MapMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_currentMap = (_currentMap + 1) % _maps.Length;
			SetMenuEntryText();
		}
		void LoadMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
							   new MapEditorScreen(_maps[_currentMap]));
		}

		void SetMenuEntryText()
		{
			_mapMenuEntry.Text = "Map: " + _maps[_currentMap];
		}
	}
}
