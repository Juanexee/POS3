using System.Text;
using DATOS;
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
                throw new InvalidOperationException("");

            // -----------------------
            // Bindear configuración Jwt a POCO y validarla
            // -----------------------
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddScoped<IVentaDatos>(provider => new VentaDatos(connectionString));
            builder.Services.AddScoped<InsumoDatos>(s => new InsumoDatos(connectionString));
            // Registrar Sesiones
            builder.Services.AddScoped<SesionDatos>(s => new SesionDatos(connectionString));
            builder.Services.AddScoped<SesionNegocio>();
           
            builder.Services.AddScoped<IVentaNegocio, VentaNegocio>(s =>
                 new VentaNegocio(
                   s.GetRequiredService<IVentaDatos>(),
                   s.GetRequiredService<SesionDatos>(),
                   s.GetRequiredService<PlatillosDatos>()
                 )
             );
            builder.Services.AddScoped<CompraDatos>(s => new CompraDatos(connectionString));
            builder.Services.AddScoped<UnidadMedidaDatos>(s => new UnidadMedidaDatos(connectionString));
            builder.Services.AddScoped<PlatillosDatos>(s => new PlatillosDatos(connectionString));
            builder.Services.AddScoped<IPlatillosDatos, PlatillosDatos>(s => new PlatillosDatos(connectionString));

            // Opcional: obtener una instancia inmediata para validar claves ahora
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtSettings = jwtSection.Get<JwtSettings>();

            if (jwtSettings == null)
                throw new InvalidOperationException("Se requiere la sección 'Jwt' en appsettings.json.");

            if (string.IsNullOrWhiteSpace(jwtSettings.Key))
                throw new InvalidOperationException("La clave 'Jwt:Key' no está configurada. Revisa appsettings.json y que esté copiado al output.");

            // -----------------------
            // Servicios
            // -----------------------
            builder.Services.AddScoped<VentaNegocio>();
            builder.Services.AddScoped<SesionNegocio>();
            builder.Services.AddScoped<PedidoNegocio>();
            builder.Services.AddScoped<InsumoNegocio>();
            builder.Services.AddScoped<CompraNegocio>();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<UnidadMedidaNegocio>();

            builder.Services.AddControllers();


            // Configuración de autenticación JWT
            // -----------------------
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
                    Title = "Racho la mimi",
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

                //-=======================documentacion

                //=== documentación XML y anotaciones===

                var xmlfile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
                c.IncludeXmlComments(xmlpath, includeControllerXmlComments: true);

                //habilitar anotacion (para atributos como [SwaggerOperation], [SwaggerResponse], etc )

                c.EnableAnnotations();

                //var xmlfile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
                //c.IncludeXmlComments(xmlpath);//
                //// habilitar anotaciones (para   
                //c.EnableAnnotations();



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
            app.MapHub<CocinaHub>("/cocinaHub");

            app.MapControllers();
           
            app.Run();


        }
    }
}