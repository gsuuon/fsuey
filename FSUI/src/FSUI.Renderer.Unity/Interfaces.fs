namespace FSUI.Renderer.Unity.Interfaces

open UnityEngine
open UnityEngine.UIElements
open FSUI.Renderer.Element
open FSUI.Renderer.Unity.Hierarchy

type ISpotlight<'props> =
    abstract member Spotlight : Element<Vector3, 'props, GameObject, GameObject>

// TODO[wip] -- Can I use something like this to avoid rendering an empty VisualElement to contain game objects?
type UIGameObjectContainerNode =
    { children : GameObject list
      parent : VisualElement
    }

type IUIGameObjectContainer =
    abstract member UIGameObjectContainer : Element<List<GameObject>, unit, UIGameObjectContainerNode, UnityNode>

