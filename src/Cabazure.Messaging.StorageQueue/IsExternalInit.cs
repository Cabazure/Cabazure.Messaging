#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class IsExternalInit { }

#pragma warning disable CA1018 // Mark attributes with AttributeUsageAttribute
internal sealed class RequiredMemberAttribute : Attribute { }

internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    public CompilerFeatureRequiredAttribute(string name) { }
}
#pragma warning restore CA1018 // Mark attributes with AttributeUsageAttribute
