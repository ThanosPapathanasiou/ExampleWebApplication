module ExampleApp.Website.Index

open Falco
open Falco.Markup
open ExampleApp.Website.Base
open ExampleApp.Website.Core

let childView =
    _main [ _class_ Bulma.container ] [
        _section [ _class_ Bulma.hero ] [
            _div [ _class_ Bulma.``hero-body`` ] [
                _h1 [ _class_ Bulma.title ] [ _text "Example" ]
                _p' "An example application using F#, Falco, htmx and simple css."
            ]
        ]
    ]

let ``GET /`` : FalconEndpoint = fun ctx ->
    let view =
        if isHtmxRequest ctx then
            childView
        else
            parentView childView
    
    Response.ofHtml view ctx