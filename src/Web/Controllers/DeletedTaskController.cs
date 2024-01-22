using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.IRepository;
using Swashbuckle.AspNetCore.Annotations;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class DeletedTaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IDeletedTaskRepository _deletedTaskRepository;

        public DeletedTaskController(
            ITaskRepository taskRepository,
            IDeletedTaskRepository deletedTaskRepository
        )
        {
            _taskRepository = taskRepository;
            _deletedTaskRepository = deletedTaskRepository;
        }

        [HttpPost("deleted-task")]
        [SwaggerOperation("Удалить задачу")]
        [SwaggerResponse(200, Type = typeof(DeletedTaskBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateDeletedTask(Guid taskId)
        {
            if(taskId == Guid.Empty)
                return BadRequest("task id is empty");

            var task = await _taskRepository.GetAsync(taskId);
            if(task == null)
                return BadRequest("task id isn't exist");

            var deletedTask = await _deletedTaskRepository.AddAsync(task);
            return deletedTask == null ? Conflict() : Ok(deletedTask.ToDeletedTaskBody());
        }

        [HttpDelete("deleted-task")]
        [SwaggerOperation("Восстановить удаленную задачу")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> RemoveDraft(Guid deletedTaskId)
        {
            if(deletedTaskId == Guid.Empty)
                return BadRequest("deletedTaskId is empty");

            var result = await _deletedTaskRepository.RemoveAsync(deletedTaskId);
            return result != false ? NoContent() : BadRequest();
        }

        [HttpGet("deleted-tasks")]
        [SwaggerOperation("Получить все удаленные задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<DeletedTaskBody>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> GetDrafts()
        {
            var drafts = await _deletedTaskRepository.GetAll();
            var result = drafts.Select(e => e.ToDeletedTaskBody());
            return Ok(result);
        }
        
    }
}