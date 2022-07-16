
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19044
AMD Ryzen 7 5800H with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=6.0.201
  [Host]   : .NET Core 6.0.4 (CoreCLR 6.0.422.16404, CoreFX 6.0.422.16404), X64 RyuJIT DEBUG
  ShortRun : .NET Core 6.0.4 (CoreCLR 6.0.422.16404, CoreFX 6.0.422.16404), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

      Method |     Mean |     Error |   StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
------------ |---------:|----------:|---------:|------:|--------:|-------:|------:|------:|----------:|
 HandWritten | 131.6 ns |  33.03 ns |  1.81 ns |  1.00 |    0.00 | 0.0706 |     - |     - |     592 B |
  FsuiRender | 668.4 ns | 262.72 ns | 14.40 ns |  5.08 |    0.18 | 0.1659 |     - |     - |    1392 B |
