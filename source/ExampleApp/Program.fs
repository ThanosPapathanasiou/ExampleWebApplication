open System.Data
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Falco

open Modules.SqliteConnectionManager
open Modules.MigrationRunner

open ExampleApp.Website.Routes

[<EntryPoint>]
let main args =
    
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddAntiforgery() |> ignore
    
    builder.Services.AddSingleton<SqliteConnectionManager>(
        fun provider ->
            let connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("db")
            SqliteConnectionManager(connectionString)
        ) |> ignore
    builder.Services.AddScoped<IDbConnection>(
        fun provider ->
            let connectionManager= provider.GetRequiredService<SqliteConnectionManager>()
            connectionManager.GetConnection()
        ) |> ignore
    builder.Services.AddSingleton<LocationOfMigrationScripts>("ExampleApp.Migrations.") |> ignore
    builder.Services.AddSingleton<MigrationRunner>() |> ignore

    let app = builder.Build()

    // migration
    app.PerformDatabaseMigrations() 
    
    // background workers
    // TODO: add background workers
    // TODO: add cron jobs
    
    // website
    app
        .UseStaticFiles()
        .UseAntiforgery()
        .UseRouting()
        .UseFalco(websiteEndpoints)
        .UseFalcoNotFound(notFoundHandler)
        
    app.Run()

    0 // Exit code

