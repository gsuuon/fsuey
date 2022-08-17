module FSUI.Renderer.Benchmarks

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes

open FSUI.Types
open FSUI.Test.Provider
open FSUI.Test.Host
open FSUI.Elements.Views
open FSUI.Renderer.Element

let pos = Root
let env = Env()

type Model =
    { name : string
    }

[<ShortRunJob>]
[<MarkdownExporterAttribute.Default>]
type SimpleBenchmark () =
    let model =
        {
            name = "foo"
        }

    let view model =
        div [] [
            text [] $"Hi {model.name}"
        ]

    let viewHand model =
        Collection [
            Text $"Hi {model.name}"
        ]

    [<Benchmark(Baseline=true)>]
    member this.HandWritten () =
        viewHand model

    [<Benchmark>]
    member this.FsuiRender () =
        view model env pos

[<ShortRunJob>]
[<MarkdownExporterAttribute.Default>]
type MediumBenchmark () =
    let model =
        {
            name = "foo"
        }

    let viewFsui model =
        let view model =
            div [] [
                text [] "Title"
                div [] [
                    text [] "Subheader"
                    div [] [ // TODO 
                    ]
                    text [] $"Hi {model.name}"
                ]
                text [] "Footer"
            ]

        view model env pos

    let viewHand model =
        Collection [
            Text "Title"
            Collection [
                Text "Subheader"
                Collection [ // TODO 
                ]
                Text $"Hi {model.name}"
            ]
            Text "Footer"
        ]

    [<Benchmark(Baseline=true)>]
    member this.HandWritten () = viewHand model

    [<Benchmark>]
    member this.FsuiRender () = viewFsui model
