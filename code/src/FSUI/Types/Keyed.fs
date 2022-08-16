namespace FSUI.Types

/// Compute equals on key, ignoring value
[<CustomEquality; NoComparison>]
type Keyed<'K, 'T when 'K : equality> = Keyed of key: 'K * value: 'T
    with
    interface System.IEquatable<Keyed<'K, 'T>> with
        member this.Equals other =
            let (Keyed (key, _)) = this
            let (Keyed (key', _)) = other
            key = key'
