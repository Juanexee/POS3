const tabla = document.getElementById('tabla-categorias');


async function cargarCategorias() {
    const url = "https://localhost:7081/"; // Mi puerto para hacer pruebas //supongo que si es otra pc hay que cambiarlo XD
    
    try {
        const token = localStorage.getItem('mi_token_pos');
        // Validación: ¿Tenemos la llave para entrar? en este caso el token
        if(!token) 
        {
            alert("No has iniciado sesión. Redirigiendo...");
            window.location.href = "login.html";
            return; // Detenemos la función (cargarCategorias) 

        }

        const url = "https://localhost:7081/api/Categorias";

        const respuesta = await fetch(url, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (!respuesta.ok) {
            throw new Error("Error al obtener datos");
        }

        const datos = await respuesta.json();
        console.log("Datos recibidos:", datos);
        
        // Aquí llamaríamos a la lógica para dibujar la tabla...
        
    } catch (error) {
        console.error("Hubo un fallo:", error);
    }
}