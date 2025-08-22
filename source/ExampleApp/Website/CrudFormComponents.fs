module ExampleApp.Website.Components.CrudFormComponents

open System
open System.ComponentModel
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open System.Data
open System.Net
open System.Reflection
open System.Threading.Tasks
open ExampleApp.Website.ParentView
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Http

open Falco
open Falco.Routing
open Falco.Markup
open Falco.Htmx
open Falco.Security

open Modules.ActiveRecord

[<AttributeUsage(AttributeTargets.Property)>]
type ComponentAttribute() =
    inherit Attribute()

let _validationErrorMessageFor (fieldId: string) (errorMessage: string) =
    let successIcon = "✔"
    let successCss  = Bulma.``is-success``
    let errorIcon   = "⚠"
    let errorCss    = Bulma.``is-danger``
    
    _p [
        _id_  $"{fieldId}_validation_message"
        _classes_ [ Bulma.help; errorCss ]
        _style_ "height: 18px"
        _hyperScript_  $"
            on mutation
                if my.innerHTML is ''
                    remove .{errorCss} from #{fieldId}
                    remove '{errorIcon}' from #{fieldId}_validation_icon
                    add .{successCss} to #{fieldId}
                    put '{successIcon}' into #{fieldId}_validation_icon
                else
                    remove .{successCss} from #{fieldId}
                    remove '{successIcon}' from #{fieldId}_validation_icon
                    add .{errorCss} to #{fieldId}
                    put '{errorIcon}' into #{fieldId}_validation_icon
        "
    ] [
        _textEnc errorMessage
    ]

// ----- ------------------ -----
// ----- TextFieldComponent -----
// ----- ------------------ -----

[<AttributeUsage(AttributeTargets.Property)>]
type TextFieldComponentAttribute() =
    inherit ComponentAttribute()

type TextFieldComponentSettings = { Id: string; Label: string; Name: string; Value: string; Readonly: bool; ErrorMessage: string }

let textFieldComponent (settings : TextFieldComponentSettings) : XmlNode  =
    let successIcon = "✔"
    let successCss  = Bulma.``is-success``
    let errorIcon   = "⚠"
    let errorCss    = Bulma.``is-danger``

    let css, icon =
        match settings.ErrorMessage = "" with
        | true -> [successCss], successIcon
        | false -> [errorCss], errorIcon

    let readOnly = if settings.Readonly then [ _disabled_ ] else []

    _div [ _classes_ [ Bulma.field ] ] [
        _label [ _classes_ [ Bulma.label; Bulma.``is-small``]; _for_ settings.Id ] [ _textEnc settings.Label ]
        _div   [ _classes_ [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            _input (
                [
                    _id_      settings.Id
                    _name_    settings.Name
                    _title_   settings.Name
                    _value_   (settings.Value |> WebUtility.HtmlEncode)
                    _type_    "Text"
                    _classes_ ([ Bulma.input; Bulma.``is-small`` ] @ css)
                ]
                @
                readOnly
            )
            
            _span [
                _id_  $"{settings.Id}_validation_icon"
                _classes_ [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ]
            ] [
                _text icon
            ]
            _validationErrorMessageFor settings.Id settings.ErrorMessage
        ]
    ]

// ----- ---------------------- -----
// ----- TextAreaFieldComponent -----
// ----- ---------------------- -----

[<AttributeUsage(AttributeTargets.Property)>]
type TextAreaComponentAttribute() =
    inherit ComponentAttribute()

type TextAreaComponentSettings = { Id: string; Label: string; Name: string; Value: string; Readonly: bool; ErrorMessage: string }

let textAreaComponent (settings : TextAreaComponentSettings) : XmlNode  =
    let successIcon = "✔"
    let successCss  = Bulma.``is-success``
    let errorIcon   = "⚠"
    let errorCss    = Bulma.``is-danger``

    let css, icon =
        match settings.ErrorMessage = "" with
        | true -> [successCss], successIcon
        | false -> [errorCss], errorIcon

    let readOnly = if settings.Readonly then [ _disabled_ ] else []

    _div [ _classes_ [ Bulma.field ] ] [
        _label [ _classes_ [ Bulma.label; Bulma.``is-small``]; _for_ settings.Id ] [ _textEnc settings.Label ]
        _div   [ _classes_ [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            _textarea (
                [
                    _id_               settings.Id
                    _name_             settings.Name
                    _title_            settings.Name
                    _classes_          ([ Bulma.textarea; Bulma.``is-small`` ] @ css)
                ]
                @
                readOnly
            ) [ _textEnc settings.Value ]
            
            _span [
                _id_  $"{settings.Id}_validation_icon"
                _classes_ [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ]
            ] [
                _text icon
            ]
            _validationErrorMessageFor settings.Id settings.ErrorMessage
        ]
    ]

// ----- ---------------------------- -----
// ----- StaticDropdownFieldComponent -----
// ----- ---------------------------- -----

[<AttributeUsage(AttributeTargets.Property)>]
type StaticDropdownComponentAttribute([<ParamArray>] options: string array) =
    inherit ComponentAttribute()
    member _.Options = options

type StaticDropdownSettings = { Id: string; Label: string; Name: string; Value: string; DropdownOptions: string array; Readonly: bool; ErrorMessage: string }

let staticDropdownFieldComponent (settings : StaticDropdownSettings) : XmlNode  =
    let successIcon = "✔"
    let successCss  = Bulma.``is-success``
    let errorIcon   = "⚠"
    let errorCss    = Bulma.``is-danger``

    let css, icon =
        match settings.ErrorMessage = "" with
        | true -> [successCss], successIcon
        | false -> [errorCss], errorIcon

    let readOnly = if settings.Readonly then [ _disabled_ ] else []
    
    let dropdownOptions =
        settings.DropdownOptions
        |> Array.map (fun t ->
            let selected = if settings.Value = t then [ _selected_ ] else [] 
            _option ([ _value_ t ]@selected) [ _textEnc t ])
        |> Array.toList
    
    _div [ _classes_ [ Bulma.field ] ] [
        _label [ _classes_ [ Bulma.label; Bulma.``is-small``]; _for_ settings.Id ] [ _textEnc settings.Label ]
        _div   [ _classes_ [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            _select ([
                _id_      settings.Id
                _name_    settings.Name
                _title_   settings.Name
                _classes_ ([ Bulma.input; Bulma.``is-small`` ] @ css)
            ]@readOnly) dropdownOptions

            _span [
                _id_  $"{settings.Id}_validation_icon"
                _classes_ [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ]
            ] [
                _text icon
            ]

            _validationErrorMessageFor settings.Id settings.ErrorMessage
        ]
    ]


// ----- FORM COMPONENT GENERIC FUNCTIONS -----

let getFormFieldFromRecord<'T when 'T :> ActiveRecord> (record: 'T) (prop: PropertyInfo) (isReadonly: bool) (errorMessage: string) = 
    let columnName = prop.GetCustomAttribute<ColumnAttribute>().Name
    let label      = prop.GetCustomAttribute<DisplayNameAttribute>().DisplayName 
    let value      = prop.GetValue(record)
    
    match prop.GetCustomAttribute<ComponentAttribute>() with
    | :? TextFieldComponentAttribute ->
        textFieldComponent
            {
                Id = columnName.ToLowerInvariant()
                Name = columnName
                Label = label
                Value = value.ToString()
                Readonly = isReadonly
                ErrorMessage = errorMessage
            }
    | :? TextAreaComponentAttribute ->
        textAreaComponent
            {
                Id = columnName.ToLowerInvariant()
                Name = columnName
                Label = label
                Value = value.ToString()
                Readonly = isReadonly
                ErrorMessage = errorMessage
            }
    | :? StaticDropdownComponentAttribute ->
        let options = prop.GetCustomAttribute<StaticDropdownComponentAttribute>().Options
        staticDropdownFieldComponent
            {
                Id = columnName.ToLowerInvariant()
                Name = columnName
                Label = label
                Value = value.ToString()
                DropdownOptions = options
                Readonly = isReadonly
                ErrorMessage = errorMessage
            }
    // TODO: handle other types of components
    // TODO: create unit test that will enforce that the following exception never happens.              
    | _ -> raise (ArgumentException "Invalid ComponentAttribute")

let getFormFieldsFromRecord<'T when 'T :> ActiveRecord> (record: 'T) (isReadonly: bool) (validationResults: ValidationResult seq)=
    
    let getErrorMessage field =
      let errorMessages = 
        validationResults
        |> Seq.toArray
        |> Array.groupBy (fun r -> r.MemberNames |> Seq.head)
        |> Array.map (fun (key, group) -> key, group[0].ErrorMessage)
        |> Map.ofArray      
        
      Map.tryFind field errorMessages |> Option.defaultValue ""
      
    let formFields =
        typeof<'T>.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        |> Array.filter (fun prop -> prop.GetCustomAttribute<DatabaseGeneratedAttribute>() = null)
        |> Array.filter (fun prop -> prop.GetCustomAttribute<ColumnAttribute>() <> null)
        |> Array.map    (fun prop ->
            let errorMessage = prop.GetCustomAttribute<ColumnAttribute>().Name |> getErrorMessage
            getFormFieldFromRecord<'T> record prop isReadonly errorMessage
        )
        |> Array.toList

    formFields

// ----- ------------- -----
// ----- PARTIAL VIEWS -----
// ----- ------------- -----

let viewFormComponent_PartialView<'T when 'T :> ActiveRecord> (record: 'T) : XmlNode =
    let recordName' = getTableName<'T>
    let recordName  = recordName'.ToLowerInvariant()
    let editUrl     = $"/{recordName}/{record.Id}/edit"
    let formFields  = getFormFieldsFromRecord<'T> record true []
    
    _div [
        _class_ Bulma.box
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _textEnc recordName' ]
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
                        _textEnc "Back"
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
                        _textEnc "Edit"
                    ]
                ]
            ]
        ]
    )

let editFormComponent_PartialView<'T when 'T :> ActiveRecord> token (record: 'T) : XmlNode =
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
            _h2 [ _class_ Bulma.title ] [ _textEnc recordName' ]
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
                        _textEnc "Cancel"
                    ]
                ]
                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``;  Bulma.``is-success`` ]
                        _typeSubmit_
                    ] [
                        _textEnc "Submit"
                    ]
                ]
            ]
            _div [ _id_ $"{recordName}_validation_results" ] [
                
            ]
        ])

let formComponent_ChildView (partialView: XmlNode) : XmlNode =
    _main [ _classes_ [ Bulma.container; ] ] [
        _section [ ] [
            _div [ _class_ Bulma.container ] [
               partialView
            ]
        ]
    ]

let pagination_ChildView<'T when 'T :> ActiveRecord>
    (records: 'T seq)
    (singleView: 'T -> XmlNode)
    : XmlNode =
    
    let title = getTableName<'T>
    _main [ _class_ Bulma.container ] [
        _section [ ] [
            _h1 [ _class_ Bulma.title ] [ _textEnc title ]
        ]
        _br []
        _section [ ] [
            _div [ _class_ Bulma.container ] [
                for record in records do
                    singleView record
            ]
        ]
        // TODO: pagination
    ]

// ----- --------- -----
// ----- ENDPOINTS -----
// ----- --------- -----

/// <summary>
/// This will return a list view of 'T models
/// </summary>
/// <param name="ctx">The http context</param>
/// <param name="singleView"></param>
/// <param name="childView">This is the view that will show off a list of `'T`. It is a child view, meaning the base XmlNode must be `main`.</param>
/// <param name="parentView">This is the base view of your entire website. The base XmlNode must be `html`.</param>
let ``GET /model``<'T when 'T :> ActiveRecord>
    (ctx: HttpContext)
    (singleView:      'T -> XmlNode)
    (childView:   'T seq -> ('T -> XmlNode)  -> XmlNode)
    (parentView: XmlNode -> XmlNode)
    : Task =

    // TODO: pagination
    // get limit and offset and use it for pagination
        
    use conn  = ctx.Plug<IDbConnection>()
    let records = conn |> readRecords<'T>  |> Seq.truncate 5 |> Seq.toArray
    
    let childView' = childView records singleView
    
    let fullView =
        if isHtmxRequest ctx then
           childView'  
        else
           childView' |> parentView
    
    Response.ofHtml fullView ctx

/// <summary>
/// This will handle the update of a 'T model.
/// </summary>
/// <param name="ctx">The http context</param>
/// <param name="successPartialView">This is the view that will show after successfully updating the model. It is a partial view, meaning the base XmlNode is a div with a .box css class.</param>
/// <param name="failedPartialView">This is the view that will show after the model update fails. It is a partial view, meaning the base XmlNode is a form with a .box css class. Since it's a form, it also requires an antiforgery token.</param>
/// <param name="childView">This is the view that will show off a `'T`. It is a child view, meaning the base XmlNode must be `main`.</param>
/// <param name="parentView">This is the base view of your entire website. The base XmlNode must be `html`.</param>
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

/// <summary>
/// This will handle the childView that handles viewing a 'T model.
/// </summary>
/// <param name="ctx">The http context</param>
/// <param name="partialView">This is the view that will show off a 'T. It is a partial view, meaning the base XmlNode is a div with a .box css class.</param>
/// <param name="childView">This is the view that will show off a `'T`. It is a child view, meaning the base XmlNode must be `main`.</param>
/// <param name="parentView">This is the base view of your entire website. The base XmlNode must be `html`.</param>
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

/// <summary>
/// This will handle returning the partialView that handles viewing a 'T model
/// </summary>
/// <param name="ctx">The http context</param>
/// <param name="partialView">This is the view that will show off a 'T. It is a partial view, meaning the base XmlNode is a div with a .box css class.</param>
/// <param name="childView">This is the view that will show off a `'T`. It is a child view, meaning the base XmlNode must be `main`.</param>
/// <param name="parentView">This is the base view of your entire website. The base XmlNode must be `html`.</param>
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

/// <summary>
/// This will handle returning the partialView that handles editing a 'T model
/// </summary>
/// <param name="ctx">The http context</param>
/// <param name="partialView">This is the view that will allow you to edit a 'T. It is a partial view, meaning the base XmlNode is a form with a .box css class. Since it's a form, it also requires an antiforgery token.</param>
/// <param name="childView">This is the view that will show off a `'T`. It is a child view, meaning the base XmlNode must be `main`.</param>
/// <param name="parentView">This is the base view of your entire website. The base XmlNode must be `html`.</param>
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
/// <param name="viewSinglePaginationItem_PartialView">
/// This is the view that will show off a single `'T` in a list view that supports pagination.
/// It is a partial view so the main XmlNode can be whatever you want to list. article, div, li, etc.
/// </param>
/// <param name="parentView">This is the base view of your entire website. The base XmlNode must be `html`.</param>
let getEndpointListForType<'T when 'T :> ActiveRecord>
    (viewSinglePaginationItem_PartialView:  'T -> XmlNode )
    (parentView: XmlNode -> XmlNode ) : HttpEndpoint list =

    // This looks complicated but bear with me.
    // parentView is the base. It is the complete html page. It 'takes' a 'main' html element as input. We call all the 'main' views, _ChildView 
    // All the _ChildView views go directly in the parentView. There's two of them.
    // One that shows the T in a paginated view.       -> pagination_ChildView
    // The other shows the T in a view/edit form view. -> formComponent_ChildView
        
    let model = getTableName<'T>.ToLowerInvariant()

    [
        get   $"/{model}"                 ( fun ctx -> ``GET /model``<'T>         ctx viewSinglePaginationItem_PartialView                           pagination_ChildView parentView )
        get   $"/{model}/{{id:int}}"      ( fun ctx -> ``GET /model/id``<'T>      ctx viewFormComponent_PartialView                               formComponent_ChildView parentView )
        put   $"/{model}/{{id:int}}"      ( fun ctx -> ``PUT /model/id``<'T>      ctx viewFormComponent_PartialView editFormComponent_PartialView formComponent_ChildView parentView )
        get   $"/{model}/{{id:int}}/view" ( fun ctx -> ``GET /model/id/view``<'T> ctx viewFormComponent_PartialView                               formComponent_ChildView parentView )
        get   $"/{model}/{{id:int}}/edit" ( fun ctx -> ``GET /model/id/edit``<'T> ctx editFormComponent_PartialView                               formComponent_ChildView parentView )
        // post  $"/{model}/{{id:int}}/validate" ( fun ctx -> ``GET /model/id/validate``<'T> ctx )
        
        
        // TODO: add missing DEL  /model/id          endpoint
        // TODO: add missing GET  /model/new         endpoint
        // TODO: add missing POST /model             endpoint
        // TODO: add pagination support for /model
    ]
