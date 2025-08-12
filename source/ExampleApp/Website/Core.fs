module ExampleApp.Website.Core

open Microsoft.AspNetCore.Http
open Falco.Markup
open Zanaptak.TypedCssClasses

[<Literal>]
let bulmaUrl = "https://cdn.jsdelivr.net/npm/bulma@1.0.4/css/bulma.min.css"
type Bulma = CssClasses<bulmaUrl>

type FalconEndpoint = HttpContext -> System.Threading.Tasks.Task

let inline _classes_ (attributes : string list) = attributes |> String.concat " " |> _class_

let inline isHtmxRequest (ctx:HttpContext) : bool =
        ctx.Request.Headers.ContainsKey "HX-Request" &&
        not (ctx.Request.Headers.ContainsKey "HX-History-Restore-Request") 

let _hyperScript_  = Attr.create "_"
let _dataTarget_   = Attr.create "data-target"
let _ariaHidden_   = Attr.create "aria-hidden"
let _ariaLabel_    = Attr.create "aria-label"
let _ariaExpanded_ = Attr.create "aria-expanded"
