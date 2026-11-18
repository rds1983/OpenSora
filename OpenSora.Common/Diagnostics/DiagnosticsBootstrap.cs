using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenSora.Diagnostics
{
	/// <summary>
	/// Starts process diagnostics: a per-session text log plus crash reporting. Logs and crash
	/// reports stay on the local machine.
	/// </summary>
	public static class DiagnosticsBootstrap
	{
		public static DiagnosticsSession Start(string applicationName)
		{
			if (string.IsNullOrWhiteSpace(applicationName))
			{
				throw new ArgumentException("An application name is required.", nameof(applicationName));
			}

			var safeName = MakeSafeFileName(applicationName);
			var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (string.IsNullOrEmpty(root))
			{
				root = Path.GetTempPath();
			}

			var logDirectory = Path.Combine(root, "OpenSora", "logs");
			var logPath = Path.Combine(logDirectory,
				string.Format("{0}-{1:yyyyMMdd-HHmmss}-{2}.log", safeName, DateTimeOffset.UtcNow, Environment.ProcessId));

			TextFileLogSink sink = null;
			try
			{
				sink = new TextFileLogSink(logPath);
				Log.AddSink(sink);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Failed to open OpenSora log file: " + ex.Message);
				sink = null;
			}

			var crashInstaller = CrashReporter.Install(Path.Combine(root, "OpenSora", "crash-reports"));

			Log.InfoFormat(LogCategory.Diagnostics,
				"{0} starting. OS={1}; Runtime={2}; Version={3}; Log={4}",
				applicationName,
				RuntimeInformation.OSDescription,
				RuntimeInformation.FrameworkDescription,
				Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown",
				logPath);

			return new DiagnosticsSession(applicationName, logPath, sink, crashInstaller);
		}

		private static string MakeSafeFileName(string value)
		{
			foreach (var invalid in Path.GetInvalidFileNameChars())
			{
				value = value.Replace(invalid, '_');
			}

			return value;
		}
	}

	public sealed class DiagnosticsSession : IDisposable
	{
		private readonly string _applicationName;
		private readonly TextFileLogSink _sink;
		private readonly IDisposable _crashInstaller;
		private bool _disposed;

		public string LogPath { get; }

		internal DiagnosticsSession(string applicationName, string logPath, TextFileLogSink sink,
			IDisposable crashInstaller)
		{
			_applicationName = applicationName;
			LogPath = logPath;
			_sink = sink;
			_crashInstaller = crashInstaller;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			Log.InfoFormat(LogCategory.Diagnostics, "{0} stopping.", _applicationName);

			_crashInstaller?.Dispose();
			if (_sink != null)
			{
				Log.RemoveSink(_sink);
				_sink.Dispose();
			}
		}
	}
}
