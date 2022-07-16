module FSUI.Renderer.Benchmarks

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes

open FSUI.Test.Provider
open FSUI.Test.Host
open FSUI.Elements.Views
open FSUI.Renderer.Element

let pos = Root
let env = Env()

type Model =
    { name : string
      pic : string
    }

[<ShortRunJob>]
[<MarkdownExporterAttribute.Default>]
type SimpleBenchmark () =
    let model =
        {
            name = "foo"
            pic = "foo.webp"
        }

    let view model =
        div [] [
            text [] $"Hi {model.name}"
            image [] model.pic
        ]

    let viewHand model =
        VisualCollection [
            VisualText $"Hi {model.name}"
            VisualImage model.pic
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
            pic = "foo.webp"
        }

    let viewFsui model =
        let view model =
            div [] [
                text [] "Title"
                div [] [
                    text [] "Subheader"
                    div [] [
                        image [] model.pic
                    ]
                    text [] $"Hi {model.name}"
                ]
                text [] "Footer"
            ]

        view model env pos

    let viewHand model =
        VisualCollection [
            VisualText "Title"
            VisualCollection [
                VisualText "Subheader"
                VisualCollection [
                    VisualImage model.pic
                ]
                VisualText $"Hi {model.name}"
            ]
            VisualText "Footer"
        ]

    [<Benchmark(Baseline=true)>]
    member this.HandWritten () = viewHand model

    [<Benchmark>]
    member this.FsuiRender () = viewFsui model
