module ExampleApp.Website.Contact

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open ExampleApp.Website.Htmx
open ExampleApp.Website.Base

let view: XmlNode =
    main [] [
        section [ _class "section" ] [
            h1 [ _class "title" ] [ Text "Contact Me" ]
            form [] [
                div [ _class "field" ] [
                    label [ _class "label" ] [ Text "Name" ]
                    div [ _class "control" ] [
                        input [
                            _class "input"
                            _type "text"
                            _placeholder "Enter your name"
                        ]
                    ]
                ]
                
                div [ _class "field" ] [
                    label [ _class "label" ] [ Text "Email" ]
                    div [ _class "control has-icons-left has-icons-right" ] [
                        input [
                            _class "input is-success"
                            _type "text"
                            _placeholder "Enter your email"
                        ]
                        span [ _class "icon is-small is-left" ] [
                            i [ _class "fas fa-user" ] [] 
                        ]
                        span [ _class "icon is-small is-right" ] [
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