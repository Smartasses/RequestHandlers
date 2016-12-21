using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FromBodyAttribute : BinderAttribute
    {
        public FromBodyAttribute() : base(BindingType.FromBody)
        {
        }
    }
}