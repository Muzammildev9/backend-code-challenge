using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
     private readonly IMessageLogic _logic;

    public MessagesController(IMessageLogic logic)
    {
        _logic = logic;
    }

    // -------------------------------------------------------
    // GET ALL
    // -------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetAllMessages(Guid organizationId)
    {
        var messages = await _logic.GetAllMessagesAsync(organizationId);
        return Ok(messages);
    }

    // -------------------------------------------------------
    // GET BY ID
    // -------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMessageById(Guid organizationId, Guid id)
    {
        var message = await _logic.GetMessageAsync(organizationId, id);

        if (message == null)
            return NotFound("Message not found.");

        return Ok(message);
    }

    // -------------------------------------------------------
    // CREATE
    // -------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> CreateMessage(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        var result = await _logic.CreateMessageAsync(organizationId, request);

        return result switch
        {
            Created<Message> created =>
                CreatedAtAction(nameof(GetMessageById),
                    new { organizationId, id = created.Value.Id },
                    created.Value),

            ValidationError ve => BadRequest(ve.Errors),
            Conflict c => Conflict(c.Message),
            _ => StatusCode(500, "Unexpected error")
        };
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMessage(Guid organizationId, Guid id,
        [FromBody] UpdateMessageRequest request)
    {
        var result = await _logic.UpdateMessageAsync(organizationId, id, request);

        return result switch
        {
            Updated => NoContent(),
            ValidationError ve => BadRequest(ve.Errors),
            NotFound nf => NotFound(nf.Message),
            Conflict c => Conflict(c.Message),
            _ => StatusCode(500, "Unexpected error")
        };
    }

    // -------------------------------------------------------
    // DELETE
    // -------------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(Guid organizationId, Guid id)
    {
        var result = await _logic.DeleteMessageAsync(organizationId, id);

        return result switch
        {
            Deleted => NoContent(),
            NotFound nf => NotFound(nf.Message),
            Conflict c => Conflict(c.Message),
            _ => StatusCode(500, "Unexpected error")
        };
    }
}

