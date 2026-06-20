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
