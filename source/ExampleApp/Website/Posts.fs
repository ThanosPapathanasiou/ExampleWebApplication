module ExampleApp.Website.Posts

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open ExampleApp.Database.ConnectionManager
open ExampleApp.Database.Models.Post
open ExampleApp.Website.Htmx
open ExampleApp.Website.Base

let indexView (posts: Post array): XmlNode =
    
    main [] [
        section [ _class "section" ] [
            h1 [ _class "title" ] [ Text "My latest posts!" ]
            h2 [ _class "subtitle" ] [ Text "let's work together!" ]
        ]
        section [ _class "section" ] [
            div [ _class "container" ] [
                for post in posts ->
                    article [ _class "article" ] [
                        h3 [] [ Text post.Title ]
                        div [ _class "content" ] [ Text post.Body ] 
                    ]
            ] 
        ]
    ]

let createPostView: XmlNode =
    main [] []

let readPostView: XmlNode =
    main [] [] 

let ``GET /posts`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let connectionManager = ctx.GetService<SqliteConnectionManager>()
        use conn = connectionManager.GetConnection()
        let posts = readPosts conn |> Seq.take 10 |> Seq.toArray
        
        let view = indexView posts
        
        if isHtmxRequest ctx then
            htmlView view next ctx    
        else
            htmlView (createPage view) next ctx
