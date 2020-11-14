Html.div [
    prop.className "container"
    prop.id "value"
    prop.style "color: red"
    prop.children [
        Html.div [
            prop.classes [ "notification"; "is-primary" ]
            prop.children [
                Html.text " This container is "
                Html.strong "centered"
                Html.text " on desktop and larger viewports. "
            ]
        ]
    ]
]