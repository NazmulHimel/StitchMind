namespace GarmentsERP.Shared.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException() : base("You do not have permission to perform this action.") { }
    public ForbiddenException(string message) : base(message) { }
}
