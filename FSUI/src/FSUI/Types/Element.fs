namespace FSUI.Types

type collection<'T> =
    System.Collections.Generic.IReadOnlyCollection<'T>

type Element<'prop, 'data, 'env, 'node>
    = 'prop collection -> 'data -> 'env -> Position -> 'node

type Renders<'env, 'node>
    = 'env -> Position -> 'node

type Applies<'prop, 'data, 'node>
    = 'prop collection -> 'data -> Position -> 'node
