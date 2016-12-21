using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FromQueryStringAttribute : BinderAttribute
    {
        public FromQueryStringAttribute() : base(BindingType.FromQuery)
        {
        }
    }
}