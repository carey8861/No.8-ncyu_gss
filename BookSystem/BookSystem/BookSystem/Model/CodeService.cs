using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookSystem.Model
{
    public class CodeService
    {
        /// <summary>
        /// 取得資料庫連線字串
        /// </summary>
        private string GetDBConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            return config.GetConnectionString("DBConn");
        }

        /// <summary>
        /// 取得書籍狀態 (BookStatus)
        /// </summary>
        public List<Code> GetBookStatusData()
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = "Select CODE_ID As Value, CODE_NAME As Text From BOOK_CODE Where CODE_TYPE = 'BOOK_STATUS'";
                return conn.Query<Code>(sql).ToList();
            }
        }

        /// <summary>
        /// 取得書籍類別 (BookClass)
        /// </summary>
        public List<Code> GetBookClassData()
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = "Select BOOK_CLASS_ID As Value, BOOK_CLASS_NAME As Text From BOOK_CLASS";
                return conn.Query<Code>(sql).ToList();
            }
        }

        /// <summary>
        /// 取得借閱人 (Keeper)
        /// 修正：確保方法名稱為 GetBookKeeperData 以配合 Controller 的呼叫
        /// </summary>
        public List<Code> GetBookKeeperData()
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                // 將中文名與英文名組合顯示，例如: 王小明 (David)
                string sql = "Select USER_ID As Value, USER_CNAME + ' (' + USER_ENAME + ')' As Text From MEMBER_M";
                return conn.Query<Code>(sql).ToList();
            }
        }
    }
}