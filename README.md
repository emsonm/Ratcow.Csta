# Ratcow.Csta
Ratcow.Csta is an implementation of the CSTA protocol (ECMA-323 (XML Protocol for Computer Supported Telecommunications Applications)/ECMA-354 (Application Session Services)) and some Avaya extensions needed to support the Avaya DMCC protocol.

The project consists of multiple elements:

1) Ratcow.Csta.Ecma323 - this is a straight implementation of the XSD for the Ed3 and Ed6 of the standard, plus the public Avaya extensions from the DMCC
2) Ratcow.Csta.Avaya.Dmcc - this is client/server API generation code for the Avaya DMCC 
3) Ratcow.Csta - this holds glue code, including a client side API implementation, and the base eventarg for all Csta events (this differes from the official Avaya implementation, which has lots of different events, making it harder to generically hook all global events.)  TODO - This needs a refactor and a clean up and the code should be more streamlined.
4) Ratcow.Csta.Engine - this holds the client and server network level code. It's still based partly on the Avaya example code and needs to be cleaned up a lot.
5) Ratcow.Csta.Server.Stub - a stub server. This implements enough to bring up a server instance and process (some) basic requests.
6) Ratcow.Csta.Testbed - a testbed client. Not pretty. For quickly testing the API.

# Motivation
* The short term goal is to create a complete replacement for the current Avaya ServiceProvider API. Once the API has been fleshed out, I will add an async/await layer, and will look at potentially implementing ECMA-348 (CSTA over SOAP.)
* The mid term goal is to create an API facade that will enable simple testing without the need for a full AES.
* The log term goal is to create a framework where implementing CSTA over any given PBX is possible. 

# Nuget support
Because we are using a few packages that are coming from the appveyor nuget feeds directly, the feeds are listed here for simplicity.

* https://ci.appveyor.com/nuget/ratcow-serialization-uhq2s66j4a3v
* https://ci.appveyor.com/nuget/ratcow-logs-7yhrdewaqyi0

on a new system run :

_nuget sources add -name Ratcow-serialization-prerelease -source https://ci.appveyor.com/nuget/ratcow-serialization-uhq2s66j4a3v_

_nuget sources add -name Ratcow-logs-prerelease -source https://ci.appveyor.com/nuget/ratcow-logs-7yhrdewaqyi0_
