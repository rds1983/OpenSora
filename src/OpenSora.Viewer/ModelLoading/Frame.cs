using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace OpenSora.Viewer.ModelLoading
{
	public class Frame
	{
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

		public void LoadFromStream(BinaryReader reader)
		{
			// 260 bytes is header size
			reader.SkipBytes(260 - (Id.Length + 1));
			Transform = reader.LoadTransform();

			if (Id == "Frame_SCENE_ROOT")
			{
				reader.SkipBytes(51);
			}

			// Skip first two bytes
			reader.SkipBytes(2);

			int length = 0;
			var id = reader.LoadZeroTerminatedString(out length);

			if (id.Contains("Frame"))
			{
				var child = new Frame
				{
					Id = id
				};

				child.LoadFromStream(reader);
				Children.Add(child);
			}
			else if (id.Contains("Mesh"))
			{
				var mesh = new MeshData
				{
					Id = id
				};

				mesh.LoadFromStream(reader);
				Meshes.Add(mesh);
			}
		}
	}
}