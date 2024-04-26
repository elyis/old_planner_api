using System.ComponentModel.DataAnnotations;
using System.Net;
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

        [HttpGet("columns"), Authorize]
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
    }
}