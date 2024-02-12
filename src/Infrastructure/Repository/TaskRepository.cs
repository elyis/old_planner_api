using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;

namespace old_planner_api.src.Infrastructure.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskModel?> AddAsync
        (
            CreateTaskBody taskBody,
            Board board,
            UserModel creator
        )
        {
            var task = new TaskModel
            {
                Title = taskBody.Title,
                Description = taskBody.Description,
                HexColor = taskBody.HexColor,
                PriorityOrder = taskBody.PriorityOrder,
                Status = taskBody.Status.ToString(),
                Board = board,
                Creator = creator,
                IsDraft = false,
                StartDate = ParseDateTime(taskBody.StartDate),
                EndDate = ParseDateTime(taskBody.EndDate),
            };

            return await AddTaskAsync(task, creator);
        }

        public async Task<TaskModel?> AddAsync
        (
            CreateDraftBody draftBody,
            Board board,
            UserModel creator,
            TaskModel? parentTask
        )
        {
            var task = new TaskModel
            {
                Title = draftBody.Title,
                Description = draftBody.Description,
                HexColor = draftBody.HexColor,
                PriorityOrder = 0,
                Status = TaskState.Undefined.ToString(),
                Board = board,
                Creator = creator,
                IsDraft = true,
                StartDate = ParseDateTime(draftBody.StartDate),
                EndDate = ParseDateTime(draftBody.EndDate),
                DraftOfTask = parentTask,
            };

            return await AddTaskAsync(task, creator);
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid boardId, bool isDraft = false)
            => await _context.Tasks
                .Include(e => e.Chat)
                .Where(e =>
                    e.IsDraft == isDraft &&
                    e.BoardId == boardId)
                .ToListAsync();

        public async Task<IEnumerable<TaskModel>> GetAll(Guid boardId, TaskState? status, bool isDraft = false)
        {
            if (status == null)
                return await _context.Tasks
                .Include(e => e.Chat)
                .Where(e =>
                    e.IsDraft == isDraft &&
                    e.BoardId == boardId)
                .ToListAsync();

            var statusString = status.ToString();
            var tasks = await _context.Tasks
                .Include(e => e.Chat)
                .Where(e =>
                    e.Status == statusString &&
                    e.IsDraft == isDraft &&
                    e.BoardId == boardId)
                .ToListAsync();

            return tasks;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid boardId)
            => await _context.Tasks
                .Include(e => e.Chat)
                .Where(e => e.BoardId == boardId)
                .ToListAsync();

        public async Task<TaskModel?> GetAsync(Guid id, bool isDraft)
            => await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDraft == isDraft);

        public async Task<bool> RemoveAsync(Guid id, bool isDraft)
        {
            var task = await GetAsync(id, isDraft);
            if (task == null)
                return true;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskModel?> ConvertDraftToTask(Guid id, UserModel user)
        {
            var draft = await _context.Tasks
                .Include(e => e.DraftOfTask)
                .Include(e => e.Board)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDraft);
            if (draft == null)
                return null;

            bool hasParentTask = draft.DraftOfTask != null;
            var task = draft.DraftOfTask;

            if (hasParentTask)
            {
                task.Description = draft.Description;
                task.Title = draft.Title;
                task.HexColor = draft.HexColor;
                task.PriorityOrder = draft.PriorityOrder;
                task.Status = draft.Status;
                task.StartDate = draft.StartDate;
                task.EndDate = draft.EndDate;
                task.Creator = user;
                task.Board = draft.Board;
                task.IsDraft = false;
            }
            else
            {
                task = new TaskModel
                {
                    Description = draft.Description,
                    Title = draft.Title,
                    HexColor = draft.HexColor,
                    PriorityOrder = draft.PriorityOrder,
                    Status = draft.Status,
                    StartDate = draft.StartDate,
                    EndDate = draft.EndDate,
                    Creator = user,
                    Board = draft.Board,
                    IsDraft = false
                };
                task = await AddTaskAsync(task, user);
            }

            _context.Tasks.Remove(draft);

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskModel?> UpdateAsync(UpdateTaskBody taskBody)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == taskBody.Id && !e.IsDraft);
            if (task == null)
                return null;

            MapTaskProperties(taskBody, task);

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskModel?> UpdateAsync(UpdateDraftBody draftBody, TaskModel? draftOfTask)
        {
            var draft = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == draftBody.Id && e.IsDraft);
            if (draft == null)
                return null;

            MapTaskProperties(draftBody, draft, draftOfTask);

            await _context.SaveChangesAsync();
            return draft;
        }

        private DateTime? ParseDateTime(string? dateTimeString) =>
            DateTime.TryParse(dateTimeString, out var date) ? date : null;


        private async Task<TaskModel?> AddTaskAsync(TaskModel task, UserModel user)
        {
            task.Chat = new TaskChat
            {
                Task = task,
                Memberships = new List<TaskChatMembership>
                {
                    new() {
                        Participant = user
                    }
                }
            };
            task = (await _context.Tasks.AddAsync(task))?.Entity;
            await _context.SaveChangesAsync();
            return task;
        }

        private void MapTaskProperties(UpdateTaskBody source, TaskModel destination)
        {
            destination.Title = source.Title;
            destination.Description = source.Description;
            destination.HexColor = source.HexColor;
            destination.PriorityOrder = source.PriorityOrder;
            destination.Status = source.Status.ToString();
            destination.StartDate = ParseDateTime(source.StartDate);
            destination.EndDate = ParseDateTime(source.EndDate);
        }

        private void MapTaskProperties(UpdateDraftBody source, TaskModel destination, TaskModel? draftOfTask)
        {
            destination.Title = source.Title;
            destination.Description = source.Description;
            destination.HexColor = source.HexColor;
            destination.StartDate = ParseDateTime(source.StartDate);
            destination.EndDate = ParseDateTime(source.EndDate);
            destination.DraftOfTask = draftOfTask;
        }

        public async Task<IEnumerable<TaskModel>> GetAllDrafts(Guid boardId, Guid userId)
            => await _context.Tasks
                .Where(e => e.BoardId == boardId && e.CreatorId == userId)
                .ToListAsync();
    }
}
