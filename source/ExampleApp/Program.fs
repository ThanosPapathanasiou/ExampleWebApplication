open ExampleApp.Database.ConnectionManager
open ExampleApp.Migrations
open ExampleApp.Website.Routes

open Falco

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

[<EntryPoint>]
let main args =
    
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddAntiforgery() |> ignore
    builder.Services.AddSingleton<SqliteConnectionManager>() |> ignore
    builder.Services.AddSingleton<LocationOfMigrationScripts>("ExampleApp.Database.Migrations.") |> ignore
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
        .UseFalco(websiteRoutes)
        .UseFalcoNotFound(notFoundHandler)
        
    app.Run()

    0 // Exit code

