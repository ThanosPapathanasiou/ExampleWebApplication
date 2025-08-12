module ExampleApp.Website.Contact

open ExampleApp.Website.Core
open ExampleApp.Website.Base
open ExampleApp.Website.Components.FormComponents

open Falco
open Falco.Markup
open Falco.Security

let childView token: XmlNode =
    _main [ _class_ Bulma.container; ] [
        _div [ _class_ Bulma.columns ] [
            _div [ _classes_ [ Bulma.column; Bulma.``is-one-fifth`` ] ] [
                _aside [ _class_ Bulma.menu ] [
                    _p [ _class_ Bulma.``menu-label`` ] [ _text "General" ]
                    _ul [ _class_ Bulma.``menu-list`` ] [
                        _li [] [
                            _a [] [ _text "Dashboard" ]
                            _a [] [ _text "Customers" ]
                        ]
                    ]
                    _p [ _class_ Bulma.``menu-label`` ] [ _text "General" ]
                    _ul [ _class_ Bulma.``menu-list`` ] [
                        _li [] [
                            _a [] [ _text "Dashboard" ]
                            _a [] [ _text "Customers" ]
                        ]
                    ]
                    _p [ _class_ Bulma.``menu-label`` ] [ _text "General" ]
                    _ul [ _class_ Bulma.``menu-list`` ] [
                        _li [] [
                            _a [] [ _text "Dashboard" ]
                            _a [] [ _text "Customers" ]
                        ]
                    ] 
                ]
            ]
            _div [ _class_ Bulma.column ] [
                _h1 [ _class_ Bulma.title ] [ _text "Contact Me" ]
                _form [ _methodPost_ ] [
                    Xsrf.antiforgeryInput token
                    textFieldComponent { Id="Name"     ; Name="Name"     ; Label="Name"     ;  Url="/jhsdkfjsd" ; Value=Initial }
                    textFieldComponent { Id="Lastname" ; Name="Lastname" ; Label="Lastname" ;  Url="/jhsdkfjsd" ; Value=Valid "Papathanasiou" }
                    textFieldComponent { Id="Email"    ; Name="Email"    ; Label="Email"    ;  Url="/jhsdkfjsd" ; Value=Invalid ("thanos@flsdkjlf", "Please insert a valid email") }
                ]
            ]
        ]
    ]

let ``GET /contact`` : FalconEndpoint = fun ctx ->
    let view token =
        if isHtmxRequest ctx then
           childView token
        else
            parentView (childView token)
    
    Response.ofHtmlCsrf view ctx
