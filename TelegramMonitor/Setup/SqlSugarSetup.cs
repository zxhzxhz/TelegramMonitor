namespace TelegramMonitor;

public static class SqlSugarSetup
{
    public static void AddSqlSugarSetup(this IServiceCollection services)
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "telegrammonitor.db");
        string connectionString = $"DataSource={dbPath}";

        SqlSugarScope sqlSugar = new SqlSugarScope(
            new ConnectionConfig()
            {
                DbType = DbType.Sqlite,
                ConnectionString = connectionString,
                IsAutoCloseConnection = true
            },
            db => { }
        );

        services.AddSingleton<ISqlSugarClient>(sqlSugar);
        services.AddScoped<SqlSugarScope>(s => sqlSugar);

        InitializeDatabase(sqlSugar);
        services.AddSingleton<SystemCacheServices>();
    }

    private static void InitializeDatabase(ISqlSugarClient db)
    {
        db.DbMaintenance.CreateDatabase();

        InitializeTable<KeywordConfig>(db);
    }

    private static void InitializeTable<T>(ISqlSugarClient db) where T : class, new()
    {
        string tableName = db.EntityMaintenance.GetEntityInfo<T>().DbTableName;

        if (!db.DbMaintenance.IsAnyTable(tableName))
        {
            db.CodeFirst.InitTables<T>();
            Log.Information($"表 {tableName} 已创建");
        }
    }
}