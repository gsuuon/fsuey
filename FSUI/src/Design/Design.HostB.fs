module Design.HostB

open Design.Common
open Design.Common.Interfaces
open Design.HostA

type Node() =
    interface INode with
        member _.Attach x = ()

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

    interface IContainer<Node, VisualContainer> with
        member val Container =
            { new ElementBase<List<Node>, VisualContainer>(swapCache.cacheEvents) with
                member _.Create children = new VisualContainer ()
                member _.Update lastData lastNode children = lastNode // TODO
            }

