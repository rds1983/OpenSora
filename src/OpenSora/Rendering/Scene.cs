﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using OpenSora.ModelLoading;
using OpenSora.UI;
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

		private readonly DefaultEffect _defaultEffect;
		private readonly RenderContext _renderContext = new RenderContext();
		private List<ModelMeshPart> _meshes;
		private readonly SpriteBatch _spriteBatch;
		private readonly Desktop _desktop;
		private readonly CharacterTalkWidget _talkWidget;
		private Rectangle _bounds;

		public ResourceLoader ResourceLoader { get; set; }

		public Dictionary<int, SceneCharacter> Characters { get; } = new Dictionary<int, SceneCharacter>();

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

		/// <summary>
		/// Opacity from 0.0f to 1.0f
		/// </summary>
		public float Opacity = 1.0f;

		public bool RenderDebugInfo = false;

		public Scene(ResourceLoader resourceLoader)
		{
			if (resourceLoader == null)
			{
				throw new ArgumentNullException(nameof(resourceLoader));
			}

			ResourceLoader = resourceLoader;
			_defaultEffect = new DefaultEffect(resourceLoader.GraphicsDevice);
			_spriteBatch = new SpriteBatch(resourceLoader.GraphicsDevice);
			Camera = new Camera();
			Camera.ViewAngleChanged += (s, a) => _renderContext.ResetView();

			Controller = new CameraInputController(Camera);

			ResetCamera();

			_desktop = new Desktop
			{
				BoundsFetcher = () => _bounds
			};

			_talkWidget = new CharacterTalkWidget();
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
			Camera.SetLookAt(new Vector3(100, 100, 100), Vector3.Zero);
		}

		private void RenderScene(Rectangle bounds)
		{
			if (Meshes == null || Meshes.Count == 0)
			{
				return;
			}

			var _device = ResourceLoader.GraphicsDevice;
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
				_renderContext.Projection = Matrix.CreateOrthographicOffCenter(
					-7.0f * Camera.Zoom, 7.0f * Camera.Zoom, 
					-4.0f * Camera.Zoom, 4.0f * Camera.Zoom, 
					NearPlaneDistance, FarPlaneDistance);

				_defaultEffect.WorldViewProjection = _renderContext.ViewProjection;
				_defaultEffect.DiffuseColor = Color.White * Opacity;

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

				_device.SamplerStates[0] = SamplerState.PointClamp;

				foreach (var pair in Characters)
				{
					var chip = pair.Value.Chip;
					chip.Animate(pair.Value.AnimationStart, pair.Value.AnimationStep);

					// Calculate angle that the billboard needs to be rotated in order to always face the camera
					var angle = -(float)Math.Atan2(pair.Value.Position.Z - Camera.Position.Z,
						pair.Value.Position.X - Camera.Position.X) - (float)Math.PI / 2;

					// Rotate and translate
					var rot = Matrix.CreateRotationY(angle);
					var view = rot * Matrix.CreateTranslation(pair.Value.Position);

					_defaultEffect.WorldViewProjection = view * _renderContext.ViewProjection;
					_defaultEffect.Texture = chip.Texture;
					foreach (var pass in _defaultEffect.CurrentTechnique.Passes)
					{
						pass.Apply();

						_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
							chip.Vertices, 0, chip.Vertices.Length,
							chip.Indices, 0, chip.Indices.Length / 3);
					}

					if (RenderDebugInfo)
					{
						_defaultEffect.WorldViewProjection = view * _renderContext.ViewProjection;
						_defaultEffect.Texture = DefaultAssets.White;
						foreach (var pass in _defaultEffect.CurrentTechnique.Passes)
						{
							pass.Apply();

							var v = Vector3.Zero;
							var vertices = new VertexPositionNormalTexture[]
							{
								new VertexPositionNormalTexture(
									v,
									Vector3.One,
									Vector2.Zero
								),
								new VertexPositionNormalTexture(
									new Vector3(v.X, v.Y + 10, v.Z),
									Vector3.One,
									Vector2.Zero
								)
							};
							_device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
						}
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

				_spriteBatch.DrawString(DefaultAssets.Font,
					string.Format("Camera: X={0}, Y={1}, Z={2}, Zoom={3}", Camera.Position.X, Camera.Position.Y, Camera.Position.Z, Camera.Zoom), 
					new Vector2(x, y), Color.White);
				y += 20;

				_spriteBatch.End();
			}
		}

		public void Render(Rectangle bounds)
		{
			_bounds = bounds;

			RenderScene(bounds);
			_desktop.Render();
		}

		public void ShowTalk(TalkString talk, int symbolsCount)
		{
			_talkWidget._imageCharacter.Renderable = talk.Portrait;
			_talkWidget._labelName.Text = talk.Name;
			_talkWidget._labelText.Text = talk.Text.Substring(0, symbolsCount);

			if (_desktop.Root != _talkWidget)
			{
				_desktop.Root = _talkWidget;
			}
		}

		public void CloseMessageWindow()
		{
			_desktop.Root = null;
		}
	}
}
