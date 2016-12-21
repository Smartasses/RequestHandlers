using System;

namespace RequestHandlers.Http
{
    public abstract class BinderAttribute : Attribute
    {
        protected BinderAttribute(BindingType bindingType)
        {
            BindingType = bindingType;
        }

        public BindingType BindingType { get; }
    }
}