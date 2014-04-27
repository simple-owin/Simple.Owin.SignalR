using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Owin.Middleware;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.Owin;

namespace Simple.Owin.SignalR
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class OwinSignalR
    {
        public static Func<AppFunc, AppFunc> Hub()
        {
            return Hub(new Dictionary<string, object>());
        }

        public static Func<AppFunc, AppFunc> Hub(IDictionary<string, object> startupEnv)
        {
            return Hub(new HubConfiguration(), startupEnv);
        }

        public static Func<AppFunc, AppFunc> Hub(HubConfiguration configuration,
            IDictionary<string, object> startupEnv)
        {
            return next => BuildHubFunc(configuration, startupEnv, next);
        }

        /// <summary>
        /// Method mostly copied from Microsoft.AspNet.SignalR OwinExtensions class, but without external dependency on Katana.
        /// </summary>
        /// <param name="configuration">The <see cref="HubConfiguration"/> to use.</param>
        /// <param name="startupEnv">Startup parameters.</param>
        /// <param name="next">Next <see cref="AppFunc"/> in pipeline.</param>
        /// <returns><see cref="AppFunc"/> ready for use.</returns>
        public static Func<IDictionary<string, object>, Task> BuildHubFunc(HubConfiguration configuration, IDictionary<string, object> startupEnv, AppFunc next)
        {
            if (configuration == null)
            {
                throw new ArgumentException("No configuration provided");
            }

            var resolver = configuration.Resolver;

            if (resolver == null)
            {
                throw new ArgumentException("No dependency resolver provider");
            }

            var token = startupEnv.GetValueOrDefault("owin.CallCancelled", CancellationToken.None);

            string instanceName = startupEnv.GetValueOrDefault("host.AppName", Guid.NewGuid().ToString());

            var protectedData = new DefaultProtectedData();

            resolver.Register(typeof(IProtectedData), () => protectedData);

            // If the host provides trace output then add a default trace listener
            var traceOutput = startupEnv.GetValueOrDefault("host.TraceOutput", (TextWriter)null);
            if (traceOutput != null)
            {
                var hostTraceListener = new TextWriterTraceListener(traceOutput);
                var traceManager = new TraceManager(hostTraceListener);
                resolver.Register(typeof(ITraceManager), () => traceManager);
            }

            // Try to get the list of reference assemblies from the host
            IEnumerable<Assembly> referenceAssemblies = startupEnv.GetValueOrDefault("host.ReferencedAssemblies",
                (IEnumerable<Assembly>)null);
            if (referenceAssemblies != null)
            {
                // Use this list as the assembly locator
                var assemblyLocator = new EnumerableOfAssemblyLocator(referenceAssemblies);
                resolver.Register(typeof(IAssemblyLocator), () => assemblyLocator);
            }

            resolver.InitializeHost(instanceName, token);

            var hub = new HubDispatcherMiddleware(new KatanaShim(next), configuration);

            return async env =>
            {
                await hub.Invoke(new OwinContext(env));
                if (!env.ContainsKey("owin.ResponseStatusCode"))
                {
                    env["owin.ResponseStatusCode"] = 200;
                }
            };
        }

        class KatanaShim : OwinMiddleware
        {
            private readonly Func<IDictionary<string, object>, Task> _appFunc;

            public KatanaShim(Func<IDictionary<string, object>, Task> appFunc)
                : base(null)
            {
                _appFunc = appFunc;
            }

            public override Task Invoke(IOwinContext context)
            {
                return _appFunc(context.Environment);
            }
        }

    }
    static class DictionaryExtensions
    {
        public static T GetValueOrDefault<T>(this IDictionary<string, object> dict, string key,
            T defaultValue = default(T))
        {
            object value;
            if (!dict.TryGetValue(key, out value)) return defaultValue;
            return value != null ? (T)value : defaultValue;
        }
    }

}
