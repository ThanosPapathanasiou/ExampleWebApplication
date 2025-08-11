module ExampleApp.Website.Contact

open ExampleApp.Website.Components.FormComponents
open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open ExampleApp.Website.Core
open ExampleApp.Website.Base

let view: XmlNode =
    main [ _class Bulma.container; ] [
        div [ _class Bulma.columns ] [
            div [ _classes [ Bulma.column; Bulma.``is-one-fifth`` ] ] [
                aside [ _class Bulma.menu ] [
                    p [ _class Bulma.``menu-label`` ] [ Text "General" ]
                    ul [ _class Bulma.``menu-list`` ] [
                        li [] [
                            a [] [ Text "Dashboard" ]
                            a [] [ Text "Customers" ]
                        ]
                    ]
                    p [ _class Bulma.``menu-label`` ] [ Text "General" ]
                    ul [ _class Bulma.``menu-list`` ] [
                        li [] [
                            a [] [ Text "Dashboard" ]
                            a [] [ Text "Customers" ]
                        ]
                    ]
                    p [ _class Bulma.``menu-label`` ] [ Text "General" ]
                    ul [ _class Bulma.``menu-list`` ] [
                        li [] [
                            a [] [ Text "Dashboard" ]
                            a [] [ Text "Customers" ]
                        ]
                    ] 
                ]
            ]
            div [ _class Bulma.column ] [
                h1 [ _class Bulma.title ] [ Text "Contact Me" ]
                form [ ] [
                    textFieldComponent { Id="Name"     ; Name="Name"     ; Label="Name"     ;  Url="/jhsdkfjsd" ; Value=Initial }
                    textFieldComponent { Id="Lastname" ; Name="Lastname" ; Label="Lastname" ;  Url="/jhsdkfjsd" ; Value=Valid "Papathanasiou" }
                    textFieldComponent { Id="Email"    ; Name="Email"    ; Label="Email"    ;  Url="/jhsdkfjsd" ; Value=Invalid ("thanos@flsdkjlf", "Please insert a valid email") }
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