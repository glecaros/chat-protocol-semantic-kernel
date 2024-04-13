namespace Backend.Interfaces;

public interface ISemanticKernelApp
{
    Task<ISemanticKernelSession> CreateSession(Guid sessionId);
    Task<ISemanticKernelSession> GetSession(Guid sessionId);
}
