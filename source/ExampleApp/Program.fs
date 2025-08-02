open ExampleApp.Database.ConnectionManager
open ExampleApp.Migrations
open ExampleApp.Scheduler
open ExampleApp.Website.Routes
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddGiraffe() |> ignore
    builder.Services.AddSingleton<SqliteConnectionManager>() |> ignore
    builder.Services.AddSingleton<LocationOfMigrationScripts>("ExampleApp.Database.Migrations.") |> ignore
    builder.Services.AddSingleton<MigrationRunner>() |> ignore
    builder.Services.RegisterScheduler() |> ignore
    
    let app = builder.Build()

    app.PerformDatabaseMigrations() 
    
    app.PerformSchedulerMigration()
    app.UseScheduler()
    
    app.UseStaticFiles() |> ignore
    app.UseGiraffe(websiteRoutes)

    app.Run()

    0 // Exit code

