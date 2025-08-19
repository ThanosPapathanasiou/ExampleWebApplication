module ExampleApp.Website.Routes

open System
open ExampleApp.Website.Components.FormComponents
open ExampleApp.Website.ParentView
open Falco
open Falco.Routing
open ExampleApp.Website.Posts

let notFoundHandler: HttpHandler = Response.redirectTemporarily "/" 

let version =
    match Environment.GetEnvironmentVariable("VERSION") with
    | null -> "latest"
    | value -> value

let healthCheckMessage() =
    let timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
    $"{{ \"timestamp\":\"%s{timestamp}\" ,\"version\":\"%s{version}\" }}"

let healthCheckEndpoints = [
    get "/version" (fun ctx -> Response.ofPlainText version ctx)
    get "/up"      (fun ctx -> Response.ofPlainText (healthCheckMessage ()) ctx)
]

let mainMenuEndpoints = [
    get "/"      Index.``GET /``
    get "/about" About.``GET /about``
]

let websiteEndpoints =
    mainMenuEndpoints @
    
    // hook up the 'Post' endpoints
    (getEndpointListForType<Post> multiPostView parentView) @
    
    healthCheckEndpoints
