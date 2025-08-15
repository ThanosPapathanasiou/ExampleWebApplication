module ExampleApp.Website.Components.TextFieldComponent

open ExampleApp.Website.Base
open Falco.Markup

type TextValue      = string
type ErrorMessage   = string
type TextFieldValue = Initial | Disabled of TextValue | Invalid of (TextValue * ErrorMessage) | Valid of TextValue
type TextField      = { Id: string; Label: string; Name: string; Value: TextFieldValue }

let textFieldComponent (textField:TextField) : XmlNode  =
    let emptySpaceIcon = "&#160;"
    let successIcon    = "✔" 
    let warningIcon    = "⚠"

    let cssClass, value, message, icon =
        match textField.Value with
        | Initial                -> [""]                  , ""   , emptySpaceIcon, emptySpaceIcon
        | Disabled value         -> [""]                  , value, emptySpaceIcon, emptySpaceIcon
        | Invalid (value, error) -> [Bulma.``is-danger`` ], value, error         , warningIcon
        | Valid    value         -> [Bulma.``is-success``], value, emptySpaceIcon, successIcon
    
    let textAttribute = if textField.Value.IsDisabled then _disabled_ else ( _type_ "Text" )
    
    _div [ _classes_ [ Bulma.field ] ] [
        _label [ _classes_ [ Bulma.label; Bulma.``is-small``]; _for_ textField.Id ] [ _text textField.Label ]
        _div   [ _classes_ [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            _input [
                _id_               textField.Id
                textAttribute

                _name_             textField.Name
                _title_            textField.Name
                _value_            value
                _type_             "Text"
                _classes_          ([ Bulma.input; Bulma.``is-small`` ] @ cssClass)
            ]
            _span [ _classes_ [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ] ] [
                _text icon
            ]
            _p [ _classes_ ([Bulma.help] @ cssClass) ] [ _text message ]
        ]
    ]
