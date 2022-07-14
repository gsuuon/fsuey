namespace Design.Scratch

module TypeScratch =
    module InterfaceAlternative =
        (*
        Alternative to using interfaces for decoupling, e.g.
            type IFoo = abstract member DoFoo : unit -> unit
            type Host() = interface IFoo with member _.DoFoo () = ()

        Trade-offs:
            - Basically everything has to be inlined
                - Debugging harder, no stack traces
            - Type inference will work somewhat differently
                - may be hard to mix with other patterns
            - C# Interop likely to be harder or impossible
                - SRTP means implementations probably can only come from F#
        *)

        type Foo = { DoFoo : unit -> unit }
        let inline (|Foo|) x = (^t : (member Foo: Foo) (x))
        let inline getFoo (Foo x) = x
        let inline doFoo (Foo x) = x.DoFoo()

        type Bar = { DoBar : unit -> unit }
        let inline (|Bar|) x = (^t : (member Bar: Bar) (x))
        let inline getBar (Bar x) = x
        let inline doBar (Bar x) = x.DoBar()

        type HostA = { Foo : Foo }
        type HostB =
            { Foo : Foo
              Bar : Bar
            }

        module AnnotatedHostType =
            let doFooWithHostA (host: HostA) =
                doFoo host

            let doFooWithHostB (host: HostB) =
                doFoo host

            let doBarWithHostB (host: HostB) =
                doBar host

            let doFooBarWithHostB (host: HostB) =
                doFoo host
                doBar host

            let getWithHostA (host: HostB) =
                let x = getFoo host
                x.DoFoo()

                let y = getBar host
                y.DoBar()

        module InferHostType =
            let inline doFooWithHost host =
                doFoo host

            let inline doFooBarWithHost host =  
                doFoo host
                doBar host

            let withHostA (host: HostA) =
                doFooWithHost host
                // doFooBarWithHost host
                    // Errors as desired
                    // 1. The type 'HostA' does not support the operator 'get_Bar'

module TypeScratch2 =
    module OpenStaticGeneric =
        type IFoo =
            abstract member Do : int -> Unit

        type MyThing<'A>() =
            static member foo () = Unchecked.defaultof<'A>
            interface IFoo with
                member _.Do x = ()

        open type MyThing<int> // whaaaat

        let x = foo()

    module OpenStaticGenericSrtpInline =
        type FooRecord =
            { Foo : unit -> unit
              Baz : string
            }
        type FooClass() = member _.Foo () = ()
        type BarClass() = member _.Bar () = ()
            
        type MyThing< ^T when 'T : (member Foo : unit -> unit)>() =
            static member inline foo () = Unchecked.defaultof<'T>

        module FooRecord =
            // open type MyThing<FooRecord>
                // error| The type 'FooRecord' does not support the operator 'Foo'

            module NeedToParenthesisRecordFieldFunction =
                // let inline doFoo x = ( ^t : (member Foo : unit -> unit) (x))
                // let withFooRecord (foo: FooRecord) = doFoo foo
                    /// error| The type 'FooRecord' does not support the operator 'Foo'

                let inline doFoo x = ( ^t : (member Foo : (unit -> unit)) (x))
                                                       // ^ parenthesis
                                                       // (unit -> unit) instead of unit -> unit
                let withFooRecord (foo: FooRecord) = doFoo foo

            let inline getBaz x =
                (^t : (member Baz : string) x)

            let withFooRecord (foo: FooRecord) =
                getBaz foo

            ()
        
        module FooClass =
            open type MyThing<FooClass>

            let x = foo() // FooClass

        module BarClass =
            // open type MyThing<BarClass>
            // The type 'BarClass' does not support the operator 'Foo'

            // let x = foo()
            // The value or constructor 'foo' is not defined. Maybe you want one of the following: floor FooClass

            ()

    module OpenStaticGenericSrtp =
        type MyThing< ^T, 'a
                        when 'T : (static member Foo : unit -> 'a)
                    >() =
            static member inline doFoo () =
                (^T : (static member Foo : unit -> 'a) ())

        module Primitives =
            type FooDoer() =
                static member Foo () = "FooDooer did foo"

            type FooDoer2() =
                static member Foo () = 1

            open type MyThing<FooDoer2, int>

            let x = doFoo

        module InterfaceCast =
            // I think this is a dead end given no co/contra variance
            // type GenericModule< ^T>() =
            //     static member inline MakeFoo< ^a when 'a :> 'T> (x: 'a) : 'T = x :> 'T
                    // can't do this

            type IBar = interface end
            type Bar() = interface IBar

            type BarDoer() =
                static member Foo () : IBar = Bar()

            type Caster() =
                static member inline Cast (x: #IBar) = x :> IBar
                // No idea how to use this
                // Since I need to specify this signature at the call site for SRTP
                // but I can't write this signature with generic type instead of IBar

            open type MyThing<BarDoer, IBar>

    module CovarianceViaFlexibleTypes =
        // Nope. Can still only cast the outer type
        // Inner type will require explicit type dependent casting
        // It makes sense, without annotations it's not possible for the current compiler
        // to deduce whether it's meant to be a covariant or contravariant generic
        type IFoo = interface end
        type Foo() = interface IFoo

        type Thing<'T>() = class end
        type ThingChild<'T>() = inherit Thing<'T>()

        let withThing (x: #Thing<'t>) = x
        let x = withThing (ThingChild<int>())

        let withIFoo (x: #Thing<IFoo>) = x
        // let y = withIFoo (Thing<Foo>())

        let z1 = Thing<Foo>()
        let z2 = ThingChild<Foo>()

        // let z1' = z1 :> Thing<IFoo>
            // Broken
        // let z2' = z2 :> Thing<Foo>
            // Works
                // I can cast the outer wrapper type, but I cannot automatically cast the inner type

module Cache =
    [<AbstractClass>]
    type Text(getCache) =
        abstract Create : unit -> unit
        member _.Render (position: int) =
            match getCache position with
            | Some x -> ()
            | None -> ()

    type IText =
        abstract member Text : Text

    type ISwappable =
        abstract Swap : unit -> unit

    type Swappable<'K,'V>() =
        let cacheA = Map.empty
        let cacheB = Map.empty
        let mutable isA = true
        member this.Cache = if isA then cacheA else cacheB
        member this.Find x = Map.find x this.Cache
        interface ISwappable with
            member _.Swap () = isA <- not isA

    type BaseProvider() =
        let mutable collection = []

        member _.MkCache =
            let swap = Swappable()
            collection <- (swap :> ISwappable) :: collection
            swap.Find

        member _.Swap () = collection |> List.iter (fun swappable -> swappable.Swap())

    type Provider() =
        inherit BaseProvider()

        interface IText with
            member val Text =
                { new Text(base.MkCache) with
                    member _.Create () = ()
                }

    let text (x: #IText) =
        x.Text.Render 0

    let renderer view (env: #BaseProvider) =
        fun () ->
            view env
            env.Swap()

    let env = Provider()
    let render = renderer text env


// module Cache =
//     type Element(cache) = class end

//     type Cache(ctrl) = class end

//     type Provider() =
//         let cache = cache()

//         interface ISwapCache with =
//             member val swap = cache.swap

//         member val Text =
//             { new Element(Cache(cache.control)) }

//     type BaseProvider()
//         interface ISwap with
//             member val Cache = Cache()

//     type Provider() =
//         inherit BaseProvider()
//         let x = this.cache

//         member val Text =


//     let renderer view =
//         let dispatch x = ()
//         let pos = root

//         fun model (env: #ISwapCache) ->
//             view model dispatch env root
//             env.swap()

//     module Layout =
//         let view model =
//             div []

//     module App =
//         let view = swapCache Layout.view
//         let model = {}
//         let env = Provider(Cache())

//         view model

