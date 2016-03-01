using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace RequestHandlers.WebApi
{
    public class DynamicHttpControllerTypeResolver : DefaultHttpControllerTypeResolver
    {
        private readonly Assembly[] _dynamicAssemblies;

        public DynamicHttpControllerTypeResolver(params Assembly[] dynamicAssemblies)
        {
            _dynamicAssemblies = dynamicAssemblies;
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            if (assembliesResolver == null)
            {
                throw new ArgumentNullException(nameof(assembliesResolver));
            }

            var result = new List<Type>();
            result.AddRange(base.GetControllerTypes(assembliesResolver));
            var assemblies = assembliesResolver.GetAssemblies();
            foreach (Assembly assembly in assemblies.Where(assembly => (assembly == null || assembly.IsDynamic) && _dynamicAssemblies.Contains(assembly)))
            {
                result.AddRange(assembly.GetTypes().Where(x => !result.Contains(x)).Where(x => IsControllerTypePredicate(x)).ToArray());
            }

            return result;
        }
    }
}