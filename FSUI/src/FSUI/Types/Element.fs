namespace FSUI.Types

type Position =
    | Ordinal of parent: Position * order: int
    | Nominal of parent: Position * name: string
    | Root
    member this.Named name =
        match this with
        | Root | Nominal _      -> this
        | Ordinal (parent, idx) -> Nominal (parent, name)

type collection<'T> = // TODO -- propcollection?
    System.Collections.Generic.IReadOnlyCollection<'T>

type Element<'prop, 'data, 'env, 'node>
    = 'prop collection -> 'data -> 'env -> Position -> 'node

type Renders<'env, 'node>
    = 'env -> Position -> 'node

type Applies<'prop, 'data, 'node>
    = 'prop collection -> 'data -> Position -> 'node

type Changes<'T> =
    { removed : array<'T>      
      created : array<'T>
      // content : array<'T>
    }

type IElementRenderer<'prop, 'data, 'visual> =
    abstract Create: 'prop collection -> 'data -> 'visual
    abstract Change: Changes<'prop> -> 'visual -> 'visual
    abstract Update: 'data -> 'data -> 'visual -> 'visual

