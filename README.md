# Huygens
Cut down CassiniDev server for internally hosting IIS sites

Derived from https://archive.codeplex.com/?p=cassinidev and released under the same license.

Library to load, host and run ASP.Net websites inside your own process.
Has functionality equivalent to the IIS Developer server.
Can expose IP ports or keep sites internal to your process.
Supports .Net MVC and Web APIs

## Internal hosting

You can host a site without exposing on an IP port like this:

```csharp
using (var server = new DirectServer(@"C:\inetpub\wwwroot\PublishSample")) // a published site
{
    var request = new SerialisableRequest{
        Method = "GET",
        RequestUri = "/values",
        Headers = new Dictionary˂string, string˃{
            { "Content-Type","application/json" }
        },
        Content = null
    };

    var result = server.DirectCall(request);

    var resultString = Encoding.UTF8.GetString(result.Content);
    Console.WriteLine(resultString);
}
```

Creating a new `DirectServer` takes a few seconds, but it can handle an unlimited number of `DirectCall` requests.

## External hosting

```csharp
using (var server = SocketServer(32768, "/", @"C:\inetpub\wwwroot\PublishSample"))
{
    server.Start();

    // accessible to web browsers etc.

    server.Stop();
}
```