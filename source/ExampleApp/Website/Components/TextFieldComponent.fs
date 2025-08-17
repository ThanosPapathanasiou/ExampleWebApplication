module ExampleApp.Website.Components.TextFieldComponent

open ExampleApp.Website.Base
open Falco.Markup

type TextField = { Id: string; Label: string; Name: string; Value: string; Readonly: bool; ErrorMessage: string }

let _validationErrorMessageFor (fieldId: string) (errorMessage: string) =
    let successIcon = "✔"
    let successCss  = Bulma.``is-success``
    let errorIcon   = "⚠"
    let errorCss    = Bulma.``is-danger``
    
    _p [
        _id_  $"{fieldId}_validation_message"
        _classes_ [ Bulma.help; Bulma.``is-danger`` ]
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
        _text errorMessage
    ]

let rec textFieldComponent (textField:TextField) : XmlNode  =
    let successIcon = "✔"
    let successCss  = Bulma.``is-success``
    let errorIcon   = "⚠"
    let errorCss    = Bulma.``is-danger``

    let css, icon =
        match textField.ErrorMessage = "" with
        | true -> [successCss], successIcon
        | false -> [errorCss], errorIcon

    let readOnly = if textField.Readonly then [ _disabled_ ] else []

    _div [ _classes_ [ Bulma.field ] ] [
        _label [ _classes_ [ Bulma.label; Bulma.``is-small``]; _for_ textField.Id ] [ _text textField.Label ]
        _div   [ _classes_ [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            _input (
                [
                    _id_               textField.Id
                    _name_             textField.Name
                    _title_            textField.Name
                    _value_            textField.Value
                    _type_             "Text"
                    _classes_          ([ Bulma.input; Bulma.``is-small`` ] @ css)
                ]
                @
                readOnly
            )
            
            _span [
                _id_  $"{textField.Id}_validation_icon"
                _classes_ [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ]
            ] [
                _text icon
            ]
            _validationErrorMessageFor textField.Id textField.ErrorMessage
        ]
    ]
