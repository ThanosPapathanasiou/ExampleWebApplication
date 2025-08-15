module ExampleApp.Website.Base

open Falco.Markup
open Zanaptak.TypedCssClasses

[<Literal>]
let bulmaUrl = "https://unpkg.com/bulma@1.0.4/css/bulma.css"
type Bulma = CssClasses<bulmaUrl>

// Falco Markup helpers and XmlAttributes
let inline _classes_ (attributes : string list) = attributes |> String.concat " " |> _class_
let inline _hxTarget_ target = Attr.create "hx-target" target
let _hyperScript_  = Attr.create "_"
let _dataTarget_   = Attr.create "data-target"
let _ariaHidden_   = Attr.create "aria-hidden"
let _ariaLabel_    = Attr.create "aria-label"
let _ariaExpanded_ = Attr.create "aria-expanded"

let inline isHtmxRequest (ctx:Microsoft.AspNetCore.Http.HttpContext) : bool =
    ctx.Request.Headers.ContainsKey "HX-Request" &&
    not (ctx.Request.Headers.ContainsKey "HX-History-Restore-Request") 



    