namespace FSUI.Elements.Interfaces

open FSUI.Renderer.Element

type IPoly<'prop, 'data, 'node> =
    abstract member Poly : RendersElement<'prop, 'data, 'node>

type IContainer<'prop, 'node> =
    abstract member Container: RendersElement<'prop, 'node list, 'node>

type IText<'prop, 'node> =
    abstract member Text: RendersElement<'prop, string, 'node>

type IButton<'prop, 'data, 'node> =
    abstract member Button: RendersElement<'prop, 'data, 'node>

type IGameObject<'prop, 'data, 'node> =
    abstract member GameObject : RendersElement<'prop, 'data, 'node>

/// Let a type from one type hierarchy to join/contain children of another type hierarchy
/// e.g. a nodeElement containing GameObject
type IJoinContain<'prop, 'child, 'container> =
    abstract member JoinContain : RendersElement< 'prop, 'child, 'container>
