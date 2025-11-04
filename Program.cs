using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Obtenemos la Cadena de Conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. REGISTRAMOS TU DB-CONTEXT (AQUÍ USAMOS POMELO)
// Le decimos a la app que "existe" un servicio llamado 'Context'
// y que debe usar MySQL con la cadena de conexión.
builder.Services.AddDbContext<Context>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// 3. REGISTRAMOS LA AUTENTICACIÓN POR COOKIES
// Esto prepara el sistema para el Login/Logout.
// Le decimos que si un usuario no está logueado, lo redirija a "/Account/Login"
builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// 4. Mantenemos el servicio de Controladores y Vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- SECCIÓN DE PIPELINE (MIDDLEWARE) ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Agregamos el soporte para archivos estáticos (CSS, JS, imágenes de avatares)
// que estén en la carpeta wwwroot
app.UseStaticFiles();

app.UseRouting();

// 5. ACTIVAMOS LA AUTENTICACIÓN
// Este middleware revisa la cookie en cada petición.
// DEBE ir ANTES de UseAuthorization.
app.UseAuthentication();

// 6. ACTIVAMOS LA AUTORIZACIÓN
// Este middleware es el que chequea los roles y los [Authorize]
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
