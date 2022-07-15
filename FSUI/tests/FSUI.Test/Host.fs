namespace FSUI.Test.Host

type Visual() = class end
type VisualText(content: string) =
    inherit Visual()
    member val Content = content with get, set

type VisualImage(path: string) =
    inherit Visual()
    member val Path = path with get, set

type VisualCollection(children: Visual list) =
    inherit Visual()
    member val Children = children with get, set
