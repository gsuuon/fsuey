namespace Scratch.Collection

type Position = Ord of int

[<AbstractClass>]
type Base<'data, 'props, 'visual, 'node>([<InlineIfLambda>]wrap: 'visual -> 'node) =
    let cached : Map<Position, 'data * 'props * 'visual> = Map.empty

    abstract member Create: 'data -> 'props -> 'visual
    abstract member Update: 'data -> 'props -> 'visual -> 'data -> 'props -> 'visual

    member this.Render (data: 'data) (props: 'props) (pos: Position) : 'node =
        match Map.tryFind pos cached with
        | Some (cachedData, cachedProps, cachedVisual) ->
            this.Update cachedData cachedProps cachedVisual data props
        | None -> this.Create data props
        |> wrap

type IContainer<'props, 'visual, 'node> =
    abstract member Container: Base<'node list, 'props, 'visual, 'node>

type IText<'props, 'visual, 'node> =
    abstract member Text: Base<string, 'props, 'visual, 'node>

type IImage<'props, 'visual, 'node> =
    abstract member Image: Base<string, 'props, 'visual, 'node>

module Host =
    type Visual() = class end
    type VisualText(content: string) = inherit Visual()
    type VisualImage(path: string) = inherit Visual()
    type VisualCollection(children: Visual list) = inherit Visual()

module Elements =
    let inline text content props (env: #IText<_,_,_>) = env.Text.Render content props
    let inline image path props (env: #IImage<_,_,_>) = env.Image.Render path props

    let div children props (env: #IContainer<_,_,_>) pos =
        let nodes : 'node list =
            children |> List.map (fun fnode -> fnode env pos)

        env.Container.Render nodes props pos

module Layout =
    open Elements

    type Model =
        { name : string
          pic : string
        }

    let view model =
        div [
            text model.name []
            image model.pic []
        ] []

module VisualProvider =
    open Host

    type VisualProp =
        | Class of string

    type Props = VisualProp list

    [<AbstractClass>]
    type VisualBase<'data, 'visual when 'visual :> Visual>() =
        inherit Base<'data, Props, 'visual, Visual>(fun x -> x :> Visual)

    type Env() =
        interface IText<Props, VisualText, Visual> with
            member val Text =
                { new VisualBase<string, VisualText>() with
                    member _.Create data props = VisualText data
                    member _.Update cachedContent cachedProps cachedVisual data props = VisualText data
                }

        interface IImage<Props, VisualImage, Visual> with
            member val Image =
                { new VisualBase<string, VisualImage>() with
                    member _.Create content props = VisualImage content
                    member _.Update cachedContent cachedVisual cachedProps content props = VisualImage content
                }

        interface IContainer<Props, VisualCollection, Visual> with
            member val Container =
                { new VisualBase<Visual list, VisualCollection>() with
                    member _.Create children props = VisualCollection children
                    member _.Update cachedChildren cachedProps cachedVisual children props = VisualCollection children
                }

module App =
    open Layout
    open VisualProvider

    let pos = Ord 0
    let env = Env()
    let model =
        {
            name = "hi"
            pic = "there"
        }

    let x = view model env pos
