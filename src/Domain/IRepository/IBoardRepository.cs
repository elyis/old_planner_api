using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Models;

namespace old_planner_api.src.Domain.IRepository
{
    public interface IBoardRepository
    {
        Task<IEnumerable<Board>> GetAll(Guid userId);
        Task<Board?> AddAsync(CreateBoardBody boardBody, UserModel user);
        Task<Board?> GetAsync(Guid id);
        Task<BoardMember?> GetBoardMemberAsync(Guid userId, Guid boardId);
        Task<IEnumerable<BoardColumn>> GetBoardColumns(Guid boardId);
        Task<BoardColumn?> GetBoardColumn(Guid columnId);
        Task<IEnumerable<UserModel>> GetBoardMembers(Guid boardId, int count, int offset);
        Task<IEnumerable<BoardMember>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId);
        Task<BoardMember?> AddBoardMember(UserModel user, Guid boardId);
        Task<BoardColumn?> AddBoardColumn(Board board, string name);
    }
}