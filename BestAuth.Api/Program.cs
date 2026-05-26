
using BestAuth.Api.Handlers;
using BestAuth.Application.Abstracts;
using BestAuth.Application.Constants;
using BestAuth.Api.Hubs;
using BestAuth.Application.Services;
using BestAuth.Domain.Entities;
using BestAuth.Infrastructure;
using BestAuth.Infrastructure.Options;
using BestAuth.Infrastructure.Processors;
using BestAuth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace BestAuth.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var jwtSection = builder.Configuration.GetSection(JwtOptions.JwtOptionsKey);
            builder.Services.Configure<JwtOptions>(jwtSection);

            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddOpenApi();

            var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? ""; // TODO throw error
            builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connection));

            builder.Services.AddIdentity<User, Role>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 1;

                o.User.RequireUniqueEmail = false;
                o.SignIn.RequireConfirmedEmail = false;
            }).AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new ArgumentException(nameof(JwtOptions));
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[CookieNames.Access];
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); 
                });
            });

            builder.Services.AddAuthorization();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRequestRepository, RequestRepository>();
            builder.Services.AddScoped<IRequestService, RequestService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }

                SeedData.EnsureSeedDataAsync(services).GetAwaiter().GetResult();
            }

            //if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            app.UseCors("FrontendPolicy");

            app.UseExceptionHandler(_ => { });
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<RequestsHub>("/hubs/requests");
            app.MapControllers();

            app.Run();
        }
    }
}
