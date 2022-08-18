#load "../TestReferences.fsx"
open TestReferences

#load "Flow.fs"
open FSUI.Flow

type Waits = Waits of string
type Returns = Returns of string

open System.Collections

let toList x = x |> Flow.asSeq |> Seq.toList

that "wait values are enumerated"
    <@ flow
        { yield Waits "foo"
          yield Waits "bar" } |> toList
        =
        [ Waits "foo"
          Waits "bar" ]
    @>

that "return values are also enumerated"
    <@ flow
        { yield Waits "foo"
          return Returns "bar" } |> toList
        =
        [ Waits "foo"   :> obj
          Returns "bar" :> obj ]
    @>

that "nested flows are enumerated, return value is bound"
    <@ flow
        { yield Waits "foo"
          let! x =
            flow {
                yield Waits "bar"
                return Returns "quix"
            }
          yield Waits "baz"
          return x
        } |> toList
        =
        [ Waits "foo"    :> obj
          null           // TODO try to get rid of this null
          Waits "bar"    :> obj
          Waits "baz"    :> obj
          Returns "quix" :> obj ]
    @>

exitCode()
