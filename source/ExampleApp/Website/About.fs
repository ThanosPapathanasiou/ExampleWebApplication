module ExampleApp.Website.About

open Falco
open Falco.Markup
open ExampleApp.Website.Base
open ExampleApp.Website.Core

let childView =
    _main [ _class_ Bulma.container ] [
        _section [ ] [
            _h1 [ _class_ Bulma.title ] [ _text "About us" ]
            _p [] [ _text
                       "Occaecati vel ex architecto et ut sed veniam odit. Saepe nemo omnis officiis. Ut enim molestiae itaque. Vitae vel assumenda deleniti tempore illum quas debitis. Hic ratione et sit quibusdam dolores repellat qui tempore. Molestiae consequatur voluptatem aspernatur aspernatur eos beatae.
                        Accusantium est omnis officiis consequuntur fuga et nobis. Alias sunt velit ipsa dolore dolorem. Id qui est itaque sit. Eveniet voluptatibus sunt laudantium.
                        Cupiditate facilis neque molestias deserunt suscipit unde. Voluptatem repellendus aut maxime est officiis natus. Occaecati nobis quibusdam totam sapiente ipsa sint molestiae molestias. Doloribus libero ea cupiditate voluptas fugiat. Qui laborum ea sit. Veritatis dolorem autem iusto ut et.
                        Dolorum facilis rem unde excepturi. Architecto nostrum fugit et est sit saepe non voluptas. Harum consequatur qui eveniet atque natus numquam. Dolorem rem ex ut modi cupiditate velit.
                        Qui ut eum quidem fugit. Quidem officiis omnis eaque pariatur eveniet est. Quas voluptatem et in repudiandae et."
                 ]
        ]
    ]

let ``GET /about`` : FalconEndpoint = fun ctx ->
    let view =
        if isHtmxRequest ctx then
            childView
        else
            parentView childView
    
    Response.ofHtml view ctx
