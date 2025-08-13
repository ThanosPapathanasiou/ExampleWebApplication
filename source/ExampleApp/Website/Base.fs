module ExampleApp.Website.Base

open Falco.Markup
open Falco.Htmx
open Microsoft.AspNetCore.Http
open Zanaptak.TypedCssClasses

[<Literal>]
let bulmaUrl = "https://cdn.jsdelivr.net/npm/bulma@1.0.4/css/bulma.min.css"
type Bulma = CssClasses<bulmaUrl>

// Falco helpers
type FalcoEndpoint = HttpContext -> System.Threading.Tasks.Task
let inline isHtmxRequest (ctx:HttpContext) : bool =
        ctx.Request.Headers.ContainsKey "HX-Request" &&
        not (ctx.Request.Headers.ContainsKey "HX-History-Restore-Request") 

// Falco Markup helpers and XmlAttributes
let inline _classes_ (attributes : string list) = attributes |> String.concat " " |> _class_
let _hyperScript_  = Attr.create "_"
let _dataTarget_   = Attr.create "data-target"
let _ariaHidden_   = Attr.create "aria-hidden"
let _ariaLabel_    = Attr.create "aria-label"
let _ariaExpanded_ = Attr.create "aria-expanded"

// ---------------------------------------------------------------------------------------------- //

/// Creates the html XmlNode that we pass to Falco to be returned to the browser.
/// Accepts 'content' for a child page. The content should be a `main [] []` element. 
let parentView (content: XmlNode) : XmlNode =
    
    /// creates a navbarItem `a` element
    let _navbarItem (text:string) (link:string) (icon:string): XmlNode  =
        let id = text.Replace(" ", "-").ToLowerInvariant()
        _a [
            _id_ id
            _classes_ [ Bulma.``navbar-item`` ]
            Hx.get link ; Attr.create "hx-target" "main" ; Hx.swapOuterHtml ; Hx.pushUrlOn
            _hyperScript_ "on click remove .is-active from <a/> in closest .navbar-menu then add .is-active to me"
        ] [
            _span [ _classes_ [ Bulma.icon ; Bulma.``is-small`` ] ] [
                _i [ _class_ icon; _ariaHidden_ "true"] [  ]
            ]
            _span [ _class_ Bulma.``ml-1`` ] [ _text text ]
        ]
 
    _html [ _id_ "html" ; _lang_ "en"; ] [
        _head [] [
            _title []  [ _text "Example Application" ]
            _meta [ _charset_ "utf-8" ]
            _meta [ _name_ "viewport"; _content_ "width=device-width, initial-scale=1" ]

            _link [ _rel_ "icon"; _href_ "/favicon.png" ]
            _link [ _rel_ "stylesheet" ; _type_ "text/css"; _href_ bulmaUrl ]
            _script [ _src_ "https://cdn.jsdelivr.net/npm/htmx.org@2.0.6/dist/htmx.min.js" ] []
            _script [ _src_ "https://unpkg.com/hyperscript.org@0.9.14" ] []
            _script [ _src_ "https://kit.fontawesome.com/2e85dbb04c.js" ] []
            
            // _hyperScript """
            //                 on keypress[key is '1'] trigger click on #home
            //                 on keypress[key is '2'] trigger click on #contact
            //                 on keypress[key is '3'] trigger click on #about-us
            //              """
        ]

        _body [ _class_ Bulma.``is-fullheight`` ; _style_ "min-height: 100vh; display: flex; flex-direction: column;" ] [
            _header [ _style_ "padding-bottom: 48px;" ] [
                _nav [ _classes_ [ Bulma.navbar; Bulma.container ] ; _role_ "navigation"; _ariaLabel_ "main navigation" ] [
                    _div [ _class_  Bulma.``navbar-brand`` ] [
                        _a [
                            _role_ "button"
                            _class_ Bulma.``navbar-burger`` ; _dataTarget_ "navbarBasicExample"
                            _hyperScript_ "on click toggle .is-active on me then toggle .is-active on .navbar-menu"
                            _ariaLabel_ "menu" ; _ariaExpanded_ "false"
                        ] [
                            _span [ _ariaHidden_ "true" ] [] ; _span [ _ariaHidden_ "true" ] []
                            _span [ _ariaHidden_ "true" ] [] ; _span [ _ariaHidden_ "true" ] []
                        ]
                    ]
                    _div [ _id_ "navbarBasicExample" ; _class_  Bulma.``navbar-menu`` ] [
                        _div [ _class_ Bulma.``navbar-start`` ] [
                            _navbarItem "Home"       "/"          "fa-solid fa-house"
                            _navbarItem "Contact"    "/contact"   "fa-solid fa-message"
                            _navbarItem "About us"   "/about"     "fa-solid fa-people-group"
                        ]
                        _div [ _class_ Bulma.``navbar-end`` ] [
                            _div [ _class_ Bulma.``navbar-item`` ] [
                                _div [ _class_ Bulma.buttons ] [
                                    _a [
                                        _class_ Bulma.button
                                        // TODO: we should actually do this with javascript and save stuff to local storage
                                        _hyperScript_ """
                                            on click toggle [@data-theme=dark] on html
                                            on click if my textContent is 'Light mode' then set my textContent to 'Dark mode' else set my textContent to 'Light mode'
                                        """ 
                                    ] [ _text "Dark mode" ]
                                ]
                            ]
                        ]
                    ]                        
                ]
            ]
            
            content // content should be main [] []
            
            _footer [ _class_ Bulma.footer ; _style_ "margin-top: auto" ] [
                _div [ _classes_ [ Bulma.content ; Bulma.``has-text-centered``] ] [
                    _p [] [
                        _text "Built using:"
                        _text " " ; _a [ _href_ "https://fsharp.org" ] [ _text "F#" ]
                        _text " " ; _a [ _href_ "https://www.falcoframework.com" ] [ _text "Falco" ]
                        _text " " ; _a [ _href_ "https://htmx.org" ] [ _text "Htmx" ]
                        _text " " ; _a [ _href_ "https://bulma.io" ] [ _text "Bulma" ]
                    ]
                    _a [
                        _href_ "https://github.com/ThanosPapathanasiou/ExampleWebApplication"
                    ] [
                        _text "Browse the code"
                    ]
                ]
            ]
        ]
    ]