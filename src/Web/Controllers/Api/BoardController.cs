using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using old_planner_api.src.Domain.Entities.Request;
using old_planner_api.src.Domain.Entities.Response;
using old_planner_api.src.Domain.IRepository;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class BoardController : ControllerBase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserRepository _userRepository;

        private readonly IJwtService _jwtService;

        public BoardController(
            IBoardRepository boardRepository,
            IUserRepository userRepository,
            IJwtService jwtService
        )
        {
            _boardRepository = boardRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        [HttpPost("board"), Authorize]
        [SwaggerOperation("Создать доску")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateBoard(
            CreateBoardBody boardBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);

            var result = await _boardRepository.AddAsync(boardBody, user);
            return result == null ? BadRequest() : Ok(result.ToBoardBody());
        }

        [HttpGet("boards"), Authorize]
        [SwaggerOperation("Получить список досок")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetBoards(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var boards = await _boardRepository.GetAll(tokenInfo.UserId);
            var result = boards.Select(e => e.ToBoardBody());
            return Ok(result);
        }

        [HttpGet("board/columns"), Authorize]
        [SwaggerOperation("Получить список колоннок")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<BoardColumnBody>))]

        public async Task<IActionResult> GetColumnsByBoard(
            [FromQuery, Required] Guid boardId
        )
        {
            var columns = await _boardRepository.GetBoardColumns(boardId);
            var result = columns.Select(e => e.ToBoardColumnBody());
            return Ok(result);
        }

        [HttpGet("board/members"), Authorize]
        [SwaggerOperation("Получить список участников доски")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ProfileBody>))]
        public async Task<IActionResult> GetBoardMembers(
            [FromQuery, Required] Guid boardId,
            [FromQuery] int count = 1,
            [FromQuery] int offset = 0
        )
        {
            var boardMembers = await _boardRepository.GetBoardMembers(boardId, count, offset);
            var result = boardMembers.Select(e => e.ToProfileBody());
            return Ok(result);
        }

        [HttpPost("board/member"), Authorize]
        [SwaggerOperation("Добавить участника доски")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> AddMember(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Required] Guid boardId,
            [FromQuery] string email
        )
        {
            var tokenPayload = _jwtService.GetTokenInfo(token);
            var boardMember = await _boardRepository.GetBoardMemberAsync(tokenPayload.UserId, boardId);
            if (boardMember == null)
                return Forbid();

            var user = await _userRepository.GetAsync(email);
            if (user == null)
                return NotFound();

            var result = await _boardRepository.AddBoardMember(user, boardId);
            return result == null ? BadRequest() : Ok();
        }

        [HttpPost("board/column"), Authorize]
        [SwaggerOperation("Создать колонку")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] string name
        )
        {
            var tokenPayload = _jwtService.GetTokenInfo(token);
            var boardMember = await _boardRepository.GetBoardMemberAsync(tokenPayload.UserId, boardId);
            if (boardMember == null)
                return Forbid();

            var board = await _boardRepository.GetAsync(boardId);
            if (board == null)
                return BadRequest();

            var result = await _boardRepository.AddBoardColumn(board, name);
            return result == null ? BadRequest() : Ok();
        }
    }
}