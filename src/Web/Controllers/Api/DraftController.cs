using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class DraftController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public DraftController(
            ITaskRepository taskRepository,
            IBoardRepository boardRepository,
            IUserRepository userRepository,
            IJwtService jwtService
        )
        {
            _taskRepository = taskRepository;
            _boardRepository = boardRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        [HttpPost("draft"), Authorize]
        [SwaggerOperation("Создать черновик")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateDraftBody draftBody,
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            if (draftBody.StartDate != null && !DateTime.TryParse(draftBody?.StartDate, out var _))
                return BadRequest("Start time format is not correct");

            if (draftBody.EndDate != null && !DateTime.TryParse(draftBody.EndDate, out var _))
                return BadRequest("End time format is not correct");

            var tokenInfo = _jwtService.GetTokenInfo(token);
            var boardMember = await _boardRepository.GetBoardMemberAsync(tokenInfo.UserId, columnId);
            if (boardMember == null)
                return Forbid();

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return NotFound();

            TaskModel? draftOfTask = null;
            if (draftBody.ModifiedTaskId != null)
            {
                draftOfTask = await _taskRepository.GetAsync((Guid)draftBody.ModifiedTaskId, false);
                if (draftOfTask == null)
                    return BadRequest("taskId is not found");
            }

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var result = await _taskRepository.AddAsync(draftBody, column, user, draftOfTask);
            return result == null ? BadRequest() : Ok(result.ToTaskBody());
        }

        [HttpGet("drafts"), Authorize]
        [SwaggerOperation("Получить черновики")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> GetDrafts(
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var boardMember = await _boardRepository.GetBoardMemberAsync(tokenInfo.UserId, boardId);
            if (boardMember == null)
                return Forbid();

            var tasks = await _taskRepository.GetAllDrafts(boardId, tokenInfo.UserId);
            var result = tasks.Select(e => e.ToTaskBody());
            return Ok(result);
        }

        [HttpPut("draft/{draftId}"), Authorize]
        [SwaggerOperation("Преобразовать черновик в задачу")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> ConvertDraftToTask(
            [FromQuery, Required] Guid boardId,
            Guid draftId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var boardMember = await _boardRepository.GetBoardMemberAsync(tokenInfo.UserId, boardId);
            if (boardMember == null)
                return Forbid();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var result = await _taskRepository.ConvertDraftToTask(draftId, user);
            return result == null ? BadRequest() : Ok(result.ToTaskBody());
        }


        [HttpPut("draft"), Authorize]
        [SwaggerOperation("Обновить черновик")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UpdateDraft(
            UpdateDraftBody draftBody,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var boardMember = await _boardRepository.GetBoardMemberAsync(tokenInfo.UserId, boardId);
            if (boardMember == null)
                return Forbid();

            TaskModel? draftOfTask = null;
            if (draftBody.ModifiedTaskId != null)
            {
                draftOfTask = await _taskRepository.GetAsync((Guid)draftBody.ModifiedTaskId, false);
                if (draftOfTask == null)
                    return BadRequest("taskId is not found");
            }

            var result = await _taskRepository.UpdateAsync(draftBody, draftOfTask);
            return result == null ? BadRequest() : Ok(result.ToTaskBody());
        }
    }
}