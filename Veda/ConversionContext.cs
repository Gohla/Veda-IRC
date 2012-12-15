using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda
{
    public class ConversionContext
    {
        public IBot Bot { get; set; }
        public IReceiveMessage Message { get; set; }
    }
}
