namespace TBS.Screens
{
	class MapEditorMenuScreen : MenuScreen
	{
		public MapEditorMenuScreen()
			: base("Map Editor")
		{
			// Create our menu entries.
			var load = new MenuEntry("Load map");
			var create = new MenuEntry("New map");
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			load.Selected += LoadMenuEntrySelected;
			create.Selected += CreateMenuEntrySelected;
			back.Selected += OnCancel;

			// Add entries to the menu.
			MenuEntries.Add(load);
			MenuEntries.Add(create);
			MenuEntries.Add(back);
		}

		private void LoadMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.AddScreen(new MapEditorLoadScreen(), e.PlayerIndex);
		}
		private void CreateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.AddScreen(new MapEditorCreateScreen(), e.PlayerIndex);
		}
	}
}
