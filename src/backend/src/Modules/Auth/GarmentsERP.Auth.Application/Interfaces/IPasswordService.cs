namespace GarmentsERP.Auth.Application.Interfaces;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
    bool MeetsComplexity(string password);
}
