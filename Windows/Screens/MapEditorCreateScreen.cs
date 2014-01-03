namespace TBS.Screens
{
	class MapEditorCreateScreen : MenuScreen
	{
		public MapEditorCreateScreen()
			: base("Create map")
		{
			// Create our menu entries.
			var create = new MenuEntry("Create");
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			create.Selected += CreateMenuEntrySelected;
			back.Selected += OnCancel;

			// Add entries to the menu.
			MenuEntries.Add(create);
			MenuEntries.Add(back);
		}

		void CreateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			/*LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
							   new MapEditorScreen("Test"));*/
		}
	}
}
