using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;

namespace old_planner_api.src.Infrastructure.Repository
{
    public class DraftRepository : IDraftRepository
    {
        private readonly AppDbContext _context;

        public DraftRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskDraft?> AddAsync(CreateDraftBody draftBody, TaskModel? modifiedTask)
        {
            var addedDraft = new TaskDraft
            {
                Title = draftBody.Title,
                Description = draftBody.Description,
                HexColor = draftBody.HexColor,
                StartDate = draftBody.StartDate == null ? null : DateTime.Parse(draftBody.StartDate),
                EndDate = draftBody.EndDate == null ? null : DateTime.Parse(draftBody.EndDate),
                ModifiedTask = modifiedTask
            };
            addedDraft = (await _context.Drafts.AddAsync(addedDraft))?.Entity;
            await _context.SaveChangesAsync();

            return addedDraft;
        }

        public async Task<IEnumerable<TaskDraft>> GetAllByTaskId(Guid taskId)
        {
            var result = await _context.Drafts
                .Where(e => e.ModifiedTaskId == taskId)
                .ToListAsync();

            return result;
        }

        public async Task<TaskDraft?> GetAsync(Guid id)
            => await _context.Drafts
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<bool> RemoveAsync(Guid id)
        {
            var draft = await GetAsync(id);
            if(draft == null)
                return true;

            _context.Drafts.Remove(draft);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskDraft?> UpdateAsync(UpdateDraftBody draftBody)
        {
            var draft = await GetAsync(draftBody.Id);
            if(draft == null)
                return null;

            draft.Title = draftBody.Title;
            draft.Description = draftBody.Description;
            draft.HexColor = draftBody.HexColor;
            draft.StartDate = draftBody.StartDate == null ? null : DateTime.Parse(draftBody.StartDate);
            draft.EndDate = draftBody.EndDate == null ? null : DateTime.Parse(draftBody.EndDate);

            await _context.SaveChangesAsync();
            return draft;
        }
    }
}