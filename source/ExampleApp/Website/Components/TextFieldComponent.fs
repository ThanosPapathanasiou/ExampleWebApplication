module ExampleApp.Website.Components.TextFieldComponent

open ExampleApp.Website.Base
open Falco.Markup

type TextField      = { Id: string; Label: string; Name: string; Value: string; Readonly: bool }

let textFieldComponent (textField:TextField) : XmlNode  =
    let emptySpaceIcon = " " //"&#160;"
    let successIcon = "✔"
    let successCss  = Bulma.``is-success``
    let errorIcon   = "⚠"
    let errorCss    = Bulma.``is-danger``

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

                    _classes_          [ Bulma.input; Bulma.``is-small`` ] // @ cssClass
                ]
                @
                readOnly
            )
            
            _span [
                _id_  $"{textField.Id}_validation_icon"
                _classes_ [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ]
            ] [
                _text emptySpaceIcon
            ]
            _p [
                _id_  $"{textField.Id}_validation_message"
                _class_ Bulma.help
            ] [
                _text emptySpaceIcon
            ]
        ]
    ]
