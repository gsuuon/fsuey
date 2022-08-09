module FSUI.Renderer.Unity.GameObject.Hooks
// plenty of performance improvements possible here

open System
open UnityEngine
open FSUI.Renderer.Unity.GameObject.Behaviors

open System.Collections.Generic

[<RequireQualifiedAccess>]
type HookKey =
    | Desc of string * Type
    | FnTyp of Type * Type
      
type Hook =
    | Effect of (GameObject -> unit)
    | Attach of HookKey * (GameObject -> (unit -> unit))

let private addUnique (err: Lazy<string>) key item (dct: Dictionary<_,_>) =
    if not <| dct.TryAdd (key, item) then
        System.Console.Error.Write (err.Force())

let create (props: Hook list) visual =
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
    
let update (lastPropsDetach: Dictionary<_, unit -> unit>) (thisProps: Hook list) visual =
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
    static member private attachesFn<'T when 'T :> behavior<Action> > (fn: unit -> unit) =
        fun (gObj: GameObject) ->
            let x = gObj.AddComponent<'T>()
            x.Value <- Action fn

            fun () -> GameObject.Destroy x

    static member on<'T when 'T :> behavior<Action>> (desc, fn)
        = Attach (HookKey.Desc (desc, typeof<'T>), Props.attachesFn<'T> fn)
    static member on<'T when 'T :> behavior<Action>> (fn)
        = Attach (HookKey.FnTyp (typeof<'T>, fn.GetType()), Props.attachesFn<'T> fn)
    static member effect (fn)
        = Effect fn

module Environment =
    open FSUI.Renderer.Cache
    open FSUI.Elements.Interfaces

    type Provider() =
        let swappers = Swappers()
        let newGameObject name = GameObject name

        interface IGameObject<Hook list, string, GameObject> with
            member val GameObject =
                let cache = swappers.Create GameObject.Destroy

                fun props name pos ->
                    let (exists, last) = cache.Stale.Remove pos
                    if exists then
                        let (props', data', visual') = last
                        let detachProps = update props' props visual'
                        cache.Fresh.Add (pos, (detachProps, name, visual'))
                        visual'
                    else
                        let visual = newGameObject name
                        let detachProps = visual |> create props
                        cache.Fresh.Add (pos, (detachProps, name, visual))
                        visual


module Application =
    open type Props
    open FSUI.Elements.Views

    let view () =
        gameObject "foo"
            [ on<update> (fun _ -> printfn "updated")
              on<update> ("prints update", fun _ -> printfn "updated")
              effect     (fun _ -> printfn "rendered")
            ]
