using System;

namespace TBS.Settings
{
	public class SettingsManager
	{
		const string Extension = ".xml";
		public string Directory { get; protected set; }

		public SettingsManager(string directory = "Content/Settings/")
		{
			Directory = directory;
		}
			
		/// <summary>
		/// Load a settings file
		/// </summary>
		public T Load<T>()
		{
			var t = typeof(T);
			return Load<T>(t.Name);
		}

		public T Load<T>(string name)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("Load: " + Directory + name + Extension);
				return Serializer.Read<T>(
					Directory +
					name +
					Extension);
			}
			catch (Exception)
			{
				var t = typeof(T);
				return (T)Activator.CreateInstance(t);
			}
		}

		public void Save<T>(T o)
		{
			var t = typeof(T);
			Save(o, t.Name);
		}

		public void Save<T>(T o, string name)
		{
			Serializer.Write(o, Directory + name + Extension);
		}
	}
}
