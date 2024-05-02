using Microsoft.EntityFrameworkCore;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Enums;
using old_planner_api.src.Domain.IRepository;
using old_planner_api.src.Domain.Models;
using old_planner_api.src.Infrastructure.Data;

namespace old_planner_api.src.Infrastructure.Repository
{
    public class BoardRepository : IBoardRepository
    {
        private readonly AppDbContext _context;

        public BoardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Board?> AddAsync(CreateBoardBody boardBody, UserModel user)
        {
            var board = new Board
            {
                Name = boardBody.Name,
                Members = new List<BoardMember>
                {
                    new() {
                        Role = BoardMemberRoles.Admin.ToString(),
                        User = user
                    }
                }
            };

            board = (await _context.Boards.AddAsync(board))?.Entity;
            await _context.SaveChangesAsync();

            return board;
        }

        public async Task<BoardMember?> AddBoardMember(UserModel user, Guid boardId)
        {
            var board = await GetAsync(boardId);
            if (board == null)
                return null;

            var boardMember = await GetBoardMemberAsync(user.Id, boardId);
            if (boardMember != null)
                return null;

            boardMember = new BoardMember
            {
                Board = board,
                User = user,
                Role = BoardMemberRoles.Participant.ToString(),
            };

            boardMember = (await _context.BoardMembers.AddAsync(boardMember))?.Entity;
            await _context.SaveChangesAsync();

            return boardMember;
        }

        public async Task<BoardMember?> GetBoardMemberAsync(Guid userId, Guid boardId)
            => await _context.BoardMembers
                .FirstOrDefaultAsync(e => e.BoardId == boardId && e.UserId == userId);

        public async Task<IEnumerable<Board>> GetAll(Guid userId)
        {
            var availableMemberBoards = await _context.BoardMembers
                .Include(e => e.Board)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return availableMemberBoards.Select(e => e.Board);
        }

        public async Task<Board?> GetAsync(Guid id)
            => await _context.Boards.FindAsync(id);

        public async Task<IEnumerable<BoardColumn>> GetBoardColumns(Guid boardId)
        {
            return await _context.BoardColumns
                .Where(e => e.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<BoardColumn?> GetBoardColumn(Guid columnId)
        {
            return await _context.BoardColumns
                .FirstOrDefaultAsync(e => e.Id == columnId);
        }

        public async Task<IEnumerable<UserModel>> GetBoardMembers(Guid boardId, int count, int offset)
        {
            var boardMembers = await _context.BoardMembers
                .Include(e => e.User)
                .OrderBy(e => e.UserId)
                .Where(e => e.BoardId == boardId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            var members = boardMembers.Select(e => e.User);
            return members;
        }

        public async Task<IEnumerable<BoardMember>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId)
        {
            return await _context.BoardMembers
                .Include(e => e.User)
                .Where(e => e.BoardId == boardId && memberIds.Contains(e.UserId))
                .ToListAsync();
        }

        public async Task<BoardColumn?> AddBoardColumn(Board board, string name)
        {
            var boardColumn = new BoardColumn
            {
                Board = board,
                Name = name,
            };

            boardColumn = (await _context.BoardColumns.AddAsync(boardColumn))?.Entity;
            await _context.SaveChangesAsync();

            return boardColumn;
        }
    }
}