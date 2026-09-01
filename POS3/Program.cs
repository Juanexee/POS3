using System.Text;
using DATOS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NEGOCIO;
using POS3.Hubs;

namespace API_REST_V3
{
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int ExpirationMinutes { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Validar la cadena de conexi�n antes
            var connectionString = builder.Configuration.GetConnectionString("RestauranteDB");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("La cadena de conexi�n 'RestauranteDB' no est� configurada.");

            // -----------------------
            // Bindear configuraci�n Jwt a POCO y validarla
            // -----------------------
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // Opcional: obtener una instancia inmediata para validar claves ahora
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtSettings = jwtSection.Get<JwtSettings>();

            if (jwtSettings == null)
                throw new InvalidOperationException("Se requiere la secci�n 'Jwt' en appsettings.json.");

            if (string.IsNullOrWhiteSpace(jwtSettings.Key))
                throw new InvalidOperationException("La clave 'Jwt:Key' no est� configurada. Revisa appsettings.json.");

            // -----------------------
            // Registrar Capa de Datos (DATOS)
            // -----------------------
            builder.Services.AddScoped<CategoriasDatos>(sp => new CategoriasDatos(connectionString));
            builder.Services.AddScoped<IVentaDatos>(provider => new VentaDatos(connectionString));
            builder.Services.AddScoped<InsumoDatos>(s => new InsumoDatos(connectionString));
            builder.Services.AddScoped<SesionDatos>(s => new SesionDatos(connectionString));
            builder.Services.AddScoped<CompraDatos>(s => new CompraDatos(connectionString));
            builder.Services.AddScoped<UnidadMedidaDatos>(s => new UnidadMedidaDatos(connectionString));
            builder.Services.AddScoped<RolesDatos>(s => new RolesDatos(connectionString));

            // CORRECCI�N: Unificamos los platillos para evitar registrar PlatillosDatos 3 veces de forma diferente
            builder.Services.AddScoped<PlatillosDatos>(s => new PlatillosDatos(connectionString));
            builder.Services.AddScoped<IPlatillosDatos, PlatillosDatos>(s => new PlatillosDatos(connectionString));

            builder.Services.AddScoped<DATOS.IRecetaDatos>(s => new DATOS.RecetaDatos(connectionString));
            builder.Services.AddScoped<MesaDatos>(s => new MesaDatos(connectionString));

            // -----------------------
            // Registrar Capa de Negocio (NEGOCIO)
            // -----------------------
            builder.Services.AddScoped<VentaNegocio>();
            builder.Services.AddScoped<SesionNegocio>();
            builder.Services.AddScoped<PedidoNegocio>();
            builder.Services.AddScoped<InsumoNegocio>();
            builder.Services.AddScoped<CompraNegocio>();
            builder.Services.AddScoped<CategoriaNegocio>();
            builder.Services.AddScoped<RecetaNegocio>();
            builder.Services.AddScoped<UnidadMedidaNegocio>();
            builder.Services.AddScoped<MesaNegocio>();
            builder.Services.AddScoped<RolNegocio>();

            // NUEVO: Registramos PlatilloNegocio para que funcione con tu PlatilloController correctamente
            builder.Services.AddScoped<PlatilloNegocio>();

            // =====================================================
            // NUEVOS SERVICIOS: App Móvil Gerencial + Analítica
            // =====================================================

            // RF-MOV-DSH-01, RF-MOV-DSH-02: Dashboard y tendencias de ventas
            builder.Services.AddScoped<AnaliticaNegocio>();

            // RF-MOV-AUD-01, RF-MOV-AUD-02: Auditoría y logs en MongoDB
            var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"] ?? "PENDIENTE";
            var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "RestauranteAuditoria";
            builder.Services.AddScoped<AuditoriaDatos>(sp =>
                new AuditoriaDatos(mongoConnectionString, mongoDatabaseName));
            builder.Services.AddScoped<AuditoriaNegocio>();

            // Registrar VentaNegocio con su constructor expl�cito por seguridad de dependencias cruzadas
            builder.Services.AddScoped<IVentaNegocio, VentaNegocio>(s =>
                 new VentaNegocio(
                   s.GetRequiredService<IVentaDatos>(),
                   s.GetRequiredService<SesionDatos>(),
                   s.GetRequiredService<PlatillosDatos>()
                 )
             );

            // -----------------------
            // Configuraci�n de Servicios B�sicos e Infraestructura
            // -----------------------
            builder.Services.AddSignalR();
            builder.Services.AddControllers();

            // Configuraci�n de autenticaci�n JWT
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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine("Error de autenticaci�n JWT: " + ctx.Exception?.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        var name = ctx.Principal?.Identity?.Name ?? "<sin nombre>";
                        Console.WriteLine("Token validado correctamente para: " + name);
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // Configuraci�n de CORS para el Frontend
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(origin => true) // Permite cualquier origen compatible con credentials (SignalR)
                          .AllowAnyMethod()   // Permite GET, POST, PUT, DELETE
                          .AllowAnyHeader()   // Permite enviar tokens JWT
                          .AllowCredentials(); // Requerido por SignalR para el transporte de credenciales
                });
            });

            // -----------------------
            // Swagger
            // -----------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Rancho la mimi",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Grupo 3",
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingrese 'Bearer' seguido del token JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] {}
                    }
                });

                // === Documentaci�n XML y anotaciones ===
                var xmlfile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
                c.IncludeXmlComments(xmlpath, includeControllerXmlComments: true);

                c.EnableAnnotations();
            });

            // -----------------------
            // Build y middlewares
            // -----------------------
            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("PermitirFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            // Mapeo de Hubs y Controladores
            app.MapHub<CocinaHub>("/cocinaHub");
            app.MapControllers();

            app.Run();
        }
    }
}   