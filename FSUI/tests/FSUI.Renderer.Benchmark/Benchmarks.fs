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

let view model =
    div [
        text [] $"Hi {model.name}"
        image [] model.pic
    ] []

let viewHand model =
    VisualCollection [
        VisualText $"Hi {model.name}"
        VisualImage model.pic
    ]

let model =
    {
        name = "foo"
        pic = "foo.webp"
    }

[<ShortRunJob>]
[<PlainExporter>]
type RendererBenchmark () =
    [<Benchmark(Baseline=true)>]
    member this.HandWritten () =
        viewHand model

    [<Benchmark>]
    member this.FsuiRender () =
        view model env pos
