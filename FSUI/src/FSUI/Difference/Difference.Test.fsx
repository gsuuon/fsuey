#load "./Difference.fs"
#load "../TestReferences.fsx"
open TestReferences

open FSUI.Difference
open type Compute

let (=^) (resultsA: Difference.Changes<'T>) (resultsB: Difference.Changes<'T>) =
    let createdA = Set resultsA.created
    let createdB = Set resultsB.created

    let removedA = Set resultsA.removed
    let removedB = Set resultsB.removed

    (createdA = createdB) && (removedA = removedB)

module Ints =
    that "removed and created are computed"
        <@ difference
            ( [ 1; 2 ]
            , [ 0; 1; 3 ]
            )
            =^
            { removed = [| 2 |]
              created = [| 0; 3 |]
            }
        @>

    that "order does not matter"
        <@ difference
            ( [ 0; 1; 2; 3]
            , [ 3; 2; 1; 0] )
            =^
            { removed = [| |]
              created = [| |]
            }
        @>

    that "fewer last ok"
        <@ difference
            ( [ 0; 1; 2 ]
            , [ 0; 1; 2; 3 ]
            )
            =^
            { removed = [||]
              created = [| 3 |]
            }
        @>

    that "fewer next ok"
        <@ difference
            ( [ 0; 1; 2; 3 ]
            , [ 0; 1; 2 ]
            )
            =^
            { removed = [| 3 |]
              created = [| |]
            }
        @>

    that "array inputs ok"
        <@ difference
            ( [| 1; 2 |]
            , [| 0; 1; 3 |]
            )
            =^
            { removed = [| 2 |]
              created = [| 0; 3 |]
            }
        @>

module Cases =
    type Item =
        | Foo of int
        | Bar of string

    that "DU cases can be differenced"
        <@ difference
            ( [ Foo 0
                Bar "hello"
                Foo 1
                Bar "world"
              ]
            , [ Bar "world"
                Foo 1
                Bar "star"
                Foo 2
                Foo 3
              ]
            ) =^ {
                created = [| Foo 2; Bar "star"; Foo 3 |]
                removed = [| Foo 0; Bar "hello" |]
            }
        @>

// type Item =
//     | Foo of int
//     | Bar of string
// #time "on"
// for _ = 0 to 100000 do
//     difference
//         ( [ Foo 0
//             Foo 1
//             Foo 5
//             Foo 4
//             Foo 3
//             Bar "b"
//             Bar "d"
//           ]
//         , [ Foo 2
//             Foo 4
//             Bar "a"
//             Bar "b"
//             Bar "c"
//             Bar "d"
//             Foo 7
//             Foo 6
//             Foo 3
//           ]
//         )
//      |> ignore
// #time "off"

exitCode ()
