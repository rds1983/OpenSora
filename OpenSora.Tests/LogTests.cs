using OpenSora.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace OpenSora.Tests
{
	public class LogTests
	{
		private sealed class TestLogSink : ILogSink
		{
			public List<LogEntry> Entries { get; } = new List<LogEntry>();

			public void Write(LogEntry entry)
			{
				Entries.Add(entry);
			}
		}

		[Fact]
		public void Write_StoresRecentEntryAndNotifiesSink()
		{
			var sink = new TestLogSink();
			var message = "log-test-" + Guid.NewGuid().ToString("N");

			Log.AddSink(sink);
			try
			{
				Log.Warning(LogCategory.Asset, message);

				var recent = Log.RecentEntries;
				Assert.Contains(recent, entry =>
					entry.Level == LogLevel.Warning &&
					entry.Category == LogCategory.Asset &&
					entry.Message == message);

				var sinkEntry = Assert.Single(sink.Entries, entry => entry.Message == message);
				Assert.Equal(LogCategory.Asset, sinkEntry.Category);
				Assert.Equal(LogLevel.Warning, sinkEntry.Level);
			}
			finally
			{
				Log.RemoveSink(sink);
			}
		}

		[Fact]
		public void TextFileLogSink_WritesStructuredEntry()
		{
			using (var temp = new TempDirectory())
			{
				var path = Path.Combine(temp.Path, "test.log");
				var message = "file-log-" + Guid.NewGuid().ToString("N");

				using (var sink = new TextFileLogSink(path))
				{
					Log.AddSink(sink);
					try
					{
						Log.Error(LogCategory.Asset, message, new InvalidDataException("fixture failure"));
					}
					finally
					{
						Log.RemoveSink(sink);
					}
				}

				var text = File.ReadAllText(path);
				Assert.Contains("ERROR", text);
				Assert.Contains("Asset", text);
				Assert.Contains(message, text);
				Assert.Contains("fixture failure", text);
			}
		}

		[Fact]
		public void CrashReporter_IncludesExceptionAndRecentLog()
		{
			using (var temp = new TempDirectory())
			{
				var context = "crash-context-" + Guid.NewGuid().ToString("N");
				var exception = new InvalidDataException("broken asset " + context);

				Log.Error(LogCategory.Asset, context, exception);

				var path = CrashReporter.WriteReport(exception, temp.Path);
				Assert.True(File.Exists(path));

				var text = File.ReadAllText(path);
				Assert.Contains("OpenSora crash report", text);
				Assert.Contains("InvalidDataException", text);
				Assert.Contains("broken asset", text);
				Assert.Contains("Recent log:", text);
				Assert.Contains(context, text);
			}
		}

		[Fact]
		public void UnknownDataReporter_ReportsOncePerSource()
		{
			var opcodeSource = "opcode-" + Guid.NewGuid().ToString("N");
			Assert.True(UnknownDataReporter.ReportOnce(opcodeSource, LogCategory.Scenario, "opcode", "0xFE", "Offset 0x0."));
			Assert.False(UnknownDataReporter.ReportOnce(opcodeSource, LogCategory.Scenario, "opcode", "0xFE", "Offset 0x0."));
			Assert.Single(Log.RecentEntries, entry => entry.Message.Contains(opcodeSource));

			var chunkSource = "chunk-" + Guid.NewGuid().ToString("N");
			Assert.True(UnknownDataReporter.ReportOnce(chunkSource, LogCategory.Asset, "chunk", "0x7F", "Length 4."));
			Assert.False(UnknownDataReporter.ReportOnce(chunkSource, LogCategory.Asset, "chunk", "0x7F", "Length 4."));
			Assert.Single(Log.RecentEntries, entry => entry.Message.Contains(chunkSource));
		}
	}
}
