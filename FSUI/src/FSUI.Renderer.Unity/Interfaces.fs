namespace FSUI.Renderer.Unity.Interfaces

open UnityEngine
open FSUI.Renderer.Element
open FSUI.Renderer.Unity.Hierarchy

type ISpotlight<'props> =
    abstract member Spotlight : Element<Vector3, 'props, GameObjectNode, UnityNode>

