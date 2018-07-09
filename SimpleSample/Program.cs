﻿using System;
using System.Collections.Generic;
using System.Text;
using Huygens;

namespace SimpleSample
{
    class Program
    {
        static void Main()
        {
            using (var subject = new DirectServer(@"C:\Temp\WrappedSites\1_rolling")) // a published site
            {
                var request = new SerialisableRequest{
                    Method = "GET",
                    RequestUri = "/values",
                    Headers = new Dictionary<string, string>{
                        { "Content-Type","application/json" }
                    },
                    Content = null
                };

                // This call fails if the exe is named "w3wp.exe", but not otherwise.
                // There is some janky testing happening in the .Net code I need to figure out.
                var result = subject.DirectCall(request);


                // note points -- System.Web.Hosting.ApplicationManager
                //                                  .RecycleLimitMonitor                                <-- maybe a replacable static here?
                //                                  .AspNetMemoryMonitor :: s_processPrivateBytesLimit  <-- can I fake this?
                //                System.Web.Hosting.UnsafeIISMethods.MgdGetSiteNameFromId              <-- this is where is blows up (unmanaged crap)

                // maybe _applicationManager._theAppManager can be faked out?

                var resultString = Encoding.UTF8.GetString(result.Content);
                Console.WriteLine(resultString);

                Console.WriteLine("[Done]");
                Console.ReadLine();
            }
        }
    }
}