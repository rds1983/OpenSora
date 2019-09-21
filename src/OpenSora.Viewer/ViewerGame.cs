using Imaging.DDSReader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using OpenSora.Viewer.UI;
using System;
using System.IO;
using System.Reflection;

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

		public ViewerGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// UI
			MyraEnvironment.Game = this;
			_mainPanel = new MainPanel();

			_mainPanel._buttonChange.Click += OnChangeFolder;
			_mainPanel._listFiles.SelectedIndexChanged += _listFiles_SelectedIndexChanged;

			_mainPanel._comboResourceType.SelectedIndex = 0;

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainPanel);

			var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			folder = @"D:\Games\Steam\steamapps\common\Trails in the Sky FC";
			SetFolder(folder);
		}

		private void LoadFile(FileAndEntry fileAndEntry)
		{
			if (fileAndEntry != null)
			{
				var entry = fileAndEntry.Entry;
				byte[] data;
				using (var stream = File.OpenRead(fileAndEntry.DataFilePath))
				using (var reader = new BinaryReader(stream))
				{
					stream.Seek(entry.Offset, SeekOrigin.Begin);
					data = reader.ReadBytes(entry.CompressedSize);
				}

				var image = DDS.LoadImage(data);
				_texture = new Texture2D(GraphicsDevice, image.Width, image.Height);
				_texture.SetData(image.Data);
			}
		}

		private void _listFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			var item = _mainPanel._listFiles.SelectedItem;
			if (item == null)
			{
				LoadFile(null);
			} else
			{
				var entry = (FileAndEntry)item.Tag;
				LoadFile(entry);
			}
		}

		private void SetFolder(string folder)
		{
			_mainPanel._listFiles.Items.Clear();

			var entries = DirProcessor.BuildEntries(folder);

			foreach(var pair in entries)
			{
				foreach(var entry in pair.Value)
				{
					if (entry.Name.EndsWith("_DS"))
					{
						_mainPanel._listFiles.Items.Add(new ListItem
						{
							Text = entry.Name,
							Tag = new FileAndEntry
							{
								DataFilePath = pair.Key,
								Entry = entry
							}
						});
					}
				}
			}

			_mainPanel._textPath.Text = folder;
		}

		private void OnChangeFolder(object sender, EventArgs e)
		{
;			var dlg = new FileDialog(FileDialogMode.ChooseFolder);

			try
			{
				if (!string.IsNullOrEmpty(_mainPanel._textPath.Text))
				{
					dlg.Folder = _mainPanel._textPath.Text;
				} else
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

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_desktop.Render();

			if (_texture != null)
			{
				_spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

				var location = _mainPanel._panelViewer.Bounds.Center;
				location.X -= _texture.Width / 2;
				location.Y -= _texture.Height / 2;

				_spriteBatch.Draw(_texture, location.ToVector2(), Color.White);

				_spriteBatch.End();
			}
		}
	}
}