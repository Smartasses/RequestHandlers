using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RequestHandlers.Http
{
    public class HttpPropertyBinding
    {
        public PropertyInfo PropertyInfo { get; set; }
        public BindingType BindingType { get; set; }
        public string PropertyName { get; set; }
    }
    class HttpRequestPropertyBinderHelper
    {
        
        private readonly BindingType _defaultBinder;
        private readonly HttpRequestAttribute.Result _parsedRouteResult;
        private readonly List<HttpPropertyBinding> _results;

        public HttpRequestPropertyBinderHelper(BindingType defaultBinder, HttpRequestAttribute.Result parsedRouteResult)
        {
            _results = new List<HttpPropertyBinding>();
            _defaultBinder = defaultBinder;
            _parsedRouteResult = parsedRouteResult;
        }

        public void AddBinder(PropertyInfo propertyInfo)
        {
            var attributeBinderType = propertyInfo.GetCustomAttribute<BinderAttribute>()?.BindingType;
            var binder = GetAutoBinderTypeFromRoute(propertyInfo);
            if(binder != null && attributeBinderType.HasValue && binder.BindingType != attributeBinderType) throw new Exception("Autobinder doesn't match attribute binder.");

            _results.Add(new HttpPropertyBinding
            {
                PropertyInfo = propertyInfo,
                BindingType = attributeBinderType ?? binder?.BindingType ?? _defaultBinder,
                PropertyName = binder?.PropertyName
            });
        }

        public IEnumerable<HttpPropertyBinding> GetPropertiesAndBinding()
        {
            foreach (var result in _results.Where(x => string.IsNullOrEmpty(x.PropertyName) && x.BindingType != BindingType.FromBody && x.BindingType != BindingType.FromForm))
            {
                var parameterName = ConvertToCamelCase(result.PropertyInfo.Name);
                result.PropertyName = GetUniqueParameterName(parameterName);
            }
            {
                var requestBodyParameterName = GetUniqueParameterName("request");
                foreach (var result in _results.Where(x => x.BindingType == BindingType.FromBody || x.BindingType == BindingType.FromForm))
                {
                    result.PropertyName = requestBodyParameterName;
                }
            }
            return _results;
        }

        private string GetUniqueParameterName(string parameterName)
        {
            var iterate = true;
            var uniqueParameterName = "";
            for (var duplicates = 0; iterate; duplicates++)
            {
                var tryName = $"{parameterName}{(duplicates > 0 ? duplicates.ToString() : "")}";
                if (_results.All(x => x.PropertyName != tryName))
                {
                    uniqueParameterName = tryName;
                    iterate = false;
                }
            }
            return uniqueParameterName;
        }

        private HttpPropertyBinding GetAutoBinderTypeFromRoute(PropertyInfo propertyInfo)
        {
            var routeNameCorrectCase = _parsedRouteResult.RouteVariable.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCulture));
            var queryStringNameCorrectCase = _parsedRouteResult.QueryStringVariables.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCulture));
            var routeNameIgnoreCase = _parsedRouteResult.RouteVariable.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCultureIgnoreCase));
            var queryStringNameIgnoreCase = _parsedRouteResult.QueryStringVariables.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCultureIgnoreCase));
            
            if (routeNameCorrectCase != null || routeNameIgnoreCase != null)
            {
                return new HttpPropertyBinding
                {
                    BindingType = BindingType.FromRoute,
                    PropertyName = routeNameCorrectCase ?? routeNameIgnoreCase
                };
            }
            else if (queryStringNameCorrectCase != null || queryStringNameIgnoreCase != null)
            {
                return new HttpPropertyBinding
                {
                    BindingType = BindingType.FromQuery,
                    PropertyName = queryStringNameCorrectCase ?? queryStringNameIgnoreCase
                };
            }
            return null;
        }
        private string ConvertToCamelCase(string phrase)
        {
            char[] chars = phrase.ToCharArray();
            if (chars.Length > 0)
            {
                chars[0] = new string(chars[0], 1).ToLower().ToCharArray()[0];
            }
            return new string(chars);
        }
    }
}