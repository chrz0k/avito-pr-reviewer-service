using Microsoft.AspNetCore.Mvc;
using PrReviewerService.Models;
using PrReviewerService.Services;

namespace PrReviewerService.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    private readonly PrService _service;

    public ApiController(PrService service)
    {
        _service = service;
    }

    private IActionResult BuildError<T>(ServiceResult<T> result)
    {
        return StatusCode(result.StatusCode, new
        {
            error = new
            {
                code = result.ErrorCode,
                message = result.ErrorMessage
            }
        });
    }

    [HttpPost("team/add")]
    public IActionResult AddTeam([FromBody] Team? team)
    {
        var res = _service.AddTeam(team);
        if (!res.IsSuccess) return BuildError(res);
        return StatusCode(201, new { team = res.Data });
    }

    [HttpGet("team/get")]
    public IActionResult GetTeam([FromQuery(Name = "team_name")] string? teamName)
    {
        var res = _service.GetTeam(teamName);
        if (!res.IsSuccess) return BuildError(res);
        return Ok(res.Data);
    }

    [HttpPost("users/setIsActive")]
    public IActionResult SetActive([FromBody] SetActiveRequest? request)
    {
        var res = _service.SetUserActive(request);
        if (!res.IsSuccess) return BuildError(res);
        return Ok(new { user = res.Data });
    }

    [HttpGet("users/getReview")]
    public IActionResult GetReview([FromQuery(Name = "user_id")] string? userId)
    {
        var res = _service.GetUserReviews(userId);
        if (!res.IsSuccess) return BuildError(res);
        return Ok(new { user_id = userId, pull_requests = res.Data });
    }

    [HttpPost("pullRequest/create")]
    public IActionResult CreatePr([FromBody] CreatePrRequest? request)
    {
        var res = _service.CreatePullRequest(request);
        if (!res.IsSuccess) return BuildError(res);
        return StatusCode(201, new { pr = res.Data });
    }

    [HttpPost("pullRequest/merge")]
    public IActionResult MergePr([FromBody] MergePrRequest? request)
    {
        var res = _service.MergePullRequest(request);
        if (!res.IsSuccess) return BuildError(res);
        return Ok(new { pr = res.Data });
    }

    [HttpPost("pullRequest/reassign")]
    public IActionResult ReassignPr([FromBody] ReassignRequest? request)
    {
        var res = _service.ReassignReviewer(request);
        if (!res.IsSuccess) return BuildError(res);
        return Ok(new
        {
            pr = res.Data.Pr,
            replaced_by = res.Data.ReplacedBy
        });
    }
}
