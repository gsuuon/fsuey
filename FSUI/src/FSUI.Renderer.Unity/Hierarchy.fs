namespace FSUI.Renderer.Unity.Hierarchy

open UnityEngine
open UnityEngine.UIElements

type GameObjectNode =
    { gameObject : GameObject
      mutable visualElementParent : VisualElement
    }

type UnityNode =
    | Screen of VisualElement
    | World of GameObjectNode
        // TODO probably not actually a game object, just too tired rn
        // don't think we want to just instantiate game objects here
        // load resource?
        // prefab path?

    static member addChild (child: UnityNode, parent: GameObjectNode) =
        match child, parent with
        | Screen screenChild, { visualElementParent = visualParent } ->
            visualParent.Add screenChild

        | World gobjChild, _ -> 
            gobjChild.gameObject.transform.SetParent parent.gameObject.transform

    static member addChild (child: UnityNode, parent: VisualElement) =
        match child with
        | Screen screenChild ->
            parent.Add screenChild
        | World gNode -> 
            gNode.visualElementParent <- parent

    static member addChildren(children: UnityNode list, parent: VisualElement) =
        children
         |> List.iter
            (function
             | Screen child -> parent.Add child
             | World x -> () // TODO
             )

        parent

    static member inline remove (x: #VisualElement) = x.RemoveFromHierarchy()
    static member inline remove (x: GameObjectNode) = GameObject.Destroy x.gameObject
