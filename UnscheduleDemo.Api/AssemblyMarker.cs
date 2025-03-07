using System.Reflection;

namespace BusPlayground.Api;

public static class AssemblyMarker
{
    public static Assembly Assembly => typeof(AssemblyMarker).Assembly;
}