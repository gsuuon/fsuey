namespace Design.Host

type Key = Ordinal of int

type VisualElement() =
    interface System.IDisposable with
        member _.Dispose() = ()

type VisualTextElement(content: string) =
    inherit VisualElement()

type VisualContainer(children: VisualElement list) =
    inherit VisualElement()
    member _.Children = children
