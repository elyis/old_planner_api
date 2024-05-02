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
            BoardColumn column,
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
                Creator = creator,
                IsDraft = false,
                StartDate = ParseDateTime(taskBody.StartDate),
                EndDate = ParseDateTime(taskBody.EndDate),
                Type = taskBody.Type.ToString(),
            };

            return await AddTaskAsync(task, creator, column);
        }

        public async Task<TaskModel?> AddAsync
        (
            CreateDraftBody draftBody,
            BoardColumn column,
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
                Creator = creator,
                IsDraft = true,
                StartDate = ParseDateTime(draftBody.StartDate),
                EndDate = ParseDateTime(draftBody.EndDate),
                DraftOfTask = parentTask,
                Type = draftBody.Type.ToString(),
            };

            return await AddTaskAsync(task, creator, column);
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, bool isDraft = false)
        {
            var result = await _context.BoardColumnTasks
                .Include(e => e.Task)
                .ThenInclude(e => e.Chat)
                .Where(e => e.ColumnId == columnId && e.Task.IsDraft == isDraft).ToListAsync();

            return result.Select(e => e.Task);
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, TaskState? status, bool isDraft = false)
        {
            if (status == null)
                return await GetAll(columnId, isDraft);

            var statusString = status.ToString();
            var tasks = await _context.BoardColumnTasks
                .Include(e => e.Task)
                .ThenInclude(e => e.Chat)
                .Where(e => e.ColumnId == columnId && e.Task.IsDraft == isDraft && e.Task.Status == statusString).ToListAsync();

            var result = tasks.Select(e => e.Task);

            return result;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId)
        {
            var result = await _context.BoardColumnTasks
                .Include(e => e.Task)
                .ThenInclude(e => e.Chat)
                .Where(e => e.ColumnId == columnId).ToListAsync();

            return result.Select(e => e.Task);
        }

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

        public async Task<TaskModel?> ConvertDraftToTask(Guid id, UserModel user, BoardColumn column)
        {
            var boardColumnTask = await _context.BoardColumnTasks
                .Include(e => e.Task)
                .ThenInclude(e => e.DraftOfTask)
                .FirstOrDefaultAsync(e =>
                    e.TaskId == id && e.Task.IsDraft && e.ColumnId == column.Id
                );


            if (boardColumnTask == null)
                return null;

            var newTask = boardColumnTask.Task.DraftOfTask;
            bool hasParentTask = newTask.DraftOfTask != null;
            var oldTask = boardColumnTask.Task;

            if (hasParentTask)
            {
                newTask.Description = oldTask.Description;
                newTask.Title = oldTask.Title;
                newTask.HexColor = oldTask.HexColor;
                newTask.PriorityOrder = oldTask.PriorityOrder;
                newTask.Status = oldTask.Status;
                newTask.StartDate = oldTask.StartDate;
                newTask.EndDate = oldTask.EndDate;
                newTask.Creator = user;
                newTask.IsDraft = false;
                newTask.Type = oldTask.Type;
            }
            else
            {
                newTask = new TaskModel
                {
                    Description = oldTask.Description,
                    Title = oldTask.Title,
                    HexColor = oldTask.HexColor,
                    PriorityOrder = oldTask.PriorityOrder,
                    Status = oldTask.Status,
                    StartDate = oldTask.StartDate,
                    EndDate = oldTask.EndDate,
                    Creator = user,
                    IsDraft = false,
                    Type = oldTask.Type
                };
                newTask = await AddTaskAsync(newTask, user, column);
            }

            _context.Tasks.Remove(oldTask);

            await _context.SaveChangesAsync();
            return newTask;
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
                .Include(e => e.Chat)
                .FirstOrDefaultAsync(e => e.Id == draftBody.Id && e.IsDraft);
            if (draft == null)
                return null;

            MapTaskProperties(draftBody, draft, draftOfTask);

            await _context.SaveChangesAsync();
            return draft;
        }

        private DateTime? ParseDateTime(string? dateTimeString) =>
            DateTime.TryParse(dateTimeString, out var date) ? date.ToUniversalTime() : null;


        private async Task<TaskModel?> AddTaskAsync(TaskModel task, UserModel user, BoardColumn column)
        {
            if (task == null)
                return null;

            task.Chat = new Chat
            {
                Task = task,
                Name = task.Title,
                Type = ChatType.Task.ToString(),
                ChatMemberships = new List<ChatMembership>
                {
                    new() {
                        User = user
                    }
                }
            };
            var boardColumnTask = new BoardColumnTask
            {
                Column = column,
                Task = task
            };
            task = (await _context.Tasks.AddAsync(task))?.Entity;
            boardColumnTask = (await _context.BoardColumnTasks.AddAsync(boardColumnTask))?.Entity;
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

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, Guid userId)
        {
            var result = await _context.BoardColumnTasks
                .Include(e => e.Task)
                .ThenInclude(e => e.Chat)
                .Where(e => e.ColumnId == columnId && e.Task.IsDraft && e.Task.CreatorId == userId).ToListAsync();

            return result.Select(e => e.Task);
        }

        public async Task<TaskPerformer?> AddTaskPerformer(TaskModel task, UserModel performer)
        {
            TaskPerformer? taskPerformer = await GetTaskPerformer(performer.Id, task.Id);
            if (taskPerformer != null)
                return null;

            taskPerformer = new TaskPerformer
            {
                Performer = performer,
                Task = task
            };

            await _context.TaskPerformers.AddAsync(taskPerformer);
            await _context.SaveChangesAsync();

            return taskPerformer;
        }

        public async Task<TaskPerformer?> GetTaskPerformer(Guid performerId, Guid taskId)
        {
            return await _context.TaskPerformers
                .FirstOrDefaultAsync(e => e.PerformerId == performerId && e.TaskId == taskId);
        }

        public async Task<IEnumerable<TaskPerformer>> AddTaskPerformers(TaskModel task, IEnumerable<UserModel> performers)
        {
            var performerIds = performers.Select(e => e.Id);
            var taskPerformers = await GetTaskPerformers(performerIds, task.Id, int.MaxValue, 0);
            var existingPerformers = taskPerformers.Select(e => e.Performer);
            var notExistingPerformers = performers.Except(existingPerformers);

            var newTaskPerformers = notExistingPerformers.Select(e => new TaskPerformer
            {
                Performer = e,
                Task = task
            });

            await _context.TaskPerformers.AddRangeAsync(newTaskPerformers);
            await _context.SaveChangesAsync();

            return newTaskPerformers;
        }

        public async Task<IEnumerable<TaskPerformer>> GetTaskPerformers(IEnumerable<Guid> performerIds, Guid taskId, int count, int offset)
        {
            return await _context.TaskPerformers
                .Include(e => e.Performer)
                .Where(e => e.TaskId == taskId && performerIds.Contains(e.PerformerId))
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskPerformer>> GetTaskPerformers(Guid taskId, int count, int offset)
        {
            return await _context.TaskPerformers
                .Include(e => e.Performer)
                .Where(e => e.TaskId == taskId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task AddTaskToColumn(TaskModel task, BoardColumn column)
        {
            var columnTask = await _context.BoardColumnTasks
                .FirstOrDefaultAsync(e => e.ColumnId == column.Id && e.TaskId == task.Id);

            if (columnTask != null)
                return;

            columnTask = new BoardColumnTask
            {
                Column = column,
                Task = task
            };
            await _context.BoardColumnTasks.AddAsync(columnTask);
            await _context.SaveChangesAsync();
        }
    }
}
