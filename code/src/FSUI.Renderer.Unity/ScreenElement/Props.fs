module FSUI.Renderer.Unity.ScreenElement.Props

open UnityEngine.UIElements

type Prop =
    | Class of name: string

let apply (el: #VisualElement) =
    function
    | Class name ->
        el.AddToClassList name

let unapply (el: #VisualElement) =
    function
    | Class name ->
        el.RemoveFromClassList name

let applyProps (el: #VisualElement) (xs: seq<Prop>) =
    xs |> Seq.iter (apply el)

let unapplyProps (el: #VisualElement) (xs: seq<Prop>) =
    xs |> Seq.iter (unapply el)
