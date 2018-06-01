using System;
using System.Collections.Generic;
using System.Text;
using Huygens;
using NUnit.Framework;

namespace HuygensTests
{
    [TestFixture]
    public class DirectConnectionTests
    {
        [Test]
        public void basic_test () {
            using (var subject = new DirectServer(@"C:\Temp\WrappedSites\PublishSample")) // a published site
            {
                var request = new SerialisableRequest{
                    Method = "GET",
                    RequestUri = "/values",
                    Headers = new Dictionary<string, string>{
                        { "Content-Type","application/json" }
                    },
                    Content = null
                };
                var result = subject.DirectCall(request);


                Assert.That(result, Is.Not.Null);
                var resultString = Encoding.UTF8.GetString(result.Content);
                Assert.That(result.StatusCode, Is.EqualTo(200), "Unexpected code:" + result.StatusCode + ". Body = " + resultString);
                Console.WriteLine(resultString);
            }
        }

        [Test]
        public void a_server_can_accept_repeated_calls () {
            using (var subject = new DirectServer(@"C:\Temp\WrappedSites\PublishSample")) // a published site
            {
                var request = new SerialisableRequest{
                    Method = "GET",
                    RequestUri = "/values",
                    Headers = new Dictionary<string, string>{ { "Content-Type","application/json" } },
                    Content = null
                };

                // The big start-up overhead is in `new DirectServer`. Each `DirectCall` is pretty quick
                var result1 = subject.DirectCall(request);
                var result2 = subject.DirectCall(request);
                var result3 = subject.DirectCall(request);
                var result4 = subject.DirectCall(request);
                var result5 = subject.DirectCall(request);
                var result6 = subject.DirectCall(request);


                Assert.That(result1, Is.Not.Null);
                var resultString = Encoding.UTF8.GetString(result6.Content);
                Assert.That(result6.StatusCode, Is.EqualTo(200), "Unexpected code:" + result6.StatusCode + ". Body = " + resultString);
                Console.WriteLine(resultString);
            }
        }
    }
}
