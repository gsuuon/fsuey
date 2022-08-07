namespace FSUI.Elements.Interfaces

open FSUI.Renderer.Element

type IPoly<'props, 'data, 'visual> =
    abstract member Poly : RendersElement<'props, 'data, 'visual>

type IContainer<'props, 'node> =
    abstract member Container: RendersElement<'props, 'node list, 'node>

type IText<'props, 'visual> =
    abstract member Text: RendersElement<'props, string, 'visual>

type IButton<'props, 'data, 'visual> =
    abstract member Button: RendersElement<'props, 'data, 'visual>

type IGameObject<'props, 'data, 'visual> =
    abstract member GameObject : RendersElement<'props, 'data, 'visual>

/// Let a type from one type hierarchy to join/contain children of another type hierarchy
/// e.g. a VisualElement containing GameObject
type IJoinContain<'props, 'child, 'container> =
    abstract member JoinContain : RendersElement< 'props, 'child, 'container>
