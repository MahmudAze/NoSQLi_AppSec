using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NoSQLinjection.Models;

namespace NoSQLinjection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;

        public AuthController(IMongoDatabase database)
        {
            // Users kolleksiyasina qosuluruq
            _usersCollection = database.GetCollection<User>("Users");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Modelin düzgünlüyünü yoxlayırıq (String olub-olmaması artıq DTO tərəfindən təmin edilir)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Təhlükəsiz sorğu (Query) yaradılması
            // Builders istifadə etdikdə, MongoDB sürücüsü daxil olan dəyərləri operator ("$ne") kimi yox, 
            // YALNIZ saf mətn (value) kimi qəbul edir.
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Username, request.Username),
                Builders<User>.Filter.Eq(u => u.Password, request.Password)
            );

            // 3. İstifadəçinin axtarılması
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Istifadeci adi ve ya parol sehvdir."
                });
            }

            return Ok(new { message = "Login successful!", redirect = "/dashboard" });


        }
    }
}
