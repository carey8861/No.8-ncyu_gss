using BookSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BookSystem.Controllers
{
    [Route("api/bookmaintain")]
    [ApiController]
    public class BookMaintainController : ControllerBase
    {
        
        [HttpPost]
        [Route("addbook")]
        public IActionResult AddBook(Book book)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
                    BookService bookService = new BookService();
                    bookService.AddBook(book);
                    return Ok(
                        new ApiResult<string>()
                        {
                            Data = string.Empty,
                            Status = true,
                            Message = string.Empty
                        });
                }
                else
                {
                    return BadRequest(ModelState);
                }

            }
            catch (Exception)
            {
                return Problem(); 
            }
        }
        [HttpPost()]
        [Route("querybook")]
        public IActionResult QueryBook([FromForm]BookQueryArg arg)
        {
            try
            {
                BookService bookService = new BookService();

                return Ok(bookService.QueryBook(arg));
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        [HttpPost()]
        [Route("loadbook")]
        public IActionResult GetBookById([FromBody]int bookId)
        {
            try
            {
                BookService bookService = new BookService();
                ApiResult<Book> result = new ApiResult<Book>
                {
                    //TODO:明細畫面結果
                    Data = bookService.GetBookById(bookId),
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
        //TODO:UpdateBook()
        [HttpPost()]
        [Route("updatebook")]
        public IActionResult UpdateBook(Book book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    BookService bookService = new BookService();
                    bookService.UpdateBook(book);
                    return Ok(
                        new ApiResult<string>()
                        {
                            Data = string.Empty,
                            Status = true,
                            Message = string.Empty
                        });
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception)
            {
                return Problem();
            }
        }


        [HttpPost()]
        [Route("deletebook")]
        public IActionResult DeleteBookById([FromBody] int bookId)
        {
            try
            {
                BookService bookService = new BookService();

                ApiResult<string> result = new ApiResult<string>
                {
                    Data = string.Empty,
                    Status = true,
                    Message = string.Empty
                };

                //TODO:書籍刪除前檢查
                //if book cannot result.Message = "該書已借出不可刪除"..
                //else bookService.DeleteBookById(bookId);
                
                var book = bookService.GetBookById(bookId);
                if (book.BookStatusId == "B" || book.BookStatusId == "C")
                {
                    result.Status = false;
                    result.Message = "該書已借出不可刪除";
                }
                else
                {
                    bookService.DeleteBookById(bookId);
                    result.Status = true;
                    result.Message = "刪除成功";
                }

                return Ok(result);
            }
            catch (Exception)
            {
                return Problem();
            }
        }
        //TODO:booklendrecord
        [HttpPost()]
        [Route("lendrecord")]
        public IActionResult GetBookLendRecord([FromBody] int bookId)
        {
            try
            {
                BookService bookService = new BookService();
                return Ok(bookService.GetBookLendRecord(bookId));
            }
            catch (Exception)
            {
                return Problem();
            }
        }
    }
}