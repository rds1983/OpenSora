﻿using Imaging.DDSReader;
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
using System.Collections.Concurrent;
using OpenSora.Scenarios;
using System.Diagnostics;

namespace OpenSora.Viewer
{
	public class ViewerGame : Game
	{
		private const int ApplicationFrameChangeDelayInMs = 100;

		class FileAndEntry
		{
			public FileAndEntry(string dataFilePath, DirEntry entry)
			{
				DataFilePath = dataFilePath;
				Entry = entry;
			}

			public string DataFilePath;
			public DirEntry Entry;
		}

		private readonly GraphicsDeviceManager _graphics;
		private MainPanel _mainPanel;
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;
		private ushort?[][,] _animationInfo;
		private DateTime? _animationLastFrameRendered;
		private int _animationFrameIndex = 0;
		private Dictionary<string, List<DirEntry>> _entries = null;
		private List<FileAndEntry> _typeEntries = null;
		private readonly State _state;
		private Queue<string> _statusMessages = new Queue<string>();
		private readonly ConcurrentDictionary<string, Texture2D> _textures = new ConcurrentDictionary<string, Texture2D>();
		private SceneRenderer _renderer;

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
			_animationFrameIndex = (int)_mainPanel._numericAnimationStart.Value.Value;
			_animationLastFrameRendered = null;
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
			_animationInfo = null;
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

		private FileAndEntry FindByName(string nameFilter, string extFilter)
		{
			nameFilter = nameFilter.ToLower();
			extFilter = extFilter.ToLower();
			foreach (var pair in _entries)
			{
				foreach (var entry in pair.Value)
				{
					var ename = entry.Name;
					if (entry.Name.Contains("."))
					{
						var parts = entry.Name.Split('.');
						if(parts[0].ToLower().Contains(nameFilter) && parts[1].ToLower().Contains(extFilter))
						{
							return new FileAndEntry(pair.Key, entry);
						}
					} else
					{
						if (ename.ToLower().Contains(nameFilter))
						{
							return new FileAndEntry(pair.Key, entry);
						}
					}
				}
			}

			return null;
		}

		private byte[] LoadData(string dataFilePath, DirEntry entry)
		{
			byte[] data;
			using (var stream = File.OpenRead(dataFilePath))
			using (var reader = new BinaryReader(stream))
			{
				stream.Seek(entry.Offset, SeekOrigin.Begin);
				data = reader.ReadBytes(entry.CompressedSize > 0 ? entry.CompressedSize : entry.DecompressedSize);
			}

			return data;
		}

		private byte[] LoadData(FileAndEntry fileAndEntry)
		{
			return LoadData(fileAndEntry.DataFilePath, fileAndEntry.Entry);
		}

		private unsafe Texture2D TextureFromEntry(string dataFilePath, DirEntry entry)
		{
			Texture2D texture;
			if (_textures.TryGetValue(entry.Name, out texture))
			{
				return texture;
			}

			var data = LoadData(dataFilePath, entry);
			if (data[0] != 'D' || data[1] != 'D' || data[2] != 'S' || data[3] != ' ')
			{
				// Compressed
				data = FalcomDecompressor.Decompress(data);
			}

			using (var stream = new MemoryStream(data))
			{
				var image = DDS.LoadImage(stream);
				texture = new Texture2D(GraphicsDevice, image.Width, image.Height);
				texture.SetData(image.Data);
			}

			_textures[entry.Name] = texture;

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

		private void PushStatusMessage(string text)
		{
			lock (_statusMessages)
			{
				_statusMessages.Enqueue(text);
			}
		}

		private void ProcessMesh(List<ModelMeshPart> meshes, string statusPrefix, MeshData meshData, int meshCount)
		{
			SceneRenderer.AddMeshData(GraphicsDevice, meshes, meshData, LoadTexture);

			PushStatusMessage(string.Format(statusPrefix + "Meshed proceeded {0}/{1}", meshes.Count, meshCount));
		}

		private void LoadModel(FileAndEntry fileAndEntry)
		{
			var statusPrefix = string.Format("Loading '{0}'", fileAndEntry.Entry.Name);
			PushStatusMessage(statusPrefix);
			var data = LoadData(fileAndEntry.DataFilePath, fileAndEntry.Entry);

			statusPrefix += ": ";

			PushStatusMessage(statusPrefix + "Decompressing");
			var decompressed = FalcomDecompressor.Decompress(data);

			PushStatusMessage(statusPrefix + "Parsing mesh data");

			Frame frame;
			using (var stream = new MemoryStream(decompressed))
			{
				frame = ModelLoader.Load(stream, fileAndEntry.Entry.Name.EndsWith("_X2") ? 2 : 3);
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

		private void LoadFile(FileAndEntry fileAndEntry)
		{
			if (fileAndEntry == null)
			{
				return;
			}

			switch (_mainPanel._comboResourceType.SelectedIndex)
			{
				case 0:
					// Texture
					_texture = TextureFromEntry(fileAndEntry.DataFilePath, fileAndEntry.Entry);
					break;
				case 1:
					// Model
					LoadModel(fileAndEntry);
					break;
				case 2:
					{
						// Image
						var chData = FalcomDecompressor.Decompress(LoadData(fileAndEntry));
						using (var chStream = new MemoryStream(chData))
						{
							_texture = ChLoader.LoadImage(GraphicsDevice, fileAndEntry.DataFilePath, fileAndEntry.Entry.Name, chStream);
						}
					}
					break;
				case 3:
					{
						Texture2D texture;
						var chData = FalcomDecompressor.Decompress(LoadData(fileAndEntry));
						using (var chStream = new MemoryStream(chData))
						{
							texture = AnimationLoader.LoadImage(GraphicsDevice, chStream);
						}

						ushort?[][,] animationInfo;
						var cpFile = Path.GetFileNameWithoutExtension(fileAndEntry.Entry.Name);
						if (cpFile.EndsWith(" "))
						{
							cpFile = cpFile.Substring(0, cpFile.Length - 1) + "P";
						}
						var cpFileEntry = FindByName(cpFile, "_CP");
						var cpData = FalcomDecompressor.Decompress(LoadData(cpFileEntry));
						using (var cpStream = new MemoryStream(cpData))
						{
							animationInfo = AnimationLoader.LoadInfo(cpStream);
						}

						_texture = texture;
						_animationInfo = animationInfo;
						ResetAnimation();

						_mainPanel._numericAnimationStart.Maximum = (_animationInfo != null ? _animationInfo.Length : 0);
						_mainPanel._textAnimationTotal.Text = "Total: " + (_animationInfo != null ? _animationInfo.Length : 0).ToString();
					}
					break;
				case 4:
					{
						var data = FalcomDecompressor.Decompress(LoadData(fileAndEntry));
						Scenario scenario;
						using (var stream = new MemoryStream(data))
						{
							scenario = Scenario.FromFCStream(stream);
						}

						_mainPanel._textScenarioLocation.Text = "Location: " + scenario.Location;

						_mainPanel._comboFunctions.Items.Clear();
						foreach(var idx in scenario.Funtions)
						{
							_mainPanel._comboFunctions.Items.Add(new ListItem
							{
								Text = string.Format("0x{0:X}", idx),
								Tag = idx
							});
						}

						var parts = scenario.Location.Split('.');
						var modelEntry = FindByName(parts[0], parts[1]);
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
				var entry = (FileAndEntry)item.Tag;

				Task.Factory.StartNew(() =>
				{
					try
					{
						_mainPanel._listFiles.Enabled = false;
						LoadFile(entry);
					}
					catch (Exception ex)
					{
						PushStatusMessage(string.Format("Failed to loading '{0}': {1}", entry.Entry.Name, ex.Message));
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
			_textures.Clear();
			_mainPanel._listFiles.Items.Clear();
			if (_entries == null)
			{
				return;
			}

			var index = _mainPanel._comboResourceType.SelectedIndex;

			if (_typeEntries == null)
			{
				_typeEntries = new List<FileAndEntry>();
				foreach (var pair in _entries)
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
							_typeEntries.Add(new FileAndEntry(pair.Key, entry));
						}
					}
				}

				// Sort
				_typeEntries = (from FileAndEntry a in _typeEntries orderby a.Entry.Name select a).ToList();
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
					var name = Path.GetFileNameWithoutExtension(a.Entry.Name);
					if (name.IndexOf(_mainPanel._textFilter.Text, StringComparison.InvariantCultureIgnoreCase) == -1)
					{
						continue;
					}
				}

				_mainPanel._listFiles.Items.Add(new ListItem
				{
					Text = a.Entry.Name,
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
				_entries = null;
				_typeEntries = null;
				if (!string.IsNullOrEmpty(folder))
				{
					_entries = DirProcessor.BuildEntries(folder);
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
			if (_texture == null || _animationInfo == null || _animationInfo.Length == 0)
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

			var now = DateTime.Now;
			if (_animationLastFrameRendered == null ||
				(_animationLastFrameRendered != null &&
				(now - _animationLastFrameRendered.Value).TotalMilliseconds >= ApplicationFrameChangeDelayInMs))
			{
				_animationFrameIndex += (int)_mainPanel._numericAnimationStep.Value.Value;
				_animationLastFrameRendered = now;
			}

			if (_animationFrameIndex >= _animationInfo.Length)
			{
				_animationFrameIndex = Math.Min((int)_mainPanel._numericAnimationStart.Value.Value, _animationInfo.Length - 1);
			}

			ushort?[,] data = _animationInfo[_animationFrameIndex];
			for(var y = 0; y <  data.GetLength(0); ++y)
			{
				for(var x = 0; x < data.GetLength(1); ++x)
				{
					var val = data[y, x];
					if (val == null)
					{
						continue;
					}

					var tileX = val.Value % AnimationLoader.ChunksPerRow;
					var tileY = val.Value / AnimationLoader.ChunksPerRow;

					var loc = new Vector2(location.X + (x * AnimationLoader.ChunkSize),
										  location.Y + (y * AnimationLoader.ChunkSize));
					var rect = new Rectangle(tileX * AnimationLoader.ChunkSize,
						tileY * AnimationLoader.ChunkSize,
						AnimationLoader.ChunkSize, AnimationLoader.ChunkSize);
					_spriteBatch.Draw(_texture, loc, rect, Color.White);
				}
			}

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