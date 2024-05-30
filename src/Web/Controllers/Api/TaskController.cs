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
        private readonly IChatRepository _chatRepository;
        private readonly IJwtService _jwtService;

        public TaskController(
            ITaskRepository taskRepository,
            IBoardRepository boardRepository,
            IUserRepository userRepository,
            IChatRepository chatRepository,
            IDeletedTaskRepository deletedTaskRepository,
            IJwtService jwtService
        )
        {
            _taskRepository = taskRepository;
            _boardRepository = boardRepository;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
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
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return NotFound();

            var messages = await _chatRepository.GetMessages(taskBody.MessageIds);

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            var result = await _taskRepository.AddAsync(taskBody, column, user, messages);
            if (result == null)
                return BadRequest();

            var chat = await _chatRepository.GetByTaskIdAsync(result.Id);
            var sessions = await _userRepository.GetUserSessionsAsync(user.Id);
            var chatMembership = await _chatRepository.CreateOrGetChatMembershipAsync(chat, user);
            await _chatRepository.CreateUserChatSessionAsync(sessions, chatMembership, DateTime.UtcNow);

            return Ok(result.ToTaskBody());
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
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] TaskState? status = null
        )
        {
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var tasks = status == null
                ? await _taskRepository.GetAll(columnId)
                : await _taskRepository.GetAll(columnId, status.Value);

            var result = tasks.Select(e => e.ToTaskBody());
            return Ok(result);
        }

        [HttpGet("task/performers"), Authorize]
        [SwaggerOperation("Получить список исполнителей задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ProfileBody>))]

        public async Task<IActionResult> GetTaskPerformers(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid taskId,
            [FromQuery, Required] Guid boardId,
            [FromQuery] int count = 1,
            [FromQuery] int offset = 0
        )
        {
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var tokenPayload = _jwtService.GetTokenInfo(token);
            var taskPerformers = await _taskRepository.GetTaskPerformers(taskId, count, offset);
            var performers = taskPerformers.Select(e => e.Performer.ToProfileBody());

            return Ok(performers);
        }

        [HttpPost("task/performers"), Authorize]
        [SwaggerOperation("Добавить исполнителей задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ProfileBody>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> AddTaskPerformers(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid taskId,
            [FromQuery, Required] Guid boardId,
            [FromBody] IEnumerable<Guid> userIds
        )
        {
            var tokenPayload = _jwtService.GetTokenInfo(token);
            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            if (!userIds.Any())
                return BadRequest();

            var task = await _taskRepository.GetAsync(taskId, false);
            if (task == null)
                return BadRequest();

            var boardMembers = (await _boardRepository.GetBoardMembers(userIds, boardId)).Select(e => e.User);
            var members = boardMembers.IntersectBy(userIds, e => e.Id);
            if (!members.Any())
                return Forbid();

            var addedPerformers = await _taskRepository.AddTaskPerformers(task, members);
            if (addedPerformers.Any())
            {
                var chat = await _chatRepository.GetByTaskIdAsync(taskId);
                foreach (var user in addedPerformers.Select(e => e.Performer))
                {
                    var sessions = await _userRepository.GetUserSessionsAsync(user.Id);
                    var chatMembership = await _chatRepository.CreateOrGetChatMembershipAsync(chat, user);
                    await _chatRepository.CreateUserChatSessionAsync(sessions, chatMembership, DateTime.UtcNow);
                }
            }
            return Ok();
        }


        [HttpPut("task"), Authorize]
        [SwaggerOperation("Обновить задачу")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(400)]
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

        [HttpPost("task/column"), Authorize]
        [SwaggerOperation("Добавить задачу в колонку любой доски")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]
        public async Task<IActionResult> AddTaskToColumn(
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] Guid columnId,
            [FromQuery, Required] Guid taskId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenInfo(token);

            var authorizeCheck = await AuthorizeCheck(boardId, token);
            if (authorizeCheck is ForbidResult)
                return authorizeCheck;

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return BadRequest();

            var userBoards = await _boardRepository.GetAll(tokenPayload.UserId);
            var boardIds = userBoards.Select(e => e.Id);
            if (!boardIds.Contains(boardId))
                return Forbid();

            var task = await _taskRepository.GetAsync(taskId, false);
            if (task == null)
                return BadRequest();

            await _taskRepository.AddTaskToColumn(task, column);
            return Ok();
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