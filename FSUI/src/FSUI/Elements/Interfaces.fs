namespace FSUI.Elements.Interfaces

open FSUI.Renderer.Element

type IPoly<'props, 'data, 'node> =
    abstract member Poly : RendersElement<'props, 'data, 'node>

type IContainer<'props, 'node> =
    abstract member Container: RendersElement<'props, 'node list, 'node>

type IText<'props, 'node> =
    abstract member Text: RendersElement<'props, string, 'node>

type IButton<'props, 'data, 'node> =
    abstract member Button: RendersElement<'props, 'data, 'node>

type IGameObject<'props, 'data, 'node> =
    abstract member GameObject : RendersElement<'props, 'data, 'node>

/// Let a type from one type hierarchy to join/contain children of another type hierarchy
/// e.g. a nodeElement containing GameObject
type IJoinContain<'props, 'child, 'container> =
    abstract member JoinContain : RendersElement< 'props, 'child, 'container>
