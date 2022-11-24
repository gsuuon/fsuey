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
      
type EffectKey =
    | AlwaysRun
    | EffectKey of string

type Prop =
    | Effect of EffectKey * (GameObject -> unit)
    | Attach of HookKey * (GameObject -> (unit -> unit))

let private addUnique (err: string) key item (dct: Dictionary<_,_>) =
    if not <| dct.TryAdd (key, item) then
        eprintf "Duplicate key: %A -- %s" key err

type CacheProps = {
    removeAttached : Dictionary<HookKey, unit -> unit>
    effects : HashSet<EffectKey>
}

let create (props: IReadOnlyCollection<Prop>) visual =
    let propsDetach = Dictionary<_, _>()
    let effects = HashSet()

    for hook in props do
        match hook with
        | Effect (key, fn) ->
            match key with
            | AlwaysRun -> fn visual
            | key ->
                if effects.Add key then fn visual
                else eprintf $"Duplicate effect in create, not running: {key}"
        | Attach (key, attach) ->
            propsDetach
             |> addUnique
                    "Rendered multiple hooks"
                    key
                    (attach visual)

    {
        removeAttached = propsDetach
        effects = effects
    }
    
let update (lastProps: CacheProps) (thisProps: IReadOnlyCollection<Prop>) visual =
    let nextPropsDetach = Dictionary<_, _>()
    let nextEffects = HashSet()

    for hook in thisProps do
        match hook with
        | Effect (key, fn) ->
            match key with
            | AlwaysRun -> fn visual
            | key ->
                if not (nextEffects.Add key) then
                    eprintf $"Duplicate effect in update, not running: {key}"

                if not (lastProps.effects.Contains key) then
                    fn visual

        | Attach (key, attach) ->
            let existed, lastDetach = lastProps.removeAttached.Remove key

            if existed then
                nextPropsDetach
                 |> addUnique
                        "Updated existing hook"
                        key
                        lastDetach
            else
                nextPropsDetach
                 |> addUnique
                        "Added new hook"
                        key
                        (attach visual)

    for (KeyValue (_, detach) ) in lastProps.removeAttached do
        detach()

    {
        removeAttached = nextPropsDetach
        effects = nextEffects
    }


let private attachFnAsNewComponent<'T when 'T :> Behavior<ApplyGameObject> > (fn: GameObject -> unit) =
    fun (gObj: GameObject) ->
        let x = gObj.AddComponent<'T>()
        x.Value <- ApplyGameObject fn

        fun () -> GameObject.Destroy x

type Props =

    [<RequiresExplicitTypeArguments>]
    static member on<'T when 'T :> Behavior<ApplyGameObject>> (desc, fn)
        = Attach (HookKey.Desc (desc, typeof<'T>), attachFnAsNewComponent<'T> fn)

    [<RequiresExplicitTypeArguments>]
    static member on<'T when 'T :> Behavior<ApplyGameObject>> (fn)
        = Attach (HookKey.FnTyp (typeof<'T>, fn.GetType()), attachFnAsNewComponent<'T> fn)

    static member effect (key: string, fn)
        = Effect (EffectKey key, fn)

    static member effect (fn: GameObject -> unit)
        = Effect (AlwaysRun, fn)
