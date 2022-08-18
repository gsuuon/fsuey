namespace FSUI.Test.Host

// TODO this is probably unecessary, track in Provider instead
// though this is more accurate -- counting in provider could miss bugs
// TODO Revisit this approach
type Mutation =
    | SetValue of name: string * value: string
    | SetAction of name: string * fn: obj
    | AddChildren of children: Visual list
    | AddClass of name: string
    | RemoveClass of name: string

and Content = 
    | None
    | Text of string
    | Action of obj // physical equality
    | Children of Content list

and Visual() = // TODO do log created
    member val MutationLog = [] with get, set
    member this.AddLog (x: Mutation) = // TODO better append structure
        this.MutationLog <- this.MutationLog @ [x]
    member this.Changes = this.MutationLog.Length

    member val ClassNames : Set<string> = Set.empty with get, set
    member this.AddClass name =
        this.AddLog (AddClass name)
        this.ClassNames <- this.ClassNames.Add name
    member this.RemoveClass name =
        this.AddLog (RemoveClass name)
        this.ClassNames <- this.ClassNames.Remove name

    abstract Content : Content
    default _.Content = Content.None

    abstract ContentMutations : Content * int
    default this.ContentMutations = this.Content, this.MutationLog.Length

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
            this.AddLog (AddChildren [child] )
            child <- x
    
    override _.Content = Content.Action action

type Collection(children: Visual list) =
    inherit Visual()

    let mutable children = children
    member this.Children
        with get() = children
        and set xs = 
            this.AddLog (AddChildren children)
            children <- xs

    override _.Content =
        Content.Children (children |> List.map (fun x -> x.Content))

    member this.Extract fn =
        children |> List.map fn

module Content =
    type ContentAdd<'T> =
        | Item of Content * 'T
        | Items of List<ContentAdd<'T> > * 'T

    let buttonItem getChild x (button: Button) =
        Items
            ([getChild button.Child
              Item (Content.Action button.Action, x) ]
            , x)
        
    let rec getContentAdd (getData: Visual -> 'a) (x: Visual) =
        match x with
        | :? Collection as collection ->
            Items (collection.Children |> List.map (getContentAdd getData), getData collection)
        | :? Button as button ->
            button |> buttonItem (getContentAdd getData) (getData button)
        | _ ->
            Item (x.Content, getData x)

    let getContentAddChanges x = getContentAdd (fun v -> v.Changes) x
    let getContentAddLogs x = getContentAdd (fun v -> v.MutationLog) x
    let getContentAddClasses x = getContentAdd (fun v -> v.ClassNames) x

    let div' x xs  = Items (xs, x)
    let text' x str = Item (Content.Text str, x)
    let button' x (child, handlerObj) =
        Items ([
            child
            Item (Content.Action handlerObj, x)
        ], x)
