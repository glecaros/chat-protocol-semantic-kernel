using Backend.Model;

namespace Backend.Interfaces;
public interface ISemanticKernelSession
{
    Guid Id { get; }
    Task<AIChatCompletion> ProcessRequest(AIChatRequest message);
}
