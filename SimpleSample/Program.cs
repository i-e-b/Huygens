using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DispatchSharp;
using Huygens;

namespace SimpleSample
{
    class Program
    {
        static void Main()
        {
            using (var subject = new DirectServer(@"C:\Temp\WrappedSites_Disabled\no_appins")) // a published site
            {
                var request = new SerialisableRequest{
                    Method = "GET",
                    RequestUri = "/values",
                    Headers = new Dictionary<string, string>{
                        { "Content-Type","application/json" }
                    },
                    Content = null
                };

                // Make a whole load of calls for profiling
                int i;
                var dispatcher = Dispatch<SerialisableRequest>.CreateDefaultMultithreaded("LoadTest", 4);

                dispatcher.AddConsumer(rq =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    var result = subject.DirectCall(request);
                    if (rq.CommandControl != null)
                    {
                        var resultString = Encoding.UTF8.GetString(result?.Content ?? new byte[0]);
                        Console.Write(rq.CommandControl);
                        Console.WriteLine(resultString);
                    }
                });

                var sw = new Stopwatch();
                sw.Start();
                dispatcher.Start();
                
                for (i = 0; i < 10000; i++)
                {
                    if (i % 100 == 0) {
                        var traceRequest = request.Clone();
                        traceRequest.CommandControl = i.ToString();
                        dispatcher.AddWork(traceRequest);
                    } else dispatcher.AddWork(request);

                }
                dispatcher.WaitForEmptyQueueAndStop();	// only call this if you're not filling the queue from elsewhere


                sw.Stop();
                var rate = i / sw.Elapsed.TotalSeconds;
                Console.WriteLine("calls per second: " + rate);



                Console.WriteLine("[Done]");
                Console.ReadLine();
            }
        }
    }
}
