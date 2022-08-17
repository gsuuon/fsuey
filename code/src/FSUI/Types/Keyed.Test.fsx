#load "../TestReferences.fsx"
open TestReferences

#load "./Keyed.fs"
open FSUI.Types

let (==) (x: #System.IEquatable<'T>) (y: 'T) =
    x.Equals y
    
that "keyed with different keys are different"
    <@ Keyed (0, 0 ) <> Keyed (1, 0) @>

that "keyed with different keys but same values are different"
    <@ Keyed (0, 0) <> Keyed (1, 0) @>

that "keyed with same keys but different values are equatable equal"
    <@ Keyed (0, 0) == Keyed (0, 1) @>

that "keyed with same keys but different values are operator equal"
    <@ Keyed (0, 0) = Keyed (0, 1) @>

exitCode()
