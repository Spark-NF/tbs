using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace TBS.Settings
{
	public static class Serializer
	{
		public static string Serialize(object item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			using (var stream = new MemoryStream())
			{
				var serializer = new XmlSerializer(item.GetType());
				serializer.Serialize(stream, item);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		public static T Deserialize<T>(string xml)
		{
			var bytes = new byte[xml.Length * sizeof(char)];
			Buffer.BlockCopy(xml.ToCharArray(), 0, bytes, 0, bytes.Length);

			using (var stream = new MemoryStream(bytes))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(stream);
			}
		}

		public static T Read<T>(string file)
		{
			using (var stream = new FileStream(file, FileMode.Open))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(stream);
			}
		}

		public static void Write<T>(T item, string file)
		{
			using (var stream = new FileStream(file, FileMode.OpenOrCreate))
			{
				var serializer = new XmlSerializer(item.GetType());
				serializer.Serialize(stream, item);
			}
		}
	}
}
