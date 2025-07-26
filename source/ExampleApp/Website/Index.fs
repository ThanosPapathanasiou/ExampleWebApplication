module ExampleApp.Website.Index

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open ExampleApp.Website.Base
open ExampleApp.Website.Htmx

let view: XmlNode =
    main [] [
        section [ _class "hero" ] [
            div [ _class "hero-body" ] [
                h1 [ _class "title" ] [ Text "Example" ]
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
