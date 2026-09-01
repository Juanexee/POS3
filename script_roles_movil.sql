-- =====================================================
-- Script SQL: Nuevos roles para RBAC App Móvil Gerencial
-- RF-MOV-AUT-03: Gerente, Administrador, Supervisor
-- =====================================================

-- 1. Insertar rol Gerente (si no existe)
IF NOT EXISTS (SELECT 1 FROM Roles WHERE nombreRol = 'Gerente')
BEGIN
    INSERT INTO Roles (nombreRol, descripcionRol, activo)
    VALUES ('Gerente', 'Acceso completo a dashboards analíticos, KPIs, reportes y auditoría del sistema', 1);
    PRINT 'Rol Gerente creado correctamente.';
END
ELSE
    PRINT 'Rol Gerente ya existe.';

-- 2. Insertar rol Supervisor (si no existe)
IF NOT EXISTS (SELECT 1 FROM Roles WHERE nombreRol = 'Supervisor')
BEGIN
    INSERT INTO Roles (nombreRol, descripcionRol, activo)
    VALUES ('Supervisor', 'Acceso a monitoreo de inventario, alertas de stock y listado de facturas', 1);
    PRINT 'Rol Supervisor creado correctamente.';
END
ELSE
    PRINT 'Rol Supervisor ya existe.';

-- 3. Agregar columna stockMinimo a la tabla Insumos (si no existe)
-- Necesaria para RF-MOV-MON-02: alertas de stock crítico por insumo
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Insumos' AND COLUMN_NAME = 'stockMinimo'
)
BEGIN
    ALTER TABLE Insumos 
    ADD stockMinimo DECIMAL(10, 2) NULL;
    PRINT 'Columna stockMinimo agregada a la tabla Insumos.';
END
ELSE
    PRINT 'Columna stockMinimo ya existe en Insumos.';

-- 4. Verificar los roles actuales
SELECT rolID, nombreRol, descripcionRol, activo 
FROM Roles 
ORDER BY rolID;

GO
