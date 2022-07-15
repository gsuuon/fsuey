module Design.HostA.Environment

open Design.Common
open Design.Common.Interfaces
open Design.HostA.Elements

type Node() =
    interface INode with
        member _.Attach x = ()

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

    interface IContainer<Node, VisualContainer> with
        member val Container =
            { new ElementBase<List<Node>, VisualContainer>(swapCache.cacheEvents) with
                member _.Create children = new VisualContainer ()
                member _.Update lastData lastNode children = lastNode // TODO
            }