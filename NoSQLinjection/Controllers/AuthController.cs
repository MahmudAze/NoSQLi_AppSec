using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace NoSQLinjection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _usersCollection;

        public AuthController(IMongoDatabase database)
        {
            // Users kolleksiyasina qosuluruq
            _usersCollection = database.GetCollection<BsonDocument>("Users");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement payload)
        {
            // TEHLUKELI: Gelen JSON melumatini birbasa xam metne ceviririk
            var jsonString = payload.GetRawText();

            // Zeiflik buradadir: Xam metni hec bir tip yoxlamasi etmeden birbasa MongoDB sorgusuna (BsonDocument) ceviririk
            var queryDocument = BsonDocument.Parse(jsonString);

            // Sorgunu kor-korane icra edirik
            var user = await _usersCollection.Find(queryDocument).FirstOrDefaultAsync();

            if (user != null)
            {
                // Eger user tapildisa (ve ya injection isledikce), giris ugurlu olur
                return Ok(new
                {
                    message = "Login ugurlu!",
                    redirect = "/dashboard"
                });
            }

            return Unauthorized(new
            {
                message = "Istifadeci adi ve ya parol sehvdir."
            });
        }
    }
}
