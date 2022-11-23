namespace FSUI.Types

type Position =
    | Ordinal of parent: Position * order: int
    | Nominal of parent: Position * name: string
    | Root
    member this.Named name =
        match this with
        | Root | Nominal _      -> this
        | Ordinal (parent, _) -> Nominal (parent, name)

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

type ElementRenderer<'prop, 'data, 'visual> =
    { create : 'prop collection -> 'data -> 'visual
      change : Changes<'prop> -> 'visual -> 'visual
      update : 'data -> 'data -> 'visual -> 'visual
    }
    interface IElementRenderer<'prop, 'data, 'visual> with
        member this.Create props data = this.create props data
        member this.Change props visual = this.change props visual
        member this.Update data' data visual = this.update data' data visual
