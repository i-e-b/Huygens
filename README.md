
**NOTE** Huygen does not support running sites hosted with `Microsoft.Owin`
 
# Huygens
Cut down CassiniDev server for internally hosting IIS sites

https://www.nuget.org/packages/Huygens
https://github.com/i-e-b/Huygens

Derived from https://archive.codeplex.com/?p=cassinidev and released under the same license.

Library to load, host and run ASP.Net websites inside your own process.
Has functionality equivalent to the IIS Developer server.
Can expose IP ports or keep sites internal to your process.
Supports .Net MVC and Web APIs

The library contains a set of interfaces and wrappers under the `Huygens.Compatibility` namespace, to help
convert between the many different HTTP request and response types in the .Net ecosystem.

**NOTE** Huygen does not support running sites hosted with `Microsoft.Owin`

To do:

* [ ] Decode HTTP chunked responses?
* [ ] Add OWIN rq/tx to the compatibility classes
* [ ] Allow DirectCall to use IRequest/IResponse or IContext

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
More than one site can be hosted in a parent process.

## External hosting

You can expose the site to a port like this:

```csharp
using (var server = new SocketServer(32768, "/", @"C:\inetpub\wwwroot\PublishSample"))
{
    server.Start();

    // accessible to web browsers etc.

    server.ShutDown();
}
```

Creating a new `SocketServer` takes a few seconds, but it can handle an unlimited number of calls.
Note that if you want to host more than one `SocketServer` in your process, each must have its own IP address.

## Azure hosting

You can't use the `SocketServer` on Azure, due to permissions limitations. The `DirectServer` is usable as long as you have a **B1** or higher application service plan, and your host app has the *Application Setting* `WEBSITE_LOAD_USER_PROFILE` = `1`
You can also host the DirectServer in a 'Web Job', 'Worker Role' or 'Service Fabric' resource.
