using System;
using System.Collections.Generic;
using Simple.Owin.SignalR;
using Simple.Owin.Static;

// The standard OWIN middleware/application delegate. Officially not as scary as it used to be.
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string,object>, System.Threading.Tasks.Task>;

namespace Demo
{
    using UseAction = Action<Func<AppFunc,AppFunc>>;
    using MapAction = Action<string, Func<AppFunc,AppFunc>>;

    /// <summary>
    /// This class is magically used by Fix to configure your application
    /// </summary>
    public class OwinAppSetup
    {
        private readonly IDictionary<string, object> _startupEnv;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinAppSetup"/> class.
        /// </summary>
        /// <param name="startupEnv">The startup environment as passed by Fix.</param>
        public OwinAppSetup(IDictionary<string, object> startupEnv)
        {
            _startupEnv = startupEnv;
        }

        /// <summary>
        /// Called by Fix to let you set up your application
        /// </summary>
        /// <param name="use">Function to add middleware or apps to the pipeline.</param>
        /// <param name="map">Function to add middleware or apps only for URLs with a certain prefix.</param>
        /// <remarks>We have to use the <c>map</c> function to restrict SignalR otherwise it goes batshit and complains about protocols in every response.</remarks>
        public void Setup(UseAction use, MapAction map)
        {
            map("/signalr", OwinSignalR.Hub(_startupEnv));
            use(Statics.AddFileAlias("/index.html", "/").AddFolder("/Scripts"));
        }
    }
}