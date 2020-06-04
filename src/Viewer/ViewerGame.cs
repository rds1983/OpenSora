using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using OpenSora.Viewer.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenSora.Dir;
using OpenSora.ModelLoading;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenSora.Rendering;

namespace OpenSora.Viewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private MainPanel _mainPanel;
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;
		private Animation _animation;
		private List<DirEntry> _typeEntries = null;
		private readonly State _state;
		private Queue<string> _statusMessages = new Queue<string>();
		private SceneRenderer _renderer;
		private ResourceLoader _resourceLoader;

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

			_mainPanel._textFilter.TextChanged += _textFilter_TextChanged;

			_mainPanel._comboResourceType.SelectedIndexChanged += _comboResourceType_SelectedIndexChanged;
			_mainPanel._comboResourceType.SelectedIndex = 4;

			_mainPanel._comboFunctions.SelectedIndexChanged += _comboFunctions_SelectedIndexChanged;

			_mainPanel._numericAnimationStart.ValueChanged += _numericAnimationStart_ValueChanged;
			_mainPanel._numericAnimationStep.ValueChanged += _numericAnimationStep_ValueChanged;

			PushStatusMessage(string.Empty);

			Desktop.Root = _mainPanel;

			if (_state != null)
			{
				SetFolder(_state.LastFolder);
			}

			_renderer = new SceneRenderer(GraphicsDevice);
		}

		private void ResetAnimation()
		{
			if (_animation == null)
			{
				return;
			}

			_animation.FrameIndex = (int)_mainPanel._numericAnimationStart.Value.Value;
			_animation.LastFrameRendered = null;
		}

		private void HideAnimationBox()
		{
			if (!_mainPanel._boxTop.Widgets.Contains(_mainPanel._boxAnimation))
			{
				return;
			}

			_mainPanel._boxTop.Proportions.RemoveAt(2);
			_mainPanel._boxTop.Widgets.Remove(_mainPanel._boxAnimation);
		}

		private void ShowAnimationBox()
		{
			if (_mainPanel._boxTop.Widgets.Contains(_mainPanel._boxAnimation))
			{
				return;
			}

			_mainPanel._boxTop.Proportions.Insert(2, new Proportion(ProportionType.Part, 1.0f));
			_mainPanel._boxTop.Widgets.Insert(2, _mainPanel._boxAnimation);
		}

		private void HideFunctionsBox()
		{
			if (!_mainPanel._boxTop.Widgets.Contains(_mainPanel._boxScenarios))
			{
				return;
			}

			_mainPanel._boxTop.Proportions.RemoveAt(2);
			_mainPanel._boxTop.Widgets.Remove(_mainPanel._boxScenarios);
		}

		private void ShowFunctionsBox()
		{
			if (_mainPanel._boxTop.Widgets.Contains(_mainPanel._boxScenarios))
			{
				return;
			}

			_mainPanel._boxTop.Proportions.Insert(2, new Proportion(ProportionType.Part, 1.0f));
			_mainPanel._boxTop.Widgets.Insert(2, _mainPanel._boxScenarios);
		}

		private void _numericAnimationStep_ValueChanged(object sender, Myra.Utility.ValueChangedEventArgs<float?> e)
		{
			ResetAnimation();
		}

		private void _numericAnimationStart_ValueChanged(object sender, Myra.Utility.ValueChangedEventArgs<float?> e)
		{
			ResetAnimation();
		}

		private void _textFilter_TextChanged(object sender, Myra.Utility.ValueChangedEventArgs<string> e)
		{
			RefreshFilesSafe();
		}

		private void OnAbout(object sender, EventArgs e)
		{
			var name = new AssemblyName(typeof(FalcomDecompressor).Assembly.FullName);
			var messageBox = Dialog.CreateMessageBox("About", "OpenSora.Viewer " + name.Version.ToString());
			messageBox.ShowModal();
		}

		private void _comboResourceType_SelectedIndexChanged(object sender, EventArgs e)
		{
			_typeEntries = null;
			_animation = null;
			_texture = null;
			ResetAnimation();
			RefreshFilesSafe();

			var idx = _mainPanel._comboResourceType.SelectedIndex;
			if (idx != 3)
			{
				HideAnimationBox();
			} else
			{
				ShowAnimationBox();
			}

			if (idx != 4)
			{
				HideFunctionsBox();
			} else
			{
				ShowFunctionsBox();
			}
		}

		private void PushStatusMessage(string text)
		{
			lock (_statusMessages)
			{
				_statusMessages.Enqueue(text);
			}
		}

		private void ProcessMesh(List<ModelMeshPart> meshes, string statusPrefix, MeshData meshData, int meshCount)
		{
			SceneRenderer.AddMeshData(GraphicsDevice, meshes, meshData, _resourceLoader.LoadTexture);

			PushStatusMessage(string.Format(statusPrefix + "Meshed proceeded {0}/{1}", meshes.Count, meshCount));
		}

		private void LoadModel(DirEntry DirEntry)
		{
			var statusPrefix = string.Format("Loading '{0}'", DirEntry.Name);
			PushStatusMessage(statusPrefix);
			var data = _resourceLoader.LoadData(DirEntry);

			statusPrefix += ": ";

			PushStatusMessage(statusPrefix + "Decompressing");
			var decompressed = FalcomDecompressor.Decompress(data);

			PushStatusMessage(statusPrefix + "Parsing mesh data");

			Frame frame;
			using (var stream = new MemoryStream(decompressed))
			{
				frame = ModelLoader.Load(stream, DirEntry.Name.EndsWith("_X2") ? 2 : 3);
			}

			var meshes = frame.Children[0].Meshes;

			var modelMeshes = new List<ModelMeshPart>();

			var tasks = new List<Task>();

			var meshesCount = (from m in meshes select m.Materials.Count).Sum();
			foreach (var meshData in meshes)
			{
				tasks.Add(Task.Factory.StartNew(() =>
				{
					ProcessMesh(modelMeshes, statusPrefix, meshData, meshesCount);
				}));
			}

			Task.WaitAll(tasks.ToArray());

			_renderer.Meshes = modelMeshes;
		}

		private void LoadFile(DirEntry entry)
		{
			if (entry == null)
			{
				return;
			}

			switch (_mainPanel._comboResourceType.SelectedIndex)
			{
				case 0:
					// Texture
					_texture = _resourceLoader.LoadTexture(entry);
					break;
				case 1:
					// Model
					LoadModel(entry);
					break;
				case 2:
					_texture = _resourceLoader.LoadImage(entry);
					break;
				case 3:
					{
						_animation = _resourceLoader.LoadAnimation(entry);
						ResetAnimation();

						_mainPanel._numericAnimationStart.Maximum = (_animation.Info != null ? _animation.Info.Length : 0);
						_mainPanel._textAnimationTotal.Text = "Total: " + (_animation.Info != null ? _animation.Info.Length : 0).ToString();
					}
					break;
				case 4:
					{
						var scenario = _resourceLoader.LoadScenario(entry);

						_mainPanel._textScenarioLocation.Text = "Location: " + scenario.Location;

						_mainPanel._comboFunctions.Items.Clear();
						foreach(var function in scenario.Functions)
						{
							_mainPanel._comboFunctions.Items.Add(new ListItem
							{
								Text = string.Format("0x{0:X}", function.Offset),
								Tag = function
							});
						}

						var parts = scenario.Location.Split('.');
						var modelEntry = _resourceLoader.FindByName(parts[0], parts[1]);
						if (modelEntry != null)
						{
							LoadModel(modelEntry);
						}
					}
					break;
			}

			lock (_statusMessages)
			{
				_statusMessages.Clear();
			}
			PushStatusMessage(string.Empty);
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
				var entry = (DirEntry)item.Tag;

				Task.Factory.StartNew(() =>
				{
					try
					{
						_mainPanel._listFiles.Enabled = false;
						LoadFile(entry);
					}
					catch (Exception ex)
					{
						PushStatusMessage(string.Format("Failed to loading '{0}': {1}", entry.Name, ex.Message));
					}
					finally
					{
						_mainPanel._listFiles.Enabled = true;
					}
				});
			}
		}

		private void RefreshFiles()
		{
			if (_resourceLoader == null)
			{
				return;
			}

			_resourceLoader.ClearTextureCache();
			_mainPanel._listFiles.Items.Clear();

			var index = _mainPanel._comboResourceType.SelectedIndex;

			if (_typeEntries == null)
			{
				_typeEntries = new List<DirEntry>();
				foreach (var pair in _resourceLoader.Entries)
				{
					foreach (var entry in pair.Value)
					{
						var add = index == 0 && entry.Name.EndsWith("_DS") ||
							index == 1 && (entry.Name.EndsWith("_X2") || entry.Name.EndsWith("_X3")) ||
							(index == 2 && entry.Name.EndsWith("_CH") && !entry.Name.StartsWith("CH")) ||
							(index == 3 && entry.Name.EndsWith("_CH") && entry.Name.StartsWith("CH")) ||
							(index == 4 && entry.Name.EndsWith("_SN"));

						if (add)
						{
							_typeEntries.Add(entry);
						}
					}
				}

				// Sort
				_typeEntries = (from DirEntry a in _typeEntries orderby a.Name select a).ToList();
			}

			if (!string.IsNullOrEmpty(_mainPanel._textFilter.Text))
			{
				Debug.WriteLine(_mainPanel._textFilter.Text);
			}

			// Add to listbox
			foreach (var a in _typeEntries)
			{
				if (!string.IsNullOrEmpty(_mainPanel._textFilter.Text))
				{
					var name = Path.GetFileNameWithoutExtension(a.Name);
					if (name.IndexOf(_mainPanel._textFilter.Text, StringComparison.InvariantCultureIgnoreCase) == -1)
					{
						continue;
					}
				}

				_mainPanel._listFiles.Items.Add(new ListItem
				{
					Text = a.Name,
					Tag = a
				});
			}

			if (index == 4)
			{
				int? idx = null;
				for(var i = 0; i < _mainPanel._listFiles.Items.Count; ++i)
				{
					if (_mainPanel._listFiles.Items[i].Text.Contains("T0310"))
					{
						idx = i;
						break;
					}
				}

				if (idx != null)
				{
					_mainPanel._listFiles.SelectedIndex = idx.Value;
				}
			}
		}

		private void RefreshFilesSafe()
		{
			try
			{
				RefreshFiles();
			}
			catch (Exception ex)
			{
				var msg = Dialog.CreateMessageBox("Error", ex.ToString());
				msg.ShowModal();
			}
		}

		private void _comboFunctions_SelectedIndexChanged(object sender, EventArgs e)
		{
			var k = _mainPanel._comboFunctions.SelectedIndex;
		}

		private void SetFolder(string folder)
		{
			try
			{
				_mainPanel._textPath.Text = folder;
				_resourceLoader = null;
				_typeEntries = null;
				if (!string.IsNullOrEmpty(folder))
				{
					_resourceLoader = new ResourceLoader(GraphicsDevice, folder);
				}

				RefreshFiles();
			}
			catch (Exception ex)
			{
				var msg = Dialog.CreateMessageBox("Error", ex.ToString());
				msg.ShowModal();
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

			dlg.ShowModal();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			//			_fpsCounter.Update(gameTime);

			if (_mainPanel._comboResourceType.SelectedIndex != 1 || _renderer.Meshes == null)
			{
				return;
			}

			var keyboardState = Keyboard.GetState();

			// Manage camera input controller
			_renderer.Controller.SetControlKeyState(CameraInputController.ControlKeys.Left, keyboardState.IsKeyDown(Keys.A));
			_renderer.Controller.SetControlKeyState(CameraInputController.ControlKeys.Right, keyboardState.IsKeyDown(Keys.D));
			_renderer.Controller.SetControlKeyState(CameraInputController.ControlKeys.Forward, keyboardState.IsKeyDown(Keys.W));
			_renderer.Controller.SetControlKeyState(CameraInputController.ControlKeys.Backward, keyboardState.IsKeyDown(Keys.S));
			_renderer.Controller.SetControlKeyState(CameraInputController.ControlKeys.Up, keyboardState.IsKeyDown(Keys.Up));
			_renderer.Controller.SetControlKeyState(CameraInputController.ControlKeys.Down, keyboardState.IsKeyDown(Keys.Down));

			var bounds = _mainPanel._panelViewer.Bounds;
			var mouseState = Mouse.GetState();
			if (bounds.Contains(mouseState.Position) && !Desktop.IsMouseOverGUI)
			{
				_renderer.Controller.SetTouchState(CameraInputController.TouchType.Move, mouseState.LeftButton == ButtonState.Pressed);
				_renderer.Controller.SetTouchState(CameraInputController.TouchType.Rotate, mouseState.RightButton == ButtonState.Pressed);

				_renderer.Controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));
			}

			_renderer.Controller.Update();
		}

		private void DrawTexture(Texture2D texture)
		{
			if(texture == null)
			{
				return;
			}

			_spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

			var location = _mainPanel._panelViewer.Bounds.Center;
			location.X -= texture.Width / 2;
			location.Y -= texture.Height / 2;

			if (location.X < _mainPanel._panelViewer.Bounds.X)
			{
				location.X = _mainPanel._panelViewer.Bounds.X;
			}

			if (location.Y < _mainPanel._panelViewer.Bounds.Y)
			{
				location.Y = _mainPanel._panelViewer.Bounds.Y;
			}

			_spriteBatch.Draw(texture, location.ToVector2(), Color.White);

			_spriteBatch.End();
		}

		private void DrawAnimation()
		{
			if (_animation == null)
			{
				return;
			}

			_spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

			var location = _mainPanel._panelViewer.Bounds.Center;
			location.X -= 128;
			location.Y -= 128;

			if (location.X < _mainPanel._panelViewer.Bounds.X)
			{
				location.X = _mainPanel._panelViewer.Bounds.X;
			}

			if (location.Y < _mainPanel._panelViewer.Bounds.Y)
			{
				location.Y = _mainPanel._panelViewer.Bounds.Y;
			}

			_animation.Animate((int)_mainPanel._numericAnimationStart.Value.Value, (int)_mainPanel._numericAnimationStep.Value.Value);
			_animation.Render(_spriteBatch, location);

			_spriteBatch.End();
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			var idx = _mainPanel._comboResourceType.SelectedIndex;
			if (idx == 0 || idx == 2)
			{
				DrawTexture(_texture);
			}
			else if (idx == 1 || idx == 4)
			{
				_renderer.Render(_mainPanel._panelViewer.Bounds);
			}
			else if (idx == 3)
			{
				DrawAnimation();
			}

			lock (_statusMessages)
			{
				if (_statusMessages.Count > 0)
				{
					_mainPanel._textStatus.Text = _statusMessages.Dequeue();
				}
			}

			Desktop.Render();
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