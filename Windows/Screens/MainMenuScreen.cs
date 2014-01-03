using Microsoft.Xna.Framework;

namespace TBS.Screens
{
    class MainMenuScreen : MenuScreen
    {
        public MainMenuScreen()
            : base("Main Menu")
        {
			// Create our menu entries.
			var playGameMenuEntry = new MenuEntry("Play Game");
			var mapEditorMenuEntry = new MenuEntry("Map Editor");
            var optionsMenuEntry = new MenuEntry("Options");
            var exitMenuEntry = new MenuEntry("Exit");

			// Hook up menu event handlers.
			playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
			mapEditorMenuEntry.Selected += MapEditorMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

			// Add entries to the menu.
			MenuEntries.Add(playGameMenuEntry);
			MenuEntries.Add(mapEditorMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

		void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.AddScreen(new GameSelectScreen(), e.PlayerIndex);
		}

		void MapEditorMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.AddScreen(new MapEditorMenuScreen(), e.PlayerIndex);
		}

        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to quit?";

            var confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }

        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }
    }
}
