using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenSora.Diagnostics
{
	public enum LogLevel
	{
		Trace = 0,
		Debug = 1,
		Info = 2,
		Warning = 3,
		Error = 4,
		Critical = 5
	}

	public enum LogCategory
	{
		General,
		Archive,
		Asset,
		Scenario,
		Vm,
		Render,
		Audio,
		Input,
		Save,
		Ui,
		Diagnostics
	}

	public sealed class LogEntry
	{
		public DateTimeOffset Timestamp { get; }
		public long Sequence { get; }
		public LogLevel Level { get; }
		public LogCategory Category { get; }
		public string Message { get; }
		public string Detail { get; }

		public LogEntry(DateTimeOffset timestamp, long sequence, LogLevel level, LogCategory category,
			string message, string detail = null)
		{
			Timestamp = timestamp;
			Sequence = sequence;
			Level = level;
			Category = category;
			Message = message ?? string.Empty;
			Detail = detail;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:HH:mm:ss.fff} {1,-5} {2}: {3}",
				Timestamp, Level.ToString().ToUpperInvariant(), Category, Message);
		}
	}

	public interface ILogSink
	{
		void Write(LogEntry entry);
	}

	/// <summary>
	/// Process-wide structured log with a bounded in-memory history and pluggable sinks. Logging is
	/// diagnostic infrastructure and must not be used as simulation state.
	/// </summary>
	public static class Log
	{
		private static readonly object SyncRoot = new object();
		private static readonly Queue<LogEntry> Entries = new Queue<LogEntry>();
		private static readonly List<ILogSink> Sinks = new List<ILogSink>();
		private static long _sequence;

		public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
		public static int Capacity { get; set; } = 500;

		public static void AddSink(ILogSink sink)
		{
			if (sink == null)
			{
				throw new ArgumentNullException(nameof(sink));
			}

			lock (SyncRoot)
			{
				Sinks.Add(sink);
			}
		}

		public static void RemoveSink(ILogSink sink)
		{
			if (sink == null)
			{
				return;
			}

			lock (SyncRoot)
			{
				Sinks.Remove(sink);
			}
		}

		public static LogEntry[] RecentEntries
		{
			get
			{
				lock (SyncRoot)
				{
					return Entries.ToArray();
				}
			}
		}

		public static void Clear()
		{
			lock (SyncRoot)
			{
				Entries.Clear();
			}
		}

		public static bool IsEnabled(LogLevel level)
		{
			return level >= MinimumLevel;
		}

		public static void Write(LogLevel level, LogCategory category, string message, Exception exception = null)
		{
			if (level < MinimumLevel)
			{
				return;
			}

			ILogSink[] sinks;
			LogEntry entry;

			lock (SyncRoot)
			{
				entry = new LogEntry(DateTimeOffset.UtcNow, ++_sequence, level, category, message,
					exception == null ? null : exception.ToString());

				Entries.Enqueue(entry);
				while (Entries.Count > Capacity)
				{
					Entries.Dequeue();
				}

				sinks = Sinks.ToArray();
			}

			foreach (var sink in sinks)
			{
				try
				{
					sink.Write(entry);
				}
				catch (Exception sinkException)
				{
					// A broken sink must never take down the application or recurse into logging.
					Console.Error.WriteLine("OpenSora log sink failed: " + sinkException.Message);
				}
			}
		}

		public static void WriteFormat(LogLevel level, LogCategory category, string format, params object[] args)
		{
			if (level < MinimumLevel)
			{
				return;
			}

			Write(level, category, string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public static void Trace(LogCategory category, string message, Exception exception = null)
		{
			Write(LogLevel.Trace, category, message, exception);
		}

		public static void Debug(LogCategory category, string message, Exception exception = null)
		{
			Write(LogLevel.Debug, category, message, exception);
		}

		public static void Info(LogCategory category, string message, Exception exception = null)
		{
			Write(LogLevel.Info, category, message, exception);
		}

		public static void Warning(LogCategory category, string message, Exception exception = null)
		{
			Write(LogLevel.Warning, category, message, exception);
		}

		public static void Error(LogCategory category, string message, Exception exception = null)
		{
			Write(LogLevel.Error, category, message, exception);
		}

		public static void Critical(LogCategory category, string message, Exception exception = null)
		{
			Write(LogLevel.Critical, category, message, exception);
		}

		public static void DebugFormat(LogCategory category, string format, params object[] args)
		{
			WriteFormat(LogLevel.Debug, category, format, args);
		}

		public static void InfoFormat(LogCategory category, string format, params object[] args)
		{
			WriteFormat(LogLevel.Info, category, format, args);
		}

		public static void WarningFormat(LogCategory category, string format, params object[] args)
		{
			WriteFormat(LogLevel.Warning, category, format, args);
		}

		public static void ErrorFormat(LogCategory category, string format, params object[] args)
		{
			WriteFormat(LogLevel.Error, category, format, args);
		}
	}
}
