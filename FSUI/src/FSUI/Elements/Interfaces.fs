namespace FSUI.Elements.Interfaces

open FSUI.Renderer.Element

type IPoly<'data, 'props, 'visual> =
    abstract member Poly : RendersElement<'data, 'props, 'visual>

type IContainer<'data, 'props, 'node> =
    abstract member Container: RendersElement<'node list, 'props, 'node>

type IText<'props, 'visual> =
    abstract member Text: RendersElement<string, 'props, 'visual>

type IButton<'data, 'props, 'visual> =
    abstract member Button: RendersElement<'data, 'props, 'visual>

type IGameObject<'data, 'props, 'visual> =
    abstract member GameObject : RendersElement<'data, 'props, 'visual>

/// Let a type from one type hierarchy to join/contain children of another type hierarchy
/// e.g. a VisualElement containing GameObject
type IJoinContain<'child, 'props, 'container> =
    abstract member JoinContain : RendersElement<'child, 'props, 'container>
