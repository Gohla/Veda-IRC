using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IContext
    {
        IBot Bot { get; set; }
        IStorage Storage { get; set; }
        IReceiveMessage Message { get; set; }
    }
}
