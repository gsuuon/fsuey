namespace FSUI.Renderer.Unity.Hierarchy

open UnityEngine
open UnityEngine.UIElements

type UnityNode =
    | Screen of VisualElement
    | World of GameObject
        // TODO probably not actually a game object, just too tired rn
        // don't think we want to just instantiate game objects here
        // load resource?
        // prefab path?


