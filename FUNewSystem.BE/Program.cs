
using FluentValidation.AspNetCore;
using FUNewsSystem.Domain.Configs;
using FUNewsSystem.Domain.Consts;
using FUNewsSystem.Domain.Extensions.SystemAccounts;
using FUNewsSystem.Domain.Models;
using FUNewsSystem.Infrastructure.DataAccess;
using FUNewsSystem.Infrastructure.Messaging;
using FUNewsSystem.Infrastructure.Repositories.CategoryRepo;
using FUNewsSystem.Infrastructure.Repositories.InvalidatedTokenRepo;
using FUNewsSystem.Infrastructure.Repositories.NewsArticleRepo;
using FUNewsSystem.Infrastructure.Repositories.NotificationRepo;
using FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo;
using FUNewsSystem.Infrastructure.Repositories.TagRepo;
using FUNewsSystem.Infrastructure.Services;
using FUNewsSystem.Service.AutoMapper;
using FUNewsSystem.Service.Jobs;
using FUNewsSystem.Service.Services.AuthService;
using FUNewsSystem.Service.Services.AuthService.AzureRedisTokenStoreService;
using FUNewsSystem.Service.Services.AuthService.BlacklistTokenService;
using FUNewsSystem.Service.Services.AuthService.TwoFactorAuthService;
using FUNewsSystem.Service.Services.CategoryService;
using FUNewsSystem.Service.Services.ConfigService;
using FUNewsSystem.Service.Services.HttpContextService;
using FUNewsSystem.Service.Services.NewsArticleService;
using FUNewsSystem.Service.Services.NotificationService;
using FUNewsSystem.Service.Services.SystemAccountService;
using FUNewsSystem.Service.Services.TagService;
using FUNewSystem.BE.Middlewares;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FUNewSystem.BE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddOData(options =>
                    options.Select()
                           .Filter()
                           .Count()
                           .OrderBy()
                           .Expand()
                           .SetMaxTop(100))
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "News Swagger API",
                    Description = "An ASP.NET Core Web API for FU News System App",
                    TermsOfService = new Uri("https://example.com/terms"),
                    License = new OpenApiLicense
                    {
                        Name = "Example License",
                        Url = new Uri("https://example.com/license")
                    }
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
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


            //Add cors
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                                "https://localhost:44352",
                                "https://localhost:7157",
                                "https://funewssystem.azurewebsites.net"
                            )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Add Database configuration
            builder.Services.AddDbContext<FunewsSystemApiDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("FUNewsConnection"));
            });

            //Add Config Azure Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(
                builder.Configuration.GetConnectionString("CacheConnection")!));

            var secret = builder.Configuration["Jwt:SecretKey"]!.Trim();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            //Add Authenticaion and Authorization
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = AuthSchemes.Cookie;
            })
                .AddCookie(AuthSchemes.Cookie)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationpublisher"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var accountId = context.Principal?.FindFirst("AccountId")?.Value;
                            Console.WriteLine($"[OnTokenValidated] AccountId: {accountId}");
                            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                            var blacklistService = context.HttpContext.RequestServices.GetRequiredService<IBlacklistTokenService>();
                            if (!string.IsNullOrEmpty(jti))
                            {
                                var isBlacklisted = await blacklistService.IsBlacklistedAsync(jti);
                                if (isBlacklisted)
                                {
                                    context.Fail("Token is blacklisted.");
                                }
                            }
                        }
                    };

                })
                .AddGoogle(AuthSchemes.Google, options =>
                {
                    options.ClientId = builder.Configuration["AuthSettings:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["AuthSettings:Google:ClientSecret"];
                    options.CallbackPath = "/Auth/signIn-google";
                })
                .AddFacebook(AuthSchemes.Facebook, options =>
                {
                    options.AppId = builder.Configuration["AuthSettings:Facebook:AppId"];
                    options.AppSecret = builder.Configuration["AuthSettings:Facebook:AppSecret"];
                    options.CallbackPath = "/Auth/signIn-facebook";
                    options.Events = new OAuthEvents
                    {
                        OnRemoteFailure = context =>
                        {
                            context.Response.Redirect("/dang-nhap");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

            // Add Http Context Accessor
            builder.Services.AddHttpContextAccessor();

            // Add Fluent Validation
            builder.Services.AddFluentValidationAutoValidation();

            //Repository
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            //Service
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IConfigService, ConfigService>();
            builder.Services.AddScoped<IHttpContextService, HttpContextService>();
            builder.Services.AddScoped<IBlacklistTokenService, BlacklistTokenService>();
            builder.Services.AddScoped<IRefreshTokenStoreSerivce, AzureRedisRefreshTokenStoreService>();
            builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            //Config automapper
            builder.Services.AddAutoMapper(typeof(CategoryProfile));
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddAutoMapper(typeof(NotificationProfile));

            //UserId
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            //Config
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

            // Thêm SignalR
            builder.Services.AddSignalR();

            // MailService
            builder.Services.AddScoped<IMailService, MailService>();

            // Hangfire Job
            builder.Services.AddHangfire(config =>
            {
                //var cs = builder.Configuration.GetConnectionString("FUNewsConnection");
                //if (string.IsNullOrEmpty(cs))
                //    throw new Exception("Missing FUNewsConnection for Hangfire.");

                //config.UseSqlServerStorage(cs);
                config.UseMemoryStorage();
            });

            builder.Services.AddHangfireServer();

            builder.Services.AddScoped<IPostJob, PostJob>();
           


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseGlobalExceptionHandler();
            app.UseHttpsRedirection();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();
            // 🔥 QUAN TRỌNG: Đặt trước MapControllers()
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            app.MapControllers();
            app.MapHub<NotificationPublisher>("/notificationpublisher");

            app.Run();
        }
    }
}
