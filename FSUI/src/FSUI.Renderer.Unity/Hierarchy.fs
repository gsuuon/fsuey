namespace FSUI.Renderer.Unity.Hierarchy

open UnityEngine
open UnityEngine.UIElements

type UnityNode =
    static member addChild (parent: VisualElement) =
        fun (child: VisualElement) -> parent.Add child

    static member addChild (parent: GameObject) =
        fun (child: GameObject) -> child.transform.SetParent parent.transform

    static member addChildren(children: VisualElement list, parent: VisualElement) =
        children |> List.iter (UnityNode.addChild parent)
        parent

    static member addChildren(children: GameObject list, parent: GameObject) =
        children |> List.iter (UnityNode.addChild parent)
        parent

    static member inline remove (x: #VisualElement) = x.RemoveFromHierarchy()
    static member inline remove (x: GameObject) = GameObject.Destroy x
