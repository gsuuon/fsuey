
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19044
AMD Ryzen 7 5800H with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=6.0.201
  [Host]   : .NET Core 6.0.4 (CoreCLR 6.0.422.16404, CoreFX 6.0.422.16404), X64 RyuJIT DEBUG
  ShortRun : .NET Core 6.0.4 (CoreCLR 6.0.422.16404, CoreFX 6.0.422.16404), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

      Method |      Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
------------ |----------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
 HandWritten |  94.26 ns | 33.67 ns | 1.845 ns |  1.00 |    0.00 | 0.0372 |     - |     - |     312 B |
  FsuiRender | 334.71 ns | 84.42 ns | 4.628 ns |  3.55 |    0.05 | 0.0706 |     - |     - |     592 B |
