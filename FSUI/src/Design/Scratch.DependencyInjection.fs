namespace Scratch.DependencyInjection

module Elements =
    open System.Collections.Generic

    type Key = Ordinal of int

    module Host =
        type IVisualElement = interface end
        type VisualTextElement(content: string) =
            interface IVisualElement
        type VisualContainer(children: IVisualElement list) =
            interface IVisualElement
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
                member _.Create x = VisualTextElement(x)
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

        [<AbstractClass>]
        type IElement<'Data, 'Visual>() =
            member _.Cache : Map<Key, 'Data * 'Visual> = Map.empty
            abstract member Create : 'Data -> 'Visual
            abstract member Update : 'Data -> 'Visual -> 'Visual
            
        type IProvider<'Data, 'Visual> =
            abstract member Element : IElement<'Data, 'Visual>

        type Node<'T> = Key -> 'T

        type Env() =
            let visualText = 
                { new IElement<_,_>() with
                    member _.Create content = VisualTextElement content
                    member _.Update content previous = previous
                }

            let visualContainer =
                { new IElement<_, VisualContainer>() with
                    member _.Create children = 
                        children
                         |> List.map (fun x -> x (Ordinal 0))
                         |> VisualContainer
                            
                    member _.Update children previous = previous
                }

            interface IProvider<string, VisualTextElement> with member _.Element = visualText
            interface IProvider<Node<IVisualElement> list, VisualContainer> with member _.Element = visualContainer

        let text (env: #IProvider<string, _>) content =
            env.Element.Create content
