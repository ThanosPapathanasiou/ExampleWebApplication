module ExampleApp.Website.Base

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open Zanaptak.TypedCssClasses

open ExampleApp.Website.Htmx

[<Literal>]
let bulmaUrl = "https://cdn.jsdelivr.net/npm/bulma@1.0.4/css/bulma.min.css"

type Bulma = CssClasses<bulmaUrl>

let inline _classes attributes =
  attributes |> String.concat " " |> _class

/// Creates the html XmlNode that we pass to Giraffe to be returned to the browser.
/// Accepts 'content' for a child page. The content should be a `main [] []` element. 
let createPage (content: XmlNode) : XmlNode =
    
    /// creates a navbarItem `a` element
    let navbarItem (text:string) (link:string) (icon:string): XmlNode  =
        let id = text.Replace(" ", "-").ToLowerInvariant()
        a [
            _id id
            _classes [Bulma.``navbar-item``]
            _hxGet link ; _hxTarget "main" ; _hxSwap "outerHTML" ; _hxPushUrl "true"
            _hyperScript "on click remove .is-active from <a/> in closest .navbar-menu then add .is-active to me"
        ] [
            span [ _classes [ Bulma.icon ; Bulma.``is-small`` ] ] [
                i [ _class icon; _ariaHidden "true"] [  ]
            ]
            span [ _class Bulma.``ml-1`` ] [ Text text ]
        ]
 
    html [ _id "html" ; _lang "en"; ] [
        head [] [
            title []  [ Text "Example Application" ]
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]

            link [ _rel "icon"; _href "/favicon.png" ]
            link [ _rel "stylesheet" ; _type "text/css"; _href bulmaUrl ]
            script [ _src "https://cdn.jsdelivr.net/npm/htmx.org@2.0.6/dist/htmx.min.js" ] []
            script [ _src "https://unpkg.com/hyperscript.org@0.9.14" ] []
            script [ _src "https://kit.fontawesome.com/2e85dbb04c.js" ] []
        ]

        body [
            _class Bulma.``is-fullheight``
            _style "min-height: 100vh; display: flex; flex-direction: column;"
            _hyperScript """
                            on keypress[key is '1'] trigger click on #home
                            on keypress[key is '2'] trigger click on #contact
                            on keypress[key is '3'] trigger click on #about-us
                         """
        ] [
            header [] [
                nav [ _class Bulma.navbar ; _role "navigation"; _ariaLabel "main navigation" ] [
                    div [ _class  Bulma.``navbar-brand`` ] [
                        a [
                            _role "button"
                            _class Bulma.``navbar-burger`` ; (attr "data-target" "navbarBasicExample")
                            _hyperScript "on click toggle .is-active on me then toggle .is-active on .navbar-menu"
                            _ariaLabel "menu" ; _ariaExpanded "false"
                        ] [
                            span [ _ariaHidden "true" ] [] ; span [ _ariaHidden "true" ] []
                            span [ _ariaHidden "true" ] [] ; span [ _ariaHidden "true" ] []
                        ]
                    ]
                    div [ _id "navbarBasicExample" ; _class  Bulma.``navbar-menu`` ] [
                        div [ _class Bulma.``navbar-start`` ] [
                            navbarItem "Home"       "/"          "fa-solid fa-house"
                            navbarItem "Contact"    "/contact"   "fa-solid fa-message"
                            navbarItem "About us"   "/about"     "fa-solid fa-people-group"
                        ]
                        div [ _class Bulma.``navbar-end`` ] [
                            div [ _class Bulma.``navbar-item`` ] [
                                div [ _class Bulma.buttons ] [
                                    a [
                                        _class Bulma.button
                                        // TODO: we should actually do this with javascript and save stuff to local storage
                                        _hyperScript """
                                            on click toggle [@data-theme=dark] on html
                                            on click if my textContent is 'Light mode' then set my textContent to 'Dark mode' else set my textContent to 'Light mode'
                                        """ 
                                    ] [ Text "Dark mode" ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            
            content // content should always be a "main [] []"
            
            footer [ _class Bulma.footer ; _style "margin-top: auto" ] [
                div [ _classes [ Bulma.content ; Bulma.``has-text-centered``] ] [
                    p [] [
                        Text "Built using F# Giraffe HTMX and simple css"
                    ]
                ]
            ]
        ]
    ]