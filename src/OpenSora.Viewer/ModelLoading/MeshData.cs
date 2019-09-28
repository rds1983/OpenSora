using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace OpenSora.Viewer.ModelLoading
{
	public class MeshData
	{
		private readonly List<string> _materials = new List<string>();
		private readonly List<VertexPositionNormalTexture> _vertices = new List<VertexPositionNormalTexture>();
		private readonly List<short> _indices = new List<short>();

		public string Id;

		public List<string> Materials
		{
			get
			{
				return _materials;
			}
		}

		public List<VertexPositionNormalTexture> Vertices
		{
			get
			{
				return _vertices;
			}
		}

		public List<short> Indices
		{
			get
			{
				return _indices;
			}
		}

		public void LoadFromStream(BinaryReader reader)
		{
			// 260 bytes is header size
			reader.SkipBytes(264 - (Id.Length + 1));

			var texturesCount = reader.ReadInt32();
			reader.SkipBytes(16);

			reader.SkipBytes(168);

			for (var i = 0; i < texturesCount; ++i)
			{
				int length;
				var id = reader.LoadZeroTerminatedString(out length);

				if (i < texturesCount - 1)
				{
					reader.SkipBytes(544 - length);
				}
				else
				{
					reader.SkipBytes(360 - length);
				}

				_materials.Add(id);
			}

			var verticesCount = reader.ReadInt32();
			for (var i = 0; i < verticesCount; ++i)
			{
				var floats = new float[10];

				for (var j = 0; j < floats.Length; ++j)
				{
					floats[j] = reader.ReadSingle();
				}

				_vertices.Add(new VertexPositionNormalTexture(
					new Vector3(floats[0], floats[1], floats[2]),
					new Vector3(floats[3], floats[4], floats[5]),
					new Vector2(floats[8], floats[9])
				));
			}

			var indicesCount = reader.ReadInt32();
			for (var i = 0; i < indicesCount; ++i)
			{
				_indices.Add(reader.ReadInt16());
			}
		}
	}
}
