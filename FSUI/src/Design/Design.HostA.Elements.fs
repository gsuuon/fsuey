module Design.HostA.Elements

// Host types, unchangeable
// Or can I use extension type?
type IVisualElement = interface end

type VisualElement() =
    interface IVisualElement
    interface System.IDisposable with
        member _.Dispose() = ()

type VisualTextElement(content: string) =
    inherit VisualElement()

type VisualButtonElement(label: string, action: unit -> unit) =
    inherit VisualElement()

type VisualContainer() =
    inherit VisualElement()
    member val Children : IVisualElement list = []



