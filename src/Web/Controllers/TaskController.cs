using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDeletedTaskRepository _deletedTaskRepository;
        private readonly IJwtService _jwtService;

        public TaskController(
            ITaskRepository taskRepository,
            IBoardRepository boardRepository,
            IUserRepository userRepository,
            IDeletedTaskRepository deletedTaskRepository,
            IJwtService jwtService
        )
        {
            _taskRepository = taskRepository;
            _boardRepository = boardRepository;
            _userRepository = userRepository;
            _deletedTaskRepository = deletedTaskRepository;
            _jwtService = jwtService;
        }

        [HttpPost("task"), Authorize]
        [SwaggerOperation("Создать задачу")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateTask(
            [FromBody] CreateTaskBody taskBody,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var board = await _boardRepository.GetAsync(boardId);
            if (board == null)
                return NotFound();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var result = await _taskRepository.AddAsync(taskBody, board, user);
            return result == null ? BadRequest() : Ok(result.ToTaskBody());
        }

        [HttpDelete("task")]
        [SwaggerOperation("Удалить задачу")]
        [SwaggerResponse(200, Type = typeof(DeletedTaskBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> RemoveTask(
            [Required] Guid taskId,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var task = await _taskRepository.GetAsync(taskId, false);
            if (task == null)
                return BadRequest("task id isn't exist");

            var deletedTask = await _deletedTaskRepository.AddAsync(task);
            return deletedTask == null ? Conflict() : Ok(deletedTask.ToDeletedTaskBody());
        }

        [HttpPatch("task")]
        [SwaggerOperation("Восстановить удаленную задачу")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> RemoveDraft(
            [Required] Guid deletedTaskId,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var result = await _deletedTaskRepository.RemoveAsync(deletedTaskId);
            return result != false ? NoContent() : BadRequest();
        }

        [HttpGet("deleted-tasks")]
        [SwaggerOperation("Получить все удаленные задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<DeletedTaskBody>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> GetDrafts(
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var drafts = await _deletedTaskRepository.GetAll();
            var result = drafts.Select(e => e.ToDeletedTaskBody());
            return Ok(result);
        }



        [HttpGet("tasks"), Authorize]
        [SwaggerOperation("Получить задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<TaskBody>))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> GetTasks(
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] TaskState? status = null
        )
        {
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var tasks = status == null
                ? await _taskRepository.GetAll(boardId)
                : await _taskRepository.GetAll(boardId, status.Value);

            var result = tasks.Select(e => e.ToTaskBody());
            return Ok(result);
        }


        [HttpPut("task"), Authorize]
        [SwaggerOperation("Обновить задачу")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UpdateTask(
            UpdateTaskBody taskBody,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var result = await _taskRepository.UpdateAsync(taskBody);
            return result == null ? BadRequest() : Ok(result.ToTaskBody());
        }

        private async Task<IActionResult> AuthorizeCheck(Guid boardId, string token)
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var boardMember = await _boardRepository.GetBoardMemberAsync(tokenInfo.UserId, boardId);
            if (boardMember == null)
                return Forbid();

            return Ok();
        }
    }
}