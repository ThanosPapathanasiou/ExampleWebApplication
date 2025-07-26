module ExampleApp.Website.About

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open ExampleApp.Website.Base
open ExampleApp.Website.Htmx

let view: XmlNode =
    main [] [
        section [ _class "section" ] [
            h1 [ _class "title" ] [ Text "About us" ]
            p [] [ Text
                       "Occaecati vel ex architecto et ut sed veniam odit. Saepe nemo omnis officiis. Ut enim molestiae itaque. Vitae vel assumenda deleniti tempore illum quas debitis. Hic ratione et sit quibusdam dolores repellat qui tempore. Molestiae consequatur voluptatem aspernatur aspernatur eos beatae.
                        Accusantium est omnis officiis consequuntur fuga et nobis. Alias sunt velit ipsa dolore dolorem. Id qui est itaque sit. Eveniet voluptatibus sunt laudantium.
                        Cupiditate facilis neque molestias deserunt suscipit unde. Voluptatem repellendus aut maxime est officiis natus. Occaecati nobis quibusdam totam sapiente ipsa sint molestiae molestias. Doloribus libero ea cupiditate voluptas fugiat. Qui laborum ea sit. Veritatis dolorem autem iusto ut et.
                        Dolorum facilis rem unde excepturi. Architecto nostrum fugit et est sit saepe non voluptas. Harum consequatur qui eveniet atque natus numquam. Dolorem rem ex ut modi cupiditate velit.
                        Qui ut eum quidem fugit. Quidem officiis omnis eaque pariatur eveniet est. Quas voluptatem et in repudiandae et."
                 ]
        ]
    ]

let ``GET /about`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        if isHtmxRequest ctx then
            htmlView view next ctx    
        else
            htmlView (createPage view) next ctx