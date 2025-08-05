module ExampleApp.Website.Contact

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open ExampleApp.Website.Htmx
open ExampleApp.Website.Base

let view: XmlNode =
    main [] [
        section [ _class Bulma.section ] [
            h1 [ _class Bulma.title ] [ Text "Contact Me" ]
            form [] [
                div [ _class Bulma.field ] [
                    label [ _class Bulma.label ] [ Text "Name" ]
                    div [ _class Bulma.control ] [
                        input [ _class Bulma.input ; _type "text" ; _placeholder "Enter your name" ]
                    ]
                ]
                
                div [ _class Bulma.field ] [
                    label [ _class Bulma.label ] [ Text "Email" ]
                    div [ _classes [ Bulma.control; Bulma.``has-icons-left``; Bulma.``has-icons-right``] ] [
                        input [
                            _classes [ Bulma.input; Bulma.``is-success`` ]
                            _type "text"
                            _placeholder "Enter your email"
                        ]
                        span [ _classes [ Bulma.icon; Bulma.``is-small``; Bulma.``is-left`` ] ] [
                            i [ _class "fas fa-user" ] [] 
                        ]
                        span [ _classes [ Bulma.icon; Bulma.``is-small``; Bulma.``is-left`` ] ] [
                            i [ _class "fas fa-check" ] [] 
                        ]
                        
                    ]
                ]
                
            ]
        ]
    ]

let ``GET /contact`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        if isHtmxRequest ctx then
            htmlView view next ctx    
        else
            htmlView (createPage view) next ctx