module FSUI.Renderer.Unity.GameObject.Behaviors

open System
open UnityEngine

[<AbstractClass>]
type Behavior<'T>() =
    inherit MonoBehaviour()

    [<DefaultValue>]
    val mutable Value : 'T

type Update() = 
    inherit Behavior<Action>()
    member this.Update () = this.Value.Invoke()

type Start() = 
    inherit Behavior<Action>()
    member this.Start () = this.Value.Invoke()

