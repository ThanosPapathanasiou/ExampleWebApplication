module ExampleApp.Website.Posts

open Falco.Htmx
open Falco.Markup

open ExampleApp.Database.Models.Post
open ExampleApp.Website.Base
open ExampleApp.Website.Components.FormComponents
open Falco.Routing

// Partial views (form as base html element)
// 1. read :id              | GET /posts/:id/view | done
// 2. edit :id              | GET /posts/:id/edit | done 

// Child views (main as the base html element)
// 1. read all              | GET /posts          | needs work
// 2. create new            | GET /posts/new      | todo
// 3. read/update :id       | GET /posts/:id      | done

// -- PARTIAL VIEWS --
let clickToEdit_PartialView (post: Post) =
    clickToEditFormComponent "Post" $"/posts/{post.Id}/edit" "/posts/" [
        textFieldComponent { Id="Title"; Name="Title"; Label="Title"; Value= Valid post.Title }
        textFieldComponent { Id="Body"; Name="Body"; Label="Body"; Value= Valid post.Body }
    ]

let saveOrCancel_PartialView token (post: Post) =
    saveOrCancelFormComponent token "Post" $"/posts/{post.Id}" $"/posts/{post.Id}/view" [
        textFieldComponent { Id="Title"; Name="Title"; Label="Title"; Value= Valid post.Title }
        textFieldComponent { Id="Body"; Name="Body"; Label="Body"; Value= Valid post.Body }
    ]

// -- CHILD VIEWS -- the base XmlNode should be `main`
let getAll_ChildView (posts: Post seq): XmlNode =
    _main [ _classes_ [ Bulma.container; ] ] [
        _section [ ] [
            _h1 [ _class_ Bulma.title ] [ _text "My latest posts!" ]
        ]
        _br []
        _section [ ] [
            _div [ _class_ Bulma.container ] [
                for post in posts ->
                    _article [ _class_ Bulma.media ] [
                        _div [ _class_ Bulma.``media-content`` ] [
                            _div [ _class_ Bulma.content ] [
                                _p [] [
                                    _strong [] [ _textEnc post.Title ]
                                    _br []
                                    _textEnc post.Body
                                    _br []
                                    _small [] [
                                        _a [
                                           Hx.get $"/posts/{post.Id}"
                                           Hx.pushUrlOn
                                           Hx.swapOuterHtml
                                           _hxTarget_ "main"
                                        ] [ _text "Read more..." ]
                                    ]
                                ]
                            ]
                        ]
                    ]
            ] 
        ]
    ]

let getSingle_ChildView partialView : XmlNode =
    _main [ _classes_ [ Bulma.container; ] ] [
        _section [ ] [
            _h1 [ _class_ Bulma.title ] [ _text "My latest posts!" ]
        ]
        _br []
        _section [ ] [
            _div [ _class_ Bulma.container ] [
               partialView
            ]
        ]
    ]

// TODO: createNew_ChildView 

// -- FALCO ENDPOINTS -- compose them on top of the generic functions provided
let ``GET /posts``           = fun ctx -> ``GET /<model>``           ctx readPosts                            getAll_ChildView     parentView
let ``GET /posts/:id``       = fun ctx -> ``GET /<model>/:id``       ctx readPost   clickToEdit_PartialView   getSingle_ChildView  parentView
let ``GET /posts/:id/view``  = fun ctx -> ``GET /<model>/:id/view``  ctx readPost   clickToEdit_PartialView   getSingle_ChildView  parentView
let ``GET /posts/:id/edit``  = fun ctx -> ``GET /<model>/:id/edit``  ctx readPost   saveOrCancel_PartialView  getSingle_ChildView  parentView

let postRoutes = [
        get  "/posts"                   ``GET /posts``
        get  "/posts/{id:int}"          ``GET /posts/:id``
        get  "/posts/{id:int}/view"     ``GET /posts/:id/view``
        get  "/posts/{id:int}/edit"     ``GET /posts/:id/edit``
        // TODO: add missing put / delete `posts` endpoints
]