
﻿using Microsoft.Extensions.Configuration;

using System.Data;
using System.Data.SqlClient;

namespace SIPROSHARED.DbContext
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }


        public IDbTransaction BeginTransaction()
        {
            var connection = CreateConnection();
            connection.Open();
            return connection.BeginTransaction();
        }
    }

}
