namespace AlgoDuck.Shared.Exceptions;

public class FeatureDisabledException : AppException
{
    public FeatureDisabledException(string message = "This feature is currently disabled.")
        : base(message, 503) { }
}