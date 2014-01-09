namespace TBS.Settings
{
	public class GeneralSettings : SettingsBase
	{
		public bool Fullscreen;
		public int Width;
		public int Height;

		public GeneralSettings()
		{
			Fullscreen = false;
			Width = 800;
			Height = 480;
		}
	}
}
