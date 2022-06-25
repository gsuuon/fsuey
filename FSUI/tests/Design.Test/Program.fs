open Design.UsageSketch

module Program =
    let testCacheSwap () =
        Component.renderView "world"
        printfn "%A" Component.freshStale
        Component.renderView "toy world"
        printfn "%A" Component.freshStale

    module DerivedStaticGeneric =
        open Scratch.DerivedStaticGeneric

        type Foo() = class end
        type Bar = {
            x : int
        }

        let testDerivedStaticGeneric () =
            Cache.CacheA.Set 0 (Foo())
            Cache.CacheA.Set 1 ({x = 0})
            let swapper = Cache.DefaultSwapper()

            printfn "Foo 0 %A" <| swapper.Retrieve<Foo> 0
            printfn "Foo 1 %A" <| swapper.Retrieve<Foo> 1
            printfn "Bar 0 %A" <| swapper.Retrieve<Bar> 0
            printfn "Bar 1 %A" <| swapper.Retrieve<Bar> 1
            
        type MyCacheA<'T>() = inherit BaseStatic<'T>()
        type MyCacheB<'T>() = inherit BaseStatic<'T>()

        type MySwapper() =
            let isA = false
            member _.Ret

        let testDerivedStaticCustomCache () =
            ()
        

    [<EntryPoint>]
    let main _ =
        testDerivedStaticGeneric()
        0
