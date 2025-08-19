module ExampleApp.Website.Components.FormComponents

open System.ComponentModel.DataAnnotations
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

let getFormFieldsFromRecord<'T when 'T :> ActiveRecord> (record: 'T) (isReadonly: bool) (validations: ValidationResult seq)=
    let errorMessages = 
        validations
        |> Seq.toArray
        |> Array.groupBy (fun r -> r.MemberNames |> Seq.head)
        |> Array.map (fun (key, group) -> key, group.[0].ErrorMessage)
        |> Map.ofArray
    
    let getErrorMessage field = 
        Map.tryFind field errorMessages |> Option.defaultValue ""
        
    getFieldsThatAreStoredInDatabase record
        |> Array.map (fun (field, value) ->
            match value with
            // TODO: handle other types of data
            | :? string as strValue ->
                textFieldComponent
                    {
                        Id=field.ToLowerInvariant()
                        Label=field
                        Name=field
                        Value = strValue
                        Readonly = isReadonly
                        ErrorMessage=getErrorMessage field
                    }
            | _ -> failwith "unhandled type"
        )
        |> Array.toList

let view_FormComponent<'T when 'T :> ActiveRecord> (record: 'T) : XmlNode =
    let recordName' = getTableName<'T>
    let recordName  = recordName'.ToLowerInvariant()
    let editUrl     = $"/{recordName}/{record.Id}/edit"
    let formFields  = getFormFieldsFromRecord<'T> record true []
    
    _div [
        _class_ Bulma.box
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _text recordName' ]
        ]
        @
        formFields
        @
        [
            _div [ _classes_ [ Bulma.field; Bulma.``is-grouped``; Bulma.``is-grouped-right`` ] ] [
                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``; ]
                        Hx.get $"/{recordName}"
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
    
    let recordName' = getTableName<'T>
    let recordName  = recordName'.ToLowerInvariant()
    let submitUrl   = $"/{recordName}/{record.Id}"
    let cancelUrl   = $"/{recordName}/{record.Id}/view"
    let validations = validateRecord<'T> record
    let formFields  = getFormFieldsFromRecord<'T> record false validations

    
    _form [
        _class_ Bulma.box
        Hx.put submitUrl
        Hx.swapOuterHtml
        Hx.targetCss ("." + Bulma.box)
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _text recordName' ]
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
                        Hx.pushUrl $"/{recordName}/{record.Id}"
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
            _div [ _id_ $"{recordName}_validation_results" ] [
                
            ]
        ])

let getSingle_ChildView partialView : XmlNode =
    _main [ _classes_ [ Bulma.container; ] ] [
        _section [ ] [
            _div [ _class_ Bulma.container ] [
               partialView
            ]
        ]
    ]

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
        use conn  = ctx.Plug<IDbConnection>()
        
        // TODO: validate antiforgery token
        
        let model = getRecordFromHttpRequest<'T> ctx.Request
        let validations = validateRecord<'T> model

        if validations.Length <> 0 then
            let view  =
                if isHtmxRequest ctx then
                    failedPartialView (Xsrf.getToken ctx)  model
                else
                    failedPartialView (Xsrf.getToken ctx)  model |> childView |> parentView
            
            return! Response.ofHtml view ctx    
        else
            
            // TODO: url encode the values
            let updatedModel = updateRecord<'T> conn model
            
            let view  =
                if isHtmxRequest ctx then
                    successPartialView updatedModel
                else
                    successPartialView updatedModel |> childView |> parentView
            
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

// // TODO: implement validate on tab-off on every field or when the user stops typing 
// let ``GET /model/id/validate``<'T when 'T :> ActiveRecord>
//     (ctx: HttpContext)
//     : Task =
//
//     use conn = ctx.Plug<IDbConnection>()
//
//     let model = getRecordFromHttpRequest<'T> ctx.Request
//     let validationResults = validateRecord<'T> model
//     let validations = 
//         _div [] [
//             _validationErrorMessageFor "Email"
//         ]    
//     
//     Response.ofHtml validations ctx

/// <summary>
/// This function will return a list of endpoints that handle CRUD operations for your active record of type `'T` 
/// </summary>
/// <param name="getAll_ChildView">This is the view that will show off a list of `'T`. It is a child view, meaning the base XmlNode must be `main`.</param>
/// <param name="parentView">This is the base view of your entire website. The base XmlNode has to be `html`.</param>
let getEndpointListForType<'T when 'T :> ActiveRecord>
    (getAll_ChildView:  'T seq -> XmlNode )
    (parentView: XmlNode -> XmlNode ) : HttpEndpoint list =
    
    let model = getTableName<'T>.ToLowerInvariant()
    
    [
        get   $"/{model}"                       ( fun ctx -> ``GET /model``<'T>         ctx                                       getAll_ChildView    parentView )
        get   $"/{model}/{{id:int}}"            ( fun ctx -> ``GET /model/id``<'T>      ctx view_FormComponent                    getSingle_ChildView parentView )
        put   $"/{model}/{{id:int}}"            ( fun ctx -> ``PUT /model/id``<'T>      ctx view_FormComponent edit_FormComponent getSingle_ChildView parentView )
        get   $"/{model}/{{id:int}}/view"       ( fun ctx -> ``GET /model/id/view``<'T> ctx view_FormComponent                    getSingle_ChildView parentView )
        get   $"/{model}/{{id:int}}/edit"       ( fun ctx -> ``GET /model/id/edit``<'T> ctx edit_FormComponent                    getSingle_ChildView parentView )
        // post  $"/{model}/{{id:int}}/validate" ( fun ctx -> ``GET /model/id/validate``<'T> ctx )
        
        
        // TODO: add missing DEL  /model/id          endpoint
        // TODO: add missing GET  /model/new         endpoint
        // TODO: add missing POST /model             endpoint
        // TODO: add pagination support for /model
    ]
