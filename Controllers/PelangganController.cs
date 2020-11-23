using Microsoft.AspNetCore.Mvc;
using VcWebApi.responsitory;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VcWebApi.Models;
using System.IO;
using System.Text;
using System;
namespace VcWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Pelanggan : ControllerBase
    {
        [FromHeader]
        public string key { get; set; }
        private readonly iresponsitory _db;
        private IConfiguration _config;
        private readonly ILogger<Pelanggan> _log;
        public Pelanggan(iresponsitory db, IConfiguration cfg, ILogger<Pelanggan> log)
        {
            _db = db;
            _config = cfg;
            _log = log;

        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Pelanggan>> GetPelanggan()
        {
            string cfg_key = _config.GetValue<string>("api_key:key");
            fData _res = new fData();
            ActionResult response = Unauthorized();
            try
            {
                if (key == cfg_key)
                {
                    var result = await _db.getdata();
                    if (result != null)
                    {
                        _res = new fData();
                        _res.data = result;
                        _res.rc = "00";
                        _res.desc = "Data berhasil ditemukan";
                        response = Ok(_res);
                    }
                    else
                    {
                        _res = new fData();
                        _res.data = result;
                        _res.rc = "05";
                        _res.desc = "Data tidak ditemukan";
                        response = StatusCode(400, _res);
                    }

                }
                else
                {
                    _res = new fData();
                    _res.rc = "05";
                    _res.desc = "Apikey tidak sesuai";
                    response = StatusCode(401, _res);
                }

            }
            catch (MyException ex)
            {
                _log.LogError(ex.Message.ToString());
                _res = new fData();
                _res.rc = "05";
                _res.desc = "Internal system Error";
                response = StatusCode(501, _res);
            }
            string json2 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            _log.LogInformation("Response Sign On to Client <== " + json2.ToString());
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<LoginResponse>> Login(Login data)
        {
            string cfg_key = _config.GetValue<string>("api_key:key");
            string json = string.Empty;
            json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            _log.LogInformation("Request login <==" + json);
            fData _res = new fData();
            ActionResult response = Unauthorized();
            CryptLib scry = new CryptLib();
            try
            {
                if (key == cfg_key)
                {
                    string desc=scry.decrypt(data.password);
                    var result = await _db.getlogin(data.username, data.password);
                    if (result != null)
                    {
                        result.token = GenerateToken(data);
                        _res = new fData();
                        _res.data = result;
                        _res.rc = "00";
                        _res.desc = "Login berhasil dilakukan";
                        response = Ok(_res);
                    }
                    else
                    {
                        _res = new fData();
                        _res.data = result;
                        _res.rc = "05";
                        _res.desc = "Login gagal dilakukan";
                        response = StatusCode(400, _res);
                    }

                }
                else
                {
                    _res = new fData();
                    _res.rc = "05";
                    _res.desc = "Apikey tidak sesuai";
                    response = StatusCode(401, _res);
                }

            }
            catch (MyException ex)
            {
                _log.LogError(ex.Message.ToString());
                _res = new fData();
                _res.rc = "05";
                _res.desc = "Internal system Error";
                response = StatusCode(501, _res);
            }
            string json2 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            _log.LogInformation("Response Login <== " + json2.ToString());
            return response;
        }
        private string GenerateToken(Login userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtToken:SecretKey"]));
            var creadentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim (JwtRegisteredClaimNames.Sub,userInfo.username),
                new Claim (JwtRegisteredClaimNames .Jti ,Guid .NewGuid ().ToString ()),
                new Claim (JwtRegisteredClaimNames.Sub,userInfo.password),
                //new Claim ("DateOfJoin",userInfo.DateOfJoin.ToString ("yyyy-MM-dd")),
                new Claim ("DateOfJoin",DateTime.Now.ToString ("yyyy-MM-dd"))
            };

            var token = new JwtSecurityToken(_config["JwtToken:Issuer"],
                _config["JwtToken:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: creadentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}