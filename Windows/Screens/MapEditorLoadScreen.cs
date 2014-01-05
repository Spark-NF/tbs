using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TBS.Screens
{
	class MapEditorLoadScreen : MenuScreen
	{
		readonly MenuEntry _mapMenuEntry;

		static List<string> _maps;
		static int _currentMap;

		public MapEditorLoadScreen()
			: base("Load map")
		{
			_maps = Directory.GetFiles("Content/Maps").ToList();
			for (var i = 0; i < _maps.Count; ++i)
				_maps[i] = _maps[i].Substring(13, _maps[i].Length - 17);

			// Create our menu entries.
			_mapMenuEntry = new MenuEntry(string.Empty);
			SetMenuEntryText();
			var load = new MenuEntry("Load");
			var delete = new MenuEntry("Delete");
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			_mapMenuEntry.Selected += MapMenuEntrySelected;
			load.Selected += LoadMenuEntrySelected;
			delete.Selected += DeleteMenuEntrySelected;
			back.Selected += OnCancel;

			// Add entries to the menu.
			MenuEntries.Add(_mapMenuEntry);
			MenuEntries.Add(load);
			MenuEntries.Add(delete);
			MenuEntries.Add(back);
		}

		void MapMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_currentMap = (_currentMap + 1) % _maps.Count;
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

		protected void DeleteMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			const string message = "Are you sure you want to delete this map?";
			var confirmExitMessageBox = new MessageBoxScreen(message);
			confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
			ScreenManager.AddScreen(confirmExitMessageBox, e.PlayerIndex);
		}

		void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
		{
			File.Delete("Content/Maps/" + _maps[_currentMap] + ".txt");
			_maps.RemoveAt(_currentMap);
			_currentMap = _currentMap % _maps.Count;
			SetMenuEntryText();
		}
	}
}
