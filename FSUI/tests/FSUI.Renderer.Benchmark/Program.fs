module FSUI.Renderer.Benchmark.Program

open FSUI.Renderer.Benchmarks
open BenchmarkDotNet.Running

BenchmarkRunner.Run<RendererBenchmark>() |> ignore
