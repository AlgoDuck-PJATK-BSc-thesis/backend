namespace WebApplication1.Shared.Exceptions;

public class AlreadyInCohortException : AppException
{
    public AlreadyInCohortException()
        : base("User already belongs to a cohort or created one.", 409) 
    {
    }
}