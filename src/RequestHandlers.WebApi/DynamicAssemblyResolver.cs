using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace RequestHandlers.WebApi
{
    public class DynamicAssemblyResolver : DefaultAssembliesResolver
    {
        private readonly Assembly[] _assembliesToLoad;

        public DynamicAssemblyResolver(params Assembly[] assembliesToLoad)
        {
            _assembliesToLoad = assembliesToLoad;
        }

        public override ICollection<Assembly> GetAssemblies()
        {
            var result = _assembliesToLoad.ToList();
            result.AddRange(base.GetAssemblies());
            return result;
        }
    }
}