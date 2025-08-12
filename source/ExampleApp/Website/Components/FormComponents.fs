module ExampleApp.Website.Components.FormComponents

open ExampleApp.Website.Core
open Falco.Markup
open Falco.Htmx

type Value          = string
type ErrorMessage   = string
type TextFieldValue = Initial | Invalid of (Value * ErrorMessage) | Valid of Value

type TextField = {
    Id            : string
    Label         : string
    Name          : string
    Url           : string
    Value         : TextFieldValue
}

let textFieldComponent (textField:TextField) =
    let emptySpaceIcon = "&#160;"
    let successIcon    = "✔" 
    let warningIcon    = "⚠"

    let cssClass, value, message, icon =
        match textField.Value with
        | Initial                -> [""]                  , ""   , emptySpaceIcon, emptySpaceIcon
        | Invalid (value, error) -> [Bulma.``is-danger`` ], value, error         , warningIcon
        | Valid    value         -> [Bulma.``is-success``], value, emptySpaceIcon, successIcon

    _div [ _classes_ [ Bulma.field ] ] [
        _label [ _class_ Bulma.label; _for_ textField.Id ] [ _text textField.Label ]
        _div   [ _classes_ [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            _input [
                _id_               textField.Id
                _name_             textField.Name
                _title_            textField.Name
                _value_            value

                _type_             "Text"
                _classes_          ([ Bulma.input ] @ cssClass)
                
                Hx.post             textField.Url
                Hx.trigger         "blur delay:200ms"
                Hx.targetClosest   Bulma.field

                _hyperScript_      """
                                    on htmx:beforeRequest if (closest <form/>).submitting then halt end
                                    then on htmx:beforeRequest add .is-loading to (closest <div/>)
                                    then on htmx:beforeRequest add @disabled to me
                                    then on htmx:beforeRequest remove (next <span/>) end
                                  """
            ]
            _span [ _classes_ [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ] ] [
                _text icon
            ]
            _p [ _classes_ ([Bulma.help] @ cssClass) ] [ _text message ]
        ]
    ]
