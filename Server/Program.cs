using BravusApp.Server.Data;
using BravusApp.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IDutiesService, DutiesService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Bravus API", Version = "v1" });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no campo abaixo.\nExemplo: Bearer xxxxx.yyyyy.zzzzz"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtKey"] ?? "super_secret_key_bravus_2025");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // true em produção
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=plantao.db"));


var app = builder.Build();
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bravus API v1"); // caminho absoluto
    c.RoutePrefix = "admin"; 
});


if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseWebAssemblyDebugging();



app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "-1");
    },
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = { [".webmanifest"] = "application/manifest+json" }
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToFile("index.html");

app.MapRazorPages();
app.MapControllers();

app.Run();
