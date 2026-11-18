using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenSora.Diagnostics
{
	/// <summary>
	/// Writes actionable crash reports containing the exception, runtime information, and the recent
	/// in-memory log tail. Reports stay local; no telemetry is uploaded.
	/// </summary>
	public static class CrashReporter
	{
		private static readonly object SyncRoot = new object();
		private static bool _installed;

		public static string DefaultReportDirectory
		{
			get
			{
				var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (string.IsNullOrEmpty(root))
				{
					root = Path.GetTempPath();
				}

				return Path.Combine(root, "OpenSora", "crash-reports");
			}
		}

		/// <summary>
		/// Hooks the process-level unhandled exception handlers. Dispose the returned object to
		/// uninstall them (useful for tests and host applications).
		/// </summary>
		public static IDisposable Install(string reportDirectory = null)
		{
			lock (SyncRoot)
			{
				if (_installed)
				{
					return new Subscription(null);
				}

				UnhandledExceptionEventHandler unhandledHandler = (sender, args) =>
				{
					var exception = args.ExceptionObject as Exception ??
						new Exception("Unknown unhandled exception: " + args.ExceptionObject);
					Log.Critical(LogCategory.Diagnostics, "Unhandled exception.", exception);
					TryWriteReport(exception, reportDirectory);
				};

				EventHandler<UnobservedTaskExceptionEventArgs> taskHandler = (sender, args) =>
				{
					Log.Error(LogCategory.Diagnostics, "Unobserved task exception.", args.Exception);
					args.SetObserved();
				};

				AppDomain.CurrentDomain.UnhandledException += unhandledHandler;
				TaskScheduler.UnobservedTaskException += taskHandler;
				_installed = true;

				return new Subscription(() =>
				{
					AppDomain.CurrentDomain.UnhandledException -= unhandledHandler;
					TaskScheduler.UnobservedTaskException -= taskHandler;
					lock (SyncRoot)
					{
						_installed = false;
					}
				});
			}
		}

		/// <summary>
		/// Writes a crash report and returns its path. This method is also used for explicit fatal
		/// paths where an exception has already been caught by the host application.
		/// </summary>
		public static string WriteReport(Exception exception, string reportDirectory = null)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			var directory = string.IsNullOrEmpty(reportDirectory) ? DefaultReportDirectory : reportDirectory;
			Directory.CreateDirectory(directory);

			var fileName = string.Format("crash-{0:yyyyMMdd-HHmmss-fff}-{1}.txt",
				DateTimeOffset.UtcNow, Environment.ProcessId);
			var path = Path.Combine(directory, fileName);
			File.WriteAllText(path, BuildReport(exception), new UTF8Encoding(false));
			return path;
		}

		private static string BuildReport(Exception exception)
		{
			var sb = new StringBuilder();
			sb.AppendLine("OpenSora crash report");
			sb.AppendLine("=====================");
			sb.AppendLine("Timestamp: " + DateTimeOffset.UtcNow.ToString("O"));
			sb.AppendLine("Process: " + Process.GetCurrentProcess().ProcessName + " (" + Environment.ProcessId + ")");
			sb.AppendLine("OS: " + RuntimeInformation.OSDescription);
			sb.AppendLine("Runtime: " + RuntimeInformation.FrameworkDescription);
			sb.AppendLine("Architecture: " + RuntimeInformation.ProcessArchitecture);
			sb.AppendLine("Version: " + (Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown"));
			sb.AppendLine();
			sb.AppendLine("Exception:");
			sb.AppendLine(exception.ToString());
			sb.AppendLine();
			sb.AppendLine("Recent log:");

			var entries = Log.RecentEntries;
			if (entries.Length == 0)
			{
				sb.AppendLine("(none)");
			}
			else
			{
				foreach (var entry in entries)
				{
					sb.Append(entry.Timestamp.ToString("O"));
					sb.Append(' ');
					sb.Append(entry.Level.ToString().ToUpperInvariant());
					sb.Append(' ');
					sb.Append(entry.Category);
					sb.Append(": ");
					sb.AppendLine(entry.Message);

					if (!string.IsNullOrEmpty(entry.Detail))
					{
						sb.AppendLine(entry.Detail);
					}
				}
			}

			return sb.ToString();
		}

		private static string TryWriteReport(Exception exception, string reportDirectory)
		{
			try
			{
				var path = WriteReport(exception, reportDirectory);
				Console.Error.WriteLine("OpenSora crash report: " + path);
				return path;
			}
			catch (Exception reportException)
			{
				Console.Error.WriteLine("Failed to write OpenSora crash report: " + reportException.Message);
				return null;
			}
		}

		private sealed class Subscription : IDisposable
		{
			private readonly Action _dispose;
			private bool _disposed;

			public Subscription(Action dispose)
			{
				_dispose = dispose;
			}

			public void Dispose()
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;
				_dispose?.Invoke();
			}
		}
	}
}
