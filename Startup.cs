﻿using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using EvanLindseyApi.Hubs;
using EvanLindseyApi.Models;

namespace EvanLindseyApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Environment.GetEnvironmentVariable("DATABASE");
            string secret = Environment.GetEnvironmentVariable("SECRET");
            var key = Encoding.UTF8.GetBytes(secret);

            services.AddDbContext<DataContext>(options => options.UseMySql(connection));

            services.Configure<AuthSettings>(options => options.SECRET = secret);

            services.AddCors();

            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(cfg =>
                    {
                        cfg.RequireHttpsMetadata = false;
                        cfg.SaveToken = true;
                        cfg.TokenValidationParameters = new TokenValidationParameters()
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuerSigningKey = true,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuer = false,
                        };
                    });

            services.AddSignalR();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            string clientHost;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                clientHost = "http://localhost:4200";
            }
            else
                clientHost = Environment.GetEnvironmentVariable("CLIENT_HOST");

            app.UseCors(cors =>
                        cors.WithOrigins(clientHost)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSignalR(routes => routes.MapHub<MessageHub>("/messagehub"));

            app.UseMvc();
        }
    }
}
