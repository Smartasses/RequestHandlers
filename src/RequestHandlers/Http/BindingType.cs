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
}