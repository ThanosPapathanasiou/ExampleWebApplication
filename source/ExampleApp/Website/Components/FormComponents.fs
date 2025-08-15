module ExampleApp.Website.Components.FormComponents

open System.Data
open System.Threading.Tasks
open ExampleApp.Website.Components.TextFieldComponent
open Falco.Routing
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Http

open Falco
open Falco.Markup
open Falco.Htmx
open Falco.Security

open Modules.ActiveRecord
open ExampleApp.Website.Base

// TODO: complete the CRUD endpoints/views, we are missing the following
//      GET    /model/new
//      POST   /model/
//      DELETE /model/:id

// TODO: Get validation to work with System.ComponentModel

// TODO: Get datatypes other than string to work

// Generic components to enable you to create a crud endpoints/views that work with an Active Model class

let clickToEditFormComponent<'T when 'T :> ActiveRecord> (record: 'T) : XmlNode =
    let title = getTableName<'T>
    let baseUrl = title.ToLowerInvariant()
    let editUrl = $"/{baseUrl}/{record.Id}/edit"
    
    let formFields =
        getColumnMembers record
        |> Array.map (fun (column, value) -> textFieldComponent { Id=column; Name=column; Label=column; Value = Valid (value :?> string) }) // TODO: make it handle types other than string
        |> Array.toList
    
    _div [
        _class_ Bulma.box
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _text title ]
        ]
        @
        formFields
        @
        [
            _div [ _classes_ [ Bulma.field; Bulma.``is-grouped``; Bulma.``is-grouped-right`` ] ] [
                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``]
                        Hx.get editUrl
                        Hx.targetCss ("." + Bulma.box)
                        Hx.pushUrlOn
                        Hx.swapOuterHtml
                    ] [
                        _text "Edit"
                    ]
                ]
            ]
    ])

let saveOrCancelFormComponent<'T when 'T :> ActiveRecord> token (record: 'T) : XmlNode =
    
    let tableName = getTableName<'T>
    let baseUrl   = tableName.ToLowerInvariant()
    let submitUrl = $"/{baseUrl}/{record.Id}"
    let cancelUrl = $"/{baseUrl}/{record.Id}/view"
    
    // TODO: make it handle types other than string
    let components =
        getColumnMembers record
        |> Array.map (fun (column, value) -> textFieldComponent { Id=column; Name=column; Label=column; Value = Valid (value :?> string) }) 
        |> Array.toList
    
    _form [
        _class_ Bulma.box
        Hx.put submitUrl
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _text tableName ]
            Xsrf.antiforgeryInput token
        ]
        @
        components
        @
        [
            _div [ _classes_ [ Bulma.field; Bulma.``is-grouped``; Bulma.``is-grouped-right`` ] ] [

                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``]
                        Hx.get cancelUrl
                        Hx.targetCss ("." + Bulma.box)
                        Hx.pushUrl $"/{baseUrl}/{record.Id}"
                        Hx.swapOuterHtml
                    ] [
                        _text "Cancel"
                    ]
                ]

                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``]
                        _typeSubmit_
                    ] [
                        _text "Submit"
                    ]
                ]
            ]
    ])

let ``GET /<model>``<'T when 'T :> ActiveRecord>
    (ctx: HttpContext)
    (childView: 'T seq   -> XmlNode)
    (parentView: XmlNode -> XmlNode)
    : Task = 
      
    use conn  = ctx.Plug<IDbConnection>()
    let model = conn |> readRecords<'T>  |> Seq.truncate 5 |> Seq.toArray

    let fullView =
        if isHtmxRequest ctx then
           model |> childView 
        else
           model |> childView |> parentView
    
    Response.ofHtml fullView ctx

let ``GET /<model>/:id``<'T when 'T :> ActiveRecord>
    (ctx: HttpContext)
    (partialView:     'T -> XmlNode)
    (childView:  XmlNode -> XmlNode)
    (parentView: XmlNode -> XmlNode)
    : Task =

    use conn = ctx.Plug<IDbConnection>()

    let route = Request.getRoute ctx
    let id    = route.GetInt64 "id"
    let model = Option.get (readRecord<'T> conn id)
    let view  =
        if isHtmxRequest ctx then
            model |> partialView  |> childView
        else
            model |> partialView  |> childView |> parentView
    
    Response.ofHtml view ctx
    
let ``GET /<model>/:id/view``<'T when 'T :> ActiveRecord>
    (ctx: HttpContext)
    (partialView:     'T -> XmlNode)
    (childView:  XmlNode -> XmlNode)
    (parentView: XmlNode -> XmlNode)
    : Task =
        
    use conn = ctx.Plug<IDbConnection>()

    let route = Request.getRoute ctx
    let id    = route.GetInt64 "id" //TODO: get the 'id' from the model
    let model = Option.get (readRecord<'T> conn id)

    let view =
        if isHtmxRequest ctx then
            model |> partialView 
        else
            model |> partialView |> childView |> parentView
    
    Response.ofHtml view ctx

let ``GET /<model>/:id/edit``<'T when 'T :> ActiveRecord>
    (ctx: HttpContext)
    (partialView: AntiforgeryTokenSet -> 'T -> XmlNode)
    (childView:  XmlNode -> XmlNode)
    (parentView: XmlNode -> XmlNode)
    : Task =
        
    use conn = ctx.Plug<IDbConnection>()

    let route = Request.getRoute ctx
    let id = route.GetInt64 "id" //TODO: get the 'id' from the model
    let model =  Option.get (readRecord<'T> conn id)

    let view token =
        if isHtmxRequest ctx then
            partialView token model 
        else
            partialView token model |> childView |> parentView
    
    Response.ofHtmlCsrf view ctx    
    
let formRoutes<'T when 'T :> ActiveRecord>
    (getAll_ChildView:  'T seq -> XmlNode )
    (getSingle_ChildView: XmlNode -> XmlNode )
    (parentView: XmlNode -> XmlNode ) =
    
    let model = getTableName<'T>.ToLowerInvariant()
    
    [
        // View a list of models
        // Shows a list of 'T items and expects the user to provide the html for them in the 'getAll_ChildView' that they provide
        get  $"/{model}"                     ( fun ctx -> ``GET /<model>``<'T>          ctx                           getAll_ChildView    parentView )
        
        // View or Edit a single model
        // Shows a single model in the way the user wants it to be shown in the provided 'getSingle_ChildView'
        // Provides click to edit functionality
        get  $"/{model}/{{id:int}}"          ( fun ctx -> ``GET /<model>/:id``<'T>      ctx clickToEditFormComponent  getSingle_ChildView parentView )
        get  $"/{model}/{{id:int}}/view"     ( fun ctx -> ``GET /<model>/:id/view``<'T> ctx clickToEditFormComponent  getSingle_ChildView parentView )
        get  $"/{model}/{{id:int}}/edit"     ( fun ctx -> ``GET /<model>/:id/edit``<'T> ctx saveOrCancelFormComponent getSingle_ChildView parentView )
        
        // TODO: add missing put / delete `posts` endpoints
        // TODO: add missing put / delete `posts` endpoints
    ]