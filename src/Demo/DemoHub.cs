namespace Demo
{
    using Microsoft.AspNet.SignalR;

    public class DemoHub : Hub
    {
        public void Say(string message)
        {
            Clients.Caller.reply(string.Format("You said '{0}'", message));
        }
    }
}