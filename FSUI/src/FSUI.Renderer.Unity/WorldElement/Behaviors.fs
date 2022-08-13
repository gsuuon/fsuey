module FSUI.Renderer.Unity.WorldElement.Behaviors

open UnityEngine

type ApplyGameObject = delegate of GameObject -> unit

[<AbstractClass>]
type Behavior<'T>() =
    inherit MonoBehaviour()

    [<DefaultValue>]
    val mutable Value : 'T

type Update() = 
    inherit Behavior<ApplyGameObject>()
    member this.Update () = this.Value.Invoke this.gameObject

type Start() = 
    inherit Behavior<ApplyGameObject>()
    member this.Start () = this.Value.Invoke this.gameObject

