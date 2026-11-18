using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenSora.Tests
{
	public class RepositoryPolicyTests
	{
		private const long MaxTrackedFileBytes = 5L * 1024 * 1024;

		private static readonly HashSet<string> ForbiddenExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".dat", ".dir", ".dds", ".ch", ".cp", ".sn", ".x2", ".x3",
			".wav", ".ogg", ".avi", ".sav", ".srm", ".iso", ".bin", ".exe", ".dll"
		};

		private static readonly string[] ForbiddenSuffixes =
		{
			"._DS", "._CH", "._CP", "._SN", "._X2", "._X3", "._DT",
			"._OP", "._CT", "._HD", "._EN", "._SY", "._CL", "._EP",
			"._LM", "._DA", "._MH", "._VS"
		};

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

		private static List<string> GetTrackedFiles(string root)
		{
			var startInfo = new ProcessStartInfo("git", "ls-files -z")
			{
				WorkingDirectory = root,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			using (var process = Process.Start(startInfo))
			{
				Assert.NotNull(process);
				var output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				Assert.Equal(0, process.ExitCode);

				return output.Split('\0', StringSplitOptions.RemoveEmptyEntries)
					.Select(path => path.Replace('/', Path.DirectorySeparatorChar))
					.ToList();
			}
		}

		[Fact]
		public void NoTrackedGameAssetsOrLargeBinaries()
		{
			var root = FindRepositoryRoot();
			var violations = new List<string>();

			foreach (var relativePath in GetTrackedFiles(root))
			{
				var fullPath = Path.Combine(root, relativePath);
				if (!File.Exists(fullPath))
				{
					violations.Add("Tracked file is missing from the working tree: " + relativePath);
					continue;
				}

				var fileInfo = new FileInfo(fullPath);
				if (fileInfo.Length > MaxTrackedFileBytes)
				{
					violations.Add(string.Format(
						"Tracked file is larger than {0} MB: {1} ({2} MB)",
						MaxTrackedFileBytes / (1024 * 1024),
						relativePath,
						Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2)));
				}

				var extension = Path.GetExtension(relativePath);
				if (ForbiddenExtensions.Contains(extension))
				{
					violations.Add("Tracked file uses a forbidden game/binary extension: " + relativePath);
				}

				if (ForbiddenSuffixes.Any(suffix =>
					relativePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
				{
					violations.Add("Tracked file looks like a game asset: " + relativePath);
				}

				var normalized = relativePath.Replace('\\', '/').ToLowerInvariant();
				var fileName = Path.GetFileName(relativePath).ToLowerInvariant();
				if (normalized.Contains("/bgm/") ||
					normalized.Contains("/save/") ||
					normalized.Contains("/saves/") ||
					normalized.Contains("/savedata/") ||
					fileName.StartsWith("ed6_dt") ||
					fileName.StartsWith("ed6_win") ||
					fileName.StartsWith("ed6_op") ||
					fileName.StartsWith("ed6_logo"))
				{
					violations.Add("Tracked file looks like game data or a local save: " + relativePath);
				}
			}

			Assert.True(violations.Count == 0,
				"Repository policy violations:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
		}

		[Fact]
		public void PolicyDocumentsExist()
		{
			var root = FindRepositoryRoot();
			Assert.True(File.Exists(Path.Combine(root, "docs", "legal.md")),
				"docs/legal.md is missing.");
			Assert.True(File.Exists(Path.Combine(root, "docs", "testing", "fixtures.md")),
				"docs/testing/fixtures.md is missing.");
			Assert.True(File.Exists(Path.Combine(root, "THIRD-PARTY-NOTICES.md")),
				"THIRD-PARTY-NOTICES.md is missing.");
		}
	}
}
