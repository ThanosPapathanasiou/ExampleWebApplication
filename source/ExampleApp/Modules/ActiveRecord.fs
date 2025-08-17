module Modules.ActiveRecord
    
open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open System.Data
open System.Reflection
open Dapper
open Microsoft.AspNetCore.Http

[<AbstractClass>]
type ActiveRecord() =
    [<Key>]
    [<Column("Id")>]
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    member val Id : int64 = 0L with get, set

// HELPER FUNCTIONS

let getTableName<'T when 'T :> ActiveRecord> : string = typeof<'T>.GetCustomAttribute<TableAttribute>().Name 

/// Get non-database generated columns
let getColumnMembers<'T when 'T :> ActiveRecord> (record: 'T) : (string * obj) array =
    typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
    |> Array.filter (fun prop -> prop.GetCustomAttribute<DatabaseGeneratedAttribute>() = null)
    |> Array.filter (fun prop -> prop.GetCustomAttribute<ColumnAttribute>() <> null)
    |> Array.map (fun prop -> 
        let columnAttr = prop.GetCustomAttribute<ColumnAttribute>()
        columnAttr.Name, prop.GetValue(record))

// DATABASE FUNCTIONS

let createRecord<'T when 'T :> ActiveRecord> (conn: IDbConnection) (record: 'T) : 'T =
    let tableName = typeof<'T>.GetCustomAttribute<TableAttribute>().Name

    // Get properties with Column attributes, excluding DatabaseGenerated identity properties
    let properties = 
        typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        |> Array.filter (fun p -> 
            let columnAttr = p.GetCustomAttribute<ColumnAttribute>()
            let dbGenAttr = p.GetCustomAttribute<DatabaseGeneratedAttribute>()
            columnAttr <> null && 
            (dbGenAttr = null || dbGenAttr.DatabaseGeneratedOption <> DatabaseGeneratedOption.Identity))

    // Get column names and parameter names
    let columnNames = 
        properties 
        |> Array.map (fun p -> p.GetCustomAttribute<ColumnAttribute>().Name)
        |> String.concat ", "
    
    let paramNames = 
        properties 
        |> Array.map (fun p -> "@" + p.Name)
        |> String.concat ", "

    // Get all column names for the RETURNING clause (including Id)
    let allColumnNames = 
        typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        |> Array.filter (fun p -> p.GetCustomAttribute<ColumnAttribute>() <> null) 
        |> Array.map (fun p -> p.GetCustomAttribute<ColumnAttribute>().Name)
        |> String.concat ", "

    // Build the SQL INSERT statement with RETURNING clause
    let sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames}) RETURNING {allColumnNames}"

    // Create a dynamic parameter object
    let parameters = DynamicParameters()
    for prop in properties do
        let value = prop.GetValue(record)
        parameters.Add(prop.Name, value)

    // Execute the query and return the created record
    conn.QueryFirst<'T>(sql, parameters)

let readRecord<'T when 'T :> ActiveRecord> (conn: IDbConnection) (id: int64) : 'T option =

    // Get the table name from the Table attribute
    let tableAttr = typeof<'T>.GetCustomAttribute<TableAttribute>()
    let tableName = 
        match tableAttr with
        | null -> failwith $"Type {typeof<'T>.Name} must have a Table attribute"
        | attr -> attr.Name

    // Get properties with Column attributes
    let properties = 
        typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        |> Array.filter (fun p -> p.GetCustomAttribute<ColumnAttribute>() <> null)

    // Get column names
    let columnNames = 
        properties 
        |> Array.map (fun p -> p.GetCustomAttribute<ColumnAttribute>().Name)
        |> String.concat ", "

    // Build the SQL SELECT statement using the known Id column from ActiveRecord
    let sql = $"SELECT {columnNames} FROM {tableName} WHERE Id = @id"

    // Execute the query
    let record = conn.QueryFirstOrDefault<'T>(sql, {| id = id |})
    if box record = null then None else Some record

let readRecords<'T when 'T :> ActiveRecord> (conn: IDbConnection) : seq<'T> =
    // Get the table name from the Table attribute
    let tableAttr = typeof<'T>.GetCustomAttribute<TableAttribute>()
    let tableName = 
        match tableAttr with
        | null -> failwith $"Type {typeof<'T>.Name} must have a Table attribute"
        | attr -> attr.Name

    // Get properties with Column attributes
    let properties = 
        typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        |> Array.filter (fun p -> p.GetCustomAttribute<ColumnAttribute>() <> null)

    // Get column names
    let columnNames = 
        properties 
        |> Array.map (fun p -> p.GetCustomAttribute<ColumnAttribute>().Name)
        |> String.concat ", "

    // Build the SQL SELECT statement
    let sql = $"SELECT {columnNames} FROM {tableName}"

    // Execute the query using Dapper
    conn.Query<'T>(sql)

// HTTP FUNCTIONS

/// This function takes an IFormCollection and gives you a record of type 'T
/// You can use it in PUT / POST handlers  
let getRecordFromHttpRequest<'T when 'T :> ActiveRecord> (request: HttpRequest) : 'T =
    let form = request.Form
    let instance = Activator.CreateInstance<'T>()
    let properties = typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
    for prop in properties do
        let value =
            if prop.Name = "Id" then
                request.RouteValues.["id"].ToString()
            else
                form.[prop.Name].ToString()
        let convertedValue = Convert.ChangeType(value, prop.PropertyType)
        prop.SetValue(instance, convertedValue)
    instance

/// This function will validate the 'T record based on its System.ComponentModel validations
let validateRecord<'T when 'T :> ActiveRecord> (record: 'T) : ValidationResult array  =
    let context = ValidationContext(record)
    let results = ResizeArray<ValidationResult>()
    Validator.TryValidateObject(record, context, results, true) |> ignore
    results.ToArray()
