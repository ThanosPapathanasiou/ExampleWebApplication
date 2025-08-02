module ExampleApp.Migrations

open System.Data
open System.IO
open System.Reflection
open System.Runtime.CompilerServices
open Dapper
open ExampleApp.Database.ConnectionManager
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

type LocationOfMigrationScripts = string

type MigrationRunner(
    logger: ILogger<MigrationRunner>,
    connectionManager: SqliteConnectionManager,
    migrationLocation:LocationOfMigrationScripts) =

    let getMigrationFiles (location:string) =
        let assembly = Assembly.GetExecutingAssembly()
        assembly.GetManifestResourceNames()
        |> Array.filter (fun name -> name.StartsWith(location) && name.EndsWith(".sql"))
        |> Array.sort // Sort to ensure consistent execution order

    let getMigrationScript (migrationFile: string) =
        use stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(migrationFile)
        use reader = new StreamReader(stream)
        reader.ReadToEnd()

    let ensureSqliteConfigured (conn: IDbConnection)=
        let pragmaConfigurations = """
            PRAGMA journal_mode = WAL;
            PRAGMA journal_size_limit = 6144000;
            PRAGMA wal_autocheckpoint = 1000;

            PRAGMA busy_timeout = 5000;
            PRAGMA cache_size = -20000;
            PRAGMA temp_store = MEMORY;
        """
        conn.Execute(pragmaConfigurations) |> ignore

    let ensureMigrationsTable (conn: IDbConnection) =
        let createTableSql = """
            CREATE TABLE IF NOT EXISTS __MigrationHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MigrationName TEXT NOT NULL,
                AppliedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );
        """
        conn.Execute(createTableSql) |> ignore

    let isMigrationApplied (conn: IDbConnection) (migrationName: string) =
        let query = "SELECT COUNT(*) FROM __MigrationHistory WHERE MigrationName = @MigrationName"
        conn.QuerySingle<int>(query, {| MigrationName = migrationName |}) > 0

    let executeMigration (conn: IDbConnection) (logger: ILogger) (migrationContent: string) (migrationName: string) =
        try
            logger.LogInformation $"Applying migration: %s{migrationName}"
            conn.Execute(migrationContent) |> ignore
            logger.LogInformation $"Successfully applied migration: %s{migrationName}"
        with
        | ex ->
            logger.LogError(ex, $"Error executing migration %s{migrationName}: %s{ex.Message}")
            raise ex

    let recordMigration (conn: IDbConnection) (migrationName: string) =
        let insertSql = "INSERT INTO __MigrationHistory (MigrationName) VALUES (@MigrationName)"
        conn.Execute(insertSql, {| MigrationName = migrationName |}) |> ignore

    member this.RunMigration() =
        let connection = connectionManager.GetConnection()
        
        ensureSqliteConfigured connection
        ensureMigrationsTable connection
        
        use transaction = connection.BeginTransaction()
        try
            let migrationFiles = getMigrationFiles migrationLocation
            for migrationFile in migrationFiles do
                let fullMigrationName = Path.GetFileName(migrationFile)
                let migrationName = fullMigrationName.TrimStart(migrationLocation.ToCharArray())
                
                if not (isMigrationApplied connection migrationName) then
                    let migrationScript = getMigrationScript fullMigrationName
                    executeMigration connection logger migrationScript migrationName
                    recordMigration connection migrationName
                    logger.LogInformation $"Applied migration: {migrationName}"
                else
                    logger.LogInformation $"Skipping already applied migration: {migrationName}"
            
            transaction.Commit()
            logger.LogInformation "All migrations completed successfully"
        with
        | ex ->
            transaction.Rollback()
            logger.LogError(ex, $"Migration failed: {ex.Message}")
            raise ex
        
type MigrationExtensions() =
    [<Extension>]
    static member PerformDatabaseMigrations (app: WebApplication) =
        use scope = app.Services.CreateScope()
        let migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>()
        migrationRunner.RunMigration()