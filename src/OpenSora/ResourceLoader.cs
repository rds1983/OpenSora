using Imaging.DDSReader;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using OpenSora.Dir;
using OpenSora.Rendering;
using OpenSora.Scenarios;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;

namespace OpenSora
{
	public class ResourceLoader
	{
		private Dictionary<string, List<DirEntry>> _entries = null;
		private readonly ConcurrentDictionary<string, Texture2D> _textures = new ConcurrentDictionary<string, Texture2D>();

		public string GamePath { get; private set; }
		public GraphicsDevice GraphicsDevice { get; private set; }

		public Dictionary<string, List<DirEntry>> Entries
		{
			get
			{
				return _entries;
			}
		}

		public ResourceLoader(GraphicsDevice device, string gamePath)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			if (string.IsNullOrEmpty(gamePath))
			{
				throw new ArgumentNullException(nameof(gamePath));
			}

			GraphicsDevice = device;
			GamePath = gamePath;

			_entries = DirProcessor.BuildEntries(gamePath);
		}

		public DirEntry FindByIndex(int index)
		{
			var dtIndex = index >> 16;
			var fileIndex = index & 0xffff;

			foreach(var pair in _entries)
			{
				if (pair.Key.Contains(dtIndex + ".dat"))
				{
					// Found data
					foreach(var entry in pair.Value)
					{
						if (entry.Index == fileIndex)
						{
							return entry;
						}
					}
				}
			}

			return null;
		}

		public DirEntry FindByName(string nameFilter, string extFilter = "")
		{
			nameFilter = nameFilter.ToLower();

			if (!string.IsNullOrEmpty(extFilter))
			{
				extFilter = extFilter.ToLower();
			}
			foreach (var pair in _entries)
			{
				foreach (var entry in pair.Value)
				{
					var ename = entry.Name;
					if (entry.Name.Contains("."))
					{
						var parts = entry.Name.Split('.');
						if (parts[0].ToLower().Contains(nameFilter) && 
							(string.IsNullOrEmpty(extFilter) || parts[1].ToLower().Contains(extFilter)))
						{
							return entry;
						}
					}
					else
					{
						if (ename.ToLower().Contains(nameFilter))
						{
							return entry;
						}
					}
				}
			}

			return null;
		}

		public byte[] LoadData(DirEntry entry)
		{
			byte[] data;
			using (var stream = File.OpenRead(entry.DatFilePath))
			using (var reader = new BinaryReader(stream))
			{
				stream.Seek(entry.Offset, SeekOrigin.Begin);
				data = reader.ReadBytes(entry.CompressedSize > 0 ? entry.CompressedSize : entry.DecompressedSize);
			}

			return data;
		}

		public Texture2D LoadTexture(DirEntry entry)
		{
			Texture2D texture;
			if (_textures.TryGetValue(entry.Name, out texture))
			{
				return texture;
			}

			var data = LoadData(entry);
			if (data[0] != 'D' || data[1] != 'D' || data[2] != 'S' || data[3] != ' ')
			{
				// Compressed
				data = FalcomDecompressor.Decompress(data);
			}

			using (var stream = new MemoryStream(data))
			{
				var image = DDS.LoadImage(stream);
				texture = new Texture2D(GraphicsDevice, image.Width, image.Height);
				texture.SetData(image.Data);
			}

			_textures[entry.Name] = texture;

			return texture;
		}

		public Texture2D LoadTexture(string name)
		{
			var nameWithoutExt = Path.GetFileNameWithoutExtension(name);

			foreach (var pair in _entries)
			{
				foreach (var entry in pair.Value)
				{
					var ext = Path.GetExtension(entry.Name);
					if (ext == "._DS" && entry.Name.IndexOf(nameWithoutExt, StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						return LoadTexture(entry);
					}
				}
			}

			return DefaultAssets.White;
		}

		public Texture2D LoadImage(DirEntry entry)
		{
			var chData = FalcomDecompressor.Decompress(LoadData(entry));
			using (var chStream = new MemoryStream(chData))
			{
				return ChLoader.LoadImage(GraphicsDevice, entry.DatFilePath, entry.Name, chStream);
			}
		}

		public Animation LoadAnimation(DirEntry entry)
		{
			Texture2D texture;
			var chData = FalcomDecompressor.Decompress(LoadData(entry));
			using (var chStream = new MemoryStream(chData))
			{
				texture = AnimationLoader.LoadImage(GraphicsDevice, chStream);
			}

			ushort?[][,] animationInfo;
			var cpFile = Path.GetFileNameWithoutExtension(entry.Name);
			if (cpFile.EndsWith(" "))
			{
				cpFile = cpFile.Substring(0, cpFile.Length - 1) + "P";
			}
			var cpFileEntry = FindByName(cpFile, "_CP");
			var cpData = FalcomDecompressor.Decompress(LoadData(cpFileEntry));
			using (var cpStream = new MemoryStream(cpData))
			{
				animationInfo = AnimationLoader.LoadInfo(cpStream);
			}

			return new Animation(animationInfo, texture);
		}

		public Scenario LoadScenario(DirEntry entry)
		{
			var data = FalcomDecompressor.Decompress(LoadData(entry));
			Scenario scenario;
			using (var stream = new MemoryStream(data))
			{
				scenario = Scenario.FromFCStream(stream);
			}

			return scenario;
		}

		public void ClearTextureCache()
		{
			_textures.Clear();
		}

	}
}