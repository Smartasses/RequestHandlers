using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RequestHandlers.Http;

namespace RequestHandlers.Mvc
{
    public class PropertyBinderHelperResult
    {
        public PropertyInfo PropertyInfo { get; set; }
        public BindingType BindingType { get; set; }
        public string ParameterName { get; set; }
    }
    class PropertyBinderHelper
    {
        
        private readonly BindingType _defaultBinder;
        private readonly HttpRequestAttributeParser.Result _parsedRouteResult;
        private readonly List<PropertyBinderHelperResult> _results;

        public PropertyBinderHelper(BindingType defaultBinder, HttpRequestAttributeParser.Result parsedRouteResult)
        {
            _results = new List<PropertyBinderHelperResult>();
            _defaultBinder = defaultBinder;
            _parsedRouteResult = parsedRouteResult;
        }

        public void AddBinder(PropertyInfo propertyInfo)
        {
            var attributeBinderType = propertyInfo.GetCustomAttribute<BinderAttribute>()?.BindingType;
            var binder = GetAutoBinderTypeFromRoute(propertyInfo);
            if(binder != null && attributeBinderType.HasValue && binder.BindingType != attributeBinderType) throw new Exception("Autobinder doesn't match attribute binder.");

            _results.Add(new PropertyBinderHelperResult
            {
                PropertyInfo = propertyInfo,
                BindingType = attributeBinderType ?? binder?.BindingType ?? _defaultBinder,
                ParameterName = binder?.ParameterName
            });
        }

        public IEnumerable<PropertyBinderHelperResult> GetPropertiesAndBinding()
        {
            foreach (var result in _results.Where(x => string.IsNullOrEmpty(x.ParameterName) && x.BindingType != BindingType.FromBody && x.BindingType != BindingType.FromForm))
            {
                var parameterName = ConvertToCamelCase(result.PropertyInfo.Name);
                result.ParameterName = GetUniqueParameterName(parameterName);
            }
            {
                var requestBodyParameterName = GetUniqueParameterName("request");
                foreach (var result in _results.Where(x => x.BindingType == BindingType.FromBody || x.BindingType == BindingType.FromForm))
                {
                    result.ParameterName = requestBodyParameterName;
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
                if (_results.All(x => x.ParameterName != tryName))
                {
                    uniqueParameterName = tryName;
                    iterate = false;
                }
            }
            return uniqueParameterName;
        }

        private PropertyBinderHelperResult GetAutoBinderTypeFromRoute(PropertyInfo propertyInfo)
        {
            var routeNameCorrectCase = _parsedRouteResult.RouteVariable.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCulture));
            var queryStringNameCorrectCase = _parsedRouteResult.QueryStringVariables.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCulture));
            var routeNameIgnoreCase = _parsedRouteResult.RouteVariable.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCultureIgnoreCase));
            var queryStringNameIgnoreCase = _parsedRouteResult.QueryStringVariables.SingleOrDefault(x => x.Equals(propertyInfo.Name, StringComparison.CurrentCultureIgnoreCase));
            
            if (routeNameCorrectCase != null || routeNameIgnoreCase != null)
            {
                return new PropertyBinderHelperResult
                {
                    BindingType = BindingType.FromRoute,
                    ParameterName = routeNameCorrectCase ?? routeNameIgnoreCase
                };
            }
            else if (queryStringNameCorrectCase != null || queryStringNameIgnoreCase != null)
            {
                return new PropertyBinderHelperResult
                {
                    BindingType = BindingType.FromQuery,
                    ParameterName = queryStringNameCorrectCase ?? queryStringNameIgnoreCase
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