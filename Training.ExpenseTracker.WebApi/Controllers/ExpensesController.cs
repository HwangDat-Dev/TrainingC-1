using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;

namespace WebApplication1.Controllers;


[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    public async Task<ActionResult<ResponseExpenses>> Create([FromBody] CreateExpenses request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetUserIdFromJwt();

        try
        {
            var result = await _expenseService.CreateAsync(userId, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
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

        var result = await _expenseService.GetListAsync(userId, req, ct);
        return Ok(result);
    }
    
    
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ResponseExpenses>> Update(
        Guid id,
        [FromBody] UpdateExpenses request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetUserIdFromJwt();

        var result = await _expenseService.UpdateAsync(userId, id, request, ct);
        if (result is null)
            return NotFound(new { message = "Không tìm thấy chi phí !!" });

        return Ok(result);
    }

    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseExpenses>> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserIdFromJwt();

        var result = await _expenseService.GetByIdAsync(userId, id, ct);
        if (result is null)
            return NotFound(new { message = "Không tìm thấy chi phí !!" });

        return Ok(result);
    }

    private Guid GetUserIdFromJwt()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("Không tìm thấy User Idd");
        return userId;
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserIdFromJwt();

        var ok = await _expenseService.DeleteAsync(userId, id, ct);
        if (!ok)
            return NotFound(new { message = "Không tìm thấy chi phí" });

        return Ok(new
        {
            message = "Xóa thành công"
        });
    }
    
    
    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryResponse>> Summary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var userId = GetUserIdFromJwt();

        try
        {
            var result = await _expenseService.GetSummaryAsync(userId, from, to, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? category,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] ExpenseSort? sort,
        CancellationToken ct)
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
            Page = 1,
            PageSize = 1000000 
        };

        var (content, fileName) = await _expenseService.ExportCSV(userId, req, ct);

        return File(
            fileContents: content,
            contentType: "text/csv; charset=utf-8",
            fileDownloadName: fileName
        );
    }
}       