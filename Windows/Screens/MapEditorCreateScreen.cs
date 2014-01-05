using System;
using System.IO;

namespace TBS.Screens
{
	class MapEditorCreateScreen : MenuScreen
	{
		private string _mapName = "";
		private int _mapWidth = 5, _mapHeight = 5;

		private readonly MenuEntry _mapNameMenuEntry;
		private readonly MenuEntry _mapWidthMenuEntry, _mapHeightMenuEntry;

		public MapEditorCreateScreen()
			: base("Create map")
		{
			_mapName = "NewMap";

			// Create our menu entries.
			_mapNameMenuEntry = new MenuEntry("Name: " + _mapName);
			_mapWidthMenuEntry = new MenuEntry("Width: " + _mapWidth);
			_mapHeightMenuEntry = new MenuEntry("Height: " + _mapHeight);
			var create = new MenuEntry("Create");
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			_mapNameMenuEntry.Focus += MapNameMenuEntryFocus;
			_mapNameMenuEntry.Unfocus += MapNameMenuEntryUnfocus;
			_mapWidthMenuEntry.Selected += MapWidthMenuEntrySelected;
			_mapHeightMenuEntry.Selected += MapHeightMenuEntrySelected;
			create.Selected += CreateMenuEntrySelected;
			back.Selected += OnCancel;

			// Add entries to the menu.
			AddMenuEntry(_mapNameMenuEntry);
			AddMenuEntry(_mapWidthMenuEntry);
			AddMenuEntry(_mapHeightMenuEntry);
			AddMenuEntry(create);
			AddMenuEntry(back);
		}

		void MapWidthMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			Clavier.Get().GetText = false;
			_mapWidth = (_mapWidth - 5 + 1) % 26 + 5;
			SetMenuEntryText();
		}
		void MapHeightMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			Clavier.Get().GetText = false;
			_mapHeight = (_mapHeight - 5 + 1) % 16 + 5;
			SetMenuEntryText();
		}
		void MapNameMenuEntryFocus(object sender, PlayerIndexEventArgs e)
		{
			Clavier.Get().GetText = true;
			Clavier.Get().Text = _mapName;
			Clavier.Get().TextEntered = SetMenuEntryText;
			System.Diagnostics.Debug.WriteLine("Name selected");
			SetMenuEntryText();
		}
		void MapNameMenuEntryUnfocus(object sender, PlayerIndexEventArgs e)
		{
			Clavier.Get().GetText = false;
		}

		void SetMenuEntryText(object sender = null, PlayerIndexEventArgs e = null)
		{
			_mapName = Clavier.Get().Text;
			_mapNameMenuEntry.Text = "Name: " + _mapName;
			_mapWidthMenuEntry.Text = "Width: " + _mapWidth;
			_mapHeightMenuEntry.Text = "Height: " + _mapHeight;
		}

		void CreateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			if (_mapName.Length <= 0)
				return;

			var txt = "# General information\n\n" +
			          _mapName + "\n" +
			          _mapWidth + "\n" +
			          _mapHeight + "\n" +
			          "1\n" +
			          "2\n" +
			          "0\n" +
			          "0\n\n\n" +
			          "# Terrain\n";
			var line = "\n" + new String('0', _mapWidth);
			for (var i = 0; i < _mapHeight; ++i)
				txt += line;
			File.WriteAllText("Content/Maps/" + _mapName + ".txt", txt);
			LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new MapEditorScreen(_mapName));
		}
	}
}
