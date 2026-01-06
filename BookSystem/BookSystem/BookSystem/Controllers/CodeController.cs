using BookSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BookSystem.Controllers
{

    [Route("api/code")]
    [ApiController]
    public class CodeController : ControllerBase
    {

        [Route("bookstatus")]
        [HttpPost()]
        public IActionResult GetBookStatusData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    Data = codeService.GetBookStatusData(),
                    Status = true,
                    Message = string.Empty
                };

                return Ok(result);
            }
            catch (Exception)
            {

                return Problem();
            }
        }
        //TODO:bookclass下拉選單、借閱人下拉選單
        [Route("bookclass")]
        [HttpPost()]
        public IActionResult GetBookClassData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    Data = codeService.GetBookClassData(),
                    Status = true,
                    Message = string.Empty
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        [Route("bookkeeper")]
        [HttpPost()]
        public IActionResult GetBookKeeperData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    Data = codeService.GetBookKeeperData(),
                    Status = true,
                    Message = string.Empty
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return Problem();
            }
        }
    }
}