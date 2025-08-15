module ExampleApp.Website.Posts

open System.ComponentModel
open System.ComponentModel.DataAnnotations.Schema

open Falco.Htmx
open Falco.Markup

open Modules.ActiveRecord
open ExampleApp.Website.Base
open ExampleApp.Website.ParentView
open ExampleApp.Website.Components.FormComponents

// ----- Model -----

[<Table("Posts")>]
type Post() =
    inherit ActiveRecord()

    [<Column("Title")>]
    [<DisplayName("Title")>]
    member val Title : string = "" with get, set
    
    [<Column("Body")>]
    [<DisplayName("Body")>]
    member val Body : string = "" with get, set  


// ----- Views -----

let getAll_ChildView (posts: Post seq): XmlNode =
    _main [ _class_ Bulma.container ] [
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
            _div [ _class_ Bulma.container ] [
               partialView
            ]
        ]
    ]


// ----- Routes -----
let postRoutes = formRoutes<Post> getAll_ChildView getSingle_ChildView parentView
