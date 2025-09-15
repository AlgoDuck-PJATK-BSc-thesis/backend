namespace AlgoDuck.Shared.Exceptions;

public class AlreadyInCohortException : AppException
{
    public AlreadyInCohortException()
        : base("User already belongs to a cohort or created one.", 409) 
    {
    }
}