module ExampleApp.Website.About

open ExampleApp.Website.ParentView
open Falco
open Falco.Markup

open ExampleApp.Website.Base

let childView =
    _main [ _class_ Bulma.container ] [
        _section [ ] [
            _h1 [ _class_ Bulma.title ] [ _text "About" ]
            _p [] [ _text
                         """
                         An example website built with F#, Falco, HTMX, and Bulma combines a functional-first programming language with a lightweight web framework, a dynamic JavaScript library for interactivity, and a modern CSS framework for styling.
                         Here’s a breakdown of how these technologies work together to create a modern web application, with insights drawn from available resources:
                         """
            ]
            _br [] 
            _p' "Overview of the Technologies"
            _br []
            _p' "F#: A functional-first programming language that emphasizes type safety, conciseness, and expressiveness. It’s great for building robust, maintainable server-side applications, especially when paired with .NET’s high-performance ecosystem."
            _br []
            _p' "Falco: A functional-first toolkit for building full-stack web applications using F# and ASP.NET Core. It provides a simple routing API, a native F# view engine (Falco.Markup), and seamless integration with libraries like HTMX. Falco is designed to be lightweight, extensible, and easy to learn, leveraging ASP.NET Core’s performance capabilities."
            _br []
            _p' "HTMX: A JavaScript library that enables dynamic, AJAX-driven interactions without heavy client-side scripting. It allows you to update parts of a webpage by making HTTP requests and swapping HTML content, all while keeping the front-end simple. With Falco, HTMX is often integrated via the Falco.Htmx package, which provides type-safe bindings for HTMX attributes."
            _br []
            _p' "Bulma: A lightweight, modern CSS framework based on Flexbox. It provides pre-built, responsive components (e.g., buttons, forms, modals) that are easy to customize, making it ideal for rapid UI development without heavy JavaScript dependencies."
        ]
    ]

let ``GET /about`` = fun ctx ->
    let view =
        if isHtmxRequest ctx then
            childView
        else
            parentView childView
    
    Response.ofHtml view ctx
