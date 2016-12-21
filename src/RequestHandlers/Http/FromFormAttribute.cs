using System;

namespace RequestHandlers.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FromFormAttribute : BinderAttribute
    {
        public FromFormAttribute() : base(BindingType.FromForm)
        {
        }
    }
}