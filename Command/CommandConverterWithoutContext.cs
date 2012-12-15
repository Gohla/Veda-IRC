using System;

namespace Veda.Command
{
    class CommandConverterWithoutContext<TFrom, TTo> : CommandConverter<TFrom, TTo>
    {
        private Func<TFrom, TTo> _converter;

        public override Type ContextType { get { return typeof(void); } }

        public CommandConverterWithoutContext(Func<TFrom, TTo> converter)
        {
            _converter = converter;
        }

        public override object Convert(object obj, object context)
        {
            return _converter((TFrom)obj);
        }
    }
}
