using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FromHeaderAttribute : BinderAttribute
    {
        public FromHeaderAttribute() : base(BindingType.FromHeader)
        {
        }
    }
}