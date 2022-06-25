namespace Design.Host

type IVisualElement = interface end

type VisualElement() =
    interface IVisualElement
    interface System.IDisposable with
        member _.Dispose() = ()

type VisualTextElement(content: string) =
    inherit VisualElement()

type VisualButtonElement(label: string, action: unit -> unit) =
    inherit VisualElement()

type VisualContainer(children: IVisualElement list) =
    inherit VisualElement()
    member _.Children = children
