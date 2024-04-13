using Backend.Interfaces;
using Backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController, Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ISemanticKernelApp _semanticKernelApp;

    public ChatController(ISemanticKernelApp semanticKernelApp)
    {
        _semanticKernelApp = semanticKernelApp;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> ProcessMessage(AIChatRequest request)
    {
        var session = request.SessionState switch {
            Guid sessionId => await _semanticKernelApp.GetSession(sessionId),
            _ => await _semanticKernelApp.CreateSession(Guid.NewGuid())
        };
        var response = await session.ProcessRequest(request);
        return Ok(response);
    }
}
