namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }

    internal class RequiredMemberAttribute : Attribute { }

    internal class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name) { }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue)
            => ReturnValue = returnValue;

        public bool ReturnValue { get; }
    }
}