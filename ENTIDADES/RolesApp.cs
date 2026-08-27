namespace ENTIDADES
{
    /// <summary>
    /// Constantes canónicas de roles de la aplicación.
    /// Usar SIEMPRE estas constantes en [Authorize(Roles=...)] y en cualquier
    /// comparación de roles, nunca strings literales sueltos.
    /// 
    /// Roles del TDR (RF-AUT-03):
    ///   Admin   → gestión total del sistema
    ///   Mesero  → toma de pedidos, mesas
    ///   Cocina  → KDS, estados de platillos
    ///   Caja    → facturación, cobros
    /// </summary>
    public static class RolesApp
    {
        public const string Admin   = "Admin";
        public const string Mesero  = "Mesero";
        public const string Cocina  = "Cocina";
        public const string Caja    = "Caja";

        /// <summary>
        /// Todos los roles que pueden operar la app (útil para [Authorize(Roles=RolesApp.Todos)])
        /// </summary>
        public const string Todos = Admin + "," + Mesero + "," + Cocina + "," + Caja;

        /// <summary>
        /// Solo roles con acceso administrativo o de caja (reportes, ventas, usuarios)
        /// </summary>
        public const string AdminOCaja = Admin + "," + Caja;

        /// <summary>
        /// Roles que pueden ver y gestionar pedidos en cocina
        /// </summary>
        public const string AdminOCocina = Admin + "," + Cocina;

        /// <summary>
        /// Roles que pueden registrar y gestionar pedidos de mesa
        /// </summary>
        public const string AdminOMesero = Admin + "," + Mesero;
    }
}
