using System;
using System.Collections.Generic;

namespace OpenSora.Diagnostics
{
	/// <summary>
	/// Reports unknown opcodes, chunks, or fields once per source/kind/value so a single malformed
	/// asset cannot flood the log.
	/// </summary>
	public static class UnknownDataReporter
	{
		private static readonly object SyncRoot = new object();
		private static readonly HashSet<string> Reported = new HashSet<string>();

		public static bool ReportOnce(string source, LogCategory category, string kind, object value,
			string detail = null)
		{
			if (string.IsNullOrEmpty(kind))
			{
				throw new ArgumentException("A kind is required.", nameof(kind));
			}

			var effectiveSource = string.IsNullOrEmpty(source) ? "unknown" : source;
			var key = string.Format("{0}|{1}|{2}|{3}", effectiveSource, category, kind, value);

			lock (SyncRoot)
			{
				if (!Reported.Add(key))
				{
					return false;
				}
			}

			var message = string.Format("{0} '{1}' is not supported (source: {2}).", kind, value, effectiveSource);
			if (!string.IsNullOrEmpty(detail))
			{
				message += " " + detail;
			}

			Log.Warning(category, message);
			return true;
		}

		/// <summary>
		/// Clears the deduplication set. Intended for tests and long-running tool sessions.
		/// </summary>
		public static void Reset()
		{
			lock (SyncRoot)
			{
				Reported.Clear();
			}
		}
	}
}
