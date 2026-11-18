using OpenSora.ModelLoading;
using OpenSora.Tests.Fixtures;
using System.IO;
using Xunit;

namespace OpenSora.Tests
{
	public class ModelFixtureTests
	{
		[Fact]
		public void ModelLoader_LoadsSyntheticX2Model()
		{
			var root = ModelLoader.Load(new MemoryStream(SyntheticModel.CreateX2()), 2);

			Assert.Equal("Frame_SCENE_ROOT", root.Id);
			Assert.Equal(1.0f, root.Transform[0]);
			Assert.Equal(1.0f, root.Transform[15]);
			Assert.Empty(root.Meshes);

			var frame = Assert.Single(root.Children);
			Assert.Equal("Frame_TEST", frame.Id);

			var mesh = Assert.Single(frame.Meshes);
			var material = Assert.Single(mesh.Materials);
			Assert.Equal("TEST_DS", material.TextureName);
			Assert.Equal(0, material.PrimitivesStart);
			Assert.Equal(1, material.PrimitivesCount);
			Assert.Equal(0, material.VerticesStart);
			Assert.Equal(3, material.VerticesCount);

			Assert.Equal(3, mesh.Vertices.Count);
			Assert.Equal(3, mesh.Indices.Count);

			var vertex = mesh.Vertices[0];
			Assert.Equal(1.0f, vertex.Position.X);
			Assert.Equal(2.0f, vertex.Position.Y);
			Assert.Equal(-3.0f, vertex.Position.Z);
			Assert.Equal(0.0f, vertex.Normal.X);
			Assert.Equal(0.0f, vertex.Normal.Y);
			Assert.Equal(1.0f, vertex.Normal.Z);
			Assert.Equal(0.25f, vertex.TextureCoordinate.X);
			Assert.Equal(0.75f, vertex.TextureCoordinate.Y);

			Assert.Equal(0, mesh.Indices[0]);
			Assert.Equal(1, mesh.Indices[1]);
			Assert.Equal(2, mesh.Indices[2]);
		}

		[Fact]
		public void ModelLoader_LoadsSyntheticX3Model()
		{
			var root = ModelLoader.Load(new MemoryStream(SyntheticModel.CreateX3()), 3);

			Assert.Equal("Frame_SCENE_ROOT", root.Id);
			Assert.Equal(1.0f, root.Transform[0]);
			Assert.Equal(1.0f, root.Transform[15]);
			Assert.Empty(root.Meshes);

			var frame = Assert.Single(root.Children);
			Assert.Equal("Frame_TEST", frame.Id);

			var mesh = Assert.Single(frame.Meshes);
			Assert.Empty(mesh.Materials);
			Assert.Equal(3, mesh.Vertices.Count);
			Assert.Equal(3, mesh.Indices.Count);
			Assert.Equal(-3.0f, mesh.Vertices[0].Position.Z);
		}
	}
}
