namespace FSUI.Elements.Interfaces

open FSUI.Renderer.Element

type IElement<'data, 'props, 'visual, 'node> =
    abstract member Element : Element<'data, 'props, 'visual, 'node>

type IContainer<'props, 'visual, 'node> =
    abstract member Container: Element<'node list, 'props, 'visual, 'node>

type IText<'props, 'visual, 'node> =
    abstract member Text: Element<string, 'props, 'visual, 'node>

type IImage<'props, 'visual, 'node> =
    abstract member Image: Element<string, 'props, 'visual, 'node>

type IButton<'data, 'props, 'visual, 'node> =
    abstract member Button: Element<'data, 'props, 'visual, 'node>

type IGameObject<'data, 'props, 'visual , 'node> =
    abstract member GameObject : Element<'data, 'props, 'visual, 'node>

/// Let a type from one type hierarchy to join/contain children of another type hierarchy
/// e.g. a VisualElement containing GameObject
type IJoinContain<'data, 'props, 'visual, 'childType> =
    abstract member JoinContain : Element<'data, 'props, 'visual, 'childType>
