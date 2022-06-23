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

    type IText<'Visual> =
        abstract member Text : ElementBase<string, 'Visual>

    type IContainer<'Visual, 'Container> =
        abstract member Container : ElementBase<List<'Visual>, 'Container>

module RenderElements =
    open CommonNodeInterfaces

    let cachedElement (elGet: 'env -> ElementBase<_,_>) content env key =
        let el = elGet env
        let cache = (el :> ICache<_,_>).Cache

        match cache.TryGetValue key with
        | true, (lastData, lastVisual) -> el.Update lastData lastVisual content
        | _ -> el.Create content

    let text<'t> = cachedElement <| fun (env: IText<'t>) -> env.Text

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

        interface IText<IVisualElement> with
            member val Text =
                { new ElementBase<string, _>(swapCache.cacheEvents) with
                    member _.Create content = new VisualTextElement (content) :> IVisualElement
                    member _.Update lastData lastNode content = lastNode // TODO
                }

        interface IContainer<IVisualElement, VisualContainer> with
            member val Container =
                { new ElementBase<List<IVisualElement>, VisualContainer>(swapCache.cacheEvents) with
                    member _.Create children = new VisualContainer (children)
                    member _.Update lastData lastNode children = lastNode // TODO
                }

module ImplementationHostB =
    open CommonNodeInterfaces
    open Design.Host

    type EnvB() =
        let swapCache = SwapCache()

        interface ISwapCache with
            member _.Cache = swapCache

        interface IText<IVisualElement> with
            member val Text =
                { new ElementBase<string, _>(swapCache.cacheEvents) with
                    member _.Create content = new VisualTextElement (content) :> IVisualElement
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

    let x y =
        text "hi" y

    let view : EnvA -> Position -> Unit =
        div [
            text "hi"
        ]

    // view (Env()) (Ordinal 0)


module ScratchInterfaceType =
    type Foo = interface end
    type FooBar() = interface Foo

    let foo (fn: int -> #Foo) = fn 0
    
    let foobar (x: int) = FooBar()

    let x = foo foobar

module ScratchInterfaceTypeDetailed =
    type IVisEl = interface end
    type VisText(c: string) =
        interface IVisEl
    type VisNum(i: int) =
        interface IVisEl

    let div (children: List<int -> #IVisEl>) =
        children |> List.mapi (fun idx x -> x idx)

    let text (content: string) (idx: int) = VisText content :> IVisEl
    let num (c: int) (idx: int) = VisNum c :> IVisEl

    let view =
        div [
            text "hi"
            num 0
        ]
