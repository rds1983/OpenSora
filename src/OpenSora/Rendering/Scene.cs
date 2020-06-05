using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using OpenSora.ModelLoading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSora.Rendering
{
	public class Scene
	{
		class MeshPartTag
		{
			public BoundingSphere BoundingSphere;
			public Texture2D Texture;
		}

		public float NearPlaneDistance = 0.1f;
		public float FarPlaneDistance = 1000.0f;

		private readonly GraphicsDevice _device;
		private readonly DefaultEffect _defaultEffect;
		private readonly RenderContext _renderContext = new RenderContext();
		private List<ModelMeshPart> _meshes;
		private readonly SpriteBatch _spriteBatch;

		public Camera Camera { get; }
		public CameraInputController Controller { get; }

		public List<ModelMeshPart> Meshes
		{
			get
			{
				return _meshes;
			}

			set
			{
				_meshes = value;
				ResetCamera();
			}
		}

		public bool RenderDebugInfo = true;

		public Scene(GraphicsDevice device)
		{
			_device = device;
			_defaultEffect = new DefaultEffect(device);
			_spriteBatch = new SpriteBatch(device);
			Camera = new Camera();
			Camera.ViewAngleChanged += (s, a) => _renderContext.ResetView();

			Controller = new CameraInputController(Camera);

			ResetCamera();
		}

		public static void AddMeshData(GraphicsDevice device, List<ModelMeshPart> meshes, MeshData meshData, Func<string, Texture2D> textureLoader)
		{
			var vertexBuffer = new VertexBuffer(device,
						VertexPositionNormalTexture.VertexDeclaration,
						meshData.Vertices.Count,
						BufferUsage.None);
			vertexBuffer.SetData(meshData.Vertices.ToArray());

			var indexBuffer = new IndexBuffer(device,
				IndexElementSize.SixteenBits,
				meshData.Indices.Count,
				BufferUsage.None);
			indexBuffer.SetData(meshData.Indices.ToArray());

			foreach (var md in meshData.Materials)
			{
				if (md.PrimitivesCount == 0 || md.VerticesCount == 0)
				{
					continue;
				}

				var tag = new MeshPartTag
				{
					BoundingSphere = BoundingSphere.CreateFromPoints(from v in meshData.Vertices select v.Position),
					Texture = textureLoader(md.TextureName)
				};

				var meshPart = new ModelMeshPart
				{
					IndexBuffer = indexBuffer,
					NumVertices = md.VerticesCount,
					PrimitiveCount = md.PrimitivesCount,
					StartIndex = md.PrimitivesStart * 3,
					VertexBuffer = vertexBuffer,
					VertexOffset = 0,
					Tag = tag
				};

				lock (meshes)
				{
					meshes.Add(meshPart);
				}
			}
		}

		public void ResetCamera()
		{
			Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);
		}

		public void Render(Rectangle bounds)
		{
			if (Meshes == null || Meshes.Count == 0)
			{
				return;
			}

			var oldViewport = _device.Viewport;
			var oldDepthStencilState = _device.DepthStencilState;
			var oldRasterizerState = _device.RasterizerState;
			var oldBlendState = _device.BlendState;
			var oldSamplerState = _device.SamplerStates[0];
			try
			{
				_device.Viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);
				_device.DepthStencilState = DepthStencilState.Default;
				_device.RasterizerState = RasterizerState.CullCounterClockwise;
				_device.BlendState = BlendState.Opaque;
				_device.SamplerStates[0] = SamplerState.LinearWrap;

				_renderContext.View = Camera.View;
				_renderContext.Projection = Matrix.CreatePerspectiveFieldOfView(
					MathHelper.ToRadians(Camera.ViewAngle),
					_device.Viewport.AspectRatio,
					NearPlaneDistance, FarPlaneDistance);

				_defaultEffect.WorldViewProjection = _renderContext.ViewProjection;
				foreach (var mesh in Meshes)
				{
					var tag = (MeshPartTag)mesh.Tag;

					if (_renderContext.Frustrum.Contains(tag.BoundingSphere) == ContainmentType.Disjoint)
					{
						continue;
					}

					_defaultEffect.Texture = tag.Texture;
					foreach (var pass in _defaultEffect.CurrentTechnique.Passes)
					{
						pass.Apply();

						_device.SetVertexBuffer(mesh.VertexBuffer);
						_device.Indices = mesh.IndexBuffer;

						_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.VertexOffset, mesh.StartIndex, mesh.PrimitiveCount);
					}
				}
			}
			finally
			{
				_device.Viewport = oldViewport;
				_device.DepthStencilState = oldDepthStencilState;
				_device.RasterizerState = oldRasterizerState;
				_device.BlendState = oldBlendState;
				_device.SamplerStates[0] = oldSamplerState;
			}

			if (RenderDebugInfo)
			{
				var x = bounds.X;
				var y = bounds.Y;

				_spriteBatch.Begin();

				_spriteBatch.DrawString(DefaultAssets.Font, "Camera: " + Camera.Position, new Vector2(x, y), Color.White);
				y += 20;

				_spriteBatch.End();
			}
		}
	}
}
