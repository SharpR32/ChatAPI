using System.Reflection;

namespace ChatAPI.Controlers.Common;

public interface IController
{
    static abstract IEndpointRouteBuilder MapRoutes(IEndpointRouteBuilder builder);

}

public static class ControllerExtensions
{
    public static IEndpointRouteBuilder MapSimpleControllersFromAssembly(this IEndpointRouteBuilder routeBuilder, params Type[] typesContainingAssemblies)
    {
        InsertDefaultIfEmpty(ref typesContainingAssemblies);
        IEnumerable<Type> controllerTypes = typesContainingAssemblies.SelectMany(ExtractControllerTypes);

        Type[] argumentTypes = new[] { typeof(IEndpointRouteBuilder) };
        var arguments = new object[] { routeBuilder };

        Type c = typeof(UserController);

        foreach (Type? controllerType in controllerTypes)
        {
            MethodInfo? methodDef = controllerType.GetMethod(nameof(IController.MapRoutes), BindingFlags.Static | BindingFlags.Public, argumentTypes);
            methodDef!.Invoke(null, arguments);
        }

        return routeBuilder;
    }

    private static IEnumerable<Type> ExtractControllerTypes(Type markerType)
    {
        Type controllerType = typeof(IController);
        Assembly assembly = markerType.Assembly;
        return assembly.ExportedTypes.Where(x => x.IsAssignableTo(controllerType) && x != controllerType);
    }

    private static void InsertDefaultIfEmpty(ref Type[] typesContainingAssemblies)
    {
        if (typesContainingAssemblies is { Length: 0 })
        {
            typesContainingAssemblies = new Type[] { typeof(ControllerExtensions) };
        }
    }
}
