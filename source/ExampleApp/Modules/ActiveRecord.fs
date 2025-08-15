module Modules.ActiveRecord
    
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open System.Data
open System.Reflection
open Dapper

[<AbstractClass>]
type ActiveRecord() =
    [<Key>]
    [<Column("Id")>]
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    member val Id : int64 = 0L with get, set

// CRUD

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


let getTableName<'T when 'T :> ActiveRecord> : string =
    typeof<'T>.GetCustomAttribute<TableAttribute>().Name

let getColumnMembers<'T> (record: 'T) =
    typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
    |> Array.filter (fun prop -> prop.GetCustomAttribute<DatabaseGeneratedAttribute>() = null)
    |> Array.filter (fun prop -> prop.GetCustomAttribute<ColumnAttribute>() <> null)
    |> Array.map (fun prop -> 
        let columnAttr = prop.GetCustomAttribute<ColumnAttribute>()
        columnAttr.Name, prop.GetValue(record))


// TODO: implement validatePerson<'T when 'T :> ActiveRecord> (record: 'T) that works with System.ComponentMode.DataAnnotations
 
// let validatePerson<'T when 'T :> ActiveRecord> (record: 'T) =
//     let context = ValidationContext(record)
//     let results = ValidationResult()
//     let isValid = Validator.TryValidateObject(record, context, results, true)
//     if isValid then
//         Ok record
//     else
//         Error results    