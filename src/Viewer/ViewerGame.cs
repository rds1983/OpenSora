using Imaging.DDSReader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Nursia;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using Nursia.Graphics3D.Utils;
using OpenSora.Viewer.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenSora;
using OpenSora.Dir;
using OpenSora.ModelLoading;

namespace OpenSora.Viewer
{
	public class ViewerGame : Game
	{
		class FileAndEntry
		{
			public string DataFilePath;
			public DirEntry Entry;
		}

		private readonly GraphicsDeviceManager _graphics;
		private Desktop _desktop = null;
		private MainPanel _mainPanel;
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;
		private Dictionary<string, List<DirEntry>> _entries;
		private NursiaModel _model;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private readonly Scene _scene = new Scene();
		private CameraInputController _controller;
		private readonly State _state;
		private static readonly List<DirectLight> _defaultLights = new List<DirectLight>();

		static ViewerGame()
		{
			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f),
				Color = new Color(1, 0.9607844f, 0.8078432f)
			});

			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f),
				Color = new Color(0.9647059f, 0.7607844f, 0.4078432f)
			});

			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f),
				Color = new Color(0.3231373f, 0.3607844f, 0.3937255f)
			});
		}

		public ViewerGame()
		{
			// Restore state
			_state = State.Load();

			if (_state != null)
			{
				_graphics = new GraphicsDeviceManager(this)
				{
					PreferredBackBufferWidth = _state.Size.X,
					PreferredBackBufferHeight = _state.Size.Y
				};
			}
			else
			{
				_graphics = new GraphicsDeviceManager(this)
				{
					PreferredBackBufferWidth = 1200,
					PreferredBackBufferHeight = 800
				};
			}

			Window.AllowUserResizing = true;
			IsMouseVisible = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}

			Window.Title = "OpenSora.Viewer";
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// UI
			MyraEnvironment.Game = this;
			_mainPanel = new MainPanel();

			_mainPanel._buttonChange.Click += OnChangeFolder;
			_mainPanel._buttonAbout.Click += OnAbout;
			_mainPanel._listFiles.SelectedIndexChanged += _listFiles_SelectedIndexChanged;

			_mainPanel._comboResourceType.SelectedIndexChanged += _comboResourceType_SelectedIndexChanged;
			_mainPanel._comboResourceType.SelectedIndex = 1;

			_mainPanel._checkLightning.PressedChanged += _checkLightning_PressedChanged;

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainPanel);

			if (_state != null)
			{
				SetFolder(_state.LastFolder);
			}

			// Nursia
			Nrs.Game = this;

			// Set camera
			_scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);
			_controller = new CameraInputController(_scene.Camera);

			_renderer.RasterizerState = RasterizerState.CullCounterClockwise;
			_renderer.BlendState = BlendState.Opaque;
		}

		private void _checkLightning_PressedChanged(object sender, EventArgs e)
		{
			_scene.Lights.Clear();
			if (_mainPanel._checkLightning.IsPressed)
			{
				_scene.Lights.AddRange(_defaultLights);
			}
		}

		private void OnAbout(object sender, EventArgs e)
		{
			var name = new AssemblyName(typeof(FalcomDecompressor).Assembly.FullName);
			var messageBox = Dialog.CreateMessageBox("About", "OpenSora.Viewer " + name.Version.ToString());
			messageBox.ShowModal(_desktop);
		}

		private void _comboResourceType_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				_mainPanel._checkLightning.Visible = _mainPanel._comboResourceType.SelectedIndex == 1;

				RefreshFiles();
			}
			catch (Exception ex)
			{
				var msg = Dialog.CreateMessageBox("Error", ex.ToString());
				msg.ShowModal(_desktop);
			}
		}

		private byte[] LoadData(string dataFilePath, DirEntry entry)
		{
			byte[] data;
			using (var stream = File.OpenRead(dataFilePath))
			using (var reader = new BinaryReader(stream))
			{
				stream.Seek(entry.Offset, SeekOrigin.Begin);
				data = reader.ReadBytes(entry.CompressedSize);
			}

			return data;
		}

		private Texture2D TextureFromEntry(string dataFilePath, DirEntry entry)
		{
			var data = LoadData(dataFilePath, entry);

			var image = DDS.LoadImage(data);
			var texture = new Texture2D(GraphicsDevice, image.Width, image.Height);
			texture.SetData(image.Data);

			return texture;
		}

		private Texture2D LoadTexture(string name)
		{
			var nameWithoutExt = Path.GetFileNameWithoutExtension(name);

			foreach (var pair in _entries)
			{
				foreach (var entry in pair.Value)
				{
					var ext = Path.GetExtension(entry.Name);
					if (ext == "._DS" && entry.Name.IndexOf(nameWithoutExt, StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						return TextureFromEntry(pair.Key, entry);
					}
				}
			}

			return DefaultAssets.White;
		}

		private void LoadFile(FileAndEntry fileAndEntry)
		{
			if (fileAndEntry != null)
			{
				var data = LoadData(fileAndEntry.DataFilePath, fileAndEntry.Entry);

				switch (_mainPanel._comboResourceType.SelectedIndex)
				{
					case 0:
						// Texture
						var image = DDS.LoadImage(data);
						_texture = new Texture2D(GraphicsDevice, image.Width, image.Height);
						_texture.SetData(image.Data);

/*						using (var output = File.OpenWrite(@"D:\Temp\" + fileAndEntry.Entry.Name + ".png"))
						{
							_texture.SaveAsPng(output, _texture.Width, _texture.Height);
						}*/
						break;
					case 1:
						// Model
						var decompressed = FalcomDecompressor.Decompress(data);
						Frame frame;
						using (var stream = new MemoryStream(decompressed))
						{
							frame = ModelLoader.Load(stream);
						}

						var meshes = frame.Children[0].Meshes;

						_model = new NursiaModel();

						foreach (var meshData in meshes)
						{
							var mesh = Mesh.Create(meshData.Vertices.ToArray(), meshData.Indices.ToArray());

							foreach (var md in meshData.Materials)
							{
								if (md.PrimitivesCount == 0 || md.VerticesCount == 0)
								{
									continue;
								}

								var meshNode = new MeshNode
								{
									Id = meshData.Id,
									BoundingSphere = BoundingSphere.CreateFromPoints(from v in meshData.Vertices select v.Position)
								};

								var material = new Material
								{
									DiffuseColor = Color.White,
									Texture = LoadTexture(md.TextureName)
								};

								var part = new MeshPart
								{
									BoundingSphere = meshNode.BoundingSphere,
									Mesh = mesh,
									Material = material,
									StartVertex = md.VerticesStart,
									VertexCount = md.VerticesCount,
									StartIndex = md.PrimitivesStart * 3,
									PrimitiveCount = md.PrimitivesCount
								};
								meshNode.Parts.Add(part);
								_model.Meshes.Add(meshNode);
							}
						}

						_scene.Models.Clear();
						_scene.Models.Add(_model);

						_scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);
						break;
				}
			}
		}

		private void _listFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			var item = _mainPanel._listFiles.SelectedItem;
			if (item == null)
			{
				LoadFile(null);
			}
			else
			{
				var entry = (FileAndEntry)item.Tag;
				try
				{
					LoadFile(entry);
				}
				catch (Exception ex)
				{
					var msg = Dialog.CreateMessageBox("Error", ex.ToString());
					msg.ShowModal(_desktop);
				}
			}
		}

		private void RefreshFiles()
		{
			_mainPanel._listFiles.Items.Clear();
			if (_entries == null)
			{
				return;
			}

			var toAdd = new List<FileAndEntry>();
			foreach (var pair in _entries)
			{
				foreach (var entry in pair.Value)
				{
					var index = _mainPanel._comboResourceType.SelectedIndex;
					var add = index == 0 && entry.Name.EndsWith("_DS") ||
						index == 1 && entry.Name.EndsWith("_X2");

					if (add)
					{
						toAdd.Add(new FileAndEntry
						{
							DataFilePath = pair.Key,
							Entry = entry
						});
					}
				}
			}

			// Sort
			toAdd = (from FileAndEntry a in toAdd orderby a.Entry.Name select a).ToList();

			// Add to listbox
			foreach (var a in toAdd)
			{
				_mainPanel._listFiles.Items.Add(new ListItem
				{
					Text = a.Entry.Name,
					Tag = a
				});
			}
		}

		private void SetFolder(string folder)
		{
			try
			{
				_mainPanel._textPath.Text = folder;
				_entries = null;
				if (!string.IsNullOrEmpty(folder))
				{
					_entries = DirProcessor.BuildEntries(folder);
				}

				RefreshFiles();
			}
			catch (Exception ex)
			{
				var msg = Dialog.CreateMessageBox("Error", ex.ToString());
				msg.ShowModal(_desktop);
			}
		}

		private void OnChangeFolder(object sender, EventArgs e)
		{
			var dlg = new FileDialog(FileDialogMode.ChooseFolder);

			try
			{
				if (!string.IsNullOrEmpty(_mainPanel._textPath.Text))
				{
					dlg.Folder = _mainPanel._textPath.Text;
				}
				else
				{
					var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
					dlg.Folder = folder;
				}
			}
			catch (Exception)
			{
			}

			dlg.Closed += (s, a) =>
			{
				if (!dlg.Result)
				{
					return;
				}

				SetFolder(dlg.FilePath);
			};

			dlg.ShowModal(_desktop);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			//			_fpsCounter.Update(gameTime);

			if (_mainPanel._comboResourceType.SelectedIndex != 1 || _model == null)
			{
				return;
			}

			var keyboardState = Keyboard.GetState();

			// Manage camera input controller
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Left, keyboardState.IsKeyDown(Keys.A));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Right, keyboardState.IsKeyDown(Keys.D));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Forward, keyboardState.IsKeyDown(Keys.W));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Backward, keyboardState.IsKeyDown(Keys.S));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Up, keyboardState.IsKeyDown(Keys.Up));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Down, keyboardState.IsKeyDown(Keys.Down));

			var bounds = _mainPanel._panelViewer.Bounds;
			var mouseState = Mouse.GetState();
			if (bounds.Contains(mouseState.Position))
			{
				_controller.SetTouchState(CameraInputController.TouchType.Move, mouseState.LeftButton == ButtonState.Pressed);
				_controller.SetTouchState(CameraInputController.TouchType.Rotate, mouseState.RightButton == ButtonState.Pressed);

				_controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));
			}

			_controller.Update();
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.CornflowerBlue);

			_desktop.Render();

			var bounds = _mainPanel._panelViewer.Bounds;

			if (_desktop.Widgets.Count > 1 && _desktop.Widgets[1] is Window)
			{
				// Do not draw resource if there's a window
				return;
			}

			if (_mainPanel._comboResourceType.SelectedIndex == 0 && _texture != null)
			{
				_spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

				var location = _mainPanel._panelViewer.Bounds.Center;
				location.X -= _texture.Width / 2;
				location.Y -= _texture.Height / 2;

				_spriteBatch.Draw(_texture, location.ToVector2(), Color.White);

				_spriteBatch.End();
			}
			else if (_mainPanel._comboResourceType.SelectedIndex == 1 && _model != null)
			{
				var device = GraphicsDevice;
				var oldViewport = device.Viewport;
				try
				{
					device.Viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);
					_renderer.Begin();
					_renderer.DrawScene(_scene);
					_renderer.End();
				}
				finally
				{
					device.Viewport = oldViewport;
				}
			}
		}

		protected override void EndRun()
		{
			base.EndRun();

			var state = new State
			{
				Size = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight),
				LastFolder = _mainPanel._textPath.Text,
			};

			state.Save();
		}
	}
}