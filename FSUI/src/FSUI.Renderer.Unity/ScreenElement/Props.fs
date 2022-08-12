namespace FSUI.Renderer.Unity.ScreenElement

open UnityEngine.UIElements

type Prop =
    | Class of name: string

module Props =
    let apply (el: #VisualElement) =
        function
        | Class name ->
            el.AddToClassList name

    let unapply (el: #VisualElement) =
        function
        | Class name ->
            el.RemoveFromClassList name

    let applyProps (el: #VisualElement) (xs: array<Prop>) =
        xs |> Array.iter (apply el)

    let unapplyProps (el: #VisualElement) (xs: array<Prop>) =
        xs |> Array.iter (unapply el)
