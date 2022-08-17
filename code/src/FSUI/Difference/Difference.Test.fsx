#load "../TestReferences.fsx"
open TestReferences

#load "../Types/Element.fs"
open FSUI.Types

#load "./Difference.fs"
open FSUI.Difference

let (=^) (resultsA: Option<Changes<'T>>) (resultsB: Option<Changes<'T>>) =
    match resultsA, resultsB with
    | None, None -> true
    | Some a, Some b ->
        let createdA = Set a.created
        let createdB = Set b.created

        let removedA = Set a.removed
        let removedB = Set b.removed

        (createdA = createdB) && (removedA = removedB)
    | _ -> false

module Ints =
    let x = difference [ 1; 2 ] [ 0; 1; 3 ]
        
    that "removed and created are computed"
        <@ difference
            [ 1; 2 ]
            [ 0; 1; 3 ]
            =^
            Some {
                removed = [| 2 |]
                created = [| 0; 3 |]
            }
        @>

    that "order does not matter"
        <@ difference
            [ 0; 1; 2; 3]
            [ 3; 2; 1; 0]
            =^
            None
        @>

    that "fewer last ok"
        <@ difference
            [ 0; 1; 2 ]
            [ 0; 1; 2; 3 ]
            =^
            Some {
                removed = [||]
                created = [| 3 |]
            }
        @>

    that "fewer next ok"
        <@ difference
            [ 0; 1; 2; 3 ]
            [ 0; 1; 2 ]
            =^
            Some {
                removed = [| 3 |]
                created = [| |]
            }
        @>

    that "array inputs ok"
        <@ difference
            [| 1; 2 |]
            [| 0; 1; 3 |]
            =^
            Some {
                removed = [| 2 |]
                created = [| 0; 3 |]
            }
        @>

module Cases =
    type Item =
        | Foo of int
        | Bar of string

    that "DU cases can be differenced"
        <@ difference
            [ Foo 0
              Bar "hello"
              Foo 1
              Bar "world"
            ]
            [ Bar "world"
              Foo 1
              Bar "star"
              Foo 2
              Foo 3
            ]
            =^
            Some {
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
