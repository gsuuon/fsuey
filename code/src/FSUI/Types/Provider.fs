namespace FSUI.Types

type IProviderState = 
    abstract Tick : unit -> unit

type IProvider =
    abstract ProviderState : IProviderState
