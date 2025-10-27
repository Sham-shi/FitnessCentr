using DbFirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DbFirst.Services;

public static class DatabaseService
{
    private static IConfigurationRoot? _configuration;
    private static string _currentDb = "SQLite"; // default MSSQL
    private static string? _mssqlConn;
    private static string? _sqliteConn;

    public static void Initialize(IConfigurationRoot configuration)
    {
        _configuration = configuration;
        _mssqlConn = configuration.GetConnectionString("MSSQL");
        _sqliteConn = configuration.GetConnectionString("SQLite");
    }

    public static void UseDatabase(string dbType)
    {
        if (dbType != "MSSQL" && dbType != "SQLite")
            throw new ArgumentException("dbType must be 'MSSQL' or 'SQLite'");

        _currentDb = dbType;
    }

    public static FitnessCenterContext CreateContext()
    {
        if (_configuration == null)
            throw new InvalidOperationException("DatabaseService not initialized. Call Initialize(...) on startup.");

        var optionsBuilder = new DbContextOptionsBuilder<FitnessCenterContext>();

        if (_currentDb == "SQLite")
        {
            if (string.IsNullOrWhiteSpace(_sqliteConn))
                throw new InvalidOperationException("SQLite connection string is empty. Check appsettings.json.");

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FitnessCentr.db");
            // Если пользователь записал просто имя файла или относительный путь, нормализуем:
            var conn = NormalizeSqliteConnectionString(_sqliteConn);
            System.Diagnostics.Debug.WriteLine($"CurrentDb: {_currentDb}");
            System.Diagnostics.Debug.WriteLine($"MSSQL Conn: {_mssqlConn}");
            System.Diagnostics.Debug.WriteLine($"SQLite Conn: {_sqliteConn}");
            optionsBuilder.UseSqlite(conn);
        }
        else // MSSQL
        {
            if (string.IsNullOrWhiteSpace(_mssqlConn))
                throw new InvalidOperationException("MSSQL connection string is empty. Check appsettings.json.");

            System.Diagnostics.Debug.WriteLine($"CurrentDb: {_currentDb}");
            System.Diagnostics.Debug.WriteLine($"MSSQL Conn: {_mssqlConn}");
            System.Diagnostics.Debug.WriteLine($"SQLite Conn: {_sqliteConn}");
            optionsBuilder.UseSqlServer(_mssqlConn);
        }

        return new FitnessCenterContext(optionsBuilder.Options);
    }

    private static string NormalizeSqliteConnectionString(string raw)
    {
        // если строка уже в формате "Data Source=..." — возвращаем как есть
        if (raw.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            return raw;

        // если передали просто имя/путь файла (например "fitness.db" или "/data/fitness.db")
        // нужно собрать корректную строку подключения.
        // Поддерживаем относительные пути: "Photos/.." или "/db/fitness.db" -> приведём к абсолютному
        var path = raw.Trim();

        // если строка начинается с "file:" — оставляем
        if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return path;

        // убираем ведущие слеши
        path = path.TrimStart('\\', '/');

        var fullPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(Directory.GetCurrentDirectory(), path);

        // Убедимся, что директория существует (необязательно)
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return $"Data Source={fullPath}";
    }
}
