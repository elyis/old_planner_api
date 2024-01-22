using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using Swashbuckle.AspNetCore.Annotations;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;

        public TaskController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [HttpPost("task")]
        [SwaggerOperation("Создать задачу")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateTask(CreateTaskBody taskBody)
        {
            if(taskBody.HexColor != null && !IsHexFormat(taskBody.HexColor))
                return BadRequest("hex color does not have hex format");

            var result = await _taskRepository.AddAsync(taskBody);
            return result == null ? BadRequest() : Ok(result.ToTaskBody());
        }

        [HttpGet("tasks")]
        [SwaggerOperation("Получить все задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<TaskBody>))]

        public async Task<IActionResult> GetTasksByStatus([FromQuery] TaskState? status = null)
        {
            var tasks = status == null 
                ? await _taskRepository.GetAll() 
                : await _taskRepository.GetAllByStatus(status.Value);

            var result = tasks.Select(e => e.ToTaskBody());
            return Ok(result);
        }


        [HttpPut("task")]
        [SwaggerOperation("Обновить задачу")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(204)]

        public async Task<IActionResult> UpdateTask(UpdateTaskBody taskBody)
        {
            if(taskBody.HexColor != null && !IsHexFormat(taskBody.HexColor))
                return BadRequest("hex color does not have hex format");

            var result = await _taskRepository.UpdateAsync(taskBody);
            return result == null ? BadRequest() : Ok(result.ToTaskBody());
        }

        [HttpDelete("task")]
        [SwaggerOperation("Удалить задачу")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> RemoveTask(Guid id)
        {
            var result = await _taskRepository.RemoveAsync(id);
            return result != false ? NoContent() : BadRequest();
        }

        private static bool IsHexFormat(string value)
        {
            var hexRegex = new Regex("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$");
            return hexRegex.IsMatch(value);
        }
    }
}