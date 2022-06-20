namespace Scratch.ObjectExpression

open Design.Common

module CommonNodeInterfaces =
    type ICached<'Key, 'Data> = System.Collections.Generic.IDictionary<'Key, 'Data>
        
    type IElement<'Data, 'Visual> =
        abstract member Cache : ICached<Position, 'Data * 'Visual>
        abstract member Create : 'Data -> 'Visual

    type IText<'Visual> =
        abstract member Text : IElement<string, 'Visual>

    type IContainer<'Visual, 'Container> =
        abstract member Container : IElement<List<'Visual>, 'Container>

module ImplementationHost =

    open CommonNodeInterfaces
    open Design.Host

    let cache () = System.Collections.Generic.Dictionary()

    type CacheTag =
        | A
        | B

    type Cache<'Data, 'Visual>(swapEvt: Event<CacheTag>, wipeEvt: Event<Unit>) =
        let cacheA = cache()
        let cacheB = cache()
        let mutable activeCache = A

        let subscription =
            wipeEvt.Publish.Subscribe (
                function
                | A -> cacheA.Clear()
                | B -> cacheB.Clear()
            )

        interface System.IDisposable with
            member _.Dispose () = subscription.Dispose()


    type Env() =
        let cacheText = cache()
        let cacheContainer = cache()

        member _.Wipe() = // TODO flesh out
            cacheText.Clear()
            cacheContainer.Clear()

        interface IText<IVisualElement> with
            member val Text =
                { new IElement<string, _> with
                    member _.Cache = cacheText
                    member _.Create content = new VisualTextElement (content) :> IVisualElement
                }

        interface IContainer<IVisualElement, VisualContainer> with
            member val Container =
                { new IElement<List<IVisualElement>, VisualContainer> with
                    member _.Cache = cacheContainer
                    member _.Create children = new VisualContainer (children)
                }

module RenderElements =
    open CommonNodeInterfaces

    let text content =
        fun (env: #IText<_>) (pos: Position) ->
            env.Text.Create content

    let div (children: List<'env -> Position -> 'visual>) =
        fun (env: #IContainer<_,_>) (pos: Position) ->
            let x =
                children
                 |> List.map
                    (fun x -> x env pos )
            let y = x |> env.Container.Create
            ()

module View =
    open RenderElements
    open ImplementationHost

    let view : Env -> Position -> Unit =
        div [
            text "hi"
        ]

    view (Env()) (Ordinal 0)

    // TODO-next how does the cache work then?


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
