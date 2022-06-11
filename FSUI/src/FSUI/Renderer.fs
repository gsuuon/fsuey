namespace FSUI.Renderer

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

type WrappedNode<'custom, 'data> =
    {
        node : Node<'custom, 'data>
        data : 'data
    }
and Node<'custom, 'data> =
    | Text of string
    | Custom of 'custom

// type PositionedNode<'t> = Position * Node<'t>

module Cache =
    type PositionCache<'node> = Map<Position, 'node>

    let create<'node> () : PositionCache<'node> = Map.empty
    
module Renderer =
    let render<'custom, 'data>
        (customRenderer: 'custom -> unit)
        (cache: Cache.PositionCache<WrappedNode<'custom, 'data>>)
        (node: WrappedNode<'custom, 'data>)
        =
        ()

module ComponentUI =
    type Message =
        | Clicked

    type Custom =
        | Spotlight of x : single * y : single
    
    type Style =
        | Green
        | Bold

    let node = 
        let update (x: Message) = ()

        {
            node =
                Node<Custom, _>.Container {
                    ordinal = [
                        { node = Node.Text "hi"
                          data = [ Bold ]
                        }
                    ]
                    nominal = Map.empty
                }
            data =
                [ Green ]
        }

module Run = 
    let cache = Cache.create()
    let node = ComponentUI.node

    let customRenderer =
        function
        | ComponentUI.Spotlight (x, y) -> ()

    Renderer.render customRenderer cache node
