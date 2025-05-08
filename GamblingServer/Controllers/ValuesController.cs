using GamblingServer.DB;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings;

namespace GamblingServer.Controllers
{
    public class authRequestModel
    {
        public string username { get; set; }
        public string password { get; set; }

    }
    [Route("auth/")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly RandomNumberGenerator _rng= RandomNumberGenerator.Create();
        private readonly GamblingContext _context;
        public LoginController(GamblingContext context)
        {
            _context = context;
        }
        [HttpPost("register")]
        public ActionResult user_register([FromBody] authRequestModel model)
        {
            try
            {
                _context.user.Add(new User
                {
                    username = model.username,
                    passhash = HashPassword(model.password, _rng),
                    creation_time = DateTime.Now,
                });
                _context.SaveChanges();
            }
            catch (MySqlException ex) {
                return BadRequest("Username already used");
            }
            return Content("User created: "+model.username);
        }
        [HttpPost("login")]
        public ActionResult user_login([FromBody] authRequestModel model) {
            var user_querry = _context.user.Where(usr => usr.username == model.username).FirstOrDefault();
            if (user_querry!=null&&Verify(model.password, user_querry.passhash))
            {
                return Content("Access granted");
            }
            else {
                var result = Content("Wrong username or password");
                result.StatusCode = 403;
                return result;
            }
        }
        private static bool Verify(string password, byte[] db_res)
        {
            const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA1; // default for Rfc2898DeriveBytes
            const int Pbkdf2IterCount = 1000; // default for Rfc2898DeriveBytes
            const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
            const int SaltSize = 128 / 8; // 128 bits

            if (password != null && db_res != null) {
                var salt= new byte[SaltSize];
                var verificationBytes = new byte[SaltSize + Pbkdf2SubkeyLength];
                Buffer.BlockCopy(db_res, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(db_res, 0, verificationBytes, 0, SaltSize);
                var subkey= KeyDerivation.Pbkdf2(password, salt, Pbkdf2Prf,Pbkdf2IterCount, Pbkdf2SubkeyLength);
                Buffer.BlockCopy(subkey, 0, verificationBytes, SaltSize, Pbkdf2SubkeyLength);
                if (db_res.SequenceEqual(verificationBytes))
                {
                    return true;
                }
            }
            return false;
        }
        private static byte[] HashPassword(string password, RandomNumberGenerator rng)
        {
            const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA1; // default for Rfc2898DeriveBytes
            const int Pbkdf2IterCount = 1000; // default for Rfc2898DeriveBytes
            const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
            const int SaltSize = 128 / 8; // 128 bits

            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, Pbkdf2Prf, Pbkdf2IterCount, Pbkdf2SubkeyLength);

            var outputBytes = new byte[SaltSize + Pbkdf2SubkeyLength];
            Buffer.BlockCopy(salt, 0, outputBytes, 0, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, SaltSize, Pbkdf2SubkeyLength);
            return outputBytes;
        }

    }
}
