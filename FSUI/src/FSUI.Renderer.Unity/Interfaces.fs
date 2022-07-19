namespace FSUI.Renderer.Unity.Interfaces

open UnityEngine
open UnityEngine.UIElements
open FSUI.Renderer.Element

type WrappedElement =
    | Screen of VisualElement
    | World of GameObject
        // TODO probably not actually a game object, just too tired rn
        // don't think we want to just instantiate game objects here
        // load resource?
        // prefab path?

type ISpotlight<'props> =
    abstract member Spotlight : Element<Vector3, 'props, GameObject, WrappedElement>

