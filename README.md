![Compass Setup](./Attachments/Pasted%20image%2020260620195846.png)
We create database and collection in MongoDB Compass. 

![Compass Setup](./Attachments/Pasted%20image%2020260620195858.png)

We insert new data. Then we test in Open API:

![Compass Setup](./Attachments/Pasted%20image%2020260620200225.png)

It worked! Let's check if it authenticates:

![Compass Setup](./Attachments/Pasted%20image%2020260620200259.png)

As you see in the above picture, it checks if the user is authenticated or not. 

So, let's attack!

![Compass Setup](./Attachments/Pasted%20image%2020260620200516.png)

The attack works successfully. So, there is vulnerable code:

```csharp
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
```


Therefore, it is the simplest project for understanding the attack. So, I am going to create frontend via React framework. I am going to adapt this project to a modern real-world scenario:

I ran npm and opened website:
![Compass Setup](./Attachments/Pasted%20image%2020260620201212.png)

I am going to make Login page in `src/App.jsx`:
```jsx
import { useState } from 'react'
import './App.css'

function App() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [error, setError] = useState('')

  const handleLogin = async (e) => {
    e.preventDefault()
    setError('')

    try {
      const response = await fetch('http://localhost:5045/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        // Frontend normal string göndərir. Hücumçu bunu arxada intercept edib JSON obyektinə çevirəcək.
        body: JSON.stringify({ username, password }),
      })

      const data = await response.json()

      if (response.ok) {
        setIsLoggedIn(true)
      } else {
        setError(data.message || 'Login failed')
      }
    } catch (err) {
      setError('Network error. Is the API running?')
    }
  }

  if (isLoggedIn) {
    return (
      <div className="dashboard">
        <h1>🚨 Admin Dashboard 🚨</h1>
        <p>Welcome! You have successfully bypassed the authentication.</p>
        <div className="secret-data">
          <h3>Top Secret Users:</h3>
          <ul>
            <li>admin - SuperSecretPassword123</li>
            <li>flag{n0sq1_1nj3ct10n_m4st3r}</li>
          </ul>
        </div>
        <button onClick={() => setIsLoggedIn(false)}>Logout</button>
      </div>
    )
  }

  return (
    <div className="login-container">
      <h2>System Login</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleLogin}>
        <div>
          <input
            type="text"
            placeholder="Username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        <br />
        <div>
          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <br />
        <button type="submit">Login</button>
      </form>
    </div>
  )
}

export default App
```

![Compass Setup](./Attachments/Pasted%20image%2020260620201417.png)

As you see, it is login page with no design.

## Allowing CORS in API

React (localhost:5173) and .NET API (localhost:5045) work in different ports, so browser will automatically trigger error and block them. To solve this, I am going to modify code in Program.cs:

```csharp
using MongoDB.Driver;
using Scalar.AspNetCore;

namespace NoSQLinjection
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // MongoDB baglantisini qeydiyyatdan keciririk
            builder.Services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = new MongoClient("mongodb://localhost:27017");

                // melumat bazasinin adini bura yaziriq (eger yoxdursa, avtomatik yaradilacaq)
                return client.GetDatabase("NoSQLInjection_TestDB");
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
```


## Attacking via Burp Suite

So, I launched Burp Suite, opened new Browser window, and intercepted to localhost:

![Compass Setup](./Attachments/Pasted%20image%2020260620214128.png)

![Compass Setup](./Attachments/Pasted%20image%2020260620214143.png)

As you see, burp suite doesn't see the localhost. I did a little research for this issue. So I modified the URL to the following URL:

```
http://[::1]:5173/
```

![Compass Setup](./Attachments/Pasted%20image%2020260620214250.png)

So, it worked.

![Compass Setup](./Attachments/Pasted%20image%2020260620214358.png)

I entered the payload:

```json
{
	"username": {"$ne": null}, 
	"password": {"$ne": null}
}
```

![Compass Setup](./Attachments/Pasted%20image%2020260620214548.png)

I forwarded the dangerous request. Here is the result:

![Compass Setup](./Attachments/Pasted%20image%2020260620214626.png)

> 📌 **Important Note on Exploitation:** > If you input `{"$ne": null}` directly into the browser's input fields, the attack will fail. This is because React serializes the input as a literal **String** (`"{\"$ne\": null}"`). To successfully execute the injection, you must intercept the request using a tool like **Burp Suite** and modify the JSON structure so that the value becomes a native **JSON Object** (`{ "$ne": null }`). This forces the MongoDB driver to interpret it as a query operator rather than a plain string.

# Mitigations

The most effective and industry-standard way to prevent `NoSQL` injection in `.NET` is to use **Strongly Typed Models** and **MongoDB Builders**.

## 1. Create User model

In the Models folder of your project (where you just created LoginRequest), create a new file called User.cs and place this code inside it:

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLinjection.Models
{
    public class User
    {
        // MongoDB-nin avtomatik yaratdığı "_id" sahəsini C#-a belə tanıdırıq
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
```
## 2. DTO (Data Transfer Object)

First, we create a model (DTO) that will only accept incoming data as Strings. If someone tries to send an object (`{"$ne": null}`) with Burp Suite, .NET's built-in Model Binder mechanism will automatically return a 400 Bad Request without even running the code.

```csharp
using System.ComponentModel.DataAnnotations;

namespace NoSQLinjection.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
```

## 3. Secure Controller Code

Now we change the Login method in `AuthController.cs` like this. Here, instead of using raw `BsonDocument`, we use MongoDB's special Builders class.

```csharp
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
            // Baza bağlantısı
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
                return Unauthorized(new { message = "Istifadeci adi ve ya parol sehvdir." });
            }

            return Ok(new { message = "Login successful!", redirect = "/dashboard" });
        }
    }
}
```

- **Type Safety**: The properties inside `LoginRequest` are strictly defined as strings. If a malicious payload comes in the form of a JSON object (`{...}`), .NET will reject it immediately instead of crashing it because it cannot convert it to a `string`.

- **Parameterized Queries**: The `Builders<User>.Filter.Eq` method "escapes" the value behind the scenes. Even if someone somehow passes in quotes and sends `"$ne"` as a string, MongoDB will look for it as "*user whose name is literally $ne*" and of course it will not find anything.

## RESULT

![](./Attachments/Pasted%20image%2020260622002122.png)

