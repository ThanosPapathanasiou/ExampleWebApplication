module ExampleApp.Database.Models.Post

open System.Data
open Dapper

type Post = { Id: int64; Title: string; Body: string }

let createPost (conn: IDbConnection) (title: string) (body: string) : Post =
    let sql = "INSERT INTO Posts(Title, Body) VALUES (@title, @body) RETURNING Id, Title, Body"
    let post = conn.QueryFirst<Post>(sql, {| Id = id; Title = title; Body = body |})
    post

let readPost (conn: IDbConnection) (id: int64) : Post option =
    let sql = "SELECT Id, Title, Body FROM Posts WHERE Id = @id"
    let post = conn.QueryFirstOrDefault<Post>(sql, {| id = id |})
    if box post = null then None else Some post

let readPosts (conn: IDbConnection) : Post seq =
    let sql = "SELECT Id, Title, Body FROM Posts"
    conn.Query<Post>(sql)

let updatePost (conn: IDbConnection) (post: Post) : Post =
    let sql = "UPDATE Posts SET Title = @title, Body = @body WHERE Id = @id RETURNING Id, Title, Body"
    let updatedPost = conn.QueryFirst<Post>(sql, {| id = post.Id; title = post.Title; body = post.Body |})
    updatedPost

let deletePost (conn: IDbConnection) (id: int64) : unit =
    let sql = "DELETE FROM Posts WHERE Id = @id"
    conn.Execute(sql, {| id = id |}) |> ignore

