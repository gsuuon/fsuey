namespace Scratch.DependencyInjection

module Elements =
    open System.Collections.Generic

    type Key = Ordinal of int

    module Host =
        type VisualElement() =
            interface System.IDisposable with
                member _.Dispose() = ()

        type VisualTextElement(content: string) =
            inherit VisualElement()

        type VisualContainer(children: VisualElement list) =
            inherit VisualElement()
            member _.Children = children

    module Scratch1 =
        open Host

        type ICached<'K, 'V when 'K : comparison> =
            abstract member Cache : IDictionary<'K, 'V>
            abstract member Wipe : unit -> unit

        type IElement<'Data, 'Visual> =
            abstract member Create : 'Data -> 'Visual
            abstract member Update : 'Data * 'Visual -> unit

        type Cached<'T>() =
            let cache = new Dictionary<_,_>()

            interface ICached<Key, 'T> with
                member _.Cache = cache
                member _.Wipe () = ()


        // Elements
        /// Interface
        type ITextElement<'Visual> =
            inherit IElement<string, 'Visual>
        type TextElement() =
            inherit Cached<string * VisualTextElement>()
                // TODO this needs to tie into element
            interface ITextElement<VisualTextElement> with
                member _.Create x = new VisualTextElement(x)
                member _.Update (content, x) = ()

        // Env
        type IText<'Visual> =
            abstract member Text : ITextElement<'Visual>


        // Host
        type MyEnv() =
            interface IText<VisualTextElement> with
                member val Text = TextElement()

    module ScratchMultipleConcreteInterface =
        // this is interesting
        type IFoo<'T> =
            abstract member A : 'T

        type MyFoos() =
            interface IFoo<string> with
                member _.A = "hi"

            interface IFoo<int> with
                member _.A = 0

        let fooString (foos: #IFoo<string>) =
            foos.A

        let fooInt (foos: #IFoo<int>) =
            foos.A

        let withFoos (foos: MyFoos) =
            let aString = fooString foos
            let aInt = fooInt foos
            0

    module MultipleConcreteInterface =
        open Host
        open System

        type Cache<'K, 'V1, 'V2 when 'K: equality and 'V2 :> IDisposable >() =
            inherit Dictionary<'K, 'V1 * 'V2>()
            member this.Wipe() =
                for x in this do
                    (snd x.Value).Dispose()
                    
                this.Clear()

        [<AbstractClass>]
        type IElement<'Data, 'Visual when 'Visual :> IDisposable>() =
            member _.Cache : Cache<Key, 'Data, 'Visual> = Cache()
            abstract member Create : 'Data -> 'Visual
            abstract member Update : 'Data -> 'Visual -> 'Visual
            
        type IProvider<'Data, 'Visual when 'Visual :> IDisposable> =
            abstract member Element : IElement<'Data, 'Visual>

        type Node<'T> = Key -> 'T

        type Env() =
            let visualText = 
                { new IElement<_,_>() with
                    member _.Create content = new VisualTextElement (content)
                    member _.Update content previous = previous
                }

            let visualContainer =
                { new IElement<_, VisualContainer>() with
                    member _.Create children = 
                        new VisualContainer (children |> List.map (fun x -> x (Ordinal 0)))
                            
                    member _.Update children previous = previous
                }

            interface IProvider<string, VisualTextElement> with member _.Element = visualText
            interface IProvider<Node<VisualElement> list, VisualContainer> with member _.Element = visualContainer

            member _.Wipe () =
                visualText.Cache.Wipe()
                visualContainer.Cache.Wipe()

        let text (env: #IProvider<string, _>) content =
            env.Element.Create content
