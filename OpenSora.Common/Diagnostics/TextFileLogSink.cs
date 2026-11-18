using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenSora.Diagnostics
{
	/// <summary>
	/// Appends structured log entries to a UTF-8 text file. The sink is thread-safe and flushes on
	/// every write so a crash can still leave a useful tail.
	/// </summary>
	public sealed class TextFileLogSink : ILogSink, IDisposable
	{
		private readonly object _syncRoot = new object();
		private readonly StreamWriter _writer;
		private bool _disposed;

		public string FilePath { get; }

		public TextFileLogSink(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("A log file path is required.", nameof(filePath));
			}

			FilePath = filePath;
			var directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
			_writer = new StreamWriter(stream, new UTF8Encoding(false))
			{
				AutoFlush = true
			};
		}

		public void Write(LogEntry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException(nameof(entry));
			}

			lock (_syncRoot)
			{
				if (_disposed)
				{
					return;
				}

				_writer.Write(entry.Timestamp.ToString("O", CultureInfo.InvariantCulture));
				_writer.Write(' ');
				_writer.Write(entry.Level.ToString().ToUpperInvariant());
				_writer.Write(' ');
				_writer.Write(entry.Category);
				_writer.Write(": ");
				_writer.WriteLine(entry.Message);

				if (!string.IsNullOrEmpty(entry.Detail))
				{
					_writer.WriteLine(entry.Detail);
				}
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;
				_writer.Dispose();
			}
		}
	}
}
