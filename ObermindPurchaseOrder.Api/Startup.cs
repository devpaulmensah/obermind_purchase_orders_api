using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using ObermindPurchaseOrder.Api.Configurations;
using ObermindPurchaseOrder.Api.Database;
using ObermindPurchaseOrder.Api.Models.Responses;
using ObermindPurchaseOrder.Api.Services.Interfaces;
using ObermindPurchaseOrder.Api.Services.Providers;

namespace ObermindPurchaseOrder.Api
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
            services.AddControllers();
            services
                .AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(Configuration.GetConnectionString("DbConnection"));
                    }
                    , ServiceLifetime.Transient).AddUnitOfWork<ApplicationDbContext>();
            
            services.AddAuthentication(x =>
                    {
                        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = async context =>
                            {
                                await Task.Delay(0);
                                
                                var user = context.Principal.FindFirst(c => c.Type == ClaimTypes.Thumbprint).Value;
                                var userData = JsonConvert.DeserializeObject<UserResponse>(user);
                                
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Thumbprint, JsonConvert.SerializeObject(userData)),
                                };
                                
                                var appIdentity = new ClaimsIdentity(claims, "obermind_user");
                                context.Principal.AddIdentity(appIdentity);
                            }
                        };
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidIssuer = Configuration["BearerTokenConfig:Issuer"],
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["BearerTokenConfig:Key"])),
                            ValidAudience = Configuration["BearerTokenConfig:Audience"],
                        };
                    });
            
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            
            services.Configure<PurchaseOrderLimits>(Configuration.GetSection("PurchaseOrderLimits"));
            services.Configure<BearerTokenConfig>(Configuration.GetSection("BearerTokenConfig"));
            
            services.AddAutoMapper(typeof(Startup));
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Obermind Purchase Order Api",
                    Description = "This is the api documentation for obermind purchase order api",
                });
                
                c.EnableAnnotations();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Provide bearer token to access endpoints",
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
                            Scheme = "oauth2",
                            Name = "Bearer",
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Obermind Purchase Order Api");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}