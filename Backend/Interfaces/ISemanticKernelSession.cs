using Backend.Model;

namespace Backend.Interfaces;
public interface ISemanticKernelSession
{
    Task<AIChatCompletion> ProcessRequest(AIChatRequest message);
}
