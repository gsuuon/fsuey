namespace Scratch.ObjectExpression

open Design.Common

module CommonNodeInterfaces =
    type CacheTag =
        | A
        | B

    type CacheEvent = {
        swap: Event<CacheTag>
        wipe: Event<unit>
    }
    let cache () = System.Collections.Generic.Dictionary()
        
    type SwapCache() =
        member val activeCache = A with get, set

        member val cacheEvents = {
            wipe = Event<unit>()
            swap = Event<CacheTag>()
        }
        member x.Wipe() =
            x.cacheEvents.wipe.Trigger()
        member x.Swap() =
            x.activeCache <-
                match x.activeCache with
                | A -> B
                | B -> A

            x.cacheEvents.swap.Trigger(x.activeCache)
    
    type ISwapCache =
        abstract member Cache : SwapCache

    type ICache<'K, 'V> =
        abstract member Cache : System.Collections.Generic.IDictionary<'K, 'V>

    [<AbstractClass>]
    type ElementBase<'Data, 'Visual>(cacheEvent: CacheEvent) =
        let cacheA = cache()
        let cacheB = cache()
        let mutable activeCache = A
        let subscription =
            cacheEvent.wipe.Publish.Subscribe (
                fun _ ->
                    match activeCache with
                    | A -> cacheA.Clear()
                    | B -> cacheB.Clear()
            )
        interface System.IDisposable with
            member _.Dispose () = subscription.Dispose()

        interface ICache<Position, 'Data * 'Visual> with
            member _.Cache =
                match activeCache with
                | A -> cacheA
                | B -> cacheB
            
        abstract member Create : 'Data -> 'Visual
        abstract member Update : 'Data -> 'Visual -> 'Data -> 'Visual

        member element.Render content position : Unit =
            match (element :> ICache<_,_>).Cache.TryGetValue position with
            | true, (lastData, lastVisual) ->
                let el = element.Update lastData lastVisual content
                () // TODO
            | _ ->
                let el = element.Create content
                () // TODO

    type IText<'Visual> =
        abstract member Text : ElementBase<string, 'Visual>

    type IContainer<'Visual, 'Container> =
        abstract member Container : ElementBase<List<'Visual>, 'Container>

    type IButton<'Visual> =
        abstract member Button : ElementBase<string * (unit -> unit), 'Visual>

module RenderElements =
    open CommonNodeInterfaces

    let text x (e: #IText<_>) = e.Text.Render x
    let button x (e: #IButton<_>) = e.Button.Render x

    let div
        (children: List<'env -> Position -> 'visual>)
        (env: #IContainer<_,_>)
        (pos: Position) =
            let renderedNodes =
                children
                 |> List.map
                    (fun x -> x env pos )

            let container = renderedNodes |> env.Container.Create
            ()

module ImplementationHostA =
    open CommonNodeInterfaces
    open Design.Host

    type EnvA() =
        let swapCache = SwapCache()

        interface ISwapCache with
            member _.Cache = swapCache

        interface IText<VisualTextElement> with
            member val Text =
                { new ElementBase<string, VisualTextElement>(swapCache.cacheEvents) with
                    member _.Create content = new VisualTextElement (content)
                    member _.Update lastData lastNode content =
                        printfn "Updated text, was '%s' now '%s'" lastData content
                        lastNode // TODO
                }

        interface IButton<VisualButtonElement> with
            member val Button =
                { new ElementBase<_, _>(swapCache.cacheEvents) with
                    member _.Create ((label, action)) = new VisualButtonElement(label, action)
                    member _.Update lastData lastNode content = lastNode
                }

        interface IContainer<IVisualElement, VisualContainer> with
            member val Container =
                { new ElementBase<List<IVisualElement>, VisualContainer>(swapCache.cacheEvents) with
                    member _.Create children = new VisualContainer (children)
                    member _.Update lastData lastNode children = lastNode // TODO
                }

    module InlinedCachedElement =
        open RenderElements

        let foo (env: 'T) =
            let pos = Ordinal 0

            let fny = button ("ok", fun () -> ())
            let fnx = text "hi"

            [ fnx; fny ] |> List.map (fun x -> x env pos) |> ignore

        let x = foo (EnvA())

module ImplementationHostB =
    open CommonNodeInterfaces
    open Design.Host

    type EnvB() =
        let swapCache = SwapCache()

        interface ISwapCache with
            member _.Cache = swapCache

        interface IText<IVisualElement> with
            member val Text =
                { new ElementBase<string, IVisualElement >(swapCache.cacheEvents) with
                    member _.Create content = new VisualTextElement (content)
                    member _.Update lastData lastNode content = lastNode // TODO
                }

        interface IContainer<IVisualElement, VisualContainer> with
            member val Container =
                { new ElementBase<List<IVisualElement>, VisualContainer>(swapCache.cacheEvents) with
                    member _.Create children = new VisualContainer (children)
                    member _.Update lastData lastNode children = lastNode // TODO
                }

module View =
    open RenderElements
    open ImplementationHostA
    open ImplementationHostB

    open CommonNodeInterfaces
    open Design.Host

    type Model = {
        greet : string
    }

    let view model =
        div [
            text model.greet
            button ("Hey", fun () -> ())
        ]

    // view { greet = "hi" } (EnvA()) (Ordinal 0)

module TypeScratch2 =
    module StaticGeneric =
        type IFoo =
            abstract member Do : int -> Unit

        type MyThing<'A>() =
            static member foo () = Unchecked.defaultof<'A>
            interface IFoo with
                member _.Do x = ()

        open type MyThing<int> // whaaaat

        let x = foo()

                
