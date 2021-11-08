using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DataAccessLayer.Tests
{
    class SqlServerDataContext : IDataContext<IDbConnection>
    {
        private readonly string _connectionString;

        public SqlServerDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Open()
        {
            SqlConnection connection = new(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
