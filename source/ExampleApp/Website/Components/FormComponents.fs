module ExampleApp.Website.Components.FormComponents

open System.Data
open System.Threading.Tasks
open ExampleApp.Website.Components.TextFieldComponent
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Http

open Falco
open Falco.Routing
open Falco.Markup
open Falco.Htmx
open Falco.Security

open Modules.ActiveRecord
open ExampleApp.Website.Base

// Generic functions to enable you to create a crud endpoints/views that work with an Active Model class

let getFormFieldsFromRecord<'T when 'T :> ActiveRecord> (record: 'T) (isReadonly: bool) =
    getColumnMembers record
        |> Array.map (fun (column, value) ->
            match value with
            | :? string as strValue -> textFieldComponent { Id=column; Label=column; Name=column; Value = strValue; Readonly = isReadonly }
            // TODO: handle other types of data
            | _ -> failwith "unhandled type"            
        )
        |> Array.toList

let view_FormComponent<'T when 'T :> ActiveRecord> (record: 'T) : XmlNode =
    let title = getTableName<'T>
    let baseUrl = title.ToLowerInvariant()
    let editUrl = $"/{baseUrl}/{record.Id}/edit"
    let formFields = getFormFieldsFromRecord<'T> record true
    
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
                        _classes_ [ Bulma.button; Bulma.``is-link``; ]
                        Hx.get $"/{baseUrl}"
                        _hxTarget_ "main"
                        Hx.pushUrlOn
                        Hx.swapOuterHtml
                    ] [
                        _text "Back"
                    ]
                ]
                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``; ]
                        Hx.get editUrl
                        Hx.targetCss ("." + Bulma.box)
                        Hx.pushUrlOn
                        Hx.swapOuterHtml
                    ] [
                        _text "Edit"
                    ]
                ]
            ]
        ]
    )

let edit_FormComponent<'T when 'T :> ActiveRecord> token (record: 'T) : XmlNode =
    
    let tableName = getTableName<'T>
    let baseUrl   = tableName.ToLowerInvariant()
    let submitUrl = $"/{baseUrl}/{record.Id}"
    let cancelUrl = $"/{baseUrl}/{record.Id}/view"
    let formFields = getFormFieldsFromRecord<'T> record false
    
    _form [
        _class_ Bulma.box
        Hx.put submitUrl
        Hx.swapOuterHtml
        Hx.targetCss ("." + Bulma.box)
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _text tableName ]
            Xsrf.antiforgeryInput token
        ]
        @
        formFields
        @
        [
            _div [ _classes_ [ Bulma.field; Bulma.``is-grouped``; Bulma.``is-grouped-right`` ] ] [
                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``; Bulma.``is-danger`` ]
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
                        _classes_ [ Bulma.button; Bulma.``is-link``;  Bulma.``is-success`` ]
                        _typeSubmit_
                    ] [
                        _text "Submit"
                    ]
                ]
            ]
        ])

let ``GET /model``<'T when 'T :> ActiveRecord>
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

let ``PUT /model/id``<'T when 'T :> ActiveRecord>
    (ctx: HttpContext)
    (successPartialView: 'T -> XmlNode)
    (failedPartialView: AntiforgeryTokenSet -> 'T -> XmlNode)
    (childView:  XmlNode -> XmlNode)
    (parentView: XmlNode -> XmlNode)
    : Task =
    task {
        // TODO: validate antiforgery token
        
        let model = getRecordFromHttpRequest<'T> ctx.Request
        let validations = validateRecord<'T> model

        let partialView ctx = 
            match validations.Length with
            | 0 -> successPartialView 
            | _ -> failedPartialView (Xsrf.getToken ctx) 
        
        let view  =
            if isHtmxRequest ctx then
                partialView ctx model
            else
                partialView ctx model |> childView |> parentView
        
        return! Response.ofHtml view ctx
    }

let ``GET /model/id``<'T when 'T :> ActiveRecord>
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

let ``GET /model/id/view``<'T when 'T :> ActiveRecord>
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

let ``GET /model/id/edit``<'T when 'T :> ActiveRecord>
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

let getEndpointListForType<'T when 'T :> ActiveRecord>
    (getSingle_ChildView: XmlNode -> XmlNode )
    (getAll_ChildView:  'T seq -> XmlNode )
    (parentView: XmlNode -> XmlNode ) : HttpEndpoint list =
    
    let model = getTableName<'T>.ToLowerInvariant()
    
    [
        get  $"/{model}"                     ( fun ctx -> ``GET /model``<'T>         ctx                                       getAll_ChildView    parentView )
        get  $"/{model}/{{id:int}}"          ( fun ctx -> ``GET /model/id``<'T>      ctx view_FormComponent                    getSingle_ChildView parentView )
        put  $"/{model}/{{id:int}}"          ( fun ctx -> ``PUT /model/id``<'T>      ctx view_FormComponent edit_FormComponent getSingle_ChildView parentView )
        get  $"/{model}/{{id:int}}/view"     ( fun ctx -> ``GET /model/id/view``<'T> ctx view_FormComponent                    getSingle_ChildView parentView )
        get  $"/{model}/{{id:int}}/edit"     ( fun ctx -> ``GET /model/id/edit``<'T> ctx edit_FormComponent                    getSingle_ChildView parentView )
        
        
        // TODO: add missing DEL  /model/id          endpoint
        // TODO: add missing POST /model/id/validate endpoint 
        // TODO: add missing GET  /model/new         endpoint
        // TODO: add missing POST /model             endpoint
    ]
