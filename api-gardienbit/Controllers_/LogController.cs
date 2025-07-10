using api_gardienbit.Repositories;
using common_gardienbit.DTO.Log;
using common_gardienbit.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_gardienbit.Controllers_
{
    [Authorize(Roles = Role.Admin)]
    [ApiController]
    [Route("api/logs")]
    public class LogController : ControllerBase
    {
        private readonly LogRepository _logRepository;
        private readonly LogActionRepository _logActionRepository;


        public LogController(LogRepository logRepository, LogActionRepository logActionRepository)
        {
            _logRepository = logRepository;
            _logActionRepository = logActionRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<GetLogDTO>> GetAll()
        {
            var logs = _logRepository.GetObjects();
            var logsDTO = logs.Select(c => new GetLogDTO
            {
                logId = c.LogId,
                logAction = new GetLogActionDTO()
                {
                    loaId = c!.LogAction!.LoaId,
                    loaName = c.LogAction.LoaName
                },
                logVauName = c.LogVauName,
                logVauId = c.LogVauId,
                logPwpId = c.LogPwpId,
                logCliEntraId = c.LogCliEntraId,
                logCliId = c.LogCliId

            }).ToList();

            return Ok(logsDTO);
        }

        [HttpGet("/api/logactions")]
        public ActionResult<IEnumerable<GetLogDTO>> GetAllLogAction()
        {
            var logs = _logActionRepository.GetObjects();
            var logsDTO = logs.Select(c => new GetLogActionDTO
            {
                loaId = c.LoaId,
                loaName = c.LoaName
            }).ToList();

            return Ok(logsDTO);
        }
    }
}
