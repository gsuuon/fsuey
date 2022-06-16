open Design.UsageSketch

module Program =

    [<EntryPoint>]
    let main _ =
        Component.renderView "world"
        printfn "%A" Component.freshStale
        Component.renderView "toy world"
        printfn "%A" Component.freshStale
        0
