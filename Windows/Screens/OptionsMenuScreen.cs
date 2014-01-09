using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace TBS.Screens
{
	internal class OptionsMenuScreen : MenuScreen
	{
		private readonly MenuEntry _languageMenuEntry;
		private readonly MenuEntry _fullScreenMenuEntry;
		private readonly MenuEntry _resolutionMenuEntry;

		private static readonly string[] Languages = {"English"};
		private static int _currentLanguage;
		private static bool _fullScreen;

		private Point _oldPos;
		private Vector2 _oldSize;
		private static int _currentResolution;

		private readonly List<Vector2> _resolutions = new List<Vector2>();
		private readonly Vector2[] _availableResolutions =
		{
			new Vector2(640, 480),
			new Vector2(800, 480),
			new Vector2(800, 600),
			new Vector2(854, 480),
			new Vector2(1024, 600),
			new Vector2(1024, 768),
			new Vector2(1152, 768),
			new Vector2(1280, 720),
			new Vector2(1280, 768),
			new Vector2(1280, 800),
			new Vector2(1280, 854),
			new Vector2(1280, 960),
			new Vector2(1280, 1024),
			new Vector2(1366, 768),
			new Vector2(1400, 1050),
			new Vector2(1440, 900),
			new Vector2(1440, 960),
			new Vector2(1600, 1200),
			new Vector2(1680, 1050),
			new Vector2(1920, 1080),
			new Vector2(1920, 1200),
			new Vector2(2048, 1080),
			new Vector2(2048, 1536),
			new Vector2(2560, 1600),
			new Vector2(2560, 2048)
		};

		public OptionsMenuScreen() : base("Options")
		{
			_fullScreen = Static.Game.GraphicsDeviceManager.IsFullScreen;
			var screen = Screen.AllScreens.First(n => n.Primary);
			var ratio = (float)screen.Bounds.Width / screen.Bounds.Height;
			foreach (var res in _availableResolutions.Where(res => 
				Math.Abs(res.X / res.Y - ratio) < 0.001&& res.X <= screen.Bounds.Width
				|| (int)res.X == 800 && (int)res.Y == 600))
				_resolutions.Add(res);

			// Load settings
			var manager = new Settings.SettingsManager();
			var settings = manager.Load<Settings.GeneralSettings>();
			_fullScreen = settings.Fullscreen;
			if (_fullScreen)
			{
				_oldSize = new Vector2(
					settings.Width,
					settings.Height);
			}

			// Create our menu entries.
			_languageMenuEntry = new MenuEntry(string.Empty);
			_fullScreenMenuEntry = new MenuEntry(string.Empty);
			_resolutionMenuEntry = new MenuEntry(string.Empty);
			SetMenuEntryText();
			var back = new MenuEntry("Back");

			// Hook up menu event handlers.
			_languageMenuEntry.Selected += LanguageMenuEntrySelected;
			_fullScreenMenuEntry.Selected += FullScreenMenuEntrySelected;
			_resolutionMenuEntry.Selected += ResolutionMenuEntrySelected;
			back.Selected += BackMenuEntrySelected;
			
			// Add entries to the menu.
			MenuEntries.Add(_languageMenuEntry);
			MenuEntries.Add(_fullScreenMenuEntry);
			MenuEntries.Add(_resolutionMenuEntry);
			MenuEntries.Add(back);
		}

		void BackMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			// Save settings
			var manager = new Settings.SettingsManager();
			var settings = manager.Load<Settings.GeneralSettings>();
			settings.Fullscreen = _fullScreen;
			settings.Width = (int)_resolutions[_currentResolution].X;
			settings.Height = (int)_resolutions[_currentResolution].Y;
			manager.Save(settings);

			// Go to previous menu
			OnCancel(sender, e);
		}

		void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_currentLanguage = (_currentLanguage + 1) % Languages.Length;
			SetMenuEntryText();
		}

		void ResolutionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_currentResolution = (_currentResolution + 1) % _resolutions.Count;

			if (_fullScreen)
			{
				_fullScreen = false;
				Static.Game.Window.IsBorderless = false;
				Static.Game.Window.Position = _oldPos;
			}
			Static.Game.GraphicsDeviceManager.PreferredBackBufferWidth = (int)_resolutions[_currentResolution].X;
			Static.Game.GraphicsDeviceManager.PreferredBackBufferHeight = (int)_resolutions[_currentResolution].Y;
			Static.Game.GraphicsDeviceManager.ApplyChanges();

			SetMenuEntryText();
		}

		void FullScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_fullScreen = !_fullScreen;

			if (_fullScreen)
			{
				_oldPos = Static.Game.Window.Position;
				_oldSize = new Vector2(
					Static.Game.GraphicsDeviceManager.PreferredBackBufferWidth,
					Static.Game.GraphicsDeviceManager.PreferredBackBufferHeight);

				var screen = Screen.AllScreens.First(n => n.Primary);
				Static.Game.Window.IsBorderless = true;
				Static.Game.Window.Position = new Point(screen.Bounds.X, screen.Bounds.Y);
				Static.Game.GraphicsDeviceManager.PreferredBackBufferWidth = screen.Bounds.Width;
				Static.Game.GraphicsDeviceManager.PreferredBackBufferHeight = screen.Bounds.Height;
			}
			else
			{
				Static.Game.Window.IsBorderless = false;
				Static.Game.Window.Position = _oldPos;
				Static.Game.GraphicsDeviceManager.PreferredBackBufferWidth = (int)_oldSize.X;
				Static.Game.GraphicsDeviceManager.PreferredBackBufferHeight = (int)_oldSize.Y;
			}

			Static.Game.GraphicsDeviceManager.IsFullScreen = _fullScreen;
			Static.Game.GraphicsDeviceManager.ApplyChanges();

			SetMenuEntryText();
		}

		void SetMenuEntryText()
		{
			_languageMenuEntry.Text = "Language: " + Languages[_currentLanguage];
			_fullScreenMenuEntry.Text = "Full screen: " + (_fullScreen ? "True" : "False");
			_resolutionMenuEntry.Text = "Resolution: " + _resolutions[_currentResolution].X + "x" + _resolutions[_currentResolution].Y;
		}
	}
}
