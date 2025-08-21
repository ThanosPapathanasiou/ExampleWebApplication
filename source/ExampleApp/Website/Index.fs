module ExampleApp.Website.Index

open Falco
open Falco.Markup

open ExampleApp.Website.ParentView

let childView =
    _main [ _class_ Bulma.container ] [
        _section [ _class_ Bulma.hero ] [
            _div [ _class_ Bulma.``hero-body`` ] [
                _h1 [ _class_ Bulma.title ] [ _textEnc "Example" ]
                _p' "An example application using F#, Falco, Htmx and Bulma."
            ]
        ]
    ]

let ``GET /`` = fun ctx ->
    let view =
        if isHtmxRequest ctx then
            childView
        else
            parentView childView
    
    Response.ofHtml view ctx