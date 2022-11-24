namespace FSUI.Elements.Interfaces

open FSUI.Types

type IPoly<'prop, 'data, 'node> =
    abstract member Poly : Applies<'prop, 'data, 'node>

type IContainer<'prop, 'node> =
    abstract member Container: Applies<'prop, 'node list, 'node>

type IText<'prop, 'node> =
    abstract member Text: Applies<'prop, string, 'node>

type IButton<'prop, 'data, 'node> =
    abstract member Button: Applies<'prop, 'data, 'node>

type IGameObject<'prop, 'data, 'node> =
    abstract member GameObject : (string -> 'prop collection -> 'data -> Position -> 'node)


/// Let a type from one type hierarchy to join/contain children of another type hierarchy
/// e.g. a nodeElement containing GameObject
type IJoinContain<'prop, 'child, 'container> =
    abstract member JoinContain : Applies<'prop, 'child, 'container>
