Simple.Owin.SignalR
===================

This makes SignalR work with the standard `Func&lt;AppFunc, AppFunc&gt;` OWIN signature instead of only working with Katana's `IAppBuilder` implementation.

It still takes a dependency on `Microsoft.Owin.dll` because it needs a bunch of stuff in there to work, but you can use it with [Fix](https://github.com/FixProject/Fix).

Vive la revolution.
