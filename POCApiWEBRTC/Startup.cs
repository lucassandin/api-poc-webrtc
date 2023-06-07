using Microsoft.OpenApi.Models;
using POCApiWEBRTC.Infra;

namespace POCApiWEBRTC;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        #region config

        services.AddDatabaseConfig(Configuration);

        #endregion config

        services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

        services.AddSingleton(Configuration);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Minimal API POC WebRtc",
            });
        });

        var myOriginCors = "_myAllowSpecificOrigins";

        services.AddCors(options =>
        {
            options.AddPolicy(name: myOriginCors,
                policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.WithOrigins(
                        "http://localhost:3000",
                        "https://localhost:3000",
                        "http://localhost:3000",
                        "https://localhost:3000/",
                        "https://poc-webrtc.vercel.app",
                        "https://poc-webrtc.vercel.app/"
                        );
                });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors(x =>
            x.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins("http://localhost:3000",
                        "https://localhost:3000",
                        "http://localhost:3000",
                        "https://localhost:3000/",
                        "https://poc-webrtc.vercel.app",
                        "https://poc-webrtc.vercel.app/"
                        ));

        app.UseAuthentication();
        app.UseAuthorization();

        #region configMigration

        using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        #endregion configMigration

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("./v1/swagger.json", "v1");
            options.RoutePrefix = "swagger";
        });
    }
}