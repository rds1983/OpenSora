using OpenSora.Utility;
using System.Collections.Generic;

namespace OpenSora.ModelLoading
{
	public class Frame
	{
		private readonly List<string> _materials = new List<string>();
		private readonly List<Frame> _children = new List<Frame>();
		private readonly List<MeshData> _meshes = new List<MeshData>();

		public string Id;
		public float[] Transform;

		public List<Frame> Children
		{
			get
			{
				return _children;
			}
		}

		public List<MeshData> Meshes
		{
			get
			{
				return _meshes;
			}
		}

		public List<string> Materials
		{
			get
			{
				return _materials;
			}
		}

		public void LoadFromStream(ModelLoadContext context)
		{
			Frame oldParent = context.Parent;
			try
			{
				context.Parent = this;

				// 260 bytes is header size
				var reader = context.Reader;
				reader.SkipBytes(260 - (Id.Length + 1));
				Transform = reader.LoadTransform();

				if (Id == "Frame_SCENE_ROOT")
				{
					reader.SkipBytes(context.Version == 2 ? 51 : 53);
				}
				else if (context.Version == 3)
				{
					var materialsCount = reader.ReadInt32();
					for(var i = 0; i < materialsCount; ++i)
					{
						reader.SkipBytes(70);
						var length = 0;
						var material = reader.LoadZeroTerminatedString(out length);
						reader.SkipBytes(780 - length);
						if (i < materialsCount - 1)
						{
							reader.SkipBytes(2);
						}
						_materials.Add(material);
					}
				}

				// Skip first two bytes
				var count = reader.ReadUInt16();
				for (var i = 0; i < count; ++i)
				{
					int length = 0;
					var id = reader.LoadZeroTerminatedString(out length);

					if (id.Contains("Frame"))
					{
						var child = new Frame
						{
							Id = id
						};

						child.LoadFromStream(context);
						Children.Add(child);
					}
					else if (id.Contains("Mesh"))
					{
						var mesh = new MeshData
						{
							Id = id
						};

						mesh.LoadFromStream(context);
						Meshes.Add(mesh);
					}
				}
			} finally
			{
				context.Parent = oldParent;
			}
		}
	}
}