using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class DraftController : ControllerBase
    {
        private readonly IDraftRepository _draftRepository;
        private readonly ITaskRepository _taskRepository;

        public DraftController(
            IDraftRepository draftRepository,
            ITaskRepository taskRepository
        )
        {
            _draftRepository = draftRepository;
            _taskRepository = taskRepository;
        }

        [HttpPost("draft")]
        [SwaggerOperation("Создать черновик задачи")]
        [SwaggerResponse(200, Type = typeof(TaskDraftBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateDraft(CreateDraftBody draftBody)
        {
            if(draftBody.HexColor != null && !IsHexFormat(draftBody.HexColor))
                return BadRequest("hex color does not have hex format");

            TaskModel? task = null;
            if(draftBody.ModifiedTaskId != null && draftBody.ModifiedTaskId != Guid.Empty)
            {
                task = await _taskRepository.GetAsync((Guid) draftBody.ModifiedTaskId);
                if(task == null)
                    return BadRequest("task id is not found");
            }

            var result = await _draftRepository.AddAsync(draftBody, task);
            return result == null ? BadRequest() : Ok(result.ToTaskDraftBody());
        }

        [HttpPut("draft")]
        [SwaggerOperation("Обновить черновик")]
        [SwaggerResponse(200, Type = typeof(TaskDraftBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> UpdateDraft(UpdateDraftBody draftBody)
        {
            if(draftBody.HexColor != null && !IsHexFormat(draftBody.HexColor))
                return BadRequest("hex color does not have hex format");

            var result = await _draftRepository.UpdateAsync(draftBody);
            return result == null ? BadRequest("id is not found") : Ok(result.ToTaskDraftBody());
        }

        [HttpDelete("draft")]
        [SwaggerOperation("Удалить черновик")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> RemoveDraft(Guid id)
        {
            var result = await _draftRepository.RemoveAsync(id);
            return result != false ? NoContent() : BadRequest();
        }

        [HttpGet("drafts")]
        [SwaggerOperation("Получить черновики")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<TaskDraftBody>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> GetDrafts(Guid taskId)
        {
            if(taskId == Guid.Empty)
                return BadRequest("task id is empty");

            var drafts = await _draftRepository.GetAllByTaskId(taskId);
            var result = drafts.Select(e => e.ToTaskDraftBody());
            return Ok(result);
        }

        private static bool IsHexFormat(string value)
        {
            var hexRegex = new Regex("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$");
            return hexRegex.IsMatch(value);
        }

    }
}