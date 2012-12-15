using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda
{
    public class ConversionContext : IConversionContext
    {
        public IBot Bot { get; set; }
        public IReceiveMessage Message { get; set; }
    }
}
