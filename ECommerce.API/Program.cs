using ECommerce.API.Extensions;
using ECommerce.BLL.Extensions;
using ECommerce.DAL.Extensions;
using ECommerce.DAL.Models.AppDbContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace ECommerce.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Add N-Tier service extensions
            builder.Services.AddBusinessLogicServices();
            builder.Services.AddDataAccessServices(builder.Configuration);
            builder.Services.AddPresentationServices(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
                context.Database.EnsureCreated();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // For serving product images

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ECommerce.API.Middleware.ExceptionMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}