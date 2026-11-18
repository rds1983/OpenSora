using OpenSora.Diagnostics;
using OpenSora.ModelLoading;
using OpenSora.Scenarios;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenSora.Tests
{
	public class DiagnosticsIntegrationTests
	{
		[Fact]
		public void Decompiler_UnknownOpcode_IsReportedOnceAndFailsActionably()
		{
			var source = "unknown-opcode-" + Guid.NewGuid().ToString("N");

			for (var i = 0; i < 2; ++i)
			{
				using (var stream = new MemoryStream(new byte[] { 0xFE }))
				using (var reader = new BinaryReader(stream))
				{
					var decompiler = new Decompiler(reader, source);
					var exception = Assert.Throws<InvalidDataException>(() => decompiler.DecompileBlock());

					Assert.Contains(source, exception.Message);
					Assert.Contains("0xFE", exception.Message);
				}
			}

			Assert.Single(Log.RecentEntries, entry =>
				entry.Category == LogCategory.Scenario &&
				entry.Message.Contains(source));
		}

		[Fact]
		public void ModelLoader_InvalidData_LogsActionableFailure()
		{
			var before = Log.RecentEntries.Count(entry =>
				entry.Level == LogLevel.Error &&
				entry.Category == LogCategory.Asset &&
				entry.Message == "Failed to load model.");

			Assert.Throws<EndOfStreamException>(() =>
				ModelLoader.Load(new MemoryStream(new byte[] { 1, 2, 3 }), 2));

			var after = Log.RecentEntries.Count(entry =>
				entry.Level == LogLevel.Error &&
				entry.Category == LogCategory.Asset &&
				entry.Message == "Failed to load model.");

			Assert.Equal(before + 1, after);
		}
	}
}
