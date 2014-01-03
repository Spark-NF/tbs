namespace TBS.Screens
{
	class OptionsMenuScreen : MenuScreen
	{
		readonly MenuEntry _languageMenuEntry;
		//readonly MenuEntry _volumeMenuEntry;

		static readonly string[] Languages = { "English" };
		static int _currentLanguage;
		//static int _volume = 10;

		public OptionsMenuScreen() : base("Options")
		{
			// Create our menu entries.
			_languageMenuEntry = new MenuEntry(string.Empty);
			//_volumeMenuEntry = new MenuEntry(string.Empty);
			SetMenuEntryText();
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			_languageMenuEntry.Selected += LanguageMenuEntrySelected;
			//_volumeMenuEntry.Selected += VolumeMenuEntrySelected;
			back.Selected += OnCancel;
			
			// Add entries to the menu.
			MenuEntries.Add(_languageMenuEntry);
			//MenuEntries.Add(_volumeMenuEntry);
			MenuEntries.Add(back);
		}

		void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_currentLanguage = (_currentLanguage + 1) % Languages.Length;
			SetMenuEntryText();
		}
		/*void VolumeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_volume = (_volume + 1) % 11;
			SetMenuEntryText();
		}*/

		void SetMenuEntryText()
		{
			_languageMenuEntry.Text = "Language: " + Languages[_currentLanguage];
			//_volumeMenuEntry.Text = "Volume: " + _volume;
		}
	}
}
