using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace OpenSora.Tests
{
	public class ArchitectureTests
	{
		private static string FindRepositoryRoot()
		{
			var directory = new DirectoryInfo(AppContext.BaseDirectory);
			while (directory != null)
			{
				if (File.Exists(Path.Combine(directory.FullName, "OpenSora.MonoGame.sln")))
				{
					return directory.FullName;
				}

				directory = directory.Parent;
			}

			throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
		}

		private static List<string> GetProjectReferences(string projectPath)
		{
			var document = XDocument.Load(projectPath);
			return document.Descendants()
				.Where(element => element.Name.LocalName == "ProjectReference")
				.Select(element => (string)element.Attribute("Include"))
				.Where(include => !string.IsNullOrEmpty(include))
				.ToList();
		}

		[Fact]
		public void ProjectReferences_MatchAllowedGraph()
		{
			var root = FindRepositoryRoot();
			var allowed = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
			{
				["OpenSora.Common.MonoGame.csproj"] = new string[0],
				["OpenSora.Viewer.MonoGame.csproj"] = new[] { "OpenSora.Common.MonoGame.csproj" },
				["OpenSora.Unpacker.csproj"] = new[] { "OpenSora.Common.MonoGame.csproj" },
				["OpenSora.Decompiler.csproj"] = new[] { "OpenSora.Common.MonoGame.csproj" },
				["OpenSora.Benchmarks.csproj"] = new[] { "OpenSora.Common.MonoGame.csproj" },
				["OpenSora.Tests.csproj"] = new[] { "OpenSora.Common.MonoGame.csproj" }
			};

			var projects = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
				.Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
					!path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
				.ToList();

			Assert.NotEmpty(projects);

			foreach (var project in projects)
			{
				var projectName = Path.GetFileName(project);
				Assert.True(allowed.ContainsKey(projectName),
					"Project '" + projectName + "' is not in the allowed dependency graph. Update ADR 0009 and this test.");
				Assert.Equal(project, Path.GetFullPath(project));

				var projectDirectory = Path.GetDirectoryName(project);
				foreach (var include in GetProjectReferences(project))
				{
					var normalized = include.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
					var referencePath = Path.GetFullPath(Path.Combine(projectDirectory, normalized));
					Assert.True(File.Exists(referencePath),
						projectName + " references a missing project: " + include);

					var referenceName = Path.GetFileName(referencePath);
					Assert.Contains(referenceName, allowed[projectName]);
				}
			}
		}

		[Fact]
		public void SimulationCode_DoesNotReferenceGraphicsInputOrUi()
		{
			var root = FindRepositoryRoot();
			var simulationDirectories = new[]
			{
				Path.Combine(root, "OpenSora.Common", "Engine"),
				Path.Combine(root, "OpenSora.Common", "Scenarios")
			};

			var forbidden = new[]
			{
				"Microsoft.Xna.Framework.Graphics",
				"Microsoft.Xna.Framework.Input",
				"Myra",
				"GraphicsDevice",
				"SpriteBatch",
				"Texture2D",
				"TextureRegion",
				"Keyboard",
				"Mouse",
				"GameTime",
				"System.Random"
			};

			foreach (var directory in simulationDirectories)
			{
				Assert.True(Directory.Exists(directory), "Missing simulation directory: " + directory);

				foreach (var file in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
				{
					var text = File.ReadAllText(file);
					foreach (var token in forbidden)
					{
						Assert.False(text.Contains(token, StringComparison.Ordinal),
							"Simulation file " + file + " contains forbidden dependency '" + token + "'.");
					}
				}
			}
		}
	}
}
