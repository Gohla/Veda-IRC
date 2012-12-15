using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IConversionContext
    {
        IBot Bot { get; }
        IReceiveMessage Message { get; }
    }
}
