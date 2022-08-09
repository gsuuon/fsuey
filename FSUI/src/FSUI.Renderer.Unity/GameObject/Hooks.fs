namespace FSUI.Renderer.Unity.GameObject

open UnityEngine

module Design =
    // plenty of performance improvements possible here
    open System

    [<RequireQualifiedAccess>]
    type HookKey =
        | Desc of string * Type
        | FnTyp of Type * Type
          
    type Hook =
        | Effect of (GameObject -> unit)
        | Attach of HookKey * (GameObject -> (unit -> unit))

    module HookProps =
        open System.Collections.Generic

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

    module Environment =
        open FSUI.Renderer.Cache
        open FSUI.Elements.Interfaces

        type Provider() =
            let swappers = Swappers()
            let create name = GameObject name

            interface IGameObject<Hook list, string, GameObject> with
                member val GameObject =
                    let cache = swappers.Create GameObject.Destroy

                    fun props name pos ->
                        let (exists, last) = cache.Stale.Remove pos
                        if exists then
                            let (props', data', visual') = last
                            let detachProps = HookProps.update props' props visual'
                            cache.Fresh.Add (pos, (detachProps, name, visual'))
                            visual'
                        else
                            let visual = create name
                            let detachProps = visual |> HookProps.create props
                            cache.Fresh.Add (pos, (detachProps, name, visual))
                            visual

    type behavior<'T>() =
        inherit MonoBehaviour()

        [<DefaultValue>]
        val mutable Value : 'T

    type update() = 
        inherit behavior<Action>()
        member this.Update () = this.Value.Invoke()

    let attachesFn<'T when 'T :> behavior<Action>> (fn: unit -> unit) (gObj: GameObject) =
        let x = gObj.AddComponent<'T>()
        x.Value <- Action fn
        fun () -> GameObject.Destroy x

    type Hooks =
        static member on<'T when 'T :> behavior<Action>> (desc, fn)
            = Attach (HookKey.Desc (desc, typeof<'T>), attachesFn<'T> fn)
        static member on<'T when 'T :> behavior<Action>> (fn)
            = Attach (HookKey.FnTyp (typeof<'T>, fn.GetType()), attachesFn<'T> fn)
        static member effect (fn)
            = Effect fn

    module Application =
        open type Hooks
        open FSUI.Elements.Views

        let view () =
            gameObject "foo"
                [ on<update> (fun _ -> printfn "updated")
                  on<update> ("prints update", fun _ -> printfn "updated")
                  effect     (fun _ -> printfn "rendered")
                ]

module Design2 =
    type GameObjectDetach = delegate of unit -> unit
    type GameObjectAttach = delegate of GameObject -> GameObjectDetach

    type ApplyGameObject = delegate of GameObject -> unit

    module Components =
        type Behaviour<'T>() =
            inherit MonoBehaviour()
            [<DefaultValue>] val mutable public Value : 'T

        let addComponent<'T when 'T :> Behaviour<ApplyGameObject>> (action: GameObject -> unit) =
            GameObjectAttach
                ( fun gameObject ->
                    let behaviour = gameObject.AddComponent<'T>()
                    behaviour.Value <- action

                    GameObjectDetach
                        ( fun () -> GameObject.Destroy behaviour )
                )

        type Start() =
            inherit Behaviour<ApplyGameObject>()
            member this.Start() = this.Value.Invoke this.gameObject

        type Update() =
            inherit Behaviour<ApplyGameObject>()
            member this.Update() = this.Value.Invoke this.gameObject

    module Behaviours =
        open Components
        type UnitFn = unit -> unit

        [<CustomEquality; NoComparison>]
        type Hook =
            /// Semantics are: if hook description changes, re-run the hook
            Hook of description: string * attach: GameObjectAttach
            with
                member this.Description =
                    let (Hook (desc, _) ) = this
                    desc

                interface System.IEquatable<Hook> with
                    member this.Equals other =
                        this.Description = other.Description

        let inline private hook<'T when 'T :> Behaviour<ApplyGameObject> >
            (desc, action: GameObject -> unit)
            =
            Hook (string typeof<'T> + "|" + desc, addComponent<'T> action)

        let nodetach = GameObjectDetach (fun () -> ())
        type Hooks =
            static member on<'T when 'T :> Behaviour<ApplyGameObject> > (action)
                = hook (action.GetType().ToString(), action)

            static member on<'T when 'T :> Behaviour<ApplyGameObject> > (desc, action)
                = hook (desc, action)

            static member hook (desc, action)
                = Hook
                    ( desc
                    , GameObjectAttach (
                        fun gObj ->
                            action gObj
                            nodetach
                        )
                    )

    module App =
        open Components
        open Behaviours
        open type Behaviours.Hooks

        open FSUI.Renderer.Element
        open FSUI.Renderer.Cache

        let foo (fn: GameObjectDetach) = ()
        let prefab a b = ()

        let diffProps propsA propsB = [], []

        type IGameObject =
            abstract GameObject : RendersElement<Hook list, string, GameObject>

        type IPrefab =
            abstract Prefab : RendersElement<Hook list, string, GameObject>

        let mkWorldElement (swappers: Swappers) (create: 'data -> GameObject) =
            let cache = swappers.Create GameObject.Destroy

            let attachProps props visual =
                props
                 |> List.map
                    (fun ((Hook (_,attach)) as hook) -> hook, attach.Invoke visual)

            let detachProps props =
                props
                 |> List.map (fun (_, (detach: GameObjectDetach)) -> detach.Invoke() )

            fun props data pos ->
                let (exists, last) = cache.Stale.Remove pos
                if exists then
                    let (props', data', visual') = last
                    let removed, created = diffProps props' props
                    let propsDetach = attachProps created visual'

                    // TODO do things with removed, created
                    visual'
                else
                    let visual = create data
                    let propsDetach = attachProps props visual

                    cache.Fresh[pos] <- (propsDetach, data, visual)

                    visual
                
        let gameObject name props =
            fun (env: #IGameObject) pos ->
                env.GameObject props name pos

        type Provider() =
            let swappers = Swappers()
            let compareProps (detachProps: List<string * GameObjectDetach>) (attachProps: List<Hook>) =
                ()
                
            interface IGameObject with
                member val GameObject =
                    let cache = swappers.Create GameObject.Destroy

                    fun props name pos ->
                        let (exists, last) = cache.Stale.Remove pos
                        if exists then
                            let (props', data', visual') = last
                            let removed, created = diffProps props' props
                            // TODO do things with removed, created
                            visual'
                        else
                            let visual = GameObject()

                            let detachProps =
                                props
                                 |> List.map
                                    (fun ((Hook (desc, attach)) as hook) -> desc, attach.Invoke visual)

                            cache.Fresh[pos] <- (detachProps, name, visual)

                            visual
                        

        let view () =
            gameObject "test"
                [ on<Update>
                    ( "increments count"
                    , fun gobj -> ()
                    )
                  hook
                    ( "position"
                    , fun x -> x.transform.position <- Vector3.zero
                    )
                 ]

        let view2 () =
            prefab "test"
                [ on<Start> (fun _ -> () )
                  on<Update> (fun _ -> () )
                ]
        
    type Render__ = // methods enable FSharp compiler delegate conversion
                  // methods enable named arguments and overloading
                  // can't convert delegates when called with pipe operator
                  // can't curry these
        // static member start (x: Action) = behaviour<Start> x
        // static member update (x: Action) = behaviour<Update> x
        static member gameObject(?name: string, ?props: List<ApplyGameObject>, ?children: List<GameObject>) =
            let name = defaultArg name "gameObject"
            let props = defaultArg props List.empty
            let children = defaultArg children List.empty
            ()

            // props
            //  |> List.fold apply
            //         (GameObject name)
             // |> List.foldBack attach
             //        children

