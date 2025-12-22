using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.UploadImageExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseById;
using Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenses;
using Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseSummary;
using Training.ExpenseTracker.Application.Interfaces;

namespace WebApplication1.Controllers;



[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    // private readonly IExpenseService _expenseService;
    //
    // public ExpensesController(IExpenseService expenseService)
    // {
    //     _expenseService = expenseService;
    // }

    [HttpPost]
    public async Task<ActionResult<ResponseExpenses>> Create(
        [FromBody] CreateExpenses request,
        [FromServices] CreateExpenseCommandHandler handler,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserIdFromJwt();

        try
        {
            var result = await handler.Handle(new CreateExpenseCommand(userId, request), ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ResponseExpenses>>> GetList(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? category,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] ExpenseSort? sort,
        [FromServices] GetExpensesQueryHandler handler,
        CancellationToken ct = default,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserIdFromJwt();

        var req = new GetExpensesRequest
        {
            From = from,
            To = to,
            Category = category,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            Sort = sort ?? ExpenseSort.SpendDateDesc,
            Page = page,
            PageSize = pageSize
        };

        var result = await handler.Handle(new GetExpensesQuery(userId, req), ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ResponseExpenses>> Update(
        Guid id,
        [FromBody] UpdateExpenses request,
        [FromServices] UpdateExpenseCommandHandler handler,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserIdFromJwt();

        try
        {
            var result = await handler.Handle(new UpdateExpenseCommand(userId, id, request), ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseExpenses>> GetById(
        Guid id,
        [FromServices] GetExpenseByIdQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserIdFromJwt();

        try
        {
            var result = await handler.Handle(new GetExpenseByIdQuery(userId, id), ct);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound(new { message = "Không tìm thấy chi phí !!" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteExpenseCommandHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserIdFromJwt();

        try
        {
            await handler.Handle(new DeleteExpenseCommand(userId, id), ct);
            return Ok(new { message = "Xóa thành công" });
        }
        catch (ArgumentException)
        {
            return NotFound(new { message = "Không tìm thấy chi phí" });
        }
    }


    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryResponse>> Summary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] GetExpenseSummaryQueryHandler handler,
        CancellationToken ct)
    {
        var userId = GetUserIdFromJwt();
        var result = await handler.Handle(new GetExpenseSummaryQuery(userId, from, to), ct);
        return Ok(result);
    }

    private Guid GetUserIdFromJwt()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("Không tìm thấy User Idd");
        return userId;
    }
    
    [HttpPost("{id:guid}/image/upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ResponseExpenses>> UploadImage(
        Guid id,
        IFormFile file,
        [FromServices] AddReceiptImageToExpenseCommandHandler handler,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file." });

        var userId = GetUserIdFromJwt();

        await using var stream = file.OpenReadStream();

        try
        {
            var result = await handler.Handle(
                new AddReceiptImageToExpenseCommand(userId, id, stream, file.FileName),
                ct
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}     