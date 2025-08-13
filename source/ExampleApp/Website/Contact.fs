module ExampleApp.Website.Contact

open ExampleApp.Website.Base
open ExampleApp.Website.Components.FormComponents

open Falco
open Falco.Markup

let sideMenu = 
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

let childView token: XmlNode =
    _main [ _class_ Bulma.container; ] [
        _div [ _class_ Bulma.columns ] [
            _div [ _classes_ [ Bulma.column; Bulma.``is-one-fifth`` ] ] [
                sideMenu
            ]
            _div [ _classes_ [ Bulma.column; ] ] [
                formComponent token "contact-me" "Contact Me" "/contact" "Submit" [
                    textFieldComponent { Id="Name"     ; Name="Name"     ; Label="Name"     ;  Value=Initial }
                    textFieldComponent { Id="Lastname" ; Name="Lastname" ; Label="Lastname" ;  Value=Valid "Papathanasiou" }
                    textFieldComponent { Id="Email"    ; Name="Email"    ; Label="Email"    ;  Value=Invalid ("thanos@email.com", "Please insert a valid email") }   
                ]
            ]
        ]
    ]

let ``GET /contact`` : FalcoEndpoint = fun ctx ->
    let view token =
        if isHtmxRequest ctx then
           childView token
        else
            parentView (childView token)
    
    Response.ofHtmlCsrf view ctx
