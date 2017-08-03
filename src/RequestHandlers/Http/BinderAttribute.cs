using System;

namespace RequestHandlers.Http
{
    public enum BindingType
    {
        None,
        FromBody,
        FromForm,
        FromQuery,
        FromHeader,
        FromRoute
    }
    public abstract class BinderAttribute : Attribute
    {
        protected BinderAttribute(BindingType bindingType)
        {
            BindingType = bindingType;
        }

        public BindingType BindingType { get; }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class FromBodyAttribute : BinderAttribute
    {
        public FromBodyAttribute() : base(BindingType.FromBody)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class FromFormAttribute : BinderAttribute
    {
        public FromFormAttribute() : base(BindingType.FromForm)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class FromHeaderAttribute : BinderAttribute
    {
        public FromHeaderAttribute() : base(BindingType.FromHeader)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class FromQueryStringAttribute : BinderAttribute
    {
        public FromQueryStringAttribute() : base(BindingType.FromQuery)
        {
        }
    }
}