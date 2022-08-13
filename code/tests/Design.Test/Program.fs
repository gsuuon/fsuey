module Host =
    type IVisualElement = interface end

    type VisualElement() =
        interface IVisualElement
        interface System.IDisposable with
            member _.Dispose() = ()

    type VisualTextElement(content: string) =
        inherit VisualElement()
        member val Content = content with get, set

    type VisualButtonElement(label: string, action: unit -> unit) =
        inherit VisualElement()
        member val Label = label with get, set
        member val Action = action with get, set

    type VisualContainer(children: IVisualElement list) =
        inherit VisualElement()
        member val Children = children with get, set


module Env =
    open Host

    open Scratch.ObjectExpression
    open Scratch.ObjectExpression.CommonNodeInterfaces
    
    type EnvA() =
        let swapCache = SwapCache()

        interface ISwapCache with
            member _.Cache = swapCache

        interface IText<IVisualElement> with
            member val Text =
                { new ElementBase<string, VisualTextElement>(swapCache.cacheEvents) with
                    member _.Create content = new VisualTextElement (content)
                    member _.Update lastData lastNode content =
                    // TODO
                        lastNode.Content <- content
                        lastNode
                }

        interface IButton<IVisualElement> with
            member val Button =
                { new ElementBase<_, _>(swapCache.cacheEvents) with
                    member _.Create ((label, action)) = new VisualButtonElement(label, action) :> IVisualElement
                    member _.Update lastData lastNode content = lastNode
                }

        interface IContainer<IVisualElement, VisualContainer> with
            member val Container =
                { new ElementBase<List<IVisualElement>, VisualContainer>(swapCache.cacheEvents) with
                    member _.Create children = new VisualContainer (children)
                    member _.Update lastData lastNode children = lastNode // TODO
                }


module Program =

    [<EntryPoint>]
    let main _ =
        0
