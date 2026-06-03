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

            // Validar la cadena de conexión antes
            var connectionString = builder.Configuration.GetConnectionString("RestauranteDB");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("La cadena de conexión 'RestauranteDB' no está configurada.");

            // -----------------------
            // Bindear configuración Jwt a POCO y validarla
            // -----------------------
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // Opcional: obtener una instancia inmediata para validar claves ahora
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtSettings = jwtSection.Get<JwtSettings>();

            if (jwtSettings == null)
                throw new InvalidOperationException("Se requiere la sección 'Jwt' en appsettings.json.");

            if (string.IsNullOrWhiteSpace(jwtSettings.Key))
                throw new InvalidOperationException("La clave 'Jwt:Key' no está configurada. Revisa appsettings.json.");

            // -----------------------
            // Registrar Capa de Datos (DATOS)
            // -----------------------
            builder.Services.AddScoped<CategoriasDatos>(sp => new CategoriasDatos(connectionString));
            builder.Services.AddScoped<IVentaDatos>(provider => new VentaDatos(connectionString));
            builder.Services.AddScoped<InsumoDatos>(s => new InsumoDatos(connectionString));
            builder.Services.AddScoped<SesionDatos>(s => new SesionDatos(connectionString));
            builder.Services.AddScoped<CompraDatos>(s => new CompraDatos(connectionString));
            builder.Services.AddScoped<UnidadMedidaDatos>(s => new UnidadMedidaDatos(connectionString));

            // CORRECCIÓN: Unificamos los platillos para evitar registrar PlatillosDatos 3 veces de forma diferente
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

            // NUEVO: Registramos PlatilloNegocio para que funcione con tu PlatilloController correctamente
            builder.Services.AddScoped<PlatilloNegocio>();

            // Registrar VentaNegocio con su constructor explícito por seguridad de dependencias cruzadas
            builder.Services.AddScoped<IVentaNegocio, VentaNegocio>(s =>
                 new VentaNegocio(
                   s.GetRequiredService<IVentaDatos>(),
                   s.GetRequiredService<SesionDatos>(),
                   s.GetRequiredService<PlatillosDatos>()
                 )
             );

            // -----------------------
            // Configuración de Servicios Básicos e Infraestructura
            // -----------------------
            builder.Services.AddSignalR();
            builder.Services.AddControllers();

            // Configuración de autenticación JWT
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
                        Console.WriteLine("Error de autenticación JWT: " + ctx.Exception?.Message);
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

            // Configuración de CORS para el Frontend
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirFrontend", policy =>
                {
                    policy.AllowAnyOrigin()   // Permite que cualquier HTML abra la API
                          .AllowAnyMethod()   // Permite GET, POST, PUT, DELETE
                          .AllowAnyHeader();  // Permite enviar tokens JWT
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

                // === Documentación XML y anotaciones ===
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