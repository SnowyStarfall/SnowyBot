using SnowyBot.Containers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Victoria;
using Victoria.Enums;

namespace SnowyBot.Structs
{
	[Serializable]
	public class LavaTable
	{
		public ConcurrentDictionary<ulong, LavaEntry> table = new();
		public static void WriteToBinaryFile<T>(string filePath, T objectToWrite)
		{
			using Stream stream = File.Open(filePath, FileMode.Create);
			JsonSerializerOptions options = new();
			options.IncludeFields = true;
			string json = JsonSerializer.Serialize(objectToWrite, options);
			byte[] bytes = Encoding.UTF8.GetBytes(json);
			stream.Write(bytes);
			stream.Dispose();
			stream.Close();
		}
		public static LavaTable ReadFromBinaryFile<LavaTable>(string filePath)
		{
			byte[] bytes = File.ReadAllBytes(filePath);
			string json = Encoding.UTF8.GetString(bytes);
			JsonSerializerOptions options = new();
			options.IncludeFields = true;
			options.MaxDepth = 64;
			return JsonSerializer.Deserialize<LavaTable>(json, options);
		}
	}
}
