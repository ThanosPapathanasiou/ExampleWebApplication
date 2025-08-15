module Modules.SqliteConnectionManager

open Dapper
open Microsoft.Data.Sqlite

type SqliteConnectionManager(connectionString: string) =
    
    /// Don't forget to dispose the connection you get.
    member this.GetConnection() =
        let connection = new SqliteConnection(connectionString)
        connection.Open()
        connection.Execute("PRAGMA foreign_keys = ON") |> ignore
        connection
