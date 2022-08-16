namespace FSUI.Test.Host

// TODO this is probably unecessary, track in Provider instead
// though this is more accurate -- counting in provider could miss bugs
// TODO Revisit this approach
[<RequireQualifiedAccess>]
type Content = 
    | None
    | Text of string
    | Action of obj // physical equality
    | Children of Visual list

and Mutation =
    | SetValue of name: string * value: string
    | SetAction of name: string * fn: (unit -> unit)
    | AddChild of idx: int * child: Visual

and Visual() = // TODO do log created
    member val MutationLog = [] with get, set
    member this.AddLog (x: Mutation) =
        this.MutationLog <- x :: this.MutationLog

    abstract Content : Content
    default _.Content = Content.None

type Text(body: string) =
    inherit Visual()

    let mutable body = body
    member this.Body
        with get() =
            body
        and set x =
            this.AddLog (SetValue ("Body", x) )
            body <- x

    override _.Content = Content.Text body

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
    
    override _.Content = Content.Action action

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

    override _.Content = Content.Children children
