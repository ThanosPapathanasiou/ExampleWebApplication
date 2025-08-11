module ExampleApp.Website.Index

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open ExampleApp.Website.Base
open ExampleApp.Website.Core

let view: XmlNode =
    main [ _class Bulma.container ] [
        section [ _class Bulma.hero ] [
            div [ _class Bulma.``hero-body`` ] [
                h1 [ _class Bulma.title ] [ Text "Example" ]
                p [] [
                    Text "An example application using F#, giraffe, htmx and simple css."
                ]
            ]
        ]
    ]

let ``GET /`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let renderView =
            match isHtmxRequest ctx with
            | true -> view
            | false -> createPage view
        htmlView renderView next ctx
