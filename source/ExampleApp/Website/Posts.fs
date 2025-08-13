module ExampleApp.Website.Posts

open Falco
open Falco.Markup
open ExampleApp.Database.Models.Post
open ExampleApp.Database.ConnectionManager
open ExampleApp.Website.Base
open ExampleApp.Website.Components.FormComponents

// list of posts, link to create one
let indexView (posts: Post seq): XmlNode =
    _main [ _classes_ [ Bulma.container; ] ] [
        _section [ ] [
            _h1 [ _class_ Bulma.title ] [ _text "My latest posts!" ]
        ]
        _section [ ] [
            _div [ _class_ Bulma.container ] [
                for post in posts ->
                    _article [ _class_ Bulma.section ] [
                        _h3 [] [ _text post.Title ]
                        _p [ _class_ Bulma.content ] [ _text post.Body ] 
                    ]
            ] 
        ]
    ]

// create a new post 
let createView token: XmlNode =
    _main [ _class_ Bulma.container; ] [
        formComponent token "create-post" "Create post" "/posts/new" "Submit" [
            textFieldComponent { Id="Name"     ; Name="Name"     ; Label="Name"     ; Value=Initial }
            textFieldComponent { Id="Lastname" ; Name="Lastname" ; Label="Lastname" ; Value=Valid "Papathanasiou" }
            textFieldComponent { Id="Email"    ; Name="Email"    ; Label="Email"    ; Value=Invalid ("thanos@email.com", "Please insert a valid email") }
        ]
    ]

// edit a post
let editView token: XmlNode =
    _main [ _class_ Bulma.container; ] [
        formComponent token "edit-post" "Edit post" "/posts/" "Submit" [
            _a [] [ _text "Something" ]
        ]
    ]

let ``GET /posts`` : FalcoEndpoint  = fun ctx ->
    let connectionManager = ctx.Plug<SqliteConnectionManager>()
    use conn = connectionManager.GetConnection()
    let posts = readPosts conn |> Seq.toArray

    let fullView =
        if isHtmxRequest ctx then
            indexView posts
        else
            parentView (indexView posts)
    
    Response.ofHtml fullView ctx

let ``GET /posts/new`` : FalcoEndpoint  = fun ctx ->
    let fullView token =
        if isHtmxRequest ctx then
            createView token
        else
            parentView (createView token)
    
    Response.ofHtmlCsrf fullView ctx
