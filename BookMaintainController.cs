// ...existing code...
using BookSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Linq; // { changed code }
using BookSystem.Service;
// ...existing code...
        using System;
        using System.Linq;
        using Microsoft.AspNetCore.Mvc;
        using BookSystem.Model;

        using System;
        using System.Linq;
        using Microsoft.AspNetCore.Mvc;
        using BookSystem.Model;

        namespace BookSystem.Controllers
        {
            [ApiController]
            [Route("api/[controller]")]
            public class BookMaintainController : ControllerBase
            {
                private readonly BookService _bookService;

                public BookMaintainController(BookService bookService)
                {
                    _bookService = bookService;
                }

                [HttpPost("loadbook")]
                public IActionResult GetBookById([FromBody] int bookId)
                {
                    try
                    {
                        var book = _bookService.QueryBook(new BookQueryArg { BookId = bookId }).FirstOrDefault();

                        var result = new ApiResult<Book>
                        {
                            Data = book ?? new Book(),
                            Status = book != null,
                            Message = book != null ? string.Empty : "書籍不存在"
                        };

                        return Ok(result);
                    }
                    catch (Exception ex)
                    {
                        return Problem(detail: ex.Message);
                    }
                }

                [HttpPost("deletebook")]
                public IActionResult DeleteBookById([FromBody] int bookId)
                {
                    try
                    {
                        var book = _bookService.QueryBook(new BookQueryArg { BookId = bookId }).FirstOrDefault();

                        var result = new ApiResult<string>
                        {
                            Data = string.Empty,
                            Status = false,
                            Message = string.Empty
                        };

                        if (book == null)
                        {
                            result.Message = "書籍不存在";
                            return NotFound(result);
                        }

                        if (book.BookStatusId == "B")
                        {
                            result.Message = "該書已借出不可刪除";
                            return BadRequest(result);
                        }

                        _bookService.DeleteBookById(bookId);
                        result.Status = true;

                        return Ok(result);
                    }
                    catch (Exception ex)
                    {
                        return Problem(detail: ex.Message);
                    }
                }
            }
        }