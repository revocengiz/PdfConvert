## PdfConvert
### Load Test with Apache Benchmark
paralell 20, total 100 request

Start project in release mode
> dotnet run -c release

> cd apache_benchmark

with dll import
> ab -k -n 100 -c 20 http://localhost:5033/pdf/adasko

with process(Because spawnin multiple processes cause cpu peak I made request throttlling bu using
https://www.tpeczek.com/2017/08/implementing-concurrent-requests-limit.html

> ab -k -n 100 -c 20 http://localhost:5033/pdf/process
