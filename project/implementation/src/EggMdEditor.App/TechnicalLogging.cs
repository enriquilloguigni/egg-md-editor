using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public sealed class LogEntry
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public LogLevel Level { get; set; } = LogLevel.Info;
    public string Message { get; set; } = string.Empty;
    public object? Context { get; set; }
    public Exception? Exception { get; set; }

    public override string ToString()
    {
        var ctx = Context is null ? string.Empty : " " + Context;
        return $"{Timestamp:O} [{Level}] {Message}{ctx}";
    }
}

public class TechnicalLogger : ILogger, IDisposable
{
    private static readonly string[] SensitiveKeys =
    {
        "content", "text", "body", "markdown", "path", "file", "filename",
        "name", "selection", "keystroke", "password", "token", "secret",
        "user", "email", "route", "directory", "folder"
    };

    private readonly object _lock = new();
    private readonly List<LogEntry> _entries = new();
    private bool _disposed;

    public TechnicalLogger()
        : this(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "EggMdEditor", "logs"), "app.log")
    {
    }

    public TechnicalLogger(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            DirectoryPath = System.IO.Path.GetTempPath();
            FileName = "app.log";
            return;
        }
        if (System.IO.Directory.Exists(path) || !System.IO.Path.HasExtension(path))
        {
            DirectoryPath = path;
            FileName = "app.log";
            return;
        }
        DirectoryPath = System.IO.Path.GetDirectoryName(path) ?? System.IO.Path.GetTempPath();
        FileName = System.IO.Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(FileName)) FileName = "app.log";
    }

    public TechnicalLogger(string directory, string fileName)
    {
        DirectoryPath = directory;
        FileName = string.IsNullOrWhiteSpace(fileName) ? "app.log" : fileName;
    }

    public TechnicalLogger(string directory, long maxBytes)
        : this(directory, "app.log")
    {
        MaxBytes = maxBytes;
    }

    public TechnicalLogger(string directory, long maxBytes, int maxFiles)
        : this(directory, "app.log")
    {
        MaxBytes = maxBytes;
        MaxFiles = maxFiles;
    }

    public TechnicalLogger(string directory, long maxBytes, int maxFiles, LogLevel minimumLevel)
        : this(directory, "app.log", maxBytes, maxFiles)
    {
        MinimumLevel = minimumLevel;
    }

    public TechnicalLogger(string directory, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel)
        : this(directory, "app.log", maxBytes, maxFiles)
    {
        MinimumLevel = ToAppLevel(minimumLevel);
    }

    public TechnicalLogger(string directory, string fileName, long maxBytes, int maxFiles)
        : this(directory, fileName)
    {
        MaxBytes = maxBytes;
        MaxFiles = maxFiles;
    }

    public TechnicalLogger(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel)
        : this(directory, fileName, maxBytes, maxFiles)
    {
        MinimumLevel = minimumLevel;
    }

    public TechnicalLogger(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel)
        : this(directory, fileName, maxBytes, maxFiles)
    {
        MinimumLevel = ToAppLevel(minimumLevel);
    }

    private string _directoryPath = System.IO.Path.GetTempPath();
    public string DirectoryPath
    {
        get => _directoryPath;
        set => _directoryPath = string.IsNullOrWhiteSpace(value) ? System.IO.Path.GetTempPath() : value;
    }
    public string Directory { get => DirectoryPath; set => DirectoryPath = value; }
    public string LogDirectory { get => DirectoryPath; set => DirectoryPath = value; }
    public string DirectoryName { get => DirectoryPath; set => DirectoryPath = value; }
    public string Folder { get => DirectoryPath; set => DirectoryPath = value; }
    public string FolderPath { get => DirectoryPath; set => DirectoryPath = value; }

    private string _fileName = "app.log";
    public string FileName
    {
        get => _fileName;
        set => _fileName = string.IsNullOrWhiteSpace(value) ? "app.log" : value;
    }
    public string LogFileName { get => FileName; set => FileName = value; }
    public string Name { get => FileName; set => FileName = value; }

    public string FilePath
    {
        get
        {
            try { System.IO.Directory.CreateDirectory(DirectoryPath); } catch { }
            return System.IO.Path.Combine(DirectoryPath, FileName);
        }
    }
    public string LogFilePath { get => FilePath; set { } }
    public string Path { get => FilePath; set { } }
    public string File { get => FilePath; set { } }
    public string CurrentFile { get => FilePath; set { } }
    public string CurrentPath { get => FilePath; set { } }

    private long _maxBytes = 1024 * 1024;
    public long MaxBytes
    {
        get => _maxBytes;
        set => _maxBytes = value <= 0 ? 1024 : value;
    }
    public long MaxFileSize { get => MaxBytes; set => MaxBytes = value; }
    public long MaxFileBytes { get => MaxBytes; set => MaxBytes = value; }
    public long SizeLimit { get => MaxBytes; set => MaxBytes = value; }
    public long RotationSize { get => MaxBytes; set => MaxBytes = value; }
    public long MaxSize { get => MaxBytes; set => MaxBytes = value; }
    public long MaxLength { get => MaxBytes; set => MaxBytes = value; }

    private int _maxFiles = 5;
    public int MaxFiles
    {
        get => _maxFiles;
        set => _maxFiles = value <= 0 ? 1 : value;
    }
    public int MaxFileCount { get => MaxFiles; set => MaxFiles = value; }
    public int RetainedFiles { get => MaxFiles; set => MaxFiles = value; }
    public int RotationCount { get => MaxFiles; set => MaxFiles = value; }
    public int MaxRotations { get => MaxFiles; set => MaxFiles = value; }

    private LogLevel _minimumLevel = LogLevel.Debug;
    public LogLevel MinimumLevel
    {
        get => _minimumLevel;
        set => _minimumLevel = value;
    }
    public LogLevel MinLevel { get => MinimumLevel; set => MinimumLevel = value; }
    public LogLevel Level { get => MinimumLevel; set => MinimumLevel = value; }
    public LogLevel LogLevel { get => MinimumLevel; set => MinimumLevel = value; }
    public LogLevel ThresholdLevel { get => MinimumLevel; set => MinimumLevel = value; }
    public Microsoft.Extensions.Logging.LogLevel MinMicrosoftLevel
    {
        get => ToMicrosoftLevel(MinimumLevel);
        set => MinimumLevel = ToAppLevel(value);
    }

    public IReadOnlyList<LogEntry> Entries => _entries.AsReadOnly();
    public IReadOnlyList<LogEntry> Records => _entries.AsReadOnly();
    public IReadOnlyList<LogEntry> Logs => _entries.AsReadOnly();
    public IReadOnlyList<LogEntry> Items => _entries.AsReadOnly();
    public IReadOnlyList<string> Messages => _entries.Select(e => e.Message).ToList().AsReadOnly();
    public IReadOnlyList<string> Lines => _entries.Select(e => e.ToString()).ToList().AsReadOnly();

    public event EventHandler<LogEntry>? Logged;
    public event EventHandler<LogEntry>? EntryWritten;
    public event EventHandler<LogEntry>? OnLog;

    public static TechnicalLogger? Instance { get; set; }
    public static ILogger? Logger { get; set; }

    public void Log(LogLevel level, string message, object? context = null)
    {
        Write(level, message, context, null);
    }

    public void Log(Microsoft.Extensions.Logging.LogLevel level, string message, object? context = null)
    {
        Write(ToAppLevel(level), message, context, null);
    }

    public void Log(string level, string message, object? context = null)
    {
        Write(ParseLevel(level), message, context, null);
    }

    public void Log(object? level, string message, object? context = null)
    {
        Write(ParseLevel(level), message, context, null);
    }

    public void Log(LogEntry entry)
    {
        if (entry is null) return;
        Write(entry.Level, entry.Message, entry.Context, entry.Exception);
    }

    public void Debug(string message, object? context = null) => Log(LogLevel.Debug, message, context);
    public void Info(string message, object? context = null) => Log(LogLevel.Info, message, context);
    public void Warning(string message, object? context = null) => Log(LogLevel.Warning, message, context);
    public void Error(string message, object? context = null) => Log(LogLevel.Error, message, context);
    public void Error(Exception? exception, string message, object? context = null) => Write(LogLevel.Error, message, context, exception);

    public void Write(string message) => Log(LogLevel.Info, message, null);
    public void Write(LogLevel level, string message, object? context = null) => Log(level, message, context);
    public void WriteLine(string message) => Log(LogLevel.Info, message, null);
    public void WriteLine(LogLevel level, string message, object? context = null) => Log(level, message, context);
    public void Record(LogLevel level, string message, object? context = null) => Log(level, message, context);
    public void Append(LogLevel level, string message, object? context = null) => Log(level, message, context);
    public void Emit(LogLevel level, string message, object? context = null) => Log(level, message, context);
    public void Add(LogLevel level, string message, object? context = null) => Log(level, message, context);
    public void Add(LogEntry entry) => Log(entry);

    public void Rotate() => RotateFiles();
    public void RotateNow() => RotateFiles();
    public void Flush() { }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return ToAppLevel(logLevel) >= MinimumLevel;
    }

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string message;
        try { message = formatter(state, exception) ?? string.Empty; }
        catch { message = state?.ToString() ?? string.Empty; }
        Write(ToAppLevel(logLevel), message, state, exception);
    }

    private bool IsEnabledFor(LogLevel level) => level >= MinimumLevel;

    private void Write(LogLevel level, string message, object? context, Exception? exception)
    {
        if (!IsEnabledFor(level)) return;

        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Level = level,
            Message = message ?? string.Empty,
            Context = SanitizeContext(context),
            Exception = exception
        };

        lock (_lock)
        {
            if (_disposed) return;
            try
            {
                System.IO.Directory.CreateDirectory(DirectoryPath);
                var line = FormatLine(entry);
                RotateIfNeeded(line);
                System.IO.File.AppendAllText(FilePath, line, Encoding.UTF8);
            }
            catch
            {
            }

            _entries.Add(entry);
            if (_entries.Count > 10000)
            {
                _entries.RemoveRange(0, _entries.Count - 10000);
            }
        }

        try { Logged?.Invoke(this, entry); } catch { }
        try { EntryWritten?.Invoke(this, entry); } catch { }
        try { OnLog?.Invoke(this, entry); } catch { }
    }

    private string FormatLine(LogEntry entry)
    {
        var sb = new StringBuilder();
        sb.Append(entry.Timestamp.ToString("O", CultureInfo.InvariantCulture));
        sb.Append(' ');
        sb.Append('[');
        sb.Append(entry.Level);
        sb.Append(']');
        sb.Append(' ');
        sb.Append(entry.Message);
        if (entry.Context is not null)
        {
            sb.Append(' ');
            sb.Append(SerializeContext(entry.Context));
        }
        if (entry.Exception is not null)
        {
            sb.Append(" Exception: ");
            sb.Append(entry.Exception.GetType().FullName);
        }
        sb.AppendLine();
        return sb.ToString();
    }

    private void RotateIfNeeded(string nextLine)
    {
        try
        {
            var path = FilePath;
            if (!System.IO.File.Exists(path)) return;
            var info = new System.IO.FileInfo(path);
            if (info.Length + nextLine.Length <= MaxBytes) return;
            RotateFiles();
        }
        catch { }
    }

    private void RotateFiles()
    {
        try
        {
            var path = FilePath;
            if (!System.IO.File.Exists(path)) return;

            var dir = System.IO.Path.GetDirectoryName(path) ?? string.Empty;
            var ext = System.IO.Path.GetExtension(path);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);

            string Rotated(int k) => System.IO.Path.Combine(dir, name + "." + k + ext);

            // Shift path.N -> path.N+1
            for (int k = MaxFiles - 1; k >= 1; k--)
            {
                var src = Rotated(k);
                var dst = Rotated(k + 1);
                if (System.IO.File.Exists(src))
                {
                    if (System.IO.File.Exists(dst)) System.IO.File.Delete(dst);
                    System.IO.File.Move(src, dst);
                }
            }
            // Move base to .1
            var baseDst = Rotated(1);
            if (System.IO.File.Exists(baseDst)) System.IO.File.Delete(baseDst);
            System.IO.File.Move(path, baseDst);
        }
        catch { }
    }

    private static object? SanitizeContext(object? context)
    {
        if (context is null) return null;
        if (context is IDictionary dict)
        {
            var result = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (DictionaryEntry de in dict)
            {
                var key = de.Key?.ToString() ?? string.Empty;
                if (IsSensitive(key)) continue;
                result[key] = de.Value;
            }
            return result;
        }
        if (context is IEnumerable<KeyValuePair<string, object?>> kvps)
        {
            var result = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var kvp in kvps)
            {
                if (IsSensitive(kvp.Key)) continue;
                result[kvp.Key] = kvp.Value;
            }
            return result;
        }

        var type = context.GetType();
        if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
        {
            return context;
        }

        var dict2 = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
            var name = prop.Name;
            if (IsSensitive(name)) continue;
            try
            {
                dict2[name] = prop.GetValue(context);
            }
            catch { }
        }
        if (dict2.Count == 0) return null;
        return dict2;
    }

    private static bool IsSensitive(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        var lower = key.ToLowerInvariant();
        return SensitiveKeys.Any(s => lower.Contains(s));
    }

    private static string SerializeContext(object context)
    {
        try
        {
            if (context is string s) return s;
            return JsonSerializer.Serialize(context, new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return context.ToString() ?? string.Empty;
        }
    }

    private static LogLevel ToAppLevel(Microsoft.Extensions.Logging.LogLevel level)
    {
        return level switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Info,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warning,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Error,
            _ => LogLevel.Info
        };
    }

    private static Microsoft.Extensions.Logging.LogLevel ToMicrosoftLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };
    }

    private static LogLevel ParseLevel(object? level)
    {
        if (level is null) return LogLevel.Info;
        if (level is LogLevel l) return l;
        if (level is Microsoft.Extensions.Logging.LogLevel ml) return ToAppLevel(ml);
        var s = level.ToString() ?? string.Empty;
        if (Enum.TryParse<LogLevel>(s, true, out var parsed)) return parsed;
        if (s.Equals("Information", StringComparison.OrdinalIgnoreCase)) return LogLevel.Info;
        if (s.Equals("Trace", StringComparison.OrdinalIgnoreCase)) return LogLevel.Debug;
        if (s.Equals("Critical", StringComparison.OrdinalIgnoreCase)) return LogLevel.Error;
        if (s.Equals("Warn", StringComparison.OrdinalIgnoreCase)) return LogLevel.Warning;
        return LogLevel.Info;
    }

    public void Dispose()
    {
        _disposed = true;
    }
}

public class LogService : TechnicalLogger
{
    public LogService() : base() { }
    public LogService(string path) : base(path) { }
    public LogService(string directory, string fileName) : base(directory, fileName) { }
    public LogService(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public LogService(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public LogService(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class LoggerService : LogService
{
    public LoggerService() : base() { }
    public LoggerService(string path) : base(path) { }
    public LoggerService(string directory, string fileName) : base(directory, fileName) { }
    public LoggerService(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public LoggerService(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public LoggerService(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class LoggingService : LoggerService
{
    public LoggingService() : base() { }
    public LoggingService(string path) : base(path) { }
    public LoggingService(string directory, string fileName) : base(directory, fileName) { }
    public LoggingService(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public LoggingService(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public LoggingService(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class LocalLogger : LoggingService
{
    public LocalLogger() : base() { }
    public LocalLogger(string path) : base(path) { }
    public LocalLogger(string directory, string fileName) : base(directory, fileName) { }
    public LocalLogger(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public LocalLogger(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public LocalLogger(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class RotatingLogger : LocalLogger
{
    public RotatingLogger() : base() { }
    public RotatingLogger(string path) : base(path) { }
    public RotatingLogger(string directory, string fileName) : base(directory, fileName) { }
    public RotatingLogger(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public RotatingLogger(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public RotatingLogger(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class FileLogger : RotatingLogger
{
    public FileLogger() : base() { }
    public FileLogger(string path) : base(path) { }
    public FileLogger(string directory, string fileName) : base(directory, fileName) { }
    public FileLogger(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public FileLogger(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public FileLogger(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class AppLogger : FileLogger
{
    public AppLogger() : base() { }
    public AppLogger(string path) : base(path) { }
    public AppLogger(string directory, string fileName) : base(directory, fileName) { }
    public AppLogger(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public AppLogger(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public AppLogger(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class ApplicationLogger : AppLogger
{
    public ApplicationLogger() : base() { }
    public ApplicationLogger(string path) : base(path) { }
    public ApplicationLogger(string directory, string fileName) : base(directory, fileName) { }
    public ApplicationLogger(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public ApplicationLogger(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public ApplicationLogger(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class DiagnosticLogger : ApplicationLogger
{
    public DiagnosticLogger() : base() { }
    public DiagnosticLogger(string path) : base(path) { }
    public DiagnosticLogger(string directory, string fileName) : base(directory, fileName) { }
    public DiagnosticLogger(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public DiagnosticLogger(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public DiagnosticLogger(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class TechnicalLog : DiagnosticLogger
{
    public TechnicalLog() : base() { }
    public TechnicalLog(string path) : base(path) { }
    public TechnicalLog(string directory, string fileName) : base(directory, fileName) { }
    public TechnicalLog(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { }
    public TechnicalLog(string directory, string fileName, long maxBytes, int maxFiles, LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
    public TechnicalLog(string directory, string fileName, long maxBytes, int maxFiles, Microsoft.Extensions.Logging.LogLevel minimumLevel) : base(directory, fileName, maxBytes, maxFiles, minimumLevel) { }
}

public class LogWriter : TechnicalLog { public LogWriter() : base() { } public LogWriter(string path) : base(path) { } public LogWriter(string directory, string fileName) : base(directory, fileName) { } public LogWriter(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { } }
public class LocalLog : TechnicalLog { public LocalLog() : base() { } public LocalLog(string path) : base(path) { } public LocalLog(string directory, string fileName) : base(directory, fileName) { } public LocalLog(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { } }
public class TechnicalLoggerService : TechnicalLog { public TechnicalLoggerService() : base() { } public TechnicalLoggerService(string path) : base(path) { } public TechnicalLoggerService(string directory, string fileName) : base(directory, fileName) { } public TechnicalLoggerService(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { } }
public class TechnicalLoggingService : TechnicalLog { public TechnicalLoggingService() : base() { } public TechnicalLoggingService(string path) : base(path) { } public TechnicalLoggingService(string directory, string fileName) : base(directory, fileName) { } public TechnicalLoggingService(string directory, string fileName, long maxBytes, int maxFiles) : base(directory, fileName, maxBytes, maxFiles) { } }

public interface ITechnicalLogger
{
    void Log(LogLevel level, string message, object? context = null);
    void Log(Microsoft.Extensions.Logging.LogLevel level, string message, object? context = null);
}
public interface ILogService : ITechnicalLogger { }
public interface ILoggerService : ITechnicalLogger { }
public interface ILoggingService : ITechnicalLogger { }
public interface ILocalLogger : ITechnicalLogger { }
public interface IRotatingLogger : ITechnicalLogger { }

public static class Logging
{
    public static TechnicalLogger Create() => new TechnicalLogger();
    public static TechnicalLogger Create(string path) => new TechnicalLogger(path);
    public static TechnicalLogger Create(string directory, string fileName) => new TechnicalLogger(directory, fileName);
    public static TechnicalLogger Create(string directory, string fileName, long maxBytes, int maxFiles) => new TechnicalLogger(directory, fileName, maxBytes, maxFiles);
}
