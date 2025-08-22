module ExampleApp.Website.Posts

open System.ComponentModel
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

open ExampleApp.Website.ParentView
open Falco.Htmx
open Falco.Markup

open Modules.ActiveRecord
open ExampleApp.Website.Components.CrudFormComponents

// ----- Model -----

[<Table("Posts")>]
type Post() =
    inherit ActiveRecord()

    [<Column("Title")>]
    [<DisplayName("Title")>]
    [<TextFieldComponent>]
    [<Required>]
    member val Title : string = "" with get, set

    [<Column("Author")>]
    [<DisplayName("Author")>]
    [<TextFieldComponent>]
    [<EmailAddress>]
    [<Required>]
    member val Author : string = "" with get, set
    
    [<Column("Type")>]
    [<DisplayName("Type")>]
    [<StaticDropdownComponent("Personal", "Technical")>]
    [<Required>]
    member val Type : string = "" with get, set

    [<Column("Body")>]
    [<DisplayName("Body")>]
    [<TextAreaComponent>]
    [<Required>]
    member val Body : string = "" with get, set

// ----- View -----

let singlePostView (post: Post): XmlNode =
    let baseUrl = getTableName<Post>.ToLowerInvariant()

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
                           Hx.get $"/{baseUrl}/{post.Id}"
                           Hx.pushUrlOn
                           Hx.swapOuterHtml
                           _hxTarget_ "main"
                        ] [ _text "Read more..." ]
                    ]
                ]
            ]
        ]
    ]

// ----- Endpoints -----
let postEndpoints = getEndpointListForType<Post> singlePostView parentView

