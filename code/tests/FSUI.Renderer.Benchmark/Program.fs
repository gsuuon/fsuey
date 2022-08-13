module FSUI.Renderer.Benchmark.Program

open FSUI.Renderer.Benchmarks

open BenchmarkDotNet.Configs
open BenchmarkDotNet.Running
open BenchmarkDotNet.Columns
open BenchmarkDotNet.Loggers
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Reports
open BenchmarkDotNet.Exporters

let config =
    ManualConfig
        .CreateEmpty()
        .AddColumnProvider(DefaultColumnProviders.Instance)
        .AddDiagnoser(MemoryDiagnoser.Default)

let printSummary (summary: Summary) =
    MarkdownExporter.Console.ExportToLog(summary, ConsoleLogger.Default)

BenchmarkRunner.Run<SimpleBenchmark>(config) |> printSummary
BenchmarkRunner.Run<MediumBenchmark>(config) |> printSummary
