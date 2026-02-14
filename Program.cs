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
builder.Services.AddScoped<VentaService>();

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
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Home/Denied";

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
        };
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
            RoleClaimType = ClaimTypes.Role,
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(
                    new { message = "Token invalido o expirado" }
                );
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
app.UseStatusCodePagesWithReExecute("/Home/Error404");

app.UseRouting();

// Autenticación (antes de autorización)
app.UseAuthentication();

// Autorización
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
