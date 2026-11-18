using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenSora.Utility;
using System.Collections.Generic;

namespace OpenSora.ModelLoading
{
	public class MeshData
	{
		private readonly List<MaterialData> _materials = new List<MaterialData>();
		private readonly List<VertexPositionNormalTexture> _vertices = new List<VertexPositionNormalTexture>();
		private readonly List<short> _indices = new List<short>();

		public string Id;

		public List<MaterialData> Materials
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

		public void LoadFromStream(ModelLoadContext context)
		{
			var reader = context.Reader;

			// 260 bytes is header size
			reader.SkipBytes(264 - (Id.Length + 1));

			if (context.Version == 2)
			{
				var texturesCount = reader.ReadInt32();
				for (var i = 0; i < texturesCount; ++i)
				{
					var material = new MaterialData
					{
						PrimitivesStart = reader.ReadInt32(),
						PrimitivesCount = reader.ReadInt32(),
						VerticesStart = reader.ReadInt32(),
						VerticesCount = reader.ReadInt32()
					};

					var unused = reader.ReadInt32();

					reader.SkipBytes(164);
					int length;
					material.TextureName = reader.LoadZeroTerminatedString(out length);

					reader.SkipBytes(360 - length);

					_materials.Add(material);
				}
			}
			else
			{
				var texturesCount = reader.ReadInt32();
				for (var i = 0; i < texturesCount; ++i)
				{
					var material = new MaterialData
					{
						PrimitivesStart = reader.ReadInt32(),
						PrimitivesCount = reader.ReadInt32(),
						VerticesStart = reader.ReadInt32(),
						VerticesCount = reader.ReadInt32()
					};

					reader.SkipBytes(96);

					var materialIndex = reader.ReadInt32();
					material.TextureName = context.Parent.Materials[materialIndex];
					_materials.Add(material);

					reader.SkipBytes(4);
				}
			}

			var verticesCount = reader.ReadInt32();
			for (var i = 0; i < verticesCount; ++i)
			{
				var floats = new float[10];

				for (var j = 0; j <= 5; ++j)
				{
					floats[j] = reader.ReadSingle();
				}

				int i1 = reader.ReadInt32();
				int i2 = reader.ReadInt32();

				for (var j = 8; j <= 9; ++j)
				{
					floats[j] = reader.ReadSingle();
				}

				_vertices.Add(new VertexPositionNormalTexture(
					new Vector3(floats[0], floats[1], -floats[2]),
					new Vector3(floats[3], floats[4], floats[5]),
					new Vector2(floats[8], floats[9])
				));
			}

			var indicesCount = reader.ReadInt32();
			for (var i = 0; i < indicesCount; ++i)
			{
				_indices.Add(reader.ReadInt16());
			}

			int maxv = 0, maxi = 0, sumv = 0, sumi = 0;
			foreach (var m in _materials)
			{
				if (m.PrimitivesStart > maxi)
				{
					maxi = m.PrimitivesStart;
				}

				sumi += m.PrimitivesCount;

				if (m.VerticesStart > maxv)
				{
					maxv = m.VerticesStart;
				}

				sumv += m.VerticesCount;
			}

			reader.SkipBytes(44);
		}
	}
}
