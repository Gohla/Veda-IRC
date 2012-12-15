using System;

namespace Veda.Command
{
    class CommandConverterWithContext<TFrom, TTo, TContext> : CommandConverter<TFrom, TTo>
    {
        private Func<TFrom, TContext, TTo> _converter;

        public override Type ContextType { get { return typeof(TContext); } }

        public CommandConverterWithContext(Func<TFrom, TContext, TTo> converter)
        {
            _converter = converter;
        }

        public override object Convert(object obj, object context)
        {
            if(context == null)
                return _converter((TFrom)obj, default(TContext));
            else
                return _converter((TFrom)obj, (TContext)context);
        }
    }
}
