using System.Reflection.Metadata;

namespace DataStarTester;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RegisterEndpointAttribute : Attribute
{
}
