module ExampleApp.Website.Routes

open System
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

let deploymentRoutes = [
    // DO NOT CHANGE THE /version and /up ENDPOINTS. THEY ARE NEEDED FOR DEPLOYMENT
    get "/version" (fun ctx -> Response.ofPlainText version ctx)
    get "/up"      (fun ctx -> Response.ofPlainText (healthCheckMessage ()) ctx)
]

let websiteRoutes =
        [
            get  "/"                        Index.``GET /``
            get  "/about"                   About.``GET /about``
        ]
        @ postRoutes
        @ deploymentRoutes
