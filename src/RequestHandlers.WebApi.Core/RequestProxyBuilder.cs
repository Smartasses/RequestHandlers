using System;
using System.Reflection;
using System.Reflection.Emit;

namespace RequestHandlers.WebApi.Core
{
    class RequestProxyBuilder
    {
        private const MethodAttributes GetSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        public Type BuildRequestProxy(string name, PropertyInfo[] propertiesToDuplicate, ModuleBuilder moduleBuilder)
        {
            var proxyRequest = moduleBuilder.DefineType(name, TypeAttributes.Public | TypeAttributes.BeforeFieldInit);
            foreach (var property in propertiesToDuplicate)
            {
                CreateProperty(proxyRequest, property);
            }
            return proxyRequest.CreateType();
        }

        private void CreateProperty(TypeBuilder proxyRequest, PropertyInfo property)
        {
            var fieldBuilder = proxyRequest.DefineField("_" + property.Name, property.PropertyType, FieldAttributes.Private);

            var propertyBuilder = proxyRequest.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);

            var getPropertyMethodBuilder = CreateGetPropertyMethod(proxyRequest, property, fieldBuilder);
            var setPropertyMethodBuilder = CreateSetPropertyMethod(proxyRequest, property, fieldBuilder);

            propertyBuilder.SetGetMethod(getPropertyMethodBuilder);
            propertyBuilder.SetSetMethod(setPropertyMethodBuilder);
        }

        private static MethodBuilder CreateGetPropertyMethod(TypeBuilder proxyRequest, PropertyInfo property, FieldBuilder fieldBuilder)
        {
            var getPropertyMethodBuilder = proxyRequest.DefineMethod("get_" + property.Name, GetSetAttr, property.PropertyType,
                Type.EmptyTypes);
            var getPropertyMethodImpl = getPropertyMethodBuilder.GetILGenerator();

            getPropertyMethodImpl.Emit(OpCodes.Ldarg_0);
            getPropertyMethodImpl.Emit(OpCodes.Ldfld, fieldBuilder);
            getPropertyMethodImpl.Emit(OpCodes.Ret);
            return getPropertyMethodBuilder;
        }

        private static MethodBuilder CreateSetPropertyMethod(TypeBuilder proxyRequest, PropertyInfo property, FieldBuilder fieldBuilder)
        {
            var setPropertyMethodBuilder = proxyRequest.DefineMethod("set_" + property.Name, GetSetAttr, null,
                new[] {property.PropertyType});
            var setPropertyMethodImpl = setPropertyMethodBuilder.GetILGenerator();

            setPropertyMethodImpl.Emit(OpCodes.Ldarg_0);
            setPropertyMethodImpl.Emit(OpCodes.Ldarg_1);
            setPropertyMethodImpl.Emit(OpCodes.Stfld, fieldBuilder);
            setPropertyMethodImpl.Emit(OpCodes.Ret);
            return setPropertyMethodBuilder;
        }
    }
}