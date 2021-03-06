using GerenciadorDeCursos.Data;
using GerenciadorDeCursos.Interfaces;
using GerenciadorDeCursos.Models;
using GerenciadorDeCursos.Repositories;
using GerenciadorDeCursos.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GerenciadorDeCursos
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Update JWT config from the settings
            services.Configure<JwtConfig>(Configuration.GetSection("JwtConfig"));

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers().AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GerenciadorDeCursos", Version = "v1" });

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", Array.Empty<string>()},
                };
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                             Reference = new OpenApiReference
                             {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                             },
                             Scheme = "oauth2",
                             Name = "Bearer",
                             In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt =>
            {   
                // Getting the secret
                var key = Encoding.ASCII.GetBytes(Configuration["JwtConfig:Secret"]);

                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {   
                    //Cheking if the jwt genereted is issued by secret key from server
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, //todo update
                    ValidateAudience = false, //todo update
                    RequireExpirationTime = false, //todo update
                    ValidateLifetime = true

                };

            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GerenciadorDeCursos v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
