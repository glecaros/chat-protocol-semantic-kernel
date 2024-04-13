namespace Backend.Interfaces;
public interface ISecretStore
{
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
