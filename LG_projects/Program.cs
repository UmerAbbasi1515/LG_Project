using LG_projects.Classes;
using LG_projects.Classes.Token;
using LG_projects.Common.CrossSiteScriptingValidation;
using LG_projects.Common.ListConvertor;
using LG_projects.DAL;
using LG_projects.Repository.Auth;
using LG_projects.Repository.Profile;
using LG_projects.Repository.Project;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//*********************// Custom***************************//

builder.Services.AddScoped<IDBLogics, DBLogics>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IListConvertor, ListConvertor>();
builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<IProjectRepo, ProjectRepo>();
builder.Services.AddScoped<IProfileRepo, ProfileRepo>();
builder.Services.AddScoped<Settings>();
builder.Services.AddScoped<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// ✅ Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//*********************// Custom***************************//
// ✅ Add Authentication
app.UseAuthentication();
app.UseAuthorization();
//*********************// Custom***************************//

app.MapControllers();

app.Run();
