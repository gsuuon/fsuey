namespace FSUI.Types

open System

/// Compute equals on key, ignoring value
[<CustomEquality; NoComparison>]
type Keyed<'K, 'T when 'K : equality> = Keyed of key: 'K * value: 'T
    with
    interface IEquatable<Keyed<'K, 'T>> with
        member this.Equals other =
            let (Keyed (key, _)) = this
            let (Keyed (key', _)) = other
            key = key'

    override this.Equals other =
        match other with
        | :? Keyed<'K, 'T> as otherKeyed ->
            (this :>IEquatable<_>).Equals otherKeyed
        | _ -> false
        
    override this.GetHashCode () =
        let (Keyed (key, _) ) = this
        key.GetHashCode()
