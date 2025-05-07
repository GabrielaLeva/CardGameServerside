using GamblingServer.DB;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings;

namespace GamblingServer.Controllers
{
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
        public ActionResult user_register(string username, string password) {
                _context.Database.EnsureCreated();
                _context.user.Add(new User
                {
                    username = username,
                    passhash = HashPassword(password, _rng),
                    creation_time = DateTime.Now,
                });
                _context.SaveChanges();
            return Content("User created: "+username);
        }
        [HttpPost("login")]
        public ActionResult user_login(string username, string password) {
            var user_querry = _context.user.Where(usr => usr.username == username).FirstOrDefault();
            if (user_querry!=null&&Verify(password, user_querry.passhash))
            {
                return Content("Access granted");
            }
            else { return Content("Wrong username or password."); }
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
