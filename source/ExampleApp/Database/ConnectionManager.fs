module ExampleApp.Database.ConnectionManager

open Dapper
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Configuration

type SqliteConnectionManager(configuration: IConfiguration) =
    
    /// Don't forget to dispose the connection you get.
    member this.GetConnection() =
        let connectionString = configuration.GetConnectionString("db")
        let connection = new SqliteConnection(connectionString)
        connection.Open()
        connection.Execute("PRAGMA foreign_keys = ON") |> ignore
        connection
