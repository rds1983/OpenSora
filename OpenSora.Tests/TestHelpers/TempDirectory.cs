using System;
using System.IO;

namespace OpenSora.Tests
{
	internal sealed class TempDirectory : IDisposable
	{
		public string Path { get; }

		public TempDirectory()
		{
			Path = System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				"OpenSora.Tests",
				Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Path);
		}

		public void Dispose()
		{
			try
			{
				Directory.Delete(Path, true);
			}
			catch (IOException)
			{
				// Best-effort cleanup; a locked file must not fail the test run.
			}
			catch (UnauthorizedAccessException)
			{
				// Best-effort cleanup; a locked file must not fail the test run.
			}
		}
	}
}
