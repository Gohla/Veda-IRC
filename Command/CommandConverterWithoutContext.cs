using System;

namespace Veda.Command
{
    class CommandConverterWithoutContext<TTo> : CommandConverter<TTo>
    {
        private Func<String, TTo> _converter;

        public override Type ToType { get { return typeof(TTo); } }
        public override Type ContextType { get { return typeof(void); } }

        public CommandConverterWithoutContext(Func<String, TTo> converter)
        {
            _converter = converter;
        }

        public override object Convert(String str, object context)
        {
            if(context == null)
                return _converter(str);
            else
                return _converter(str);
        }
    }
}
