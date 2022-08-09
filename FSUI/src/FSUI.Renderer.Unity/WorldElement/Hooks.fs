module FSUI.Renderer.Unity.WorldElement.Hooks
// plenty of performance improvements possible here

open System
open System.Collections.Generic
open UnityEngine
open FSUI.Renderer.Unity.WorldElement.Behaviors

[<RequireQualifiedAccess>]
type HookKey =
    | Desc of string * Type
    | FnTyp of Type * Type
      
type Prop =
    | Effect of (GameObject -> unit)
    | Attach of HookKey * (GameObject -> (unit -> unit))

let private addUnique (err: Lazy<string>) key item (dct: Dictionary<_,_>) =
    if not <| dct.TryAdd (key, item) then
        System.Console.Error.Write (err.Force())

let create (props: Prop list) visual =
    let propsDetach = Dictionary<_, _>()

    for hook in props do
        match hook with
        | Effect fn -> fn visual
        | Attach (key, attach) ->
            propsDetach
             |> addUnique
                    (lazy ("Rendered multiple hooks with the same key: " + key.ToString()))
                    key
                    (attach visual)

    propsDetach
    
let update (lastPropsDetach: Dictionary<_, unit -> unit>) (thisProps: Prop list) visual =
    let nextPropsDetach = Dictionary<_, _>()

    for hook in thisProps do
        match hook with
        | Effect fn -> fn visual
        | Attach (key, attach) ->
            let existed, lastDetach = lastPropsDetach.Remove key

            if existed then
                nextPropsDetach
                 |> addUnique
                        (lazy ("Updated existing hook with duplicate key: " + key.ToString() ) )
                        key
                        lastDetach
            else
                nextPropsDetach
                 |> addUnique
                        (lazy ("Added new hook with duplicate key: " + key.ToString() ) )
                        key
                        (attach visual)

    for (KeyValue (_, detach) ) in lastPropsDetach do
        detach()

    nextPropsDetach

type Props =
    static member private attachesFn<'T when 'T :> Behavior<Action> > (fn: unit -> unit) =
        fun (gObj: GameObject) ->
            let x = gObj.AddComponent<'T>()
            x.Value <- Action fn

            fun () -> GameObject.Destroy x

    static member on<'T when 'T :> Behavior<Action>> (desc, fn)
        = Attach (HookKey.Desc (desc, typeof<'T>), Props.attachesFn<'T> fn)
    static member on<'T when 'T :> Behavior<Action>> (fn)
        = Attach (HookKey.FnTyp (typeof<'T>, fn.GetType()), Props.attachesFn<'T> fn)
    static member effect (fn)
        = Effect fn
