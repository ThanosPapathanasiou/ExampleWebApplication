module ExampleApp.Website.Components.FormComponents

open ExampleApp.Website.Base
open Falco.Markup
open Falco.Htmx
open Falco.Security

type TextValue      = string
type ErrorMessage   = string
type TextFieldValue = Initial | Invalid of (TextValue * ErrorMessage) | Valid of TextValue

type TextField = { Id: string; Label: string; Name: string; Value: TextFieldValue }
let textFieldComponent (textField:TextField) : XmlNode  =
    let emptySpaceIcon = "&#160;"
    let successIcon    = "✔" 
    let warningIcon    = "⚠"

    let cssClass, value, message, icon =
        match textField.Value with
        | Initial                -> [""]                  , ""   , emptySpaceIcon, emptySpaceIcon
        | Invalid (value, error) -> [Bulma.``is-danger`` ], value, error         , warningIcon
        | Valid    value         -> [Bulma.``is-success``], value, emptySpaceIcon, successIcon

    _div [ _classes_ [ Bulma.field ] ] [
        _label [ _classes_ [ Bulma.label; Bulma.``is-small``]; _for_ textField.Id ] [ _text textField.Label ]
        _div   [ _classes_ [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            _input [
                _id_               textField.Id
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
let clickToEditFormComponent title editUrl backUrl formFields : XmlNode =
    _div [
        _class_ Bulma.box
        Hx.targetThis
        Hx.swapOuterHtml
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _text title ]
        ]
        @
        formFields
        @
        [
            _div [ _classes_ [ Bulma.field; Bulma.``is-grouped``; Bulma.``is-grouped-right`` ] ] [
                // _div [ _class_ Bulma.control ] [
                //     _button [
                //         _classes_ [ Bulma.button; Bulma.``is-link``]
                //         Hx.get backUrl
                //         Hx.pushUrlOn
                //         _hxTarget_ "main"
                //     ] [
                //         _text "Back"
                //     ]
                // ]
                
                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``]
                        Hx.get editUrl
                    ] [
                        _text "Edit"
                    ]
                ]
            ]
    ])
let saveOrCancelFormComponent token title submitUrl cancelUrl formFields : XmlNode =
    _form [
        _class_ Bulma.box
        Hx.targetThis
        Hx.swapOuterHtml
        Hx.put submitUrl
    ] (
        [
            _h2 [ _class_ Bulma.title ] [ _text title ]
            Xsrf.antiforgeryInput token
        ]
        @
        formFields
        @
        [
            _div [ _classes_ [ Bulma.field; Bulma.``is-grouped``; Bulma.``is-grouped-right`` ] ] [
                _div [ _class_ Bulma.control ] [
                    _button [
                        _classes_ [ Bulma.button; Bulma.``is-link``]
                        Hx.get cancelUrl
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
let formComponent token name title submitUrl buttonText formFields : XmlNode =
    _form [
        _name_  name
        _class_ Bulma.box

        Hx.post submitUrl
        _hxTarget_ "main"
        Hx.swapOuterHtml
        
        // TODO: on form submit make it so the form and its input / button components are disabled. 
    ] (
        [
            Xsrf.antiforgeryInput token
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
                        _typeSubmit_
                    ] [
                        _text buttonText
                    ]
                ]
            ]
        ]
    )
