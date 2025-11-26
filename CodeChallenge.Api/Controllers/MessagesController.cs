using Microsoft.AspNetCore.Mvc;
using CodeChallenge.Api.Logic;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/organizations/{organizationId:guid}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageLogic _logic;

    public MessagesController(IMessageLogic logic)
    {
        _logic = logic;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organizationId)
    {
        var result = await _logic.GetAllMessagesAsync(organizationId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid organizationId, Guid id)
    {
        var message = await _logic.GetMessageAsync(organizationId, id);
        return message is null ? NotFound() : Ok(message);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        // 🔹 Validate request
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _logic.CreateMessageAsync(organizationId, request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        // 🔹 Validate request
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _logic.UpdateMessageAsync(organizationId, id, request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid organizationId, Guid id)
    {
        var result = await _logic.DeleteMessageAsync(organizationId, id);
        return result.ToActionResult();
    }
}
