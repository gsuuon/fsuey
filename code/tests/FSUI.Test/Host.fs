namespace FSUI.Test.Host

type Mutation =
    | SetValue of name: string * value: string
    | SetAction of name: string * fn: (unit -> unit)
    | AddChild of idx: int * child: Visual

and Visual() =
    member val MutationLog = [] with get, set
    member this.AddLog (x: Mutation) =
        this.MutationLog <- x :: this.MutationLog

type Text(content: string) =
    inherit Visual()

    let mutable content = content
    member this.Content
        with get() =
            content
        and set x =
            this.AddLog (SetValue ("Content", x) )
            content <- x

type Button(child: Visual, action: unit -> unit) =
    inherit Visual()

    let mutable action = action
    let mutable child = child

    member this.Action
        with get() = action
        and set x =
            this.AddLog (SetAction ("Action", x) )
            action <- x

    member this.Child
        with get() = child
        and set x =     
            this.AddLog (AddChild (0, child) )
            child <- x

type Collection(children: Visual list) =
    inherit Visual()

    let mutable children = children
    member this.Children
        with get() = children
        and set xs = 
            List.iteri
                (fun idx child ->
                    this.AddLog (AddChild (idx, child) ) )
                xs

            children <- xs
