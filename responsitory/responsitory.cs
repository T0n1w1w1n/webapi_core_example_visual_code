using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Dapper;
using Npgsql;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VcWebApi.Models;

namespace VcWebApi.responsitory
{
    public interface iresponsitory
    {
        Task<List<Pelanggan>> getdata();
        Task<LoginResponse> getlogin(string username, string password);
    }
    public class cresponsitory : iresponsitory
    {
        private readonly IConfiguration _config;
        public cresponsitory(IConfiguration config)
        {
            _config = config;
        }
        public IDbConnection Connection
        {
            get
            {
                var ConnectionString = _config.GetValue<string>("ConnectionStrings:Connection");
                return new NpgsqlConnection(ConnectionString);
            }
        }
        public async Task<List<Pelanggan>> getdata()
        {
            string QueryData = "SELECT ID,NAMA,NOHP,ALAMAT FROM M_PELANGGAN";
            using (IDbConnection conn = Connection)
            {
                var result = await conn.QueryAsync<Pelanggan>(QueryData);
                return result.ToList();
            }
        }
        public async Task<LoginResponse> getlogin(string username, string password)
        {

            string QueryData = " SELECT B.ID AS ID_PELANGGAN,B.NAMA AS NAMA_PELANGGAN,A.USERNAME FROM USERS_PELANGGAN A "
                             + " LEFT JOIN M_PELANGGAN B ON A.ID_PELANGGAN=B.ID"
                             + " WHERE LOWER(A.USERNAME)='" + username.ToLower() + "' "
                             + " AND A.PASSWORD='" + password + "'";

            using (IDbConnection conn = Connection)
            {

                var result = await conn.QueryAsync<LoginResponse>(QueryData);
                return result.FirstOrDefault();
            }
        }
    }
}