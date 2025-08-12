module ExampleApp.Website.Posts

open Falco
open Falco.Markup
open ExampleApp.Website.Core
open ExampleApp.Website.Base
open ExampleApp.Database.Models.Post
open ExampleApp.Database.ConnectionManager

let childView (posts: Post array): XmlNode =
    
    _main [ _classes_ [ Bulma.container; ] ] [
        _section [ ] [
            _h1 [ _class_ Bulma.title ] [ _text "My latest posts!" ]
            _h2 [ _class_ Bulma.subtitle ] [ _text "let's work together!" ]
        ]
        _section [ ] [
            _div [ _class_ Bulma.container ] [
                for post in posts ->
                    _article [ _class_ "article" ] [
                        _h3 [] [ _text post.Title ]
                        _div [ _class_ Bulma.content ] [ _text post.Body ] 
                    ]
            ] 
        ]
    ]

let createPostView: XmlNode =
    _main [] []

let readPostView: XmlNode =
    _main [] [] 

let ``GET /posts`` : FalconEndpoint  =
    fun ctx ->
        let connectionManager = ctx.Plug<SqliteConnectionManager>()
        use conn = connectionManager.GetConnection()
        let posts = readPosts conn |> Seq.take 10 |> Seq.toArray

        let view =
            if isHtmxRequest ctx then
                childView posts
            else
                parentView (childView posts)
        
        Response.ofHtml view ctx


