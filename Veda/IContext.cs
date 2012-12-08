using ReactiveIRC.Interface;

namespace Veda
{
    public interface IContext
    {
        IBot Bot { get; set; }
        IClientConnection Connection { get; set; }
        IReceiveMessage Message { get; set; }
    }
}
