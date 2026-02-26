# Sistema de Gestión de Ventas

Sistema web para gestión integral de ventas, inventario y productos desarrollado con ASP.NET Core 9.0 y PostgreSQL. Implementa autenticación dual (Cookie/JWT), control de roles, y trazabilidad completa de operaciones.

## Descripción General

Aplicación web monolítica que administra el ciclo completo de ventas de un negocio: gestión de productos con control de stock, registro transaccional de ventas con actualización atómica de inventario, ajustes manuales de stock, dashboard con métricas y estadísticas, y gestión de usuarios con roles diferenciados.

El sistema registra automáticamente trazabilidad de movimientos de stock mediante `AjusteStock`, vincula cada operación con el usuario que la realizó, y asegura integridad referencial mediante transacciones de base de datos.

## Arquitectura

### Patrón Arquitectónico

**MVC (Model-View-Controller)** con capa de servicios adicional:

- **Capa de Presentación**: Razor Views para interfaz web + API REST para integración externa
- **Capa de Controladores**:
  - Controladores MVC para vistas (HomeController, ProductoController, VentaController, etc.)
  - Controladores API REST bajo `/api` con autenticación JWT para el metodo searchJWT
- **Capa de Servicios**: Lógica de negocio compleja (VentaService, DashboardService, JwtService, UserService, SupabaseStorageService)
- **Capa de Datos**: Entity Framework Core con Context.cs como DbContext
- **Capa de Modelos**: Entidades de dominio con Data Annotations

### Decisiones de Diseño

1. **Autenticación Dual**: Cookie-based para MVC (sesiones largas de 9h), JWT Bearer para API (tokens de 5 min)
2. **Actualización Atómica de Stock**: Uso de `ExecuteUpdateAsync` en lugar de cargar entidades completas, evitando condiciones de carrera
3. **Trazabilidad Automática**: Cada venta/anulación genera automáticamente un registro de `AjusteStock` con detalles
4. **Cultura Invariante**: Configuración `en-US` para decimales con punto, evitando problemas de localización
5. **Segregación de Roles**: Políticas de autorización (`Admin`, `Vendedor`) aplicadas a nivel de controlador y servicio

## Tecnologías Utilizadas

### Framework y Runtime

- **ASP.NET Core 9.0**: Framework web principal
- **.NET 9.0**: Runtime y SDK
- **C# 12**: Lenguaje de programación con nullable reference types habilitado

### Base de Datos

- **PostgreSQL**: Base de datos relacional (hospedada en Supabase)
- **Entity Framework Core 9.0**: ORM para acceso a datos
- **Npgsql.EntityFrameworkCore.PostgreSQL 9.0.0**: Driver de PostgreSQL para EF Core
- **Migraciones EF Core**: Control de versiones del esquema de base de datos

### Autenticación y Seguridad

- **ASP.NET Core Authentication**: Middleware de autenticación
- **Cookie Authentication**: Autenticación basada en cookies para vistas MVC
- **JWT Bearer Authentication**: Autenticación basada en tokens para API REST
- **System.IdentityModel.Tokens.Jwt 7.6.0**: Generación y validación de tokens JWT
- **BCrypt.Net-Next 4.0.3**: Hashing seguro de contraseñas

### Almacenamiento Externo

- **Supabase 1.1.1**: Cliente para Supabase (Storage y Database)
- **Supabase Storage**: Almacenamiento de imágenes de productos y avatares de usuarios

### Herramientas de Desarrollo

- **Microsoft.EntityFrameworkCore.Tools 9.0.0**: CLI para migraciones
- **Microsoft.EntityFrameworkCore.Design 9.0.10**: Herramientas de diseño en tiempo de desarrollo

## Estructura del Proyecto

```
SistemaGestionVentas/
│
├── Controllers/              # Controladores MVC y lógica de presentación
│   ├── HomeController.cs           # Dashboard y página principal
│   ├── LoginController.cs          # Autenticación con cookies
│   ├── ProductoController.cs       # CRUD de productos con upload de imágenes
│   ├── VentaController.cs          # Registro y anulación de ventas
│   ├── AjusteStockController.cs    # Ajustes manuales de inventario
│   ├── CategoriaController.cs      # Gestión de categorías
│   ├── ProveedorController.cs      # Gestión de proveedores
│   ├── UsuarioController.cs        # ABM de usuarios
│   ├── PerfilController.cs         # Edición de perfil de usuario
│   ├── MetodoPagoController.cs     # Métodos de pago
│   ├── MotivoAjusteController.cs   # Motivos de ajuste de stock
│   ├── BaseController.cs           # Controlador base con helpers
│   └── Api/                        # Controladores API REST
│       ├── AuthApiController.cs         # Login con JWT
│       ├── ProductoApiController.cs     # Búsqueda de productos
│       ├── MetodoPagoApiController.cs
│       └── MotivoAjusteApiController.cs
│
├── Models/                   # Entidades de dominio
│   ├── Usuario.cs                 # Usuarios del sistema (admin/empleado)
│   ├── Producto.cs                # Productos con stock y precios
│   ├── Categoria.cs               # Categorías de productos
│   ├── Proveedor.cs               # Proveedores
│   ├── Venta.cs                   # Cabecera de ventas
│   ├── DetalleVenta.cs            # Líneas de venta con precio histórico
│   ├── MetodoPago.cs              # Formas de pago
│   ├── AjusteStock.cs             # Cabecera de ajustes de inventario
│   ├── AjusteStockDetalle.cs      # Detalle de productos en ajuste
│   ├── MotivoAjuste.cs            # Motivos de ajuste
│   ├── DashboardViewModel.cs      # ViewModel para dashboard
│   ├── DetalleViewModel.cs        # ViewModel para detalle de venta
│   ├── LoginViewModel.cs          # ViewModel para login
│   └── ErrorViewModel.cs          # ViewModel para errores
│
├── Services/                 # Lógica de negocio
│   ├── VentaService.cs            # Transacciones de venta con atomicidad
│   ├── DashboardService.cs        # Estadísticas y métricas del negocio
│   ├── JwtService.cs              # Generación de tokens JWT
│   ├── UserService.cs             # Información del usuario autenticado
│   └── SupabaseStorageService.cs # Upload/Delete de imágenes en Supabase
│
├── Data/                     # Contexto de Entity Framework
│   └── Context.cs                 # DbContext con configuración de modelo
│
├── Migrations/               # Migraciones de Entity Framework Core
│   ├── 20260107205233_InicialPostgreSQL.cs
│   ├── 20260119211705_AddTimestampsToProducto.cs
│   ├── 20260122184807_ChangeEstadoToBool.cs
│   ├── 20260222224647_AddPrecioCostoHistoricoToDetalleVenta.cs
│   ├── 20260222224811_AddPrecioCostoHistoricoToDetalleVenta2.cs
│   ├── 20260223221236_CleanDashboardData.cs
│   └── ContextModelSnapshot.cs    # Snapshot del modelo actual
│
├── Views/                    # Vistas Razor
│   ├── Home/                      # Dashboard y páginas de error
│   ├── Login/                     # Formulario de login
│   ├── Producto/                  # CRUD de productos con filtros
│   ├── Venta/                     # Listado, registro y detalle de ventas
│   ├── AjusteStock/               # Ajustes de stock
│   ├── Categoria/                 # Gestión de categorías
│   ├── Proveedor/                 # Gestión de proveedores
│   ├── Usuario/                   # ABM de usuarios
│   ├── Perfil/                    # Edición de perfil
│   ├── MetodoPago/                # Métodos de pago
│   ├── MotivoAjuste/              # Motivos de ajuste
│   ├── Shared/                    # Layouts y parciales compartidos
│   ├── _ViewImports.cshtml        # Imports globales
│   └── _ViewStart.cshtml          # Layout por defecto
│
├── wwwroot/                  # Archivos estáticos
│   ├── css/                       # Hojas de estilo
│   └── js/                        # Scripts JavaScript
│
├── Properties/
│   └── launchSettings.json        # Configuración de ejecución
│
├── Program.cs                # Punto de entrada y configuración de servicios
├── appsettings.json          # Configuración de la aplicación
├── appsettings.Development.json
└── SistemaGestionVentas.csproj   # Archivo de proyecto .NET
```

## Modelos Principales

### Usuario

Representa usuarios del sistema con roles diferenciados.

**Propiedades:**

- `Id`, `DNI` (único), `Nombre`, `Apellido`, `Email` (único), `Pass` (hash BCrypt)
- `Avatar`: URL de imagen en Supabase Storage
- `Rol`: 1 (Admin) o 2 (Empleado)
- `Estado`: bool (true=Activo, false=Inactivo)

**Relaciones:**

- Auditoría: Productos, Ventas, AjustesStock creados o modificados por este usuario

### Producto

Representa artículos del inventario.

**Propiedades:**

- `Codigo` (único), `Nombre`, `PrecioCosto`, `PrecioVenta`, `Stock` (decimal con 3 decimales)
- `Pesable`: bool (indica si se vende por peso)
- `Imagen`: URL en Supabase Storage
- `Estado`: bool, `CreatedAt`, `UpdatedAt`

**Relaciones:**

- `Categoria` (obligatorio): N:1
- `Proveedor` (opcional): N:1
- `UsuarioCreador`, `UsuarioModificador`: N:1 (auditoría)

**Lógica de negocio:**

- Stock se actualiza atómicamente mediante `ExecuteUpdateAsync` en transacciones
- `UpdatedAt` se actualiza manualmente en cada modificación de stock

### Venta

Cabecera de una transacción de venta.

**Propiedades:**

- `Fecha` (UTC), `Total`, `Estado` (true=Completada, false=Anulada)
- `MotivoAnulacion`: Razón si fue anulada

**Relaciones:**

- `MetodoPago` (obligatorio): N:1
- `UsuarioCreador`, `UsuarioModificador`: N:1
- `Detalles`: 1:N (DetalleVenta, cascade delete)

**Lógica de negocio:**

- Al registrar: crea automáticamente `AjusteStock` con TipoMovimiento=2 (Baja)
- Al anular: crea automáticamente `AjusteStock` con TipoMovimiento=1 (Alta) para devolver stock
- Cálculo de total desde precios vigentes al momento de la transacción

### DetalleVenta

Línea de producto dentro de una venta.

**Propiedades:**

- `Cantidad` (decimal), `PrecioUnitario`, `PrecioCosto`

**Relaciones:**

- `Venta` (obligatorio): N:1 (cascade delete)
- `Producto` (obligatorio): N:1 (restrict delete para preservar historial)

**Decisión de diseño:**

- `PrecioUnitario` y `PrecioCosto` se copian desde `Producto` al momento de la venta para preservar historial de precios

### AjusteStock

Trazabilidad de movimientos de inventario.

**Propiedades:**

- `Fecha`, `TipoMovimiento` (1=Alta, 2=Baja), `Nota`

**Relaciones:**

- `Usuario` (opcional): quien realizó el ajuste
- `MotivoAjuste` (obligatorio): razón del ajuste
- `Venta` (opcional): referencia si es generado por venta/anulación
- `Detalles`: 1:N (AjusteStockDetalle, cascade delete)

**Integración:**

- Se crea automáticamente en `VentaService.RegistrarVentaAsync` y `AnularVentaAsync`
- Puede crearse manualmente desde `AjusteStockController`

### Categoria

Clasificación de productos.

**Propiedades:**

- `Nombre` (único), `Estado`

**Relaciones:**

- `Productos`: 1:N

### Proveedor

Proveedores de productos.

**Propiedades:**

- `NombreEmpresa`, `NombreContacto`, `Telefono`, `Direccion`, `Email`, `Notas`, `Estado`

**Relaciones:**

- `Productos`: 1:N

### MetodoPago

Formas de pago aceptadas.

**Propiedades:**

- `Nombre` (único), `Estado`

### MotivoAjuste

Motivos predefinidos para ajustes de stock.

**Propiedades:**

- `Nombre` (único), `Descripcion`, `Estado`

**Motivos esperados:**

- ID 1: Venta (usado automáticamente por VentaService)
- ID 2: Anulación de venta (usado automáticamente por VentaService)
- Otros: Ajustes manuales (entrada de mercadería, merma, robo, etc.)

## Autenticación y Seguridad

### Esquema Dual de Autenticación

#### 1. Cookie Authentication (Vistas MVC)

**Configuración:**

- Esquema: `CookieAuthenticationDefaults.AuthenticationScheme`
- LoginPath: `/Login`
- ExpireTimeSpan: 9 horas
- Claims almacenados: `NameIdentifier` (UserId), `Name`, `Role`, `Avatar`

**Flujo:**

1. Usuario envía credenciales a `POST /Login`
2. Se valida contra BD, contraseña verifica con BCrypt
3. Se crean Claims y ClaimsIdentity
4. Se llama a `HttpContext.SignInAsync` para crear cookie encriptada
5. Cookie se envía automáticamente en cada request subsiguiente

**Manejo de redirecciones:**

- Requests a `/api/*`: retorna 401/403 sin redirigir
- Requests a vistas: redirige a `/Login` o `/Home/Denied`

#### 2. JWT Bearer Authentication (API REST)

**Configuración:**

- SecretKey: Llave simétrica de 32 caracteres (HS256)
- Issuer: `SistemaGestionVentas`
- Audience: `UsuariosSGV`
- Validación: Issuer, Audience, Lifetime, IssuerSigningKey
- RoleClaimType: `ClaimTypes.Role`

**Flujo:**

1. Cliente hace `POST /api/auth/login` con email/password
2. `AuthApiController` valida credenciales
3. `JwtService.GenerarToken` crea token con Claims (NameIdentifier, Name, Role)
4. Token expira en **5 minutos**
5. Cliente envía token en header: `Authorization: Bearer {token}`
6. Middleware valida token en cada request

**Endpoints API:**

- `POST /api/auth/login`: Obtener token
- `GET /api/productos/searchjwt?q={query}`: Requiere JWT Bearer
- `GET /api/productos/search?q={query}`: Requiere Cookie o JWT
- `GET /api/productos/{id}`: Requiere Cookie o JWT

### Políticas de Autorización

Definidas en `Program.cs`:

```csharp
// Política Admin: Solo rol 1
options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "1"));

// Política Vendedor: Roles 1 y 2
options.AddPolicy("Vendedor", policy => policy.RequireClaim(ClaimTypes.Role, "1", "2"));
```

**Aplicación:**

- `[Authorize(Policy = "Admin")]`: Solo administradores (UsuarioController, algunos métodos de configuración)
- `[Authorize(Policy = "Vendedor")]`: Administradores y empleados (ProductoController, VentaController, etc.)
- `[Authorize]`: Cualquier usuario autenticado

### Segregación por Rol en Servicios

**DashboardService:**

- Admin (rol 1): ve estadísticas de todos los usuarios
- Empleado (rol 2): ve solo sus propias ventas

**VentaController:**

- Admin (rol 1): ve todas las ventas, puede filtrar por usuario
- Empleado (rol 2): ve solo sus ventas creadas

**UserService:**

- `GetCurrentUserId()`: Extrae UserId del claim `NameIdentifier`
- `GetCurrentUserRole()`: Extrae Role del claim `Role`
- Lanza `UnauthorizedAccessException` si no hay claims

### Hash de Contraseñas

**BCrypt.Net-Next:**

- `BCrypt.Net.BCrypt.HashPassword(password)`: Al crear/modificar usuario
- `BCrypt.Net.BCrypt.Verify(password, hash)`: Al validar login
- Algoritmo: BCrypt con salt automático

## Flujo de la Aplicación

### Flujo de Autenticación (MVC)

```
1. Usuario → GET /Login
2. LoginController.Index() → renderiza vista
3. Usuario ingresa email/password → POST /Login
4. LoginController.Index(model):
   - Valida ModelState
   - Busca usuario por email en BD
   - Verifica hash con BCrypt.Verify
   - Crea Claims (NameIdentifier, Name, Role, Avatar)
   - SignInAsync() → cookie encriptada
   - Redirect a /Home/Index
5. Requests subsiguientes incluyen cookie automáticamente
6. Middleware UseAuthentication() valida cookie y popula User.Claims
```

### Flujo de Registro de Venta

```
1. Usuario autenticado → GET /Venta/Create
2. VentaController.Create():
   - Carga SelectLists de MetodoPago
   - Renderiza vista con formulario
3. Usuario selecciona productos (AJAX a /api/productos/search)
4. Usuario envía formulario → POST /Venta/Create
5. VentaController.Create(venta, detalles):
   - Obtiene userId desde UserService
   - Llama a VentaService.RegistrarVentaAsync()
6. VentaService.RegistrarVentaAsync():
   a) Inicia transacción (BeginTransactionAsync)
   b) Crea AjusteStock con TipoMovimiento=2 (Baja)
   c) Por cada detalle:
      - Consulta precio y stock vigente del producto
      - Valida stock >= cantidad
      - Actualiza stock atómicamente (ExecuteUpdateAsync)
      - Actualiza UpdatedAt manualmente
      - Crea DetalleVenta con precios históricos
      - Crea AjusteStockDetalle
   d) Calcula total acumulado
   e) Guarda Venta con SaveChangesAsync
   f) Commit de transacción
7. Si cualquier paso falla → Rollback automático
8. Redirect a /Venta/Index con notificación
```

**Garantías de atomicidad:**

- Transacción envuelve todas las operaciones
- `ExecuteUpdateAsync` actualiza stock directamente en BD sin cargar entidad
- Si stock cambia entre validación y update, `afectados == 0` causa rollback
- DeleteBehavior.Restrict en DetalleVenta.Producto evita borrado accidental

### Flujo de Anulación de Venta

```
1. Usuario → POST /Venta/Anular/{id}
2. VentaController.Anular(id, motivoAnulacion):
   - Obtiene userId desde UserService
   - Llama a VentaService.AnularVentaAsync()
3. VentaService.AnularVentaAsync():
   a) Inicia transacción
   b) Busca venta con Include de Detalles
   c) Valida que Estado == true (no anulada previamente)
   d) Crea AjusteStock con TipoMovimiento=1 (Alta por anulación)
   e) Por cada DetalleVenta:
      - Suma cantidad al stock del producto (ExecuteUpdateAsync)
      - Actualiza UpdatedAt
      - Crea AjusteStockDetalle
   f) Marca Venta.Estado = false
   g) Asigna MotivoAnulacion
   h) SaveChanges y Commit
4. Redirect a /Venta/Index
```

### Flujo de Dashboard

```
1. Usuario → GET /Home/Index?fechaInicio=2025-01-01&fechaFin=2025-01-31
2. HomeController.Index(fechaInicio, fechaFin):
   - Obtiene userId y userRole desde UserService
   - Llama a DashboardService.GetDashboardAsync()
3. DashboardService.GetDashboardAsync():
   - Convierte fechas locales a UTC
   - Aplica filtros de fecha y rol:
     * Admin (rol 1): ventas de todos los usuarios
     * Empleado (rol 2): solo UsuarioCreadorId == userId
   - Consultas agregadas:
     * Gráficos: GroupBy(Fecha.Date) → Venta, Costo, CantV, CantP
     * Top Vendedores: GroupBy(Usuario) → OrderBy(totalVenta) → Take(5)
     * Top Productos: GroupBy(Producto) → OrderBy(cantidad) → Take(5)
     * Productos Bajo Stock: Where(Stock < 20) → OrderBy(Stock) → Take(15)
4. Serializa Graficos a JSON para Chart.js
5. Renderiza vista con DashboardViewModel
```

### Flujo de Upload de Imagen

```
1. Usuario → POST /Producto/Create con IFormFile
2. ProductoController.Create(producto):
   - Valida ModelState
   - Llama a SupabaseStorageService.UploadImageAsync(producto.Fimagen)
3. SupabaseStorageService.UploadImageAsync():
   - Valida que file no sea null/vacío
   - Genera nombre único: {folder}/{Guid}{extension}
   - Convierte IFormFile a byte[]
   - Llama a Supabase.Storage.From("sgv").Upload()
   - Obtiene URL pública con bucket.GetPublicUrl()
4. Asigna URL a producto.Imagen
5. Guarda en BD con _context.SaveChanges()
```

### Flujo de Eliminación de Imagen

```
1. Usuario → POST /Producto/Edit con nueva imagen
2. Si producto.Imagen existe:
   - Llama a SupabaseStorageService.DeleteFileAsync(producto.Imagen)
3. SupabaseStorageService.DeleteFileAsync():
   - Parsea URL: /storage/v1/object/public/{bucket}/{path}
   - Extrae bucketName y fileName
   - Llama a bucket.Remove(new List<string> { fileName })
4. Upload de nueva imagen (si se envió)
5. Update en BD
```

## Instalación y Configuración

### Requisitos Previos

- **.NET 9.0 SDK**: [Descargar](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PostgreSQL** (local o remoto): El proyecto está configurado para usar Supabase, pero puede adaptarse a PostgreSQL local
- **Cuenta Supabase** (opcional): Para Storage de imágenes. Si no se usa, comentar la configuración en `Program.cs`

### Pasos de Instalación

1. **Clonar el repositorio**

   ```bash
   git clone https://github.com/JuanCZ55/Sistema-Gestion-Ventas.git
   cd Sistema-Gestion-Ventas/SistemaGestionVentas
   ```

2. **Configurar cadena de conexión**

   Editar `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=tu_host;Database=tu_db;Username=tu_user;Password=tu_password;port=5432;"
     }
   }
   ```

3. **Configurar Supabase (opcional)**

   Si usas Supabase Storage para imágenes:

   ```json
   {
     "Supabase": {
       "Url": "https://tu_proyecto.supabase.co",
       "Key": "tu_anon_key"
     }
   }
   ```

   Si **NO** usas Supabase, comentar en `Program.cs`:

   ```csharp
   // builder.Services.AddSingleton<Supabase.Client>(...);
   // builder.Services.AddSingleton<SupabaseStorageService>();
   ```

4. **Configurar clave JWT**

   Generar una clave secreta de 32+ caracteres:

   ```json
   {
     "TokenAuthentication": {
       "SecretKey": "tu_clave_secreta_de_32_caracteres_minimo",
       "Issuer": "SistemaGestionVentas",
       "Audience": "UsuariosSGV"
     }
   }
   ```

5. **Restaurar dependencias**

   ```bash
   dotnet restore
   ```

6. **Aplicar migraciones**

   ```bash
   dotnet ef database update
   ```

   Esto crea todas las tablas en PostgreSQL según las migraciones existentes.

7. **Crear usuario inicial (seed manual)**

   Conectar a PostgreSQL y ejecutar:

   ```sql
   -- Crear motivos de ajuste requeridos
   INSERT INTO "MotivoAjuste" ("Nombre", "Descripcion", "Estado") VALUES
   ('Venta', 'Ajuste automático por venta', true),
   ('Anulación de venta', 'Devolución de stock por anulación', true);

   -- Crear usuario admin (password: Admin123)
   -- Hash BCrypt de "Admin123"
   INSERT INTO "Usuario" ("DNI", "Nombre", "Apellido", "Email", "Pass", "Rol", "Estado") VALUES
   ('12345678', 'Admin', 'Sistema', 'admin@sistema.com',
    '$2a$11$9XJHqK8xqJZ5r5OZv5EfOuE7qP9LlK3JqF8qD1Q8Zl4Y8pJ6YpY8e', 1, true);

   -- Crear método de pago por defecto
   INSERT INTO "MetodoPago" ("Nombre", "Estado") VALUES
   ('Efectivo', true);

   -- Crear categoría por defecto
   INSERT INTO "Categoria" ("Nombre", "Estado") VALUES
   ('General', true);
   ```

   **Nota:** El hash mostrado es un ejemplo. Usar BCrypt para generar uno real.

8. **Ejecutar aplicación**

   ```bash
   dotnet run
   ```

   O con hot reload:

   ```bash
   dotnet watch run
   ```

9. **Acceder a la aplicación**

   Abrir navegador en `https://localhost:5001` o `http://localhost:5000`

   Credenciales por defecto:
   - Email: `admin@sistema.com`
   - Password: `Admin123` (según el hash que generaste)

### Configuración de Producción

**appsettings.Production.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod_host;Database=prod_db;Username=prod_user;Password=prod_password;port=5432;SSL Mode=Require;"
  },
  "Supabase": {
    "Url": "SUPABASE_URL_ENV_VAR",
    "Key": "SUPABASE_KEY_ENV_VAR"
  },
  "TokenAuthentication": {
    "SecretKey": "JWT_SECRET_KEY_ENV_VAR",
    "Issuer": "SistemaGestionVentas",
    "Audience": "UsuariosSGV"
  }
}
```

**Variables de entorno recomendadas:**

- `ASPNETCORE_ENVIRONMENT=Production`
- Inyectar secretos mediante Azure Key Vault, AWS Secrets Manager, o variables de entorno del sistema

**Consideraciones:**

- Habilitar HTTPS forzado
- Configurar CORS si API se consume desde otro dominio
- Implementar rate limiting en endpoints API
- Configurar logging a archivo o servicio externo (Serilog, Application Insights)

## Consideraciones Técnicas

### 1. Actualización Atómica de Stock

**Problema:** Condiciones de carrera cuando múltiples ventas concurrentes intentan actualizar el mismo producto.

**Solución implementada:**

```csharp
// Actualización atómica mediante ExecuteUpdateAsync
int afectados = await _context.Producto
    .Where(p => p.Id == item.IdProducto && p.Stock >= item.Cantidad)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - item.Cantidad));

if (afectados == 0) {
    // Stock insuficiente o producto modificado por otra transacción
    await transaction.RollbackAsync();
    return new Result { IsSuccess = false, ErrorMessage = "Error atómico..." };
}
```

**Beneficios:**

- No carga entidad completa en memoria
- SQL ejecutado directamente: `UPDATE Producto SET Stock = Stock - @cantidad WHERE Id = @id AND Stock >= @cantidad`
- Si otra transacción modificó el stock entre validación y update, `afectados == 0` detecta el conflicto

### 2. Preservación de Precios Históricos

**Decisión:** `DetalleVenta` almacena `PrecioUnitario` y `PrecioCosto` del momento de la venta.

**Razón:** Permitir:

- Informes de rentabilidad histórica precisos
- Auditoría de precios cobrados
- Independencia de cambios futuros en `Producto.PrecioVenta`

**Implementación:**

```csharp
var productoInfo = await _context.Producto
    .Where(p => p.Id == item.IdProducto)
    .Select(p => new { p.PrecioVenta, p.PrecioCosto, p.Stock })
    .FirstOrDefaultAsync();

venta.Detalles.Add(new DetalleVenta {
    ProductoId = item.IdProducto,
    Cantidad = item.Cantidad,
    PrecioUnitario = productoInfo.PrecioVenta,  // Copia del precio vigente
    PrecioCosto = productoInfo.PrecioCosto      // Copia del costo vigente
});
```

### 3. Trazabilidad Automática de Stock

**Diseño:** Cada operación que modifica stock genera automáticamente un `AjusteStock`.

**Flujo:**

- Venta → `AjusteStock` con `TipoMovimiento=2` (Baja), `MotivoAjusteId=1` (Venta), `VentaId` referenciado
- Anulación → `AjusteStock` con `TipoMovimiento=1` (Alta), `MotivoAjusteId=2` (Anulación), `VentaId` referenciado
- Ajuste manual → `AjusteStock` creado desde `AjusteStockController`, sin `VentaId`

**Ventajas:**

- Historial completo de movimientos
- Auditoría de quién modificó stock y cuándo
- Rastreabilidad de stock por venta/anulación

### 4. Manejo Diferenciado de Errores

**API REST:**

```csharp
if (context.Request.Path.StartsWithSegments("/api")) {
    context.Response.StatusCode = 401;  // o 403, 500
    await context.Response.WriteAsJsonAsync(new { success = false, message = "..." });
    return;
}
```

**Vistas MVC:**

```csharp
context.Response.Redirect("/Login");  // o /Home/Error, /Home/Denied
```

**Razón:** Clientes API esperan JSON con códigos HTTP, navegadores esperan redirecciones HTML.

### 5. Cultura Invariante para Decimales

**Configuración:**

```csharp
var defaultCulture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new[] { defaultCulture },
    SupportedUICultures = new[] { defaultCulture }
});
```

**Razón:** JavaScript y PostgreSQL usan punto decimal (`.`). Evitar problemas con culturas que usan coma (`,`) como separador decimal (es-AR, es-ES, etc.).

**Consecuencia:** Todas las vistas Razor deben usar `<input type="text">` con validación JavaScript personalizada para decimales, o usar `type="number"` con `step="0.01"`.

### 6. DeleteBehavior Configurado

**Context.cs:**

```csharp
modelBuilder.Entity<DetalleVenta>()
    .HasOne(d => d.Venta)
    .WithMany(v => v.Detalles)
    .OnDelete(DeleteBehavior.Cascade);  // Borrar Venta → borra sus Detalles

modelBuilder.Entity<DetalleVenta>()
    .HasOne(d => d.Producto)
    .WithMany()
    .OnDelete(DeleteBehavior.Restrict);  // NO borrar Producto si tiene Detalles
```

**Razón:**

- Cascade: Mantener integridad referencial al borrar venta
- Restrict: Preservar historial de ventas si se intenta borrar producto

### 7. Timestamps Manuales en Producto

**Decisión:** `UpdatedAt` se actualiza manualmente después de `ExecuteUpdateAsync`.

**Razón:** `ExecuteUpdateAsync` no dispara triggers de EF Core ni propiedades computadas. Se debe actualizar explícitamente:

```csharp
var productoEntity = await _context.Producto.FindAsync(item.IdProducto);
if (productoEntity != null) {
    productoEntity.UpdatedAt = DateTime.UtcNow;
    _context.Update(productoEntity);
}
```

Se tuvo que utilizar utc por el driver de npgls

## Cumplimiento de Requerimientos

Esta sección detalla dónde se implementa cada requerimiento técnico del proyecto.

### 1. Modelo Relacional Mínimo (4 tablas, 1:N)

**✅ Implementado: 10 tablas con múltiples relaciones 1:N**

**Ubicación:** [`Models/`](Models/) y [`Data/Context.cs`](Data/Context.cs)

**Tablas principales:**

- `Usuario` (1) → (N) `Producto` (UsuarioCreador, UsuarioModificador)
- `Usuario` (1) → (N) `Venta` (UsuarioCreador, UsuarioModificador)
- `Usuario` (1) → (N) `AjusteStock`
- `Categoria` (1) → (N) `Producto`
- `Proveedor` (1) → (N) `Producto`
- `MetodoPago` (1) → (N) `Venta`
- `Venta` (1) → (N) `DetalleVenta`
- `Producto` (1) → (N) `DetalleVenta`
- `MotivoAjuste` (1) → (N) `AjusteStock`
- `AjusteStock` (1) → (N) `AjusteStockDetalle`
- `Producto` (1) → (N) `AjusteStockDetalle`

**Configuración de relaciones:**

```csharp
// Data/Context.cs - líneas 26-62
modelBuilder.Entity<DetalleVenta>()
    .HasOne(d => d.Venta)
    .WithMany(v => v.Detalles)
    .HasForeignKey(d => d.VentaId)
    .OnDelete(DeleteBehavior.Cascade);
```

**Migraciones:** Esquema completo en [`Migrations/ContextModelSnapshot.cs`](Migrations/ContextModelSnapshot.cs)

---

### 2. Autenticación, Autorización y Roles

**✅ Implementado: Sistema dual Cookie + JWT con 2 roles**

**Autenticación Cookie (MVC):**

- **Ubicación:** [`Controllers/LoginController.cs`](Controllers/LoginController.cs) (líneas 25-76)
- **Configuración:** [`Program.cs`](Program.cs) (líneas 51-85)
- **Flujo:** POST `/Login` → validación BCrypt → `SignInAsync()` → cookie encriptada
- **Expiración:** 9 horas
- **Claims:** `NameIdentifier`, `Name`, `Role`, `Avatar`

**Autenticación JWT (API):**

- **Login:** [`Controllers/Api/AuthApiController.cs`](Controllers/Api/AuthApiController.cs)
- **Generación de tokens:** [`Services/JwtService.cs`](Services/JwtService.cs) (líneas 17-46)
- **Configuración:** [`Program.cs`](Program.cs) (líneas 86-113)
- **Expiración:** 5 minutos
- **Algoritmo:** HS256 (HMAC-SHA256)

**Roles:**

- **Definición:** `Usuario.Rol` = 1 (Admin) o 2 (Empleado)
- **Políticas:**
  ```csharp
  // Program.cs - líneas 116-122
  options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "1"));
  options.AddPolicy("Vendedor", policy => policy.RequireClaim(ClaimTypes.Role, "1", "2"));
  ```

**Autorización por controlador:**

- `[Authorize(Policy = "Admin")]`: [`UsuarioController.cs`](Controllers/UsuarioController.cs)
- `[Authorize(Policy = "Vendedor")]`: [`ProductoController.cs`](Controllers/ProductoController.cs), [`VentaController.cs`](Controllers/VentaController.cs), etc.

**Segregación por rol:**

- [`DashboardService.cs`](Services/DashboardService.cs) (línea 29): Admin ve todos, Empleado solo sus ventas
- [`VentaController.cs`](Controllers/VentaController.cs) (líneas 49-51): Filtro por `UsuarioCreadorId`

**Hash de contraseñas:** BCrypt.Net-Next en [`LoginController.cs`](Controllers/LoginController.cs) (línea 40)

---

### 3. Avatar y Gestión de Archivos

**✅ Implementado: Supabase Storage para avatares y productos**

**Servicio de almacenamiento:**

- **Ubicación:** [`Services/SupabaseStorageService.cs`](Services/SupabaseStorageService.cs)
- **Métodos:**
  - `UploadImageAsync(IFormFile file, string folder)` (líneas 14-53)
  - `DeleteFileAsync(string url)` (líneas 56-88)
- **Bucket:** `sgv` en Supabase Storage
- **Generación de nombres:** `{folder}/{Guid}{extension}`

**Avatar de usuarios:**

- **Modelo:** `Usuario.Avatar` (URL de Supabase) - [`Models/Usuario.cs`](Models/Usuario.cs) (línea 27)
- **Upload:** [`UsuarioController.Create()`](Controllers/UsuarioController.cs)
- **IFormFile:** `Usuario.Favatar` (NotMapped)

**Imágenes de productos:**

- **Modelo:** `Producto.Imagen` (URL de Supabase) - [`Models/Producto.cs`](Models/Producto.cs) (línea 34)
- **Upload:** [`ProductoController.Create()`](Controllers/ProductoController.cs) (líneas 187-196)
- **Delete:** [`ProductoController.Edit()`](Controllers/ProductoController.cs) (líneas 281-288)
- **IFormFile:** `Producto.Fimagen` (NotMapped)

**Configuración:**

- **Registro del cliente:** [`Program.cs`](Program.cs) (líneas 21-32)
- **Credenciales:** `appsettings.json` → `Supabase:Url`, `Supabase:Key`

**Ejemplo de uso:**

```csharp
// ProductoController.cs - líneas 187-196
if (producto.Fimagen != null) {
    var result = await _storageService.UploadImageAsync(producto.Fimagen, "productos");
    if (result.ok) {
        producto.Imagen = result.url;
    }
}
```

---

### 4. CRUD con Vue.js vía AJAX

**✅ Implementado: Vue.js 3 con CRUD completo vía API REST**

**Implementaciones completas:**

#### A) CRUD de Motivos de Ajuste

**Backend API:**

- **Ubicación:** [`Controllers/Api/MotivoAjusteApiController.cs`](Controllers/Api/MotivoAjusteApiController.cs)
- **Endpoints:**
  - `GET /api/MotivoAjuste` - Listar todos (línea 22)
  - `POST /api/MotivoAjuste` - Crear (línea 29)
  - `PATCH /api/MotivoAjuste/{id}` - Actualizar (línea 46)
  - `PATCH /api/MotivoAjuste/{id}/Estado` - Cambiar estado (línea 76)
- **Autorización:** `[Authorize(Policy = "Admin")]`

**Frontend Vue.js:**

- **Ubicación:** [`Views/MotivoAjuste/Index.cshtml`](Views/MotivoAjuste/Index.cshtml)
- **Framework:** Vue.js 3 (Composition API - `createApp`)
- **Características:**
  - Data reactiva: `motivos[]`, `modal`, `modalEstado`
  - Lifecycle: `mounted()` para inicializar Bootstrap Modals y cargar datos
  - Métodos CRUD:
    - `cargarMotivos()` - GET con fetch API (línea 141)
    - `guardarMotivo()` - POST/PATCH para crear/editar (línea 173)
    - `confirmarCambioEstado()` - PATCH para activar/desactivar (línea 243)
  - Directivas Vue: `v-for`, `v-model`, `v-on:click`, `v-if`, `:class`, `:disabled`
  - Integración Bootstrap: Control de modales desde Vue
  - Manejo de errores: Validación de nombre duplicado, errores HTTP

**Ejemplo de código Vue.js:**

```javascript
// Views/MotivoAjuste/Index.cshtml - líneas 107-266
createApp({
  data() {
    return {
      motivos: [],
      modal: {
        motivo: { id: 0, nombre: "", descripcion: "" },
        titulo: "",
        error: "",
      },
    };
  },
  mounted() {
    this.bsModalInstance = new bootstrap.Modal(
      document.getElementById("modalMotivo"),
    );
    this.cargarMotivos();
  },
  methods: {
    async cargarMotivos() {
      const res = await fetch("/api/MotivoAjuste");
      const data = await res.json();
      if (data.success) this.motivos = data.data;
    },
    async guardarMotivo() {
      const esNuevo = this.modal.motivo.id === 0;
      const url = esNuevo
        ? "/api/MotivoAjuste"
        : `/api/MotivoAjuste/${this.modal.motivo.id}`;
      const res = await fetch(url, {
        method: esNuevo ? "POST" : "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(this.modal.motivo),
      });
      // ... manejo de respuesta
    },
  },
}).mount("#app");
```

**Template Vue (HTML):**

```html
<!-- Tabla con v-for -->
<tr v-for="m in motivos" :key="m.id">
  <td class="text-capitalize">{{ m.nombre }}</td>
  <td>{{ m.descripcion }}</td>
  <td>
    <span :class="['badge', m.estado ? 'bg-success' : 'bg-danger']">
      {{ m.estado ? 'Activo' : 'Inactivo' }}
    </span>
  </td>
  <td>
    <button v-on:click="abrirModalEditar(m)">Editar</button>
    <button v-on:click="abrirModalEstado(m)">
      {{ m.estado ? 'Desactivar' : 'Activar' }}
    </button>
  </td>
</tr>

<!-- Formulario con v-model -->
<input type="text" v-model="modal.motivo.nombre" maxlength="50" />
<textarea v-model="modal.motivo.descripcion" maxlength="200"></textarea>
```

---

#### B) CRUD de Métodos de Pago

**Backend API:**

- **Ubicación:** [`Controllers/Api/MetodoPagoApiController.cs`](Controllers/Api/MetodoPagoApiController.cs)
- **Endpoints:**
  - `GET /api/MetodoPago` - Listar todos (línea 22)
  - `GET /api/MetodoPago/{id}` - Obtener por ID (línea 97)
  - `POST /api/MetodoPago` - Crear (línea 29)
  - `PATCH /api/MetodoPago/{id}` - Actualizar (línea 47)
  - `PATCH /api/MetodoPago/{id}/Estado` - Cambiar estado (línea 71)
- **Autorización:** `[Authorize(Policy = "Admin")]`

**Frontend Vue.js:**

- **Ubicación:** [`Views/MetodoPago/Index.cshtml`](Views/MetodoPago/Index.cshtml)
- **Framework:** Vue.js 3 (Composition API)
- **Similitudes con MotivoAjuste:**
  - Misma arquitectura Vue.js 3
  - CRUD completo con fetch API
  - Modales controlados por Vue + Bootstrap
  - Validación en tiempo real con `v-model`
  - Manejo de estados de loading con spinner
- **Diferencias:**
  - Modelo más simple (solo `nombre` y `estado`)
  - Sin campo `descripcion`

**Patrón de respuesta API:**

```json
{
  "success": true,
  "message": "Método de pago creado exitosamente.",
  "data": { "id": 1, "nombre": "efectivo", "estado": true }
}
```

---

#### Características técnicas comunes

**1. Vue.js 3 Composition API:**

- `const { createApp } = Vue` importado desde CDN
- Reactividad automática con `data()`
- Ciclo de vida con `mounted()`
- Métodos asíncronos con `async/await`

**2. Fetch API nativo:**

- Sin librerías externas (no Axios)
- `safeParse()` helper para manejo seguro de JSON (líneas 100-106 en ambas vistas)
- Headers: `'Content-Type': 'application/json'`

**3. Integración Bootstrap:**

- Modales controlados por instancias JS: `new bootstrap.Modal()`
- `.show()` y `.hide()` desde métodos Vue

**4. Validación:**

- Frontend: Validación preventiva con trim() y verificación de campos requeridos
- Backend: Validación de duplicados con `isDuplicate()` en controladores API

**5. UX:**

- Toasts para notificaciones: `window.showToast('success', mensaje)`
- Estados de loading con spinner de Bootstrap
- Confirmación modal para acciones destructivas (cambio de estado)

---

#### Otros usos de AJAX (sin Vue.js)

**Búsqueda de productos:**

- **Vistas:** [`Views/Venta/Create.cshtml`](Views/Venta/Create.cshtml), [`Views/Venta/Edit.cshtml`](Views/Venta/Edit.cshtml)
- **API:** [`Controllers/Api/ProductoApiController.cs`](Controllers/Api/ProductoApiController.cs)
- **Implementación:** Fetch API nativo (sin Vue.js)
- **Uso:** Autocompletado en formularios de ventas

---

### 5. Paginado Server-Side Obligatorio

**✅ Implementado: Skip/Take con EF Core**

**ProductoController.Index:**

- **Ubicación:** [`Controllers/ProductoController.cs`](Controllers/ProductoController.cs) (líneas 31-108)
- **Implementación:**
  ```csharp
  // Líneas 89-95
  var total = await query.CountAsync();
  int pageSize = 10;
  if (page < 1) page = 1;
  query = query.OrderBy(p => p.Id)
               .Skip((page - 1) * pageSize)
               .Take(pageSize);
  ```
- **Parámetros:** `?page=1&nombre=...&precioMin=...&categoriaId=...`
- **ViewBag:** `Page`, `Total`, `PageSize` para controles de navegación

**VentaController.Index:**

- **Ubicación:** [`Controllers/VentaController.cs`](Controllers/VentaController.cs) (líneas 27-148)
- **Implementación:**
  ```csharp
  // Líneas 113-120
  var total = await query.CountAsync();
  int pageSize = 10;
  if (page < 1) page = 1;
  query = query.OrderByDescending(v => v.Fecha)
               .Skip((page - 1) * pageSize)
               .Take(pageSize);
  ```
- **Filtros adicionales:** `metodoPagoId`, `fechaMin`, `fechaMax`, `estado`, `usuarioCreadorId`

**Filtros por rol:**

- Empleados (rol 2) solo ven sus propios registros mediante filtro `UsuarioCreadorId == currentUserId`

**Ventajas:**

- SQL eficiente con `LIMIT` y `OFFSET`
- No carga todos los registros en memoria
- Compatible con tablas grandes

---

### 6. Búsqueda AJAX en Relaciones

**✅ Implementado: Búsqueda de productos con relaciones**

**API de búsqueda:**

- **Ubicación:** [`Controllers/Api/ProductoApiController.cs`](Controllers/Api/ProductoApiController.cs)

**Endpoint 1: Búsqueda con autenticación Cookie/JWT**

```csharp
// Líneas 49-78
[HttpGet("search")]
[Authorize]
public async Task<IActionResult> Search([FromQuery] string q)
```

- **URL:** `GET /api/productos/search?q={query}`
- **Autenticación:** Cookie o JWT
- **Query:** `EF.Functions.Like(p.Codigo, $"%{q}%") || EF.Functions.Like(p.Nombre, $"%{q}%")`
- **Límite:** 20 resultados

**Endpoint 2: Búsqueda exclusiva con JWT**

```csharp
// Líneas 20-47
[HttpGet("searchjwt")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public async Task<IActionResult> SearchJWT([FromQuery] string q)
```

- **URL:** `GET /api/productos/searchjwt?q={query}`
- **Autenticación:** Solo JWT Bearer
- **Uso:** Clientes externos/API

**Endpoint 3: Detalle con relaciones**

```csharp
// Líneas 80-117
[HttpGet("{id}")]
[Authorize]
public async Task<IActionResult> GetById(int id)
```

- **URL:** `GET /api/productos/{id}`
- **Proyección:** Incluye `Categoria` y `Proveedor` con `Select()`
- **Respuesta:**
  ```json
  {
    "id": 1,
    "codigo": "P001",
    "nombre": "Producto",
    "precioVenta": 100.0,
    "stock": 50,
    "imagen": "https://...",
    "categoria": { "id": 1, "nombre": "General" },
    "proveedor": { "id": 1, "nombreContacto": "Juan" }
  }
  ```

**Optimización:**

- `AsNoTracking()` para consultas read-only
- `.Select()` para proyecciones planas (evita N+1)
- Índice único en `Producto.Codigo` y `Producto.Nombre` (líneas 33-34 en [`Data/Context.cs`](Data/Context.cs))

**Frontend (consumo AJAX):**

- JavaScript nativo con `fetch()` en vistas de Venta
- Autocompletado de productos al crear/editar ventas

---

### 7. API con Autenticación JWT

**✅ Implementado: API REST con JWT Bearer**

**Endpoint de login:**

- **Ubicación:** [`Controllers/Api/AuthApiController.cs`](Controllers/Api/AuthApiController.cs)
- **Método:**
  ```csharp
  // Líneas 20-38
  [HttpPost("login")]
  public IActionResult Login([FromBody] LoginViewModel model)
  ```
- **URL:** `POST /api/auth/login`
- **Request:**
  ```json
  {
    "email": "admin@sistema.com",
    "password": "Admin123"
  }
  ```
- **Response:**
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "expires_in_seconds": 300
  }
  ```

**Generación de tokens:**

- **Ubicación:** [`Services/JwtService.cs`](Services/JwtService.cs) (líneas 17-46)
- **Claims:** `NameIdentifier`, `Name`, `Role`
- **Algoritmo:** HS256 (HMAC-SHA256)
- **Expiración:** 5 minutos (`DateTime.UtcNow.AddMinutes(5)`)
- **Configuración:** `appsettings.json` → `TokenAuthentication:SecretKey`

**Validación de tokens:**

- **Middleware:** [`Program.cs`](Program.cs) (líneas 86-113)
- **Configuración:**
  ```csharp
  options.TokenValidationParameters = new TokenValidationParameters {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = jwtSettings["Issuer"],
      ValidAudience = jwtSettings["Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(secretKey),
      RoleClaimType = ClaimTypes.Role
  };
  ```

**Endpoints protegidos:**

- `GET /api/productos/searchjwt?q={query}` - Solo JWT Bearer
- `GET /api/productos/search?q={query}` - Cookie o JWT
- `GET /api/productos/{id}` - Cookie o JWT

**Uso del token:**

```bash
# 1. Obtener token
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@sistema.com","password":"Admin123"}'

# 2. Usar token
curl -X GET "https://localhost:5001/api/productos/searchjwt?q=prod" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

**Manejo de errores JWT:**

- [`Program.cs`](Program.cs) (líneas 103-111): Retorna JSON con status 401
- Diferenciación entre rutas `/api` y vistas MVC

**Seguridad:**

- SecretKey de 32+ caracteres
- Tokens de corta duración (5 min)
- Validación de Issuer, Audience y Lifetime
- RoleClaimType configurado para autorización por roles

---

## Resumen de Cumplimiento

| Requerimiento                        | Estado                     | Ubicación Principal                                                                                                                                        |
| ------------------------------------ | -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Modelo relacional (4 tablas 1:N)** | ✅ Completo (10 tablas)    | [`Models/`](Models/), [`Data/Context.cs`](Data/Context.cs)                                                                                                 |
| **Autenticación y roles**            | ✅ Completo (Cookie + JWT) | [`Program.cs`](Program.cs), [`LoginController.cs`](Controllers/LoginController.cs)                                                                         |
| **Avatar y archivos**                | ✅ Completo (Supabase)     | [`SupabaseStorageService.cs`](Services/SupabaseStorageService.cs)                                                                                          |
| **CRUD con Vue.js AJAX**             | ✅ Completo (Vue.js 3)     | [`MotivoAjusteApiController.cs`](Controllers/Api/MotivoAjusteApiController.cs), [`MetodoPagoApiController.cs`](Controllers/Api/MetodoPagoApiController.cs) |
| **Paginado server-side**             | ✅ Completo (Skip/Take)    | [`ProductoController.cs`](Controllers/ProductoController.cs), [`VentaController.cs`](Controllers/VentaController.cs)                                       |
| **Búsqueda AJAX relaciones**         | ✅ Completo                | [`ProductoApiController.cs`](Controllers/Api/ProductoApiController.cs)                                                                                     |
| **API con JWT**                      | ✅ Completo                | [`AuthApiController.cs`](Controllers/Api/AuthApiController.cs), [`JwtService.cs`](Services/JwtService.cs)                                                  |

## Contacto

- **GitHub:** [JuanCZ55](https://github.com/JuanCZ55)
- **Repositorio:** [Sistema-Gestion-Ventas](https://github.com/JuanCZ55/Sistema-Gestion-Ventas)
