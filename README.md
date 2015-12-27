# HttpForward

Very simple service that forwards requests received on one HTTP endpoint to another.
One example use of this service is exposing an HTTP endpoint for services hosted on HTTPS only, or viceversa.

## Configuration

Change the appSettings values in app.config to match your scenario:

```xml
<appSettings>
  <add key="listeningPrefix" value="http://+:8848/"/>
  <add key="forwardingAddress" value="https://localhost:4433"/>
  <add key="ignoreSslErrors" value="true" />
  <add key="authorization" value="user:pwd"/>
</appSettings>
```

* __Listening prefix__: The endpoint where the process will listen at
* __Forwarding address__: The base address where requests will be forwarded to
* __Ignore SSL errors__: If true, errors due to SSL certificates on the forwarding address will be ignored
* __Authorization__: Optional. If specified, requests that do not have an Authorization header value will get a Basic auth header with the given user and passowrd values

## Setup

Copy the output files into a folder, then _from an elevated command window_ run:

    httpforward -i

You can then immediately start the service via:

    net start httpforward
    
## Credits

This project uses software libraries and programs from other authors, including:

- **CommandLineParser**: https://github.com/gsscoder/commandline
