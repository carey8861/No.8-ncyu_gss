using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookSystem.Model
{
    public class BookService
    {
        private string GetDBConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            return config.GetConnectionString("DBConn");
        }

        public List<Book> QueryBook(BookQueryArg arg)
        {
            var result = new List<Book>();
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"
                    Select 
	                    A.BOOK_ID As BookId,
                        A.BOOK_NAME As BookName,
	                    A.BOOK_CLASS_ID As BookClassId,
                        B.BOOK_CLASS_NAME As BookClassName,
	                    Convert(VarChar(10),A.BOOK_BOUGHT_DATE,120) As BookBoughtDate,
                        A.BOOK_STATUS As BookStatusId,
                        C.CODE_NAME As BookStatusName,
                        A.BOOK_KEEPER As BookKeeperId,
                        D.USER_CNAME As BookKeeperCname
                    From BOOK_DATA As A
	                    Inner Join BOOK_CLASS As B On A.BOOK_CLASS_ID=B.BOOK_CLASS_ID
	                    Inner Join BOOK_CODE As C On A.BOOK_STATUS=C.CODE_ID And C.CODE_TYPE = 'BOOK_STATUS'
                        Left Join MEMBER_M As D On A.BOOK_KEEPER = D.USER_ID
	                    Where 
                        (A.BOOK_NAME Like @BOOK_NAME Or @BOOK_NAME = '') And
                        (A.BOOK_CLASS_ID = @BOOK_CLASS_ID Or @BOOK_CLASS_ID = '') And
                        (A.BOOK_KEEPER = @BOOK_KEEPER_ID Or @BOOK_KEEPER_ID = '') And
                        (A.BOOK_STATUS = @BOOK_STATUS_ID Or @BOOK_STATUS_ID = '')
                        Order By A.CREATE_DATE DESC";
                
                Dictionary<string, Object> parameter = new Dictionary<string, object>();
                parameter.Add("@BOOK_NAME", arg.BookName != null ? "%" + arg.BookName + "%" : string.Empty);
                parameter.Add("@BOOK_CLASS_ID", arg.BookClassId ?? string.Empty);
                parameter.Add("@BOOK_KEEPER_ID", arg.BookKeeperId ?? string.Empty);
                parameter.Add("@BOOK_STATUS_ID", arg.BookStatusId ?? string.Empty);

                result = conn.Query<Book>(sql, parameter).ToList();
            }
            return result;
        }

        public void AddBook(Book book)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"
                Insert Into BOOK_DATA
                (
	                BOOK_NAME,BOOK_CLASS_ID,
	                BOOK_AUTHOR,BOOK_BOUGHT_DATE,
	                BOOK_PUBLISHER,BOOK_NOTE,
	                BOOK_STATUS,BOOK_KEEPER,
	                BOOK_AMOUNT,
	                CREATE_DATE,CREATE_USER,MODIFY_DATE,MODIFY_USER
                )
                Select 
	                @BOOK_NAME,@BOOK_CLASS_ID,
	                @BOOK_AUTHOR,@BOOK_BOUGHT_DATE,
	                @BOOK_PUBLISHER,@BOOK_NOTE,
	                @BOOK_STATUS,@BOOK_KEEPER,
	                0 As BOOK_AMOUNT,
	                GetDate() As CREATE_DATE,'Admin' As CREATE_USER,GetDate() As MODIFY_DATE,'Admin' As MODIFY_USER";

                Dictionary<string, Object> parameter = new Dictionary<string, object>();
                parameter.Add("@BOOK_NAME", book.BookName);
                parameter.Add("@BOOK_CLASS_ID", book.BookClassId);
                parameter.Add("@BOOK_AUTHOR", book.BookAuthor);
                parameter.Add("@BOOK_BOUGHT_DATE", book.BookBoughtDate);
                parameter.Add("@BOOK_PUBLISHER", book.BookPublisher);
                parameter.Add("@BOOK_NOTE", book.BookNote);
                parameter.Add("@BOOK_STATUS", "A"); // 新增預設 A (可借出)

                // 修正重點：新增時沒有借閱人，必須給 DBNull，不能給空字串
                parameter.Add("@BOOK_KEEPER", DBNull.Value); 

                conn.Execute(sql, parameter);
            }
        }

        public void UpdateBook(Book book)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                // 檢查：若借閱狀態為 A (可借出) 或 U (不可借出)，則必須清空借閱人
                if (book.BookStatusId == "A" || book.BookStatusId == "U")
                {
                    book.BookKeeperId = null;
                }

                string sql = @"
                    Update BOOK_DATA Set 
                        BOOK_NAME = @BOOK_NAME,
                        BOOK_CLASS_ID = @BOOK_CLASS_ID,
                        BOOK_AUTHOR = @BOOK_AUTHOR,
                        BOOK_BOUGHT_DATE = @BOOK_BOUGHT_DATE,
                        BOOK_PUBLISHER = @BOOK_PUBLISHER,
                        BOOK_NOTE = @BOOK_NOTE,
                        BOOK_STATUS = @BOOK_STATUS,
                        BOOK_KEEPER = @BOOK_KEEPER,
                        MODIFY_DATE = GetDate(),
                        MODIFY_USER = 'Admin'
                    Where BOOK_ID = @BOOK_ID
                ";

                Dictionary<string, Object> parameter = new Dictionary<string, object>();
                parameter.Add("@BOOK_NAME", book.BookName);
                parameter.Add("@BOOK_CLASS_ID", book.BookClassId);
                parameter.Add("@BOOK_AUTHOR", book.BookAuthor);
                parameter.Add("@BOOK_BOUGHT_DATE", book.BookBoughtDate);
                parameter.Add("@BOOK_PUBLISHER", book.BookPublisher);
                parameter.Add("@BOOK_NOTE", book.BookNote);
                parameter.Add("@BOOK_STATUS", book.BookStatusId);
                
                // 修正重點：如果 KeeperId 是空字串或 null，轉為 DBNull.Value
                parameter.Add("@BOOK_KEEPER", string.IsNullOrEmpty(book.BookKeeperId) ? (object)DBNull.Value : book.BookKeeperId);
                
                parameter.Add("@BOOK_ID", book.BookId);

                conn.Execute(sql, parameter);

                // 如果狀態變成 B (已借出) 或 C (已借出未領)，且有指定借閱人，則寫入紀錄
                if ((book.BookStatusId == "B" || book.BookStatusId == "C") && !string.IsNullOrEmpty(book.BookKeeperId))
                {
                    sql = @"
                            Insert Into BOOK_LEND_RECORD
                            (
                                BOOK_ID,KEEPER_ID,LEND_DATE,
                                CRE_DATE,CRE_USR,MOD_DATE,MOD_USR
                            )
                            VALUES
                            (
                                @BOOK_ID, @KEEPER_ID, GetDate(),
                                GetDate(), 'Admin', GetDate(), 'Admin'
                            )
                            ";
                    parameter.Clear();
                    parameter.Add("@BOOK_ID", book.BookId);
                    parameter.Add("@KEEPER_ID", book.BookKeeperId);

                    conn.Execute(sql, parameter);
                }
            }
        }

        public void DeleteBookById(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"Delete From BOOK_DATA Where BOOK_ID=@BOOK_ID";
                conn.Execute(sql, new { BOOK_ID = bookId });
            }
        }

        public Book GetBookById(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"
                    Select 
                        A.BOOK_ID As BookId,
                        A.BOOK_NAME As BookName,
                        A.BOOK_CLASS_ID As BookClassId,
                        A.BOOK_AUTHOR As BookAuthor,
                        Convert(VarChar(10),A.BOOK_BOUGHT_DATE,120) As BookBoughtDate,
                        A.BOOK_PUBLISHER As BookPublisher,
                        A.BOOK_NOTE As BookNote,
                        A.BOOK_STATUS As BookStatusId,
                        A.BOOK_KEEPER As BookKeeperId
                    From BOOK_DATA As A
                    Where A.BOOK_ID = @BOOK_ID";
                
                return conn.QueryFirstOrDefault<Book>(sql, new { BOOK_ID = bookId });
            }
        }

        public List<BookLendRecord> GetBookLendRecord(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"
                    Select
                        Convert(varchar(10), L.LEND_DATE, 111) As LendDate,
                        L.KEEPER_ID As BookKeeperId,
                        M.USER_ENAME As BookKeeperEname,
                        M.USER_CNAME As BookKeeperCname
                    From BOOK_LEND_RECORD L
                    Inner Join MEMBER_M M On L.KEEPER_ID = M.USER_ID
                    Where L.BOOK_ID = @BOOK_ID
                    Order By L.LEND_DATE Desc";

                return conn.Query<BookLendRecord>(sql, new { BOOK_ID = bookId }).ToList();
            }
        }
    }
}