namespace FSUI.Renderer.Unity

open UnityEngine
open UnityEngine.UIElements

open FSUI.Types
open FSUI.Elements.Interfaces

open FSUI.Renderer.Cache
open FSUI.Renderer.Element
open FSUI.Renderer.Unity

module Util =
    open System

    let printToUnity () =
        let mutable currentLine = ""

        Console.SetOut
            { new IO.TextWriter() with
                member _.Write (msg: string) =
                    currentLine <- currentLine + msg
                member _.WriteLine () =
                    Debug.Log currentLine
                    currentLine <- ""
                member _.Encoding =
                    System.Text.Encoding.UTF8
            }

module Flow =
    open FSUI.Flow

    type UnityFlow() =
        inherit FlowBuilder() 
        member _.Run (x: Flow<_, YieldInstruction>) = Flow.asEnumerator x

    let flow = UnityFlow()

module Renderer =
    type private Once(fn) as this =
        // Probably should just use the straighforward mutable option cache
        // Really wanted to avoid branching
        // do an actual benchmark on this vs other memoization (versionedfuncs / state)
        member val private fn =
            fun x ->
                let y = fn x
                this.fn <- fun _ -> y
                y
            with get, set

        member this.Invoke x = this.fn x

    let mount<'T when 'T : (new : unit -> 'T) and 'T :> IProvider > (document: UIDocument) =
        let env = new 'T()
        let addOnce = Once (fun el -> document.rootVisualElement.Add el)

        fun view ->
            view env Root |> addOnce.Invoke
            env.ProviderState.Tick()

[<AutoOpen>]
module Types =
    type ScreenProp = ScreenElement.Props.Prop
    type ScreenElement = VisualElement

    type WorldElement = GameObject

    type WorldElementType =
        | Empty of name: string
        | Prefab of resourcePath: string
        with
        member this.Create () =
            match this with
            | Empty name -> GameObject(name)
            | Prefab path ->
                let prefab = Resources.Load<GameObject>(path)
                GameObject.Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity)

    // TODO move this somewhere
    type VisualGameObjectContainer(children: GameObject list) =
        inherit VisualElement()

        member this.RemoveFromHierarchy () =
            children |> List.iter (Graph.remove)

            base.RemoveFromHierarchy()

type ScreenElementRecord<'d, 'v> = {
    create : 'd -> 'v
    update : 'd -> 'd -> 'v -> 'v
}

[<AutoSerializable(false)>] // Unity will try to serialize this and hit depth limits
type UnityProvider() =
    let ulog x = Debug.Log x
    let swappers = Swappers()

    let changeScreenProps (changes: Changes<ScreenProp>) (el: #VisualElement) =
        changes.created |> ScreenElement.Props.applyProps el
        changes.removed |> ScreenElement.Props.unapplyProps el
        el

    let screenBase wrap diffData (el: ScreenElementRecord<'data, 'visual>) =
        createBase
            wrap
            diffData
            (fun x -> x :> VisualElement)
            (swappers.Create Graph.remove) 
            { create =
                fun props data ->
                    let visual = el.create data
                    props |> ScreenElement.Props.applyProps visual
                    visual
              update = el.update
              change = changeScreenProps
            }

    let screen x = screenBase id (<>) x

    let world (x: IElementRenderer<'props, 'data, WorldElement>) =
        create id (swappers.Create Graph.remove) x

    let polyString = // Example of multiple specializations of an interface (can't `member val`)
        screen {
            create = fun d    -> Label d
            update = fun d' d e -> e.text <- d; e
        }

    interface IProvider with
        member _.ProviderState = swappers

    interface IText<ScreenProp, ScreenElement> with
        member val Text =
            screen {
                create = fun d      -> Label d
                update = fun d' d e -> e.text <- d ;e
            }

    interface IContainer<ScreenProp, ScreenElement> with
        member val Container =
            screen {
                create = fun d      -> Graph.addChildren (d, new VisualElement())
                update = fun d' d e -> Graph.addChildren (d, e)
                    // Assumes adding same element is no-op, swapper takes care of removing stale children
            }

    interface IGameObject<WorldElement.Hooks.Prop, WorldElement list, WorldElement> with
        member val GameObject =
            WorldElement.Element.create GameObject swappers

    member val Prefab
        : string -> string -> WorldElement.Hooks.Prop collection -> WorldElement list -> Position -> WorldElement
        =
        let cache = swappers.Create GameObject.Destroy

        let loadPath path name =
            let prefab = Resources.Load<GameObject> path 
            let gObj = GameObject.Instantiate<GameObject> prefab
            gObj.name <- name
            gObj.SetActive false
            gObj

        fun path name props children pos ->
            match cache.Stale.Remove pos : bool * _ with
            | true, (props', data', visual') ->
                let detachProps = WorldElement.Hooks.update props' props visual'
                cache.Fresh.Add (pos, (detachProps, children, visual'))
                Graph.addChildren (children, visual')
            | false, _ ->
                let visual = loadPath path name
                let detachProps = visual |> WorldElement.Hooks.create props
                cache.Fresh.Add (pos, (detachProps, children, visual))
                Graph.addChildren (children, visual)

    // TODO Can I avoid rendering an empty VisualElement to contain game objects?
    interface IJoinContain<ScreenProp, GameObject list, ScreenElement> with
        member val JoinContain =
            let activateAll = List.iter (fun (g: GameObject) -> g.SetActive true)

            screen {
                create = fun d ->
                    activateAll d

                    VisualGameObjectContainer d

                update = fun d' d e ->
                    // TODO probably only want to activate the new ones, incase something is inactived on purpose
                    // instead of just being removed
                    // can use diff for this
                    activateAll d

                    e // TODO
            }
    
    // TODO can I hide the Keyed<'Key, unit -> unit> so I can use any IEquitable as 'Key?
    interface IButton<ScreenProp, ScreenElement * Keyed<string, unit -> unit>, ScreenElement> with
        member val Button = 
            screenBase
                (fun (el, Keyed (key, act) ) -> (el, Keyed (key, System.Action act)))
                (<>)
                {
                    create = fun (child, Keyed (_, action) ) ->
                        ScreenNode.addChild
                            (Button action ) // directly stick the fn on without converting to delegate or we can't remove
                            child

                    update = fun (_, Keyed (_, action') ) (child, Keyed (_, action) ) e ->
                        e.remove_clicked action'
                        e.add_clicked action

                        ScreenNode.addChild e child
                }
    
    // NOTE These are just examples of multiple specializations
    interface IPoly<ScreenProp, obj, VisualElement> with
        member val Poly =
            screen {
                create = fun d      -> Label (string d)
                update = fun d' d e -> e.text <- (string d); e
            }

    interface IPoly<ScreenProp, string, VisualElement> with
        member _.Poly = polyString
