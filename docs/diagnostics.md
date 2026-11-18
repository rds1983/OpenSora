# Diagnostics, logging, and crash reports

OpenSora keeps diagnostics local. Logs and crash reports are not uploaded anywhere.

## Logging

Use `OpenSora.Diagnostics.Log` for diagnostic output:

```csharp
Log.Info(LogCategory.Asset, "Loaded texture " + entry.Name);
Log.Warning(LogCategory.Scenario, "Unknown opcode at 0x1234");
Log.Error(LogCategory.Asset, "Failed to load model.", exception);
```

Levels: `Trace`, `Debug`, `Info`, `Warning`, `Error`, `Critical`.

Categories: `General`, `Archive`, `Asset`, `Scenario`, `Vm`, `Render`, `Audio`, `Input`, `Save`,
`Ui`, `Diagnostics`.

Logging is diagnostic infrastructure, not simulation state. Do not make gameplay depend on log
entries, and do not log every rendered frame by default.

### Sinks and history

- `TextFileLogSink` appends UTF-8 entries and flushes after every write.
- `Log.RecentEntries` keeps a bounded in-memory tail (500 entries by default) for crash reports and
  the debug overlay.
- Additional sinks can be attached with `Log.AddSink` for tools and tests.

### File locations

`DiagnosticsBootstrap.Start(applicationName)` creates a per-session log:

- Windows: `%LOCALAPPDATA%\OpenSora\logs\`
- Linux/macOS: the platform's local application data directory under `OpenSora/logs/`

The viewer, `Unpacker`, and `Decompiler` start diagnostics at launch.

## Crash reports

`CrashReporter.Install()` hooks process-level unhandled exceptions. A report contains:

- timestamp, process name/id, OS, runtime, architecture, and version;
- the full exception and stack trace;
- the recent in-memory log tail.

Reports are written to:

- Windows: `%LOCALAPPDATA%\OpenSora\crash-reports\`
- Linux/macOS: local application data under `OpenSora/crash-reports/`

The viewer also writes a report explicitly for exceptions caught at the top level, so a report is
produced even when a host catches the failure.

## Unknown data deduplication

Use `UnknownDataReporter.ReportOnce` for unknown opcodes, chunks, or fields:

```csharp
UnknownDataReporter.ReportOnce(
    source: scriptName,
    category: LogCategory.Scenario,
    kind: "opcode",
    value: "0xFE",
    detail: "Offset 0x1234.");
```

The first occurrence for a source/kind/value is logged as a warning; repeats are suppressed. This
keeps one malformed asset from flooding the log. `UnknownDataReporter.Reset()` clears the dedup set
for tests or long-running tool sessions.

The scenario decompiler uses this for unknown opcodes and includes the source and offset in the
exception message.

## Debug overlay

In the viewer, press **F12** to toggle a diagnostics overlay. It shows the most recent log entries,
including warnings and errors, without opening an external file.

## Tests

- `LogTests` covers sinks, recent history, crash report contents, and unknown-data deduplication.
- `DiagnosticsIntegrationTests` covers unknown scenario opcodes and a forced model-load failure.

## Privacy

No telemetry or automatic upload exists. Logs and crash reports stay on the local machine. Do not add
automatic upload without an explicit opt-in and a privacy review.
