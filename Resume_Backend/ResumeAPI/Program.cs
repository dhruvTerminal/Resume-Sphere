using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ResumeAPI.Data;
using ResumeAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Environment-aware Configuration Helpers ─────────────────────────────────
static string? FirstNonEmpty(params string?[] values)
{
    return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}

var defaultConnection = FirstNonEmpty(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    builder.Configuration["ConnectionStrings:DefaultConnection"],
    builder.Configuration["DATABASE_URL"]
);

if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "Database connection string is missing. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");
}

var jwtToken = FirstNonEmpty(
    builder.Configuration["AppSettings:Token"],
    builder.Configuration["AppSettings__Token"],
    builder.Configuration["JWT_SECRET"]
);

if (string.IsNullOrWhiteSpace(jwtToken) || jwtToken.Length < 32)
{
    throw new InvalidOperationException(
        "JWT secret is missing or too short. Set AppSettings__Token or JWT_SECRET (minimum 32 chars).");
}

var aiServiceUrl = FirstNonEmpty(
    builder.Configuration.GetSection("AiService:BaseUrl").Value,
    builder.Configuration["AiService__BaseUrl"],
    builder.Configuration["AI_SERVICE_URL"],
    "http://localhost:8000"
)!;

// ─── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

// ─── Application Services ──────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IResumeParserService, ResumeParserService>();
builder.Services.AddScoped<ISkillExtractorService, SkillExtractorService>();
builder.Services.AddScoped<IJobMatchService, JobMatchService>();
builder.Services.AddScoped<ISkillGapService, SkillGapService>();
builder.Services.AddScoped<ILearningResourceService, LearningResourceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<ILearningPlanService, LearningPlanService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IResumeGenerationService, ResumeGenerationService>();
builder.Services.AddScoped<IUploadModerationService, UploadModerationService>();

// ─── AI Service (Python FastAPI) ───────────────────────────────────────────────
builder.Services.AddHttpClient<IAiAnalysisService, AiAnalysisService>(client =>
{
    client.BaseAddress = new Uri(aiServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
});

// ─── Authentication ────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtToken)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// ─── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Smart Resume Matcher API",
        Version     = "v1",
        Description = "Upload resumes, extract skills, match jobs, detect skill gaps, and get learning resources."
    });

    // Add JWT support to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// ─── CORS ──────────────────────────────────────────────────────────────────────
var frontendUrl = builder.Configuration["FRONTEND_URL"];
var allowedOrigins = new List<string>
{
    "http://localhost:5173",
    "http://127.0.0.1:5173",
    "http://localhost:5174",
    "http://127.0.0.1:5174"
};

if (!string.IsNullOrWhiteSpace(frontendUrl))
{
    allowedOrigins.Add(frontendUrl);
}

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins.Distinct().ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Resume Matcher API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
