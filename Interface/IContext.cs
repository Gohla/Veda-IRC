using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IContext
    {
        IBot Bot { get; }
        ICommand Command { get; }
        IStorage Storage { get; }
        IReceiveMessage Message { get; }
    }
}
