using System.IO;
using System.Text;

namespace OpenSora.Tests.Fixtures
{
	/// <summary>
	/// Builds minimal X2 (version 2) and X3 (version 3) models with one triangle mesh. Fixture data is
	/// generated from code and never copied from a game installation.
	/// </summary>
	public static class SyntheticModel
	{
		private const string RootFrameId = "Frame_SCENE_ROOT";
		private const string ChildFrameId = "Frame_TEST";
		private const string MeshId = "Mesh_TEST";
		private const string TextureName = "TEST_DS";

		public static byte[] CreateX2()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(new byte[4]); // version 2 header

				WriteFrameHeader(writer, RootFrameId);
				WriteIdentityTransform(writer);
				writer.Write(new byte[51]);

				writer.Write((ushort)1); // child count
				WriteFrameHeader(writer, ChildFrameId);
				WriteIdentityTransform(writer);
				writer.Write((ushort)1); // child count of child frame
				WriteMeshHeader(writer, MeshId);

				writer.Write(1); // textures count
				writer.Write(0); // primitives start
				writer.Write(1); // primitives count
				writer.Write(0); // vertices start
				writer.Write(3); // vertices count
				writer.Write(0); // unused
				writer.Write(new byte[164]);

				WriteZeroTerminated(writer, TextureName);
				var textureNameLength = TextureName.Length + 1;
				writer.Write(new byte[360 - textureNameLength]);

				WriteVertices(writer);
				WriteIndices(writer);
				writer.Write(new byte[44]);

				return stream.ToArray();
			}
		}

		public static byte[] CreateX3()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(new byte[48]); // version 3 header

				WriteFrameHeader(writer, RootFrameId);
				WriteIdentityTransform(writer);
				writer.Write(new byte[53]);

				writer.Write((ushort)1); // child count
				WriteFrameHeader(writer, ChildFrameId);
				WriteIdentityTransform(writer);
				writer.Write(0); // version 3 frame materials count (int32)
				writer.Write((ushort)1); // child count of child frame
				WriteMeshHeader(writer, MeshId);

				writer.Write(0); // textures count

				WriteVertices(writer);
				WriteIndices(writer);
				writer.Write(new byte[44]);

				return stream.ToArray();
			}
		}

		private static void WriteZeroTerminated(BinaryWriter writer, string value)
		{
			writer.Write(Encoding.ASCII.GetBytes(value));
			writer.Write((byte)0);
		}

		private static void WriteFrameHeader(BinaryWriter writer, string id)
		{
			WriteZeroTerminated(writer, id);
			writer.Write(new byte[260 - (id.Length + 1)]);
		}

		private static void WriteMeshHeader(BinaryWriter writer, string id)
		{
			WriteZeroTerminated(writer, id);
			writer.Write(new byte[264 - (id.Length + 1)]);
		}

		private static void WriteIdentityTransform(BinaryWriter writer)
		{
			writer.Write(1.0f);
			writer.Write(0.0f);
			writer.Write(0.0f);
			writer.Write(0.0f);

			writer.Write(0.0f);
			writer.Write(1.0f);
			writer.Write(0.0f);
			writer.Write(0.0f);

			writer.Write(0.0f);
			writer.Write(0.0f);
			writer.Write(1.0f);
			writer.Write(0.0f);

			writer.Write(0.0f);
			writer.Write(0.0f);
			writer.Write(0.0f);
			writer.Write(1.0f);
		}

		private static void WriteVertex(BinaryWriter writer, float x, float y, float z, float nx, float ny, float nz,
			float u, float v)
		{
			writer.Write(x);
			writer.Write(y);
			writer.Write(z);
			writer.Write(nx);
			writer.Write(ny);
			writer.Write(nz);
			writer.Write(0); // unused integer
			writer.Write(0); // unused integer
			writer.Write(u);
			writer.Write(v);
		}

		private static void WriteVertices(BinaryWriter writer)
		{
			writer.Write(3); // vertices count
			WriteVertex(writer, 1, 2, 3, 0, 0, 1, 0.25f, 0.75f);
			WriteVertex(writer, 2, 2, 3, 0, 0, 1, 0.50f, 0.75f);
			WriteVertex(writer, 1, 3, 3, 0, 0, 1, 0.25f, 1.00f);
		}

		private static void WriteIndices(BinaryWriter writer)
		{
			writer.Write(3); // indices count
			writer.Write((short)0);
			writer.Write((short)1);
			writer.Write((short)2);
		}
	}
}
