module ExampleApp.Website.Base

open Falco
open Falco.Markup
open Falco.Htmx
open Zanaptak.TypedCssClasses
open ExampleApp.Database.ConnectionManager

[<Literal>]
let bulmaUrl = "https://cdn.jsdelivr.net/npm/bulma@1.0.4/css/bulma.min.css"
type Bulma = CssClasses<bulmaUrl>

// Falco Markup helpers and XmlAttributes
let inline _classes_ (attributes : string list) = attributes |> String.concat " " |> _class_
let inline _hxTarget_ target = Attr.create "hx-target" target
let _hyperScript_  = Attr.create "_"
let _dataTarget_   = Attr.create "data-target"
let _ariaHidden_   = Attr.create "aria-hidden"
let _ariaLabel_    = Attr.create "aria-label"
let _ariaExpanded_ = Attr.create "aria-expanded"


// FALCO HELPERS AND GENERIC CRUD ENDPOINTS

let inline isHtmxRequest (ctx:Microsoft.AspNetCore.Http.HttpContext) : bool =
    ctx.Request.Headers.ContainsKey "HX-Request" &&
    not (ctx.Request.Headers.ContainsKey "HX-History-Restore-Request") 

let ``GET /<model>``<'T>
    (ctx: Microsoft.AspNetCore.Http.HttpContext)
    (readAll: System.Data.IDbConnection -> 'T seq)
    (childView: 'T seq -> Falco.Markup.XmlNode)
    (parentView: Falco.Markup.XmlNode -> Falco.Markup.XmlNode)
    : System.Threading.Tasks.Task = 

    let inline isHtmxRequest (ctx:Microsoft.AspNetCore.Http.HttpContext) : bool =
        ctx.Request.Headers.ContainsKey "HX-Request" &&
        not (ctx.Request.Headers.ContainsKey "HX-History-Restore-Request") 
       
    let connectionManager = ctx.Plug<SqliteConnectionManager>()
    use conn  = connectionManager.GetConnection()
    let model =  conn |> readAll  |> Seq.truncate 5 |> Seq.toArray

    let fullView =
        if isHtmxRequest ctx then
           model |> childView 
        else
           model |> childView |> parentView
    
    Response.ofHtml fullView ctx

let ``GET /<model>/:id``<'T>
    (ctx: Microsoft.AspNetCore.Http.HttpContext)
    (readSingle: System.Data.IDbConnection -> int64 -> 'T Option)
    (partialView: 'T -> Falco.Markup.XmlNode)
    (childView: Falco.Markup.XmlNode -> Falco.Markup.XmlNode)
    (parentView: Falco.Markup.XmlNode -> Falco.Markup.XmlNode)
    : System.Threading.Tasks.Task =

    let connectionManager = ctx.Plug<SqliteConnectionManager>()
    use conn = connectionManager.GetConnection()

    let route  = Request.getRoute ctx
    let id = route.GetInt64 "id"
    let model   = Option.get (readSingle conn id)

    let fullView =
        if isHtmxRequest ctx then
            model |> partialView  |> childView 
        else
            model |> partialView  |> childView |> parentView
    
    Response.ofHtml fullView ctx
    
let ``GET /<model>/:id/view``<'T>
    (ctx: Microsoft.AspNetCore.Http.HttpContext)
    (readSingle: System.Data.IDbConnection -> int64 -> 'T Option)
    (partialView: 'T -> Falco.Markup.XmlNode)
    (childView: Falco.Markup.XmlNode -> Falco.Markup.XmlNode)
    (parentView: Falco.Markup.XmlNode -> Falco.Markup.XmlNode)
    : System.Threading.Tasks.Task =
        
    let connectionManager = ctx.Plug<SqliteConnectionManager>()
    use conn = connectionManager.GetConnection()

    let route = Request.getRoute ctx
    let id = route.GetInt64 "id" //TODO: get the 'id' from the model
    let model =  Option.get (readSingle conn id)

    let fullView =
        if isHtmxRequest ctx then
            model |> partialView 
        else
            model |> partialView |> childView |> parentView
    
    Response.ofHtml fullView ctx

let ``GET /<model>/:id/edit``<'T>
    (ctx: Microsoft.AspNetCore.Http.HttpContext)
    (readSingle: System.Data.IDbConnection -> int64 -> 'T Option)
    (partialView: Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet -> 'T -> Falco.Markup.XmlNode)
    (childView: Falco.Markup.XmlNode -> Falco.Markup.XmlNode)
    (parentView: Falco.Markup.XmlNode -> Falco.Markup.XmlNode)
    : System.Threading.Tasks.Task =
        
    let connectionManager = ctx.Plug<SqliteConnectionManager>()
    use conn = connectionManager.GetConnection()

    let route = Request.getRoute ctx
    let id = route.GetInt64 "id" //TODO: get the 'id' from the model
    let model =  Option.get (readSingle conn id)

    let fullView token =
        if isHtmxRequest ctx then
            partialView token model 
        else
            partialView token model |> childView |> parentView
    
    Response.ofHtmlCsrf fullView ctx    
    
    
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
            Hx.get link ; _hxTarget_ "main" ; Hx.swapOuterHtml ; Hx.pushUrlOn
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
                            _navbarItem "Home"   "/"       "fa-solid fa-house"
                            _navbarItem "Posts"  "/posts"  "fa-solid fa-message"
                            _navbarItem "About"  "/about"  "fa-solid fa-people-group"
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

    