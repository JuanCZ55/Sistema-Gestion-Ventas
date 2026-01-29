using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar DbContext con PostgreSQL
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton<Supabase.Client>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var url = config["Supabase:Url"];
    ArgumentNullException.ThrowIfNull(url);
    var key = config["Supabase:Key"];
    ArgumentNullException.ThrowIfNull(key);
    var options = new SupabaseOptions { AutoConnectRealtime = false };
    var supabase = new Supabase.Client(url, key, options);
    supabase.InitializeAsync().Wait();
    return supabase;
});

builder.Services.AddSingleton<SupabaseStorageService>();

builder.Services.AddSingleton<JwtService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();

// Configuración de JWT
var jwtSettings = builder.Configuration.GetSection("TokenAuthentication");
var secretKeyString = jwtSettings["SecretKey"];
ArgumentNullException.ThrowIfNull(secretKeyString);
var secretKey = Encoding.UTF8.GetBytes(secretKeyString);

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/login/Logout";
        //options.AccessDeniedPath = "/login/Denegado";
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["jwt"];
                return Task.CompletedTask;
            },
        };
    });

// Configuración de Políticas (Roles)
builder.Services.AddAuthorization(options =>
{
    // Política Admin: Solo para rol 1
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "1"));

    // Política Vendedor: Acceso para rol 1 o 2
    options.AddPolicy("Vendedor", policy => policy.RequireClaim(ClaimTypes.Role, "1", "2"));
});

// Servicios de controladores y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline de middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Archivos estáticos
app.UseStaticFiles();

app.UseRouting();

// Autenticación (antes de autorización)
app.UseAuthentication();

// Autorización
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
