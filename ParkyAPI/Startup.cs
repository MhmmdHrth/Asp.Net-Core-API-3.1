using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParkyAPI.Data;
using ParkyAPI.ParkyMapper;
using ParkyAPI.Repository;
using ParkyAPI.Repository.IRepository;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace ParkyAPI
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
            services.AddCors();

            services.AddDbContext<ApplicationDbContext>
                (options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<INationalParkRepository, NationalParkRepository>();
            services.AddScoped<ITrailRepository, TrailRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            //----------------------------------------------------------------------API SWAGGER--------------------------------------------------------------------------------------------//

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            //-----------------------------------------------------------------------JWT TOKEN---------------------------------------------------------------------------------------------//

            var appSettingsSection = Configuration.GetSection("AppSettings");

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret); //get value from appsettings.json secret

            services.Configure<AppSettings>(appSettingsSection);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------//

            services.AddAutoMapper(typeof(ParkyMappings));

            //services.AddSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("ParkyOpenAPISpec",
            //        new Microsoft.OpenApi.Models.OpenApiInfo()
            //        {
            //            Title = "Parky API",
            //            Version = "1",
            //            Description = "First .Net Core API Project",
            //            Contact = new Microsoft.OpenApi.Models.OpenApiContact()
            //            {
            //                Email = "mhmmdhrth99@gmail.com",
            //                Name = "Muhammad Harith",
            //                Url = new Uri("https://www.linkedin.com/in/harith-jamdil-a500b5190/")
            //            }
            //        });

            //    //options.SwaggerDoc("ParkyOpenAPISpecTrails",
            //    //    new Microsoft.OpenApi.Models.OpenApiInfo()
            //    //    {
            //    //        Title = "Parky API (Trails)",
            //    //        Version = "1",
            //    //        Description = "First .Net Core API Project",
            //    //        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
            //    //        {
            //    //            Email = "mhmmdhrth99@gmail.com",
            //    //            Name = "Muhammad Harith",
            //    //            Url = new Uri("https://www.linkedin.com/in/harith-jamdil-a500b5190/")
            //    //        }
            //    //    });

            //    var xmlCommentFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentFile);

            //    options.IncludeXmlComments(xmlCommentsFullPath);
            //});

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                                                desc.GroupName.ToUpperInvariant());
                }

                options.RoutePrefix = "";
            });
            //app.UseSwaggerUI(options =>
            //{
            //    options.SwaggerEndpoint("/swagger/ParkyOpenAPISpec/swagger.json", "Parky API");
            //    //options.SwaggerEndpoint("/swagger/ParkyOpenAPISpecTrails/swagger.json", "Parky API trails");
            //    options.RoutePrefix = "";
            //});

            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}