namespace FSUI.Renderer.Unity

open UnityEngine
open UnityEngine.UIElements

module ScreenElement =
    let addChild (parent: #VisualElement) (child: #VisualElement) =
        parent.Add child
        parent

module WorldElement =
    let addChild (parent: GameObject) (child: GameObject) =
        child.transform.SetParent parent.transform
        parent

type Graph =
    static member addChildren(children: #VisualElement list, parent: #VisualElement) =
        children |> List.fold ScreenElement.addChild parent

    static member addChildren(children: GameObject list, parent: GameObject) =
        children |> List.fold WorldElement.addChild parent

    static member inline remove (x: #VisualElement) = x.RemoveFromHierarchy()
    static member inline remove (x: GameObject) = GameObject.Destroy x
