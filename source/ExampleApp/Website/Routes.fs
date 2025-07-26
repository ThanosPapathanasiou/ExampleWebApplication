module ExampleApp.Website.Routes

open System
open Giraffe
open Microsoft.AspNetCore.Http

let version =
    match Environment.GetEnvironmentVariable("VERSION") with
    | null -> "latest"
    | value -> value
    
let healthCheckMessage() =
    let timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
    $"{{ \"timestamp\":\"%s{timestamp}\" ,\"version\":\"%s{version}\" }}"

let websiteRoutes: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [
        GET  >=> route  "/"                        >=> Index.``GET /``
        GET  >=> route  "/posts"                   >=> Posts.``GET /posts``

        GET  >=> route  "/contact"                 >=> Contact.``GET /contact``
        GET  >=> route  "/about"                   >=> About.``GET /about``
        
        // DO NOT CHANGE THE /version and /up ENDPOINTS. THEY ARE NEEDED FOR DEPLOYMENT
        GET  >=> route  "/version"                 >=> setStatusCode 200 >=> text version
        GET  >=> route  "/up"                      >=> setStatusCode 200 >=> warbler (fun _ -> text (healthCheckMessage()))
        setStatusCode 404 >=> text "Not Found"
    ]