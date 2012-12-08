using System;

namespace Veda.Command
{
    class CommandConverterWithContext<TTo, TContext> : CommandConverter<TTo>
    {
        private Func<String, TContext, TTo> _converter;

        public override Type ToType { get { return typeof(TTo); } }
        public override Type ContextType { get { return typeof(TContext); } }

        public CommandConverterWithContext(Func<String, TContext, TTo> converter)
        {
            _converter = converter;
        }

        public override object Convert(String str, object context)
        {
            if(context == null)
                return _converter(str, default(TContext));
            else
                return _converter(str, (TContext)context);
        }
    }
}
