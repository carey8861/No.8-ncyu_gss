using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookSystem.Model
{
	public class BookService
	{
		/// <summary>
		/// 取得預設連線字串
		/// </summary>
		/// <returns></returns>
		private string GetDBConnectionString()
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.Build();

			// 注意：請確保 appsettings.json 裡面的名稱也是 "DBConn"
			return config.GetConnectionString("DBConn");
		}

		public List<Book> QueryBook(BookQueryArg arg)
		{
			var result = new List<Book>();
			using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
			{
				string sql = @"
					SELECT 
						A.BOOK_ID AS BookId,
						A.BOOK_NAME AS BookName,
						A.BOOK_CLASS_ID AS BookClassId,
						B.BOOK_CLASS_NAME AS BookClassName,
						A.BOOK_BOUGHT_DATE AS BookBoughtDate,
						A.BOOK_STATUS AS BookStatusId,
						C.CODE_NAME AS BookStatusName,
						A.BOOK_KEEPER AS BookKeeperId
					FROM BOOK_DATA AS A
					INNER JOIN BOOK_CLASS AS B ON A.BOOK_CLASS_ID = B.BOOK_CLASS_ID
					INNER JOIN BOOK_CODE AS C ON A.BOOK_STATUS = C.CODE_ID AND C.CODE_TYPE = 'BOOK_STATUS'
					WHERE (A.BOOK_ID = @BOOK_ID OR @BOOK_ID = 0)
					AND (A.BOOK_NAME LIKE @BOOK_NAME OR @BOOK_NAME = '')";

				var parameter = new Dictionary<string, object>
				{
					{ "@BOOK_ID", arg.BookId },
					{ "@BOOK_NAME", string.IsNullOrEmpty(arg.BookName) ? string.Empty : "%" + arg.BookName + "%" }
				};

				result = conn.Query<Book>(sql, parameter).ToList();
			}
			return result;
		}

		public void AddBook(Book book)
		{
			using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
			{
				string sql = @"
				INSERT INTO BOOK_DATA
				(
					BOOK_NAME, BOOK_CLASS_ID,
					BOOK_AUTHOR, BOOK_BOUGHT_DATE,
					BOOK_PUBLISHER, BOOK_NOTE,
					BOOK_STATUS, BOOK_KEEPER,
					BOOK_AMOUNT,
					CREATE_DATE, CREATE_USER, MODIFY_DATE, MODIFY_USER
				)
				VALUES
				(
					@BOOK_NAME, @BOOK_CLASS_ID,
					@BOOK_AUTHOR, @BOOK_BOUGHT_DATE,
					@BOOK_PUBLISHER, @BOOK_NOTE,
					@BOOK_STATUS, @BOOK_KEEPER,
					0,
					GETDATE(), 'Admin', GETDATE(), 'Admin'
				)";

				var parameter = new Dictionary<string, object>
				{
					{ "@BOOK_NAME", book.BookName },
					{ "@BOOK_CLASS_ID", book.BookClassId },
					{ "@BOOK_AUTHOR", book.BookAuthor },
					{ "@BOOK_BOUGHT_DATE", book.BookBoughtDate },
					{ "@BOOK_PUBLISHER", book.BookPublisher },
					{ "@BOOK_NOTE", book.BookNote },
					{ "@BOOK_STATUS", "A" },
					{ "@BOOK_KEEPER", book.BookKeeperId ?? string.Empty }
				};

				conn.Execute(sql, parameter);
			}
		}

		public void UpdateBook(Book book)
		{
			using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
			{
				try
				{
					string sql = @"
					UPDATE BOOK_DATA
					SET 
						BOOK_NAME = @BOOK_NAME,
						BOOK_CLASS_ID = @BOOK_CLASS_ID,
						BOOK_AUTHOR = @BOOK_AUTHOR,
						BOOK_BOUGHT_DATE = @BOOK_BOUGHT_DATE,
						BOOK_PUBLISHER = @BOOK_PUBLISHER,
						BOOK_NOTE = @BOOK_NOTE,
						BOOK_STATUS = @BOOK_STATUS,
						BOOK_KEEPER = @BOOK_KEEPER,
						MODIFY_DATE = GETDATE(),
						MODIFY_USER = 'Admin'
					WHERE BOOK_ID = @BOOK_ID";

					var parameter = new Dictionary<string, object>
					{
						{ "@BOOK_NAME", book.BookName },
						{ "@BOOK_CLASS_ID", book.BookClassId },
						{ "@BOOK_AUTHOR", book.BookAuthor },
						{ "@BOOK_BOUGHT_DATE", book.BookBoughtDate },
						{ "@BOOK_PUBLISHER", book.BookPublisher },
						{ "@BOOK_NOTE", book.BookNote },
						{ "@BOOK_STATUS", book.BookStatusId },
						{ "@BOOK_KEEPER", book.BookKeeperId ?? string.Empty },
						{ "@BOOK_ID", book.BookId }
					};

					conn.Execute(sql, parameter);

					if (book.BookStatusId == "B" || book.BookStatusId == "C")
					{
						sql = @"
						INSERT INTO BOOK_LEND_RECORD
						(
							BOOK_ID, KEEPER_ID, LEND_DATE,
							CRE_DATE, CRE_USR, MOD_DATE, MOD_USR
						)
						VALUES
						(
							@BOOK_ID, @KEEPER_ID, GETDATE(),
							GETDATE(), 'Admin', GETDATE(), 'Admin'
						)";

						var lendParams = new Dictionary<string, object>
						{
							{ "@BOOK_ID", book.BookId },
							{ "@KEEPER_ID", book.BookKeeperId ?? string.Empty }
						};

						conn.Execute(sql, lendParams);
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		public void DeleteBookById(int bookId)
		{
			using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
			{
				string sql = @"DELETE FROM BOOK_DATA WHERE BOOK_ID = @BOOK_ID";

				var parameter = new Dictionary<string, object>
				{
					{ "@BOOK_ID", bookId }
				};

				conn.Execute(sql, parameter);
			}
		}
	}
}
