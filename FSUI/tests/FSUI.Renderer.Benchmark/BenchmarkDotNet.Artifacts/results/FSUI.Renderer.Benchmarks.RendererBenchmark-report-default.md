
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19044
AMD Ryzen 7 5800H with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=6.0.201
  [Host]   : .NET Core 6.0.4 (CoreCLR 6.0.422.16404, CoreFX 6.0.422.16404), X64 RyuJIT DEBUG
  ShortRun : .NET Core 6.0.4 (CoreCLR 6.0.422.16404, CoreFX 6.0.422.16404), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

      Method |      Mean |    Error |   StdDev | Ratio | RatioSD |
------------ |----------:|---------:|---------:|------:|--------:|
 HandWritten |  94.12 ns | 14.06 ns | 0.771 ns |  1.00 |    0.00 |
  FsuiRender | 321.69 ns | 27.08 ns | 1.484 ns |  3.42 |    0.04 |
