# Guía Suprema de Estudio y Arquitectura: Sistema POS Restaurante (POSWEB2 & POS3)

Esta guía de estudio detalla a fondo la arquitectura, el flujo de datos, los conceptos avanzados de programación y los mecanismos de seguridad implementados en el ecosistema del restaurante **"Rancho la Mimi"**. Combina el frontend moderno en JavaScript (POSWEB2) con el backend empresarial multicapa en .NET Core (POS3) y la persistencia en SQL Server.

---

## 1. Arquitectura General de la Solución (Full-Stack)

El sistema está diseñado bajo el patrón **Cliente-Servidor Desacoplado**, lo que permite que el cliente (Frontend) y el servidor (API Backend) evolucionen de forma independiente y se comuniquen mediante protocolos web estándar.

```mermaid
graph TD
    subgraph Frontend [Capa de Presentación - Cliente (POSWEB2)]
        UI[Páginas HTML5 / CSS3] <--> Ctrl[Controladores JavaScript]
        Ctrl <--> Model[Modelos de Dominio JS]
        Ctrl <--> Serv[Servicios HTTP / API Clients]
        Serv <--> WS[SignalR WebSocket Client]
    end

    subgraph Backend [Servidor de Servicios - API (POS3)]
        API[Controladores de API - ASP.NET Core]
        Hub[SignalR Hubs - CocinaHub]
        Neg[Capa de Reglas de Negocio - NEGOCIO]
        Dat[Capa de Acceso a Datos - DATOS]
        Ent[Entidades y DTOs - ENTIDADES]
    end

    subgraph Base de Datos [Persistencia (SQL Server)]
        SP[Stored Procedures & UDTT]
        DB[(RestauranteDB)]
    end

    %% Conexiones
    Serv <-->|HTTP REST / JWT Auth| API
    WS <-->|WebSockets / SignalR| Hub
    API <--> Neg
    Hub <--> Neg
    Neg <--> Dat
    Dat <-->|ADO.NET / SQLClient| SP
    SP <--> DB
    Ent -.->|Compartido en Backend| API
    Ent -.->|Compartido en Backend| Neg
    Ent -.->|Compartido en Backend| Dat
```

---

## 2. Arquitectura del Backend (.NET Core en 4 Capas)

El backend `POS3` sigue una arquitectura clásica de **N Capas (N-Layer Architecture)** para garantizar la separación de responsabilidades, la mantenibilidad y la escalabilidad del sistema:

1. **Capa de API REST (POS3)**:
   * **Propósito**: Expone los endpoints HTTP consumidos por el Frontend y administra la infraestructura (Swagger, autenticación JWT, políticas CORS y SignalR).
   * **Archivos Clave**: [Program.cs](file:///c:/Users/MUNDO%20TECH/Documents/Juan.exe/UNAN/UNAN%20TERCER%20A%C3%91O/1%20SEMESTRE/WEB%20II/POS3/POS3/Program.cs), controladores en `Controllers/` y hubs en `Hubs/`.
2. **Capa de Lógica de Negocio (NEGOCIO)**:
   * **Propósito**: Contiene las reglas del negocio, validaciones y orquestación de operaciones. No sabe cómo se guardan los datos ni cómo se exponen en la web; solo valida que los datos sean correctos según las reglas del restaurante.
   * **Archivos Clave**: `NEGOCIO/VentaNegocio.cs`, `NEGOCIO/UsuarioNegocio.cs`, `NEGOCIO/CategoriaNegocio.cs`.
3. **Capa de Acceso a Datos (DATOS)**:
   * **Propósito**: Ejecuta las consultas y los procedimientos almacenados en la base de datos SQL Server. Utiliza ADO.NET clásico para lograr el máximo rendimiento y control sobre las transacciones de SQL Server.
   * **Archivos Clave**: `DATOS/VentaDatos.cs`, `DATOS/CategoriasDatos.cs`, `DATOS/RecetaDatos.cs`.
4. **Capa de Entidades y DTOs (ENTIDADES)**:
   * **Propósito**: Contiene los modelos de datos que viajan entre todas las capas. Es una capa transversal libre de lógica pesada (contiene POCOs y DTOs).
   * **Archivos Clave**: `ENTIDADES/Venta.cs`, `ENTIDADES/UsuarioDTO.cs`, `ENTIDADES/Platillo.cs`.

---

## 3. Conceptos Clave de Programación y Buenas Prácticas

El proyecto implementa varios principios de ingeniería de software fundamentales:

### A. Inyección de Dependencias (Dependency Injection - DI)
En lugar de que una clase cree manualmente instancias de sus dependencias utilizando el operador `new` (lo que crearía un acoplamiento fuerte), las dependencias se inyectan a través del constructor. 
En [Program.cs](file:///c:/Users/MUNDO%20TECH/Documents/Juan.exe/UNAN/UNAN%20TERCER%20A%C3%91O/1%20SEMESTRE/WEB%20II/POS3/POS3/Program.cs), registramos las capas con un ciclo de vida `Scoped` (una instancia por cada petición HTTP):
```csharp
// Registro de la Capa de Datos
builder.Services.AddScoped<CategoriasDatos>(sp => new CategoriasDatos(connectionString));
// Registro de la Capa de Negocio
builder.Services.AddScoped<CategoriaNegocio>();
```
Luego, en [CategoriaController.cs](file:///c:/Users/MUNDO%20TECH/Documents/Juan.exe/UNAN/UNAN%20TERCER%20A%C3%91O/1%20SEMESTRE/WEB%20II/POS3/POS3/Controllers/CategoriaController.cs), el Framework de ASP.NET Core provee automáticamente la instancia de `CategoriaNegocio` requerida:
```csharp
public class CategoriaController : ControllerBase
{
    private readonly CategoriaNegocio _categoriaNegocio;

    public CategoriaController(CategoriaNegocio categoriaNegocio)
    {
        _categoriaNegocio = categoriaNegocio;
    }
}
```

### B. Uso de DTOs (Data Transfer Objects) vs. Entidades de Base de Datos
* **Entidades** (como `Usuario` o `Venta`): Representan la estructura interna de la base de datos. Pueden contener campos sensibles como `PasswordHash` o `PasswordSalt`.
* **DTOs** (como `UsuarioDTO` o `LoginDto`): Representan el contrato de datos que viaja por la red.
* **Por qué es buena práctica**: Protege la integridad de tus datos internos. Al hacer login, por ejemplo, el frontend envía un `LoginDto` (con la clave plana) y el backend responde con información segura del usuario (sin contraseñas ni hashes).

### C. Programación Asíncrona (Async / Await)
Tanto en C# (backend) como en JavaScript (frontend), las operaciones de E/S (lecturas a la base de datos o llamadas HTTP) son costosas. Utilizar `async` y `await` evita el bloqueo de hilos de ejecución en el servidor y de la interfaz en el navegador:
```csharp
// Backend en VentasController.cs
[HttpPost("registrar")]
public async Task<IActionResult> RegistrarVenta([FromBody] Venta venta)
{
    var respuesta = _ventaNegocio.RegistrarVenta(venta);
    if (respuesta.Success)
    {
        // El hilo de ejecución no se detiene bloqueado; notifica y continúa
        await _hubContext.Clients.All.SendAsync("PedidoActualizado");
    }
    return Ok(respuesta);
}
```

### D. Seguridad Criptográfica Avanzada (Argon2id)
Para almacenar las contraseñas, el backend no utiliza texto plano (lo cual sería una vulnerabilidad grave) ni algoritmos obsoletos como MD5 o SHA1. Utiliza **Argon2id** (a través de la biblioteca `Konscious.Security.Cryptography`), ganador de la Password Hashing Competition.
* **Salt**: Se genera una cadena aleatoria de bytes única para cada usuario utilizando un generador criptográfico fuerte (`RandomNumberGenerator`). Esto evita ataques de diccionario y tablas arcoíris.
* **Hash**: El Argon2id combina la clave plana con el Salt mediante configuraciones intensivas de memoria y paralelismo para ralentizar los ataques de fuerza bruta.
* **Verificación**: En [UsuarioNegocio.cs](file:///c:/Users/MUNDO%20TECH/Documents/Juan.exe/UNAN/UNAN%20TERCER%20A%C3%91O/1%20SEMESTRE/WEB%20II/POS3/NEGOCIO/UsuarioNegocio.cs#L147-L170), el método `Login` toma la clave provista, recupera el Salt de la base de datos, hashea la clave de prueba y compara los arrays resultantes byte a byte de forma segura.

---

## 4. Integraciones Avanzadas con SQL Server

Una de las joyas técnicas de este proyecto es cómo optimiza la comunicación y la coherencia de datos con el motor SQL Server.

### A. Tipos de Tabla Definidos por el Usuario (UDTT - User Defined Table Types)
Al registrar una venta, en lugar de realizar una llamada de base de datos por la cabecera de la factura y múltiples llamadas individuales para registrar cada detalle de platillo, se pasa una lista estructurada completa en una sola llamada de red.
1. En SQL Server se define un tipo tabla (`dbo.DetalleVentaType`):
   ```sql
   CREATE TYPE dbo.DetalleVentaType AS TABLE (
       platilloID INT,
       cantidad INT,
       precio_unitario DECIMAL(18,2),
       comentario VARCHAR(255)
   );
   ```
2. En C# ([VentaDatos.cs](file:///c:/Users/MUNDO%20TECH/Documents/Juan.exe/UNAN/UNAN%20TERCER%20A%C3%91O/1%20SEMESTRE/WEB%20II/POS3/DATOS/VentaDatos.cs#L24-L66)), mapeamos la lista de objetos a un objeto `DataTable` en memoria y lo pasamos al parámetro estructurado del comando:
   ```csharp
   DataTable tablaDetalles = new DataTable();
   tablaDetalles.Columns.Add("platilloID", typeof(int));
   tablaDetalles.Columns.Add("cantidad", typeof(int));
   // ... rellenar filas ...

   var parametroLista = cmd.Parameters.AddWithValue("@detalles", tablaDetalles);
   parametroLista.SqlDbType = SqlDbType.Structured;
   parametroLista.TypeName = "dbo.DetalleVentaType";
   ```
3. El Procedimiento Almacenado `sp_RegistrarVentaCompleta_QR` recibe este parámetro y puede hacer operaciones complejas de base de datos (por ejemplo, insertar todo con un `INSERT INTO ... SELECT` e incluso restar el stock de ingredientes vinculados en la tabla de insumos).

### B. Paso de Parámetros en Formato JSON
En la gestión de recetas (`RecetaDatos.cs`), el sistema serializa la lista de ingredientes en un string JSON y lo envía al procedimiento almacenado `sp_GestionarReceta_Transactional`:
```csharp
string detallesJson = JsonSerializer.Serialize(detalles);
cmd.Parameters.AddWithValue("@detallesReceta", detallesJson);
```
En la base de datos, el procedimiento utiliza funciones nativas como `OPENJSON` para convertir ese string JSON en registros de tabla individuales y procesar la actualización de forma segura dentro de una transacción de SQL.

### C. Manejo Inteligente de Integridad Referencial (Borrado Seguro)
En [CategoriasDatos.cs](file:///c:/Users/MUNDO%20TECH/Documents/Juan.exe/UNAN/UNAN%20TERCER%20A%C3%91O/1%20SEMESTRE/WEB%20II/POS3/DATOS/CategoriasDatos.cs#L122-L155), el sistema implementa una lógica excelente de borrado seguro:
1. **Intenta un borrado físico** de la categoría: `DELETE FROM Categorias WHERE categoriaID = @id`.
2. Si la categoría tiene platillos asociados, SQL Server arroja una excepción con el código de error `547` (violación de clave foránea).
3. El bloque `catch` de C# intercepta este error y aplica un **borrado lógico** (desactivación): `UPDATE Categorias SET activo = 0 WHERE categoriaID = @id`.
```csharp
try
{
    // 1. Intento de borrado físico
    int filas = cmd.ExecuteNonQuery();
    if (filas > 0) return true;
}
catch (SqlException ex)
{
    if (ex.Number == 547) // Conflicto de FK
    {
        // 2. Fallback automático a borrado lógico por integridad
        string queryUpdate = "UPDATE Categorias SET activo = 0 WHERE categoriaID = @id";
        // ... ejecuta actualización ...
    }
}
```

---

## 5. Comunicación en Tiempo Real con SignalR

Para evitar que los chefs de la cocina o los meseros tengan que estar recargando constantemente el navegador para ver si ingresaron nuevos pedidos (técnica ineficiente llamada *polling*), el backend implementa **SignalR**.

* **CocinaHub**: Declara una conexión WebSocket permanente.
* **Flujo del mensaje**:
  1. El cliente (por ejemplo, un comensal por QR o un cajero) registra un pedido llamando a `VentasController.RegistrarVenta()`.
  2. Tras guardar los datos en SQL Server con éxito, el controlador utiliza `IHubContext<CocinaHub>` para enviar un evento a todos los visores conectados:
     ```csharp
     await _hubContext.Clients.All.SendAsync("PedidoActualizado");
     ```
  3. En el Frontend, los visores de cocina escuchan el evento `PedidoActualizado` y recargan inmediatamente su lista de pedidos en pantalla de forma automática y silenciosa.

---

## 6. Autenticación y Autorización mediante JWT (JSON Web Tokens)

Para asegurar que solo los usuarios autorizados (como administradores o cajeros) realicen acciones administrativas, el sistema implementa tokens JWT de la siguiente manera:

```
[ FRONTEND ]                                             [ BACKEND (API) ]
  |                                                        |
  |-- 1. POST /api/Auth/login (User/Pass) ---------------->| (Valida credenciales en BD)
  |<-- 2. Retorna Token JWT (Firmado con Key secreta) -----|
  |                                                        |
  | (Guarda Token en localStorage)                         |
  |                                                        |
  |-- 3. GET /Categoria/Leer (Sin autenticación) --------->| [AllowAnonymous] -> Retorna datos
  |                                                        |
  |-- 4. POST /Categoria/Insertar + Header Authorization ->| [Authorize(Roles = "Administrador")]
  |      (Bearer <token>)                                  | (Valida firma del token y roles)
  |                                                        | -> Retorna 200 OK o 401 Unauthorized
```

### Decodificación de Claims en el Frontend
Una vez que el frontend recibe el token, utiliza una utilidad para decodificar la información sin requerir llamadas adicionales a la API:
```javascript
// shared/services/authService.js
decodificarToken(token) {
    try {
        const payloadBase64 = token.split('.')[1]; // Extrae la parte central del JWT
        const payloadDecodificado = atob(payloadBase64); // Decodifica Base64
        return JSON.parse(payloadDecodificado); // Devuelve claims de roles y datos de usuario
    } catch (e) {
        return null;
    }
}
```

---

## 7. Preguntas de Autoevaluación y Guía Práctica

Consolida tus bases de programación respondiendo a los siguientes escenarios del código del sistema:

1. **¿Qué patrón de diseño se aplica al configurar la inyección de dependencias en `Program.cs` y por qué es mejor que instanciar clases directamente?**
   * *Respuesta de estudio*: Se aplica el patrón de **Inversión de Control (IoC)**. Permite que las clases dependan de abstracciones (interfaces como `IVentaDatos` o `IRecetaDatos`) en lugar de implementaciones concretas, haciendo que el código sea testeable con pruebas unitarias y fácil de modificar.
2. **¿Por qué el método `ActualizarUsuario` en `UsuarioNegocio.cs` recibe parámetros opcionales para la contraseña (`passwordHash` y `passwordSalt` como nulos)?**
   * *Respuesta de estudio*: Porque al actualizar el perfil de un usuario, el administrador puede decidir no cambiar la contraseña. Al permitir parámetros nulos, el código de acceso a datos en SQL Server solo actualiza la contraseña si se provee una nueva, manteniendo la contraseña actual intacta si no se ingresó una nueva.
3. **¿Cuál es la ventaja de usar un tipo de tabla definido por el usuario (UDTT) para insertar detalles de facturación en comparación con hacer un bucle de inserción en C#?**
   * *Respuesta de estudio*: Optimiza el rendimiento de red. Realizar un bucle en C# requiere $N$ viajes de ida y vuelta al servidor de base de datos para insertar $N$ detalles, lo cual es ineficiente y puede bloquear la conexión. El UDTT permite empaquetar toda la orden en un único envío y realizar la inserción y la validación transaccional directamente en la base de datos.
4. **Si el servidor de base de datos SQL Server rechaza un registro de venta porque uno de los ingredientes en la receta tiene "stock insuficiente", ¿cómo maneja la aplicación este error desde la base de datos hasta el usuario en la pantalla?**
   * *Respuesta de estudio*: El procedimiento almacenado genera un error `RAISERROR` con una severidad que aborta la transacción y lanza una excepción. C# captura esta excepción SQL en el bloque `catch` de `VentaDatos.cs` y la eleva como una excepción general. El `VentasController.cs` captura el error en su bloque `try-catch` y retorna un estado HTTP `400 BadRequest` o `500` con el mensaje explicativo. Finalmente, el frontend intercepta el error en su `HttpService` y lo muestra dinámicamente en pantalla mediante una alerta al usuario.
