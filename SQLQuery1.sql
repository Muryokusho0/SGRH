-- Created by GitHub Copilot in SSMS - review carefully before executing

-- =============================================
-- Sistema de Gestión de Reservas de Habitaciones (SGRH)
-- Base de datos para gestión hotelera integral
-- Versión: 1.0
-- =============================================

USE SGRH
GO

-- =============================================
-- TABLA: CategoriaHabitacion
-- Descripción: Define los tipos de habitaciones (Suite, Doble, Simple, etc.)
-- con sus características base
-- =============================================
CREATE TABLE dbo.CategoriaHabitacion(
	CategoriaHabitacionId INT CONSTRAINT pk_ch_id PRIMARY KEY IDENTITY(1,1),
	nombreCategoria VARCHAR(50) NOT NULL,        -- Ej: Suite Presidencial, Habitación Doble
	capacidad INT NOT NULL,                      -- Número máximo de personas
	descripcion NVARCHAR(255) NOT NULL,          -- Descripción detallada de la categoría
	precioBase DECIMAL(10,2) NOT NULL           -- Precio base fuera de temporadas especiales
)
-- Índice único para evitar categorías duplicadas
CREATE UNIQUE INDEX IX_CategoriaHabitacion_nombreCategoria ON dbo.CategoriaHabitacion(nombreCategoria)  
GO

-- =============================================
-- TABLA: Temporada
-- Descripción: Define períodos de temporada (Alta, Baja, Media)
-- para aplicar tarifas diferenciadas
-- Nota: fechaFin es EXCLUSIVA (rango semi-abierto [inicio, fin))
-- =============================================
CREATE TABLE dbo.Temporada(
	TemporadaId INT CONSTRAINT pk_t_id PRIMARY KEY IDENTITY(1,1),
	nombreTemporada VARCHAR(50) NOT NULL,        -- Ej: Temporada Alta, Navidad, Verano
	fechaInicio DATE NOT NULL,
	fechaFin DATE NOT NULL,
	-- Valida que la fecha de inicio sea anterior a la fecha fin
	CONSTRAINT CK_Temporada_Rango_FinExclusivo CHECK (fechaInicio < fechaFin)
)
-- Índice para búsquedas eficientes por rango de fechas
CREATE INDEX IX_Temporada_Rango ON dbo.Temporada(fechaInicio, fechaFin) INCLUDE(nombreTemporada)
GO

-- =============================================
-- EXTENDED PROPERTY: Documentación de fechaFin
-- Descripción: Aclara que fechaFin es exclusiva en el rango temporal
-- Para cubrir hasta el día X inclusive, usar fechaFin = DATEADD(day,1,X)
-- =============================================
EXEC sys.sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Rango de Temporada: [fechaInicio, fechaFin). fechaFin es EXCLUSIVA. Para cubrir hasta el día X inclusive, use fechaFin = DATEADD(day,1,X).',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Temporada',
    @level2type = N'COLUMN', @level2name = N'fechaFin';
GO

-- =============================================
-- VISTA: vTemporada_Mostrable
-- Descripción: Vista auxiliar para mostrar temporadas con ambas fechas fin
-- Proporciona tanto fechaFinExclusiva (valor almacenado) como fechaFinInclusiva
-- (último día incluido en la temporada) para facilitar visualización
-- =============================================
CREATE OR ALTER VIEW dbo.vTemporada_Mostrable
AS
SELECT
    TemporadaId,
    nombreTemporada,
    fechaInicio,
    fechaFinExclusiva = fechaFin,                    -- Valor almacenado en BD
    fechaFinInclusiva = DATEADD(day, -1, fechaFin)  -- Último día incluido
FROM dbo.Temporada;
GO

-- =============================================
-- TABLA: TarifaTemporada
-- Descripción: Relaciona categorías de habitación con temporadas
-- permitiendo precios dinámicos según la época del año
-- =============================================
CREATE TABLE dbo.TarifaTemporada(
	TarifaTemporadaId INT CONSTRAINT pk_tt_id PRIMARY KEY IDENTITY(1,1),
	CategoriaHabitacionId INT NOT NULL,
	TemporadaId INT NOT NULL,
	Precio DECIMAL(10,2) NOT NULL,              -- Precio específico para esta combinación
	CONSTRAINT fk_tt_chId FOREIGN KEY (CategoriaHabitacionId) REFERENCES dbo.CategoriaHabitacion(CategoriaHabitacionId),
	CONSTRAINT fk_tt_tId FOREIGN KEY (TemporadaId) REFERENCES dbo.Temporada(TemporadaId)
)
-- Índice único para evitar tarifas duplicadas para la misma temporada/categoría
CREATE UNIQUE INDEX IX_TarifaTemporada_Categoria ON dbo.TarifaTemporada(TemporadaId,CategoriaHabitacionId) INCLUDE(Precio)
GO

-- =============================================
-- TABLA: ServicioAdicional
-- Descripción: Catálogo de servicios extras ofrecidos
-- (Desayuno, Spa, Tours, etc.)
-- =============================================
CREATE TABLE dbo.ServicioAdicional(
	ServicioAdicionalId INT CONSTRAINT pk_s_id PRIMARY KEY IDENTITY(1,1),
	nombreServicio NVARCHAR(50) NOT NULL,        -- Ej: Desayuno Buffet, Masaje
	tipoServicio NVARCHAR(50) NOT NULL          -- Categorización: Alimentación, Spa, Transporte
)
-- Índice para filtrar servicios por tipo eficientemente
CREATE INDEX IX_ServicioAdicional_Tipo_Nombre ON dbo.ServicioAdicional(tipoServicio,nombreServicio)
GO

-- =============================================
-- TABLA: ServicioCategoriaPrecio
-- Descripción: Define precios de servicios según la categoría
-- de habitación (servicios pueden variar en precio por categoría)
-- Ejemplo: Desayuno en Suite = $50, Desayuno en Simple = $30
-- =============================================
CREATE TABLE dbo.ServicioCategoriaPrecio(
	ServicioAdicionalId INT NOT NULL,
	CategoriaHabitacionId INT NOT NULL,
	Precio DECIMAL(10,2) NOT NULL,
	CONSTRAINT fk_scp_saId FOREIGN KEY (ServicioAdicionalId) REFERENCES dbo.ServicioAdicional(ServicioAdicionalId),
	CONSTRAINT fk_scp_chId FOREIGN KEY (CategoriaHabitacionId) REFERENCES dbo.CategoriaHabitacion(CategoriaHabitacionId),
	CONSTRAINT pk_scp_sachId PRIMARY KEY(ServicioAdicionalId,CategoriaHabitacionId)
)
-- Índice para búsquedas por categoría
CREATE INDEX IX_ServicioCategoriaPrecio_Categoria ON dbo.ServicioCategoriaPrecio(CategoriaHabitacionId, ServicioAdicionalId) INCLUDE(Precio)
GO

-- =============================================
-- TABLA: ServicioTemporada
-- Descripción: Relación muchos-a-muchos entre servicios adicionales y temporadas
-- Permite definir qué servicios están disponibles en cada temporada
-- Uso: Activar/desactivar servicios estacionales (ej: Tours de verano, Spa de invierno)
-- Si un servicio no está en esta tabla para una temporada, no está disponible
-- =============================================
CREATE TABLE dbo.ServicioTemporada (
    ServicioAdicionalId INT NOT NULL,
    TemporadaId INT NOT NULL,
    CONSTRAINT pk_st_ServicioTemporada PRIMARY KEY (ServicioAdicionalId, TemporadaId),
    CONSTRAINT fk_st_s FOREIGN KEY (ServicioAdicionalId) REFERENCES dbo.ServicioAdicional(ServicioAdicionalId),
    CONSTRAINT fk_st_t FOREIGN KEY (TemporadaId) REFERENCES dbo.Temporada(TemporadaId)
);
-- Índice para buscar servicios disponibles en una temporada específica
CREATE INDEX IX_ServicioTemporada_Temporada ON dbo.ServicioTemporada (TemporadaId, ServicioAdicionalId);
GO

-- =============================================
-- TABLA: Habitacion
-- Descripción: Inventario físico de habitaciones del hotel
-- Cada habitación pertenece a una categoría que define sus características
-- =============================================
CREATE TABLE dbo.Habitacion(
	HabitacionId INT CONSTRAINT pk_h_id PRIMARY KEY IDENTITY(1,1),
	CategoriaHabitacionId INT NOT NULL,
	NumeroHabitacion DECIMAL(5,0) NOT NULL UNIQUE,  -- Número visible de la habitación (101, 201, etc.)
	Piso INT NOT NULL,                              -- Piso donde se ubica la habitación
	CONSTRAINT fk_fchId FOREIGN KEY (CategoriaHabitacionId) REFERENCES dbo.CategoriaHabitacion(CategoriaHabitacionId)
)
-- Índice para agrupar habitaciones por categoría
CREATE INDEX IX_Habitacion_Categoria ON dbo.Habitacion(CategoriaHabitacionId) INCLUDE(NumeroHabitacion, Piso)
GO

-- =============================================
-- TABLA: HabitacionHistorial
-- Descripción: Registro histórico de estados de habitaciones
-- para trazabilidad (Disponible, Ocupada, Limpieza, Mantenimiento)
-- Implementa patrón SCD Type 2 (Slowly Changing Dimension)
-- Permite rastrear todos los cambios de estado con períodos de vigencia
-- =============================================
CREATE TABLE dbo.HabitacionHistorial(
	HabitacionHistorialId INT CONSTRAINT pk_hh_hhid PRIMARY KEY IDENTITY(1,1),
	HabitacionId INT NOT NULL,
	EstadoHabitacion VARCHAR(50) NOT NULL CHECK (EstadoHabitacion IN ('Disponible', 'Ocupada', 'Mantenimiento', 'Limpieza')),
	FechaInicio DATETIME NOT NULL,
	FechaFin DATETIME,                              -- NULL indica estado actual/vigente
	MotivoCambio NVARCHAR(255),                     -- Razón del cambio de estado
	CONSTRAINT fk_hh_hId FOREIGN KEY (HabitacionId) REFERENCES dbo.Habitacion(HabitacionId),
	-- Evita registros duplicados de historial
	CONSTRAINT uq_habitacion_historial UNIQUE (HabitacionId, FechaInicio, FechaFin),
	-- Valida que FechaInicio < FechaFin cuando FechaFin no es NULL
	CONSTRAINT ck_habitacion_historial CHECK (FechaFin IS NULL OR FechaInicio < FechaFin),
    -- Valida que MotivoCambio sea requerido para estados de mantenimiento o limpieza, y nulo para otros estados
    constraint ck_hh_Motivo_requerido CHECK ((EstadoHabitacion IN ('Mantenimiento', 'Limpieza') AND MotivoCambio IS NOT NULL) OR (EstadoHabitacion IN ('Disponible', 'Ocupada') AND MotivoCambio IS NULL))
)
-- Índice para consultas de historial por habitación ordenadas por fecha
CREATE INDEX IX_HabitacionHistorial_Habitacion_Fecha ON dbo.HabitacionHistorial(HabitacionId,FechaInicio DESC) INCLUDE(EstadoHabitacion, FechaFin, MotivoCambio)
-- Índice único filtrado: garantiza solo un registro vigente (FechaFin NULL) por habitación
CREATE UNIQUE INDEX UX_HabitacionHistorial_Habitacion_Vigente ON dbo.HabitacionHistorial(HabitacionId) WHERE FechaFin IS NULL
GO

-- =============================================
-- TABLA: Cliente
-- Descripción: Información de clientes del hotel
-- Almacena datos personales y de contacto para gestión de reservas
-- =============================================
CREATE TABLE dbo.Cliente(
	ClienteId INT CONSTRAINT pk_c_id PRIMARY KEY IDENTITY(1,1),
	NationalID VARCHAR(20) NOT NULL UNIQUE,         -- Cédula/Pasaporte/DNI
	nombreCliente NVARCHAR(100) NOT NULL,
	apellidoCliente NVARCHAR(100) NOT NULL,
	email VARCHAR(100) NOT NULL UNIQUE,
	telefono VARCHAR(20) NOT NULL
)
GO

-- =============================================
-- TABLA: Usuario
-- Descripción: Usuarios del sistema con roles diferenciados
-- ADMIN: Administrador con acceso completo al sistema
-- RECEPCIONISTA: Personal de recepción con acceso limitado
-- CLIENTE: Clientes que pueden hacer reservas online
-- Relación: CLIENTE debe tener ClienteId, ADMIN/RECEPCIONISTA no
-- =============================================
CREATE TABLE dbo.Usuario (
    UsuarioId INT CONSTRAINT pk_u_id PRIMARY KEY IDENTITY(1,1),
    ClienteId INT NULL,                             -- Solo para rol CLIENTE
    Username NVARCHAR(100) NOT NULL,                -- Usuario o email para login
    PasswordHash NVARCHAR(255) NOT NULL,            -- Hash de contraseña (nunca texto plano)
    Rol VARCHAR(20) NOT NULL CONSTRAINT CK_Usuario_Rol CHECK (Rol IN ('ADMIN', 'RECEPCIONISTA', 'CLIENTE')),
    Activo BIT NOT NULL DEFAULT 1,                  -- Para deshabilitar usuarios sin eliminarlos
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Usuario_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Usuario_Username UNIQUE (Username),
    CONSTRAINT fk_u_cId FOREIGN KEY (ClienteId) REFERENCES dbo.Cliente(ClienteId),
	-- Valida que CLIENTES tengan ClienteId y ADMIN/RECEPCIONISTA no lo tengan
	CONSTRAINT CK_Usuario_ClienteId_SegunRol CHECK 
	((Rol = 'CLIENTE' AND ClienteId IS NOT NULL) OR (Rol IN ('ADMIN', 'RECEPCIONISTA') AND ClienteId IS NULL))
)
-- Índices para optimizar búsquedas por cliente y por rol
CREATE INDEX IX_Usuario_Cliente ON dbo.Usuario (ClienteId) INCLUDE (Rol, Activo, Username);
CREATE INDEX IX_Usuario_Rol_Activo ON dbo.Usuario (Rol, Activo) INCLUDE (Username, ClienteId);
GO

-- =============================================
-- TABLA: Reserva
-- Descripción: Reservas realizadas por clientes
-- Estados: 
--   - Pendiente: Recién creada, aún no confirmada/pagada
--   - Confirmada: Pagada y confirmada, snapshot de precios inmutable
--   - Cancelada: Reserva cancelada
-- =============================================
CREATE TABLE dbo.Reserva(
	ReservaId INT CONSTRAINT pk_r_rId PRIMARY KEY IDENTITY(1,1),
	ClienteId INT NOT NULL,
	EstadoReserva VARCHAR(50) NOT NULL CHECK (EstadoReserva IN ('Pendiente','Confirmada','Cancelada','Finalizada')),
	FechaReserva DATETIME NOT NULL,                 -- Fecha en que se realizó la reserva
	FechaEntrada DATETIME NOT NULL,                 -- Check-in
	FechaSalida DATETIME NOT NULL,                  -- Check-out
	CONSTRAINT fk_r_cId FOREIGN KEY (ClienteId) REFERENCES dbo.Cliente(ClienteId),
	-- Valida que la fecha de entrada sea anterior a la salida
	CONSTRAINT chk_fecha CHECK (FechaEntrada < FechaSalida)
)
-- Índice para búsquedas de reservas por cliente ordenadas por fecha
CREATE INDEX IX_Reserva_Cliente_Fecha ON dbo.Reserva (ClienteId, FechaReserva DESC) INCLUDE (EstadoReserva, FechaEntrada, FechaSalida);
-- Índice para verificar disponibilidad de habitaciones en rangos de fechas
CREATE INDEX IX_Reserva_Rango_Estado ON dbo.Reserva (FechaEntrada, FechaSalida, EstadoReserva) INCLUDE (ClienteId, FechaReserva);
GO

-- =============================================
-- TABLA: DetalleReserva
-- Descripción: Habitaciones específicas asignadas a cada reserva
-- Una reserva puede incluir múltiples habitaciones
-- TarifaAplicada se congela al momento de crear el detalle (snapshot de precio)
-- =============================================
CREATE TABLE dbo.DetalleReserva(
	DetalleReservaId INT CONSTRAINT pk_dr_drId PRIMARY KEY IDENTITY(1,1),
	ReservaId INT NOT NULL,
	HabitacionId INT NOT NULL,
	TarifaAplicada DECIMAL(10,2) NOT NULL CHECK (TarifaAplicada > 0),  -- Tarifa fijada al momento de reservar
	CONSTRAINT fk_dr_rId FOREIGN KEY (ReservaId) REFERENCES dbo.Reserva(ReservaId),
	CONSTRAINT fk_dr_hId FOREIGN KEY (HabitacionId) REFERENCES dbo.Habitacion(HabitacionId)
)
-- Índice único: una habitación no puede estar en la misma reserva dos veces
CREATE UNIQUE INDEX IX_DetalleReserva_Habitacion ON dbo.DetalleReserva (HabitacionId, ReservaId) INCLUDE (TarifaAplicada);
-- Índice para consultar todas las habitaciones de una reserva
CREATE INDEX IX_DetalleReserva_Reserva ON dbo.DetalleReserva (ReservaId) INCLUDE (HabitacionId, TarifaAplicada);
GO

-- =============================================
-- TABLA: ReservaServicioAdicional
-- Descripción: Servicios adicionales contratados para una reserva
-- Cantidad permite múltiples unidades del mismo servicio (ej: 3 desayunos)
-- PrecioUnitarioAplicado y SubTotalAplicado se congelan al reservar
-- =============================================
CREATE TABLE dbo.ReservaServicioAdicional(
	ReservaServicioAdicionalId INT CONSTRAINT pk_rsa_rsaId PRIMARY KEY IDENTITY(1,1),
	ReservaId INT NOT NULL,
	ServicioAdicionalId INT NOT NULL,
	Cantidad INT NOT NULL DEFAULT 1 CHECK(Cantidad > 0),                -- Ej: 3 desayunos
	PrecioUnitarioAplicado DECIMAL(10,2) NOT NULL CHECK (PrecioUnitarioAplicado > 0),  -- Precio fijado al reservar
	SubTotalAplicado AS (Cantidad * PrecioUnitarioAplicado) PERSISTED,  -- Columna calculada persistida
	CONSTRAINT fk_rsa_rId FOREIGN KEY (ReservaId) REFERENCES dbo.Reserva(ReservaId),
	CONSTRAINT fk_rsa_saId FOREIGN KEY (ServicioAdicionalId) REFERENCES dbo.ServicioAdicional(ServicioAdicionalId),
	-- Un servicio solo puede aparecer una vez por reserva (ajustar cantidad en lugar de duplicar)
	CONSTRAINT uq_reserva_servicio UNIQUE (ReservaId, ServicioAdicionalId)
)
-- Índice para consultar servicios de una reserva específica
CREATE INDEX IX_RSA_Reserva ON dbo.ReservaServicioAdicional (ReservaId) INCLUDE (ServicioAdicionalId, Cantidad, PrecioUnitarioAplicado);
-- Índice para reportes de uso por servicio
CREATE INDEX IX_RSA_Servicio ON dbo.ReservaServicioAdicional (ServicioAdicionalId) INCLUDE (ReservaId, Cantidad, PrecioUnitarioAplicado);
GO

-- ============================================================
-- MIGRACIÓN: Temporadas recurrentes + Servicio todas temporadas
-- Ejecutar en SSMS en la base de datos SGRH
-- ============================================================

-- 1. Agregar columnas de temporada recurrente
ALTER TABLE dbo.Temporada
ADD EsRecurrente BIT NOT NULL DEFAULT 0,
    MesInicio    TINYINT NULL,
    DiaInicio    TINYINT NULL,
    MesFin       TINYINT NULL,
    DiaFin       TINYINT NULL;
GO

-- Hacer FechaInicio y FechaFin opcionales (nullable) para temporadas recurrentes
ALTER TABLE dbo.Temporada ALTER COLUMN fechaInicio DATE NULL;
ALTER TABLE dbo.Temporada ALTER COLUMN fechaFin    DATE NULL;
GO

-- Constraint: si no es recurrente, fechas deben estar presentes
ALTER TABLE dbo.Temporada
ADD CONSTRAINT CK_Temporada_FechaORecurrente CHECK (
    (EsRecurrente = 0 AND fechaInicio IS NOT NULL AND fechaFin IS NOT NULL)
    OR
    (EsRecurrente = 1 AND MesInicio IS NOT NULL AND DiaInicio IS NOT NULL
                      AND MesFin   IS NOT NULL AND DiaFin    IS NOT NULL)
);
GO

-- 2. Agregar columna AplicaTodasTemporadas en ServicioAdicional
ALTER TABLE dbo.ServicioAdicional
ADD AplicaTodasTemporadas BIT NOT NULL DEFAULT 0;
GO

-- 3. Actualizar función fnTemporadaIdPorFecha para soporte recurrente
DROP FUNCTION IF EXISTS dbo.fnTemporadaIdPorFecha;
GO

CREATE OR ALTER FUNCTION dbo.fnTemporadaIdPorFecha (@Fecha DATE)
RETURNS INT
AS
BEGIN
    DECLARE @TemporadaId INT;
    DECLARE @Mes TINYINT = MONTH(@Fecha);
    DECLARE @Dia TINYINT = DAY(@Fecha);

    -- Primero buscar temporada específica (con año exacto)
    SELECT TOP (1) @TemporadaId = t.TemporadaId
    FROM dbo.Temporada t
    WHERE t.EsRecurrente = 0
      AND @Fecha >= t.fechaInicio
      AND @Fecha <  t.fechaFin
    ORDER BY t.fechaInicio DESC;

    IF @TemporadaId IS NOT NULL
        RETURN @TemporadaId;

    -- Luego buscar temporada recurrente por mes/día

    -- Rango normal (sin cruzar año): ej. 15-mar al 14-jun
    SELECT TOP (1) @TemporadaId = t.TemporadaId
    FROM dbo.Temporada t
    WHERE t.EsRecurrente = 1
      AND t.MesInicio <= t.MesFin   -- no cruza año
      AND (
          (@Mes > t.MesInicio OR (@Mes = t.MesInicio AND @Dia >= t.DiaInicio))
          AND
          (@Mes < t.MesFin   OR (@Mes = t.MesFin   AND @Dia <  t.DiaFin))
      );

    IF @TemporadaId IS NOT NULL
        RETURN @TemporadaId;

    -- Rango que cruza año: ej. 15-dic al 14-ene
    SELECT TOP (1) @TemporadaId = t.TemporadaId
    FROM dbo.Temporada t
    WHERE t.EsRecurrente = 1
      AND t.MesInicio > t.MesFin    -- cruza año
      AND (
          (@Mes > t.MesInicio OR (@Mes = t.MesInicio AND @Dia >= t.DiaInicio))
          OR
          (@Mes < t.MesFin   OR (@Mes = t.MesFin   AND @Dia <  t.DiaFin))
      );

    RETURN @TemporadaId;
END
GO

-- =============================================
-- TRIGGER: TR_Temporada_NoSolapamiento
-- Descripción: Previene que dos temporadas tengan rangos de fechas superpuestos
-- Momento: Se ejecuta DESPUÉS de INSERT o UPDATE en Temporada
-- Lógica: Compara las fechas de la nueva temporada con todas las existentes
--         usando lógica de intersección de rangos: (inicio1 < fin2) AND (fin1 > inicio2)
-- Importante: Garantiza que siempre haya a lo más una temporada activa por fecha
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_Temporada_NoSolapamiento
ON dbo.Temporada
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Verifica si alguna temporada existente se solapa con la nueva
    -- Condición de solapamiento: (inicio1 < fin2) AND (fin1 > inicio2)
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Temporada t
          ON t.TemporadaId <> i.TemporadaId
         AND i.fechaInicio < t.fechaFin
         AND i.fechaFin   > t.fechaInicio
    )
    BEGIN
        RAISERROR ('No se permite solapamiento entre temporadas.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

-- =============================================
-- TRIGGER: TR_DetalleReserva_NoSolapamientoHabitacion
-- Descripción: Evita double-booking - una habitación no puede estar asignada
--              a dos reservas activas que se superpongan en el tiempo
-- Momento: Se ejecuta DESPUÉS de INSERT o UPDATE en DetalleReserva
-- Lógica: Verifica que la habitación no esté reservada por otra reserva
--         en el mismo período (solo para reservas Pendientes/Confirmadas)
-- Nota: Reservas canceladas no bloquean disponibilidad
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_DetalleReserva_NoSolapamientoHabitacion
ON dbo.DetalleReserva
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Busca conflictos de disponibilidad de habitación
    -- Ignora reservas canceladas
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Reserva rNew
          ON rNew.ReservaId = i.ReservaId
        JOIN dbo.DetalleReserva drExist
          ON drExist.HabitacionId = i.HabitacionId
         AND drExist.ReservaId <> i.ReservaId
        JOIN dbo.Reserva rExist
          ON rExist.ReservaId = drExist.ReservaId
        WHERE rNew.EstadoReserva IN ('Pendiente','Confirmada')
          AND rExist.EstadoReserva IN ('Pendiente','Confirmada')
          -- Validación de solapamiento de fechas
          AND rNew.FechaEntrada < rExist.FechaSalida
          AND rNew.FechaSalida  > rExist.FechaEntrada
    )
    BEGIN
        RAISERROR ('Conflicto: la habitación ya está asignada a otra reserva en el mismo período.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

-- ============================================================
-- TRIGGER 1: TR_Reserva_CambioFechas_RevalidarHabitaciones
-- Cambio: paso 2c — saltar validación si el servicio aplica a todas las temporadas
-- ============================================================
DROP TRIGGER IF EXISTS dbo.TR_Reserva_CambioFechas_RevalidarHabitaciones;
GO

CREATE OR ALTER TRIGGER dbo.TR_Reserva_CambioFechas_RevalidarHabitaciones
ON dbo.Reserva
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT (UPDATE(FechaEntrada) OR UPDATE(FechaSalida) OR UPDATE(EstadoReserva))
        RETURN;

    /* 1) Validación general: no solapamiento de habitaciones entre reservas activas */
    IF EXISTS (
        SELECT 1
        FROM inserted rNew
        JOIN dbo.DetalleReserva drNew
          ON drNew.ReservaId = rNew.ReservaId
        JOIN dbo.DetalleReserva drExist
          ON drExist.HabitacionId = drNew.HabitacionId
         AND drExist.ReservaId <> rNew.ReservaId
        JOIN dbo.Reserva rExist
          ON rExist.ReservaId = drExist.ReservaId
        WHERE rNew.EstadoReserva IN ('Pendiente','Confirmada')
          AND rExist.EstadoReserva IN ('Pendiente','Confirmada')
          AND rNew.FechaEntrada < rExist.FechaSalida
          AND rNew.FechaSalida  > rExist.FechaEntrada
    )
    BEGIN
        RAISERROR ('Cambio inválido: al modificar fechas/estado se genera solapamiento de habitaciones.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    /* 2) Si cambió FechaEntrada o FechaSalida y la reserva NO está confirmada, recalcula tarifas */
    IF UPDATE(FechaEntrada) OR UPDATE(FechaSalida)
    BEGIN
        IF OBJECT_ID('tempdb..#Changed') IS NOT NULL DROP TABLE #Changed;

        SELECT
            i.ReservaId,
            i.FechaEntrada
        INTO #Changed
        FROM inserted i
        JOIN deleted d ON d.ReservaId = i.ReservaId
        WHERE (i.FechaEntrada <> d.FechaEntrada OR i.FechaSalida <> d.FechaSalida)
          AND i.EstadoReserva <> 'Confirmada';

        IF NOT EXISTS (SELECT 1 FROM #Changed)
            RETURN;

        /* 2a) Temporada por nueva FechaEntrada */
        IF OBJECT_ID('tempdb..#TemporadaSel') IS NOT NULL DROP TABLE #TemporadaSel;

        SELECT
            c.ReservaId,
            TemporadaId = dbo.fnTemporadaIdPorFecha(CAST(c.FechaEntrada AS DATE))
        INTO #TemporadaSel
        FROM #Changed c;

        /* 2b) Recalcular TarifaAplicada de DetalleReserva */
        ;WITH CalcTarifa AS (
            SELECT
                dr.DetalleReservaId,
                TarifaCalculada = COALESCE(tt.Precio, ch.precioBase)
            FROM #Changed c
            JOIN dbo.DetalleReserva dr ON dr.ReservaId = c.ReservaId
            JOIN dbo.Habitacion h ON h.HabitacionId = dr.HabitacionId
            JOIN dbo.CategoriaHabitacion ch ON ch.CategoriaHabitacionId = h.CategoriaHabitacionId
            JOIN #TemporadaSel ts ON ts.ReservaId = c.ReservaId
            LEFT JOIN dbo.TarifaTemporada tt
              ON tt.TemporadaId = ts.TemporadaId
             AND tt.CategoriaHabitacionId = h.CategoriaHabitacionId
        )
        UPDATE dr
           SET dr.TarifaAplicada = ct.TarifaCalculada
        FROM dbo.DetalleReserva dr
        JOIN CalcTarifa ct ON ct.DetalleReservaId = dr.DetalleReservaId;

        /* 2c) Validar disponibilidad de servicios por temporada
               CAMBIO: se omite la validación si el servicio tiene AplicaTodasTemporadas = 1 */
        IF EXISTS (
            SELECT 1
            FROM #TemporadaSel ts
            JOIN dbo.ReservaServicioAdicional rsa ON rsa.ReservaId = ts.ReservaId
            JOIN dbo.ServicioAdicional sa ON sa.ServicioAdicionalId = rsa.ServicioAdicionalId
            WHERE ts.TemporadaId IS NOT NULL
              AND sa.AplicaTodasTemporadas = 0   -- solo validar si NO aplica a todas
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.ServicioTemporada st
                  WHERE st.ServicioAdicionalId = rsa.ServicioAdicionalId
                    AND st.TemporadaId = ts.TemporadaId
              )
        )
        BEGIN
            RAISERROR ('Hay servicios adicionales en la reserva que no están disponibles en la temporada correspondiente a la nueva FechaEntrada.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        /* 2d) Recalcular PrecioUnitarioAplicado (MAX por categorías presentes en la reserva) */
        ;WITH PrecioCalc AS (
            SELECT
                rsa.ReservaServicioAdicionalId,
                NuevoPrecio = MAX(scp.Precio)
            FROM #TemporadaSel ts
            JOIN dbo.ReservaServicioAdicional rsa ON rsa.ReservaId = ts.ReservaId
            JOIN dbo.DetalleReserva dr ON dr.ReservaId = rsa.ReservaId
            JOIN dbo.Habitacion h ON h.HabitacionId = dr.HabitacionId
            JOIN dbo.ServicioCategoriaPrecio scp
              ON scp.ServicioAdicionalId = rsa.ServicioAdicionalId
             AND scp.CategoriaHabitacionId = h.CategoriaHabitacionId
            GROUP BY rsa.ReservaServicioAdicionalId
        )
        UPDATE rsa
           SET rsa.PrecioUnitarioAplicado = pc.NuevoPrecio
        FROM dbo.ReservaServicioAdicional rsa
        JOIN PrecioCalc pc ON pc.ReservaServicioAdicionalId = rsa.ReservaServicioAdicionalId;

        /* 2e) Validación: debe existir precio de cada servicio para al menos una categoría presente */
        IF EXISTS (
            SELECT 1
            FROM #TemporadaSel ts
            JOIN dbo.ReservaServicioAdicional rsa ON rsa.ReservaId = ts.ReservaId
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.DetalleReserva dr
                JOIN dbo.Habitacion h ON h.HabitacionId = dr.HabitacionId
                JOIN dbo.ServicioCategoriaPrecio scp
                  ON scp.ServicioAdicionalId = rsa.ServicioAdicionalId
                 AND scp.CategoriaHabitacionId = h.CategoriaHabitacionId
                WHERE dr.ReservaId = rsa.ReservaId
            )
        )
        BEGIN
            RAISERROR ('No existe precio de algún servicio para ninguna categoría presente en la reserva (ServicioCategoriaPrecio incompleto).', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
END
GO

-- =============================================
-- TRIGGER: TR_HabitacionHistorial_Consistencia
-- Descripción: Mantiene la integridad del historial de estados de habitaciones
--              garantizando que no haya gaps ni solapamientos
-- Momento: Se ejecuta DESPUÉS de INSERT o UPDATE en HabitacionHistorial
-- Lógica:
--   0) Evita múltiples vigentes en la misma operación
--   1) Previene solapamiento de rangos de fechas para la misma habitación
--   2) Cierra automáticamente el estado vigente anterior cuando se inserta uno nuevo
-- Nota: Permite tener un historial temporal consistente tipo SCD Type 2
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_HabitacionHistorial_Consistencia
ON dbo.HabitacionHistorial
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- (0) Evitar múltiples "vigentes" en la misma operación
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE FechaFin IS NULL
        GROUP BY HabitacionId
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR('No se permite insertar múltiples estados vigentes (FechaFin NULL) para la misma habitación en una sola operación.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- (1) Validar que no haya solapamiento de rangos
    --     Usa '99991231' como fecha máxima para registros vigentes (FechaFin NULL)
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.HabitacionHistorial h
          ON h.HabitacionId = i.HabitacionId
         AND h.HabitacionHistorialId <> i.HabitacionHistorialId
         AND i.FechaInicio < ISNULL(h.FechaFin, '99991231')
         AND ISNULL(i.FechaFin, '99991231') > h.FechaInicio
    )
    BEGIN
        RAISERROR ('Conflicto: el rango de estado se solapa con otro registro del historial.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- (2) Cerrar automáticamente el vigente anterior cuando se inserta uno nuevo vigente
    ;WITH VigentesNuevos AS (
        SELECT HabitacionId, FechaInicio
        FROM inserted
        WHERE FechaFin IS NULL
    )
    UPDATE h
       SET h.FechaFin = v.FechaInicio
    FROM dbo.HabitacionHistorial h
    JOIN VigentesNuevos v
      ON v.HabitacionId = h.HabitacionId
    WHERE h.FechaFin IS NULL
      AND h.FechaInicio < v.FechaInicio
      AND h.HabitacionHistorialId NOT IN (SELECT HabitacionHistorialId FROM inserted);
END
GO

-- =============================================
-- TRIGGER: TR_DetalleReserva_NoPermitirMantenimiento
-- Descripción: Impide que se asignen habitaciones que están en mantenimiento
--              a reservas activas
-- Momento: Se ejecuta DESPUÉS de INSERT o UPDATE en DetalleReserva
-- Lógica: Verifica el estado de la habitación en HabitacionHistorial
--         y rechaza la asignación si está en mantenimiento durante el período de reserva
-- Nota: Valida solapamiento entre período de mantenimiento y período de reserva
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_DetalleReserva_NoPermitirMantenimiento
ON dbo.DetalleReserva
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Verifica si alguna habitación asignada está en mantenimiento
    -- durante el período de la reserva
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Reserva r
          ON r.ReservaId = i.ReservaId
        JOIN dbo.HabitacionHistorial hh
          ON hh.HabitacionId = i.HabitacionId
        WHERE r.EstadoReserva IN ('Pendiente','Confirmada')
          AND hh.EstadoHabitacion = 'Mantenimiento'
          -- solapamiento mantenimiento vs reserva
          AND r.FechaEntrada < ISNULL(hh.FechaFin, '99991231')
          AND r.FechaSalida  > hh.FechaInicio
    )
    BEGIN
        RAISERROR ('No se puede asignar una habitación en mantenimiento a una reserva activa.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

-- =============================================
-- TRIGGER: TR_Reserva_Confirmar_RequiereHabitaciones
-- Descripción: Valida que una reserva tenga al menos una habitación asignada
--              antes de permitir confirmarla
-- Momento: Se ejecuta DESPUÉS de UPDATE en Reserva
-- Lógica: Al cambiar estado a 'Confirmada', verifica que exista al menos
--         un registro en DetalleReserva para esa reserva
-- Nota: Previene reservas confirmadas sin habitaciones asignadas
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_Reserva_Confirmar_RequiereHabitaciones
ON dbo.Reserva
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Solo actuar si cambió el estado
    IF NOT UPDATE(EstadoReserva)
        RETURN;

    -- Verificar que reservas confirmadas tengan habitaciones
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.EstadoReserva = 'Confirmada'
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.DetalleReserva dr
              WHERE dr.ReservaId = i.ReservaId
          )
    )
    BEGIN
        RAISERROR ('No se puede confirmar una reserva sin habitaciones asignadas.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

-- =============================================
-- TRIGGER: TR_Cliente_NoBorrarConReservas
-- Descripción: Protege la integridad referencial impidiendo eliminar clientes
--              que tienen reservas asociadas
-- Momento: Se ejecuta EN LUGAR DE DELETE en Cliente
-- Lógica: Verifica si el cliente tiene reservas. Si las tiene, rechaza el borrado.
--         Si no tiene reservas, procede con la eliminación
-- Nota: Tipo INSTEAD OF permite interceptar y validar antes de ejecutar el DELETE
-- Consideración: Alternativa sería soft-delete (campo Activo=0)
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_Cliente_NoBorrarConReservas
ON dbo.Cliente
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar si algún cliente a eliminar tiene reservas
    IF EXISTS (
        SELECT 1
        FROM deleted d
        WHERE EXISTS (SELECT 1 FROM dbo.Reserva r WHERE r.ClienteId = d.ClienteId)
    )
    BEGIN
        RAISERROR ('No se puede eliminar un cliente que tiene reservas.', 16, 1);
        RETURN;
    END

    -- Si no tiene reservas, proceder con el borrado
    DELETE c
    FROM dbo.Cliente c
    JOIN deleted d ON d.ClienteId = c.ClienteId;
END
GO
-- =============================================
-- PROCEDIMIENTO ALMACENADO: spDetalleReserva_Insert
-- Descripción: Procedimiento para insertar un nuevo detalle de reserva
-- Lógica: Verifica que la reserva no esté confirmada (no se pueden agregar habitaciones a reservas confirmadas)
--         Calcula la tarifa aplicable según la temporada y categoría de habitación
--         Inserta el nuevo detalle con la tarifa calculada
-- Nota: Este procedimiento encapsula la lógica de negocio para mantener consistencia y evitar errores al
--       agregar habitaciones a reservas, especialmente en lo que respecta a tarifas y estado de la reserva
-- =============================================
CREATE OR ALTER PROCEDURE dbo.spDetalleReserva_Insert
    @ReservaId     INT,
    @HabitacionId  INT
AS
BEGIN
    SET NOCOUNT ON;

    /* 1) No permitir agregar si la reserva está confirmada */
    IF EXISTS (
        SELECT 1
        FROM dbo.Reserva r
        WHERE r.ReservaId = @ReservaId
          AND r.EstadoReserva = 'Confirmada'
    )
    BEGIN
        RAISERROR('Reserva confirmada: no se permite agregar habitaciones (DetalleReserva).', 16, 1);
        RETURN;
    END

    /* 2) Calcular tarifa (temporada por FechaEntrada) */
    DECLARE @FechaEntrada DATE;
    SELECT @FechaEntrada = CAST(r.FechaEntrada AS DATE)
    FROM dbo.Reserva r
    WHERE r.ReservaId = @ReservaId;

    IF @FechaEntrada IS NULL
    BEGIN
        RAISERROR('Reserva no existe o no tiene FechaEntrada válida.', 16, 1);
        RETURN;
    END

    DECLARE @TemporadaId INT = dbo.fnTemporadaIdPorFecha(@FechaEntrada);

    DECLARE @TarifaAplicada DECIMAL(10,2);

    SELECT @TarifaAplicada = COALESCE(tt.Precio, ch.precioBase)
    FROM dbo.Habitacion h
    JOIN dbo.CategoriaHabitacion ch
      ON ch.CategoriaHabitacionId = h.CategoriaHabitacionId
    LEFT JOIN dbo.TarifaTemporada tt
      ON tt.TemporadaId = @TemporadaId
     AND tt.CategoriaHabitacionId = h.CategoriaHabitacionId
    WHERE h.HabitacionId = @HabitacionId;

    IF @TarifaAplicada IS NULL OR @TarifaAplicada <= 0
    BEGIN
        RAISERROR('No se pudo calcular TarifaAplicada (habitación no existe o tarifa inválida).', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.DetalleReserva (ReservaId, HabitacionId, TarifaAplicada)
    VALUES (@ReservaId, @HabitacionId, @TarifaAplicada);
END
GO

-- =============================================
-- PROCEDIMIENTO ALMACENADO: spReservaServicioAdicional_Insert
-- Descripción: Procedimiento para insertar un nuevo servicio adicional a una reserva
-- Lógica: Verifica que la reserva no esté confirmada (no se pueden agregar servicios a reservas confirmadas)
--         Verifica que la reserva tenga habitaciones para determinar precio por categoría
--         Determina la temporada por FechaEntrada y valida disponibilidad del servicio en esa temporada
--         Calcula el precio unitario aplicable (MAX de precios por categorías presentes en la reserva) y lo congela al momento de insertar el servicio adicional
-- Nota: Este procedimiento encapsula la lógica de negocio para mantener consistencia y evitar errores al
--       agregar servicios adicionales a reservas, especialmente en lo que respecta a precios y estado de la reserva
-- =============================================
CREATE OR ALTER PROCEDURE dbo.spReservaServicioAdicional_Insert
    @ReservaId           INT,
    @ServicioAdicionalId INT,
    @Cantidad            INT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @Cantidad IS NULL OR @Cantidad <= 0
    BEGIN
        RAISERROR('Cantidad inválida. Debe ser > 0.', 16, 1);
        RETURN;
    END

    /* 1) No permitir agregar si la reserva está confirmada */
    IF EXISTS (
        SELECT 1
        FROM dbo.Reserva r
        WHERE r.ReservaId = @ReservaId
          AND r.EstadoReserva = 'Confirmada'
    )
    BEGIN
        RAISERROR('Reserva confirmada: no se permite agregar servicios (ReservaServicioAdicional).', 16, 1);
        RETURN;
    END

    /* 2) Debe haber habitaciones para determinar precio por categoría */
    IF NOT EXISTS (SELECT 1 FROM dbo.DetalleReserva dr WHERE dr.ReservaId = @ReservaId)
    BEGIN
        RAISERROR('No se puede agregar un servicio: la reserva no tiene habitaciones (DetalleReserva) para determinar precio por categoría.', 16, 1);
        RETURN;
    END

    /* 3) Determinar temporada por FechaEntrada */
    DECLARE @FechaEntrada DATE;
    SELECT @FechaEntrada = CAST(r.FechaEntrada AS DATE)
    FROM dbo.Reserva r
    WHERE r.ReservaId = @ReservaId;

    IF @FechaEntrada IS NULL
    BEGIN
        RAISERROR('Reserva no existe o no tiene FechaEntrada válida.', 16, 1);
        RETURN;
    END

    DECLARE @TemporadaId INT = dbo.fnTemporadaIdPorFecha(@FechaEntrada);

    /* 4) Validar disponibilidad del servicio en temporada (si hay temporada) */
    IF @TemporadaId IS NOT NULL
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.ServicioTemporada st
            WHERE st.ServicioAdicionalId = @ServicioAdicionalId
              AND st.TemporadaId = @TemporadaId
        )
        BEGIN
            RAISERROR('Servicio no disponible para la temporada determinada por la FechaEntrada.', 16, 1);
            RETURN;
        END
    END

    /* 5) Calcular precio unitario: MAX de precios por categorías presentes en la reserva */
    DECLARE @PrecioUnitarioAplicado DECIMAL(10,2);

    SELECT @PrecioUnitarioAplicado = MAX(scp.Precio)
    FROM dbo.DetalleReserva dr
    JOIN dbo.Habitacion h
      ON h.HabitacionId = dr.HabitacionId
    JOIN dbo.ServicioCategoriaPrecio scp
      ON scp.ServicioAdicionalId = @ServicioAdicionalId
     AND scp.CategoriaHabitacionId = h.CategoriaHabitacionId
    WHERE dr.ReservaId = @ReservaId;

    IF @PrecioUnitarioAplicado IS NULL OR @PrecioUnitarioAplicado <= 0
    BEGIN
        RAISERROR('No existe precio del servicio para ninguna categoría incluida en la reserva.', 16, 1);
        RETURN;
    END

    IF EXISTS (
    SELECT 1
    FROM dbo.ReservaServicioAdicional
    WHERE ReservaId = @ReservaId
      AND ServicioAdicionalId = @ServicioAdicionalId
)
BEGIN
    RAISERROR('El servicio ya existe en la reserva. Actualiza Cantidad en lugar de duplicar.', 16, 1);
    RETURN;
END
    INSERT INTO dbo.ReservaServicioAdicional (ReservaId, ServicioAdicionalId, Cantidad, PrecioUnitarioAplicado)
    VALUES (@ReservaId, @ServicioAdicionalId, @Cantidad, @PrecioUnitarioAplicado);
END
GO

-- =============================================
-- TRIGGER: TR_DetalleReserva_CalcularTarifa_Update
-- Descripción: Recalcula automáticamente la tarifa si se modifica la habitación
--              o la reserva asociada en un detalle existente
-- Momento: Se ejecuta DESPUÉS de UPDATE en DetalleReserva
-- Lógica: Similar al trigger de INSERT, recalcula la tarifa usando la misma
--         lógica de temporada/categoría cuando cambian campos clave
-- Nota: Mantiene consistencia de precios al modificar detalles de reserva
--       Solo recalcula si cambian ReservaId o HabitacionId
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_DetalleReserva_CalcularTarifa_Update
ON dbo.DetalleReserva
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT (UPDATE(ReservaId) OR UPDATE(HabitacionId))
        RETURN;

    ;WITH Calc AS (
        SELECT
            i.DetalleReservaId,
            TarifaCalculada = COALESCE(tt.Precio, ch.precioBase)
        FROM inserted i
        JOIN dbo.Reserva r
          ON r.ReservaId = i.ReservaId
        JOIN dbo.Habitacion h
          ON h.HabitacionId = i.HabitacionId
        JOIN dbo.CategoriaHabitacion ch
          ON ch.CategoriaHabitacionId = h.CategoriaHabitacionId
        OUTER APPLY (SELECT dbo.fnTemporadaIdPorFecha(CAST(r.FechaEntrada AS DATE)) AS TemporadaId) ts
        LEFT JOIN dbo.TarifaTemporada tt
          ON tt.TemporadaId = ts.TemporadaId
         AND tt.CategoriaHabitacionId = h.CategoriaHabitacionId
    )
    UPDATE dr
       SET dr.TarifaAplicada = c.TarifaCalculada
    FROM dbo.DetalleReserva dr
    JOIN Calc c ON c.DetalleReservaId = dr.DetalleReservaId;
END
GO

-- ============================================================
-- TRIGGER 2: TR_RSA_CalcularPrecio_Update
-- Cambio: paso 2 — saltar validación de temporada si AplicaTodasTemporadas = 1
-- Nota: usa nombres de temp tables con prefijo #RSA_ para evitar colisión
--       con las del trigger padre (aprendido en sesión anterior)
-- ============================================================
DROP TRIGGER IF EXISTS dbo.TR_RSA_CalcularPrecio_Update;
GO

CREATE TRIGGER dbo.TR_RSA_CalcularPrecio_Update
ON dbo.ReservaServicioAdicional
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT (UPDATE(ReservaId) OR UPDATE(ServicioAdicionalId) OR UPDATE(Cantidad))
        RETURN;

    IF OBJECT_ID('tempdb..#RSA_TemporadaSel') IS NOT NULL DROP TABLE #RSA_TemporadaSel;

    SELECT
        i.ReservaServicioAdicionalId,
        i.ReservaId,
        i.ServicioAdicionalId,
        i.Cantidad,
        TemporadaId = dbo.fnTemporadaIdPorFecha(CAST(r.FechaEntrada AS DATE))
    INTO #RSA_TemporadaSel
    FROM inserted i
    JOIN dbo.Reserva r ON r.ReservaId = i.ReservaId;

    /* 2) Bloquear si el servicio NO está disponible para la temporada
          CAMBIO: se omite si AplicaTodasTemporadas = 1 */
    IF EXISTS (
        SELECT 1
        FROM #RSA_TemporadaSel ts
        JOIN dbo.ServicioAdicional sa ON sa.ServicioAdicionalId = ts.ServicioAdicionalId
        WHERE ts.TemporadaId IS NOT NULL
          AND sa.AplicaTodasTemporadas = 0       -- solo validar si NO aplica a todas
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.ServicioTemporada st
              WHERE st.ServicioAdicionalId = ts.ServicioAdicionalId
                AND st.TemporadaId = ts.TemporadaId
          )
    )
    BEGIN
        RAISERROR('Servicio no disponible para la temporada determinada por la FechaEntrada.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    /* 3) Reserva sin habitaciones */
    IF EXISTS (
        SELECT 1
        FROM #RSA_TemporadaSel ts
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.DetalleReserva dr WHERE dr.ReservaId = ts.ReservaId
        )
    )
    BEGIN
        RAISERROR('No se puede recalcular: la reserva no tiene habitaciones.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    /* 4) Validar precio del servicio para al menos una categoría */
    IF EXISTS (
        SELECT 1
        FROM #RSA_TemporadaSel ts
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.DetalleReserva dr
            JOIN dbo.Habitacion h ON h.HabitacionId = dr.HabitacionId
            JOIN dbo.ServicioCategoriaPrecio scp
              ON scp.ServicioAdicionalId = ts.ServicioAdicionalId
             AND scp.CategoriaHabitacionId = h.CategoriaHabitacionId
            WHERE dr.ReservaId = ts.ReservaId
        )
    )
    BEGIN
        RAISERROR('No existe precio del servicio para ninguna categoría incluida en la reserva.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    /* 5) Calcular precio unitario (MAX por categorías presentes) */
    IF OBJECT_ID('tempdb..#RSA_PrecioCalc') IS NOT NULL DROP TABLE #RSA_PrecioCalc;

    SELECT
        ts.ReservaServicioAdicionalId,
        PrecioUnitarioAplicado = MAX(scp.Precio)
    INTO #RSA_PrecioCalc
    FROM #RSA_TemporadaSel ts
    JOIN dbo.DetalleReserva dr ON dr.ReservaId = ts.ReservaId
    JOIN dbo.Habitacion h ON h.HabitacionId = dr.HabitacionId
    JOIN dbo.ServicioCategoriaPrecio scp
      ON scp.ServicioAdicionalId = ts.ServicioAdicionalId
     AND scp.CategoriaHabitacionId = h.CategoriaHabitacionId
    GROUP BY ts.ReservaServicioAdicionalId;

    /* 6) Verificar que se pudo calcular precio para todas las filas */
    IF EXISTS (
        SELECT 1
        FROM #RSA_TemporadaSel ts
        LEFT JOIN #RSA_PrecioCalc pc ON pc.ReservaServicioAdicionalId = ts.ReservaServicioAdicionalId
        WHERE pc.ReservaServicioAdicionalId IS NULL
           OR pc.PrecioUnitarioAplicado IS NULL
           OR pc.PrecioUnitarioAplicado <= 0
    )
    BEGIN
        RAISERROR('No se pudo calcular el precio unitario para uno o más servicios.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    /* 7) Aplicar recalculo */
    UPDATE rsa
       SET rsa.PrecioUnitarioAplicado = pc.PrecioUnitarioAplicado
    FROM dbo.ReservaServicioAdicional rsa
    JOIN #RSA_PrecioCalc pc ON pc.ReservaServicioAdicionalId = rsa.ReservaServicioAdicionalId;
END
GO

PRINT 'Triggers actualizados correctamente para respetar AplicaTodasTemporadas.';
GO

-- =============================================
-- TABLA: AuditoriaEvento
-- Descripción: Tabla principal de auditoría para trazabilidad persistente
-- Guarda eventos de auditoría del sistema (RNF-06: trazabilidad, RNF-07: consulta)
-- Registra quién, qué, cuándo y contexto de cada acción
-- Campos técnicos opcionales para debugging: RequestId, IpOrigen, UserAgent
-- =============================================
CREATE TABLE dbo.AuditoriaEvento (
    AuditoriaEventoId BIGINT IDENTITY(1,1) CONSTRAINT PK_AuditoriaEvento PRIMARY KEY,
    FechaUtc          DATETIME2(3) NOT NULL CONSTRAINT DF_AuditoriaEvento_FechaUtc DEFAULT SYSUTCDATETIME(),

    -- Quién ejecutó la acción
    UsuarioId         INT NOT NULL,
    Rol               VARCHAR(20) NOT NULL,               -- snapshot del rol al momento
    UsernameSnapshot  NVARCHAR(100) NOT NULL,             -- snapshot del username/email

    -- Qué hizo
    Accion            NVARCHAR(50) NOT NULL,          -- Ej: CREATE, UPDATE, DELETE, LOGIN, CANCEL, CONFIRM
    Modulo            NVARCHAR(100) NOT NULL,         -- Ej: Reservas, Tarifas, Habitaciones, Servicios, Usuarios
    Entidad           NVARCHAR(100) NOT NULL,             -- Ej: Reserva, DetalleReserva, TarifaTemporada
    EntidadId         NVARCHAR(64) NOT NULL,              -- PK como texto para soportar compuestas o GUID

    -- Contexto técnico (para auditoría operativa)
    RequestId         UNIQUEIDENTIFIER NOT NULL,          -- correlación por request/API call
    IpOrigen          VARCHAR(45) NOT NULL,               -- IPv4/IPv6
    UserAgent         NVARCHAR(255) NOT NULL,

    -- Descripción humana opcional
    Descripcion       NVARCHAR(500) NOT NULL,

    CONSTRAINT FK_AuditoriaEvento_Usuario
        FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario(UsuarioId)
);

-- Índices típicos para consulta de auditoría (RNF-07)
-- Índice para consultas generales ordenadas por fecha
CREATE INDEX IX_AuditoriaEvento_FechaUtc
ON dbo.AuditoriaEvento (FechaUtc DESC)
INCLUDE (UsuarioId, Rol, Accion, Modulo, Entidad, EntidadId);

-- Índice para rastrear acciones de un usuario específico
CREATE INDEX IX_AuditoriaEvento_Usuario_FechaUtc
ON dbo.AuditoriaEvento (UsuarioId, FechaUtc DESC)
INCLUDE (Accion, Modulo, Entidad, EntidadId);

-- Índice para auditar cambios en un módulo específico
CREATE INDEX IX_AuditoriaEvento_Modulo_FechaUtc
ON dbo.AuditoriaEvento (Modulo, FechaUtc DESC)
INCLUDE (Accion, UsuarioId, Entidad, EntidadId);
GO

-- =============================================
-- TABLA: AuditoriaEventoDetalle
-- Descripción: Tabla de detalle para auditoría granular campo por campo
-- Guarda valores antes/después como texto (puede ser JSON)
-- Permite trazabilidad fina de cambios individuales en campos
-- Relación: CASCADE DELETE con AuditoriaEvento para limpieza automática
-- =============================================
CREATE TABLE dbo.AuditoriaEventoDetalle (
    AuditoriaEventoDetalleId BIGINT IDENTITY(1,1) CONSTRAINT PK_AuditoriaEventoDetalle PRIMARY KEY,
    AuditoriaEventoId        BIGINT NOT NULL,
    Campo                    NVARCHAR(128) NOT NULL,
    ValorAnterior            NVARCHAR(MAX) NOT NULL,
    ValorNuevo               NVARCHAR(MAX) NOT NULL,

    CONSTRAINT FK_AuditoriaEventoDetalle_Evento
        FOREIGN KEY (AuditoriaEventoId) REFERENCES dbo.AuditoriaEvento(AuditoriaEventoId)
        ON DELETE CASCADE
);

-- Índice para consultar detalles de un evento específico
CREATE INDEX IX_AuditoriaEventoDetalle_Evento
ON dbo.AuditoriaEventoDetalle (AuditoriaEventoId)
INCLUDE (Campo);
GO

-- =============================================
-- VISTA: vReservaCostoTotal
-- Descripción: Vista calculada que muestra el costo total de cada reserva
-- Incluye: TotalHabitaciones, TotalServicios, TotalReserva (suma de ambos)
-- Uso: Reportes financieros, facturación, análisis de ingresos
-- Nota: Usa COALESCE para manejar reservas sin habitaciones o servicios
-- =============================================
CREATE OR ALTER VIEW dbo.vReservaCostoTotal
AS
SELECT
    r.ReservaId,
    r.ClienteId,
    r.EstadoReserva,
    r.FechaReserva,
    r.FechaEntrada,
    r.FechaSalida,
    TotalHabitaciones = COALESCE((
        SELECT SUM(dr.TarifaAplicada)
        FROM dbo.DetalleReserva dr
        WHERE dr.ReservaId = r.ReservaId
    ), 0),
    TotalServicios = COALESCE((
        SELECT SUM(rsa.SubTotalAplicado)
        FROM dbo.ReservaServicioAdicional rsa
        WHERE rsa.ReservaId = r.ReservaId
    ), 0),
    TotalReserva = COALESCE((
        SELECT SUM(dr.TarifaAplicada)
        FROM dbo.DetalleReserva dr
        WHERE dr.ReservaId = r.ReservaId
    ), 0) + COALESCE((
        SELECT SUM(rsa.SubTotalAplicado)
        FROM dbo.ReservaServicioAdicional rsa
        WHERE rsa.ReservaId = r.ReservaId
    ), 0)
FROM dbo.Reserva r;
GO

-- =============================================
-- VISTA: vUsoServicios
-- Descripción: Vista de análisis de uso de servicios adicionales
-- Muestra: Cantidad total consumida e ingreso estimado por servicio
-- Filtro: Solo reservas confirmadas (para ingresos reales)
-- Uso: Reportes de popularidad de servicios, análisis de rentabilidad
-- =============================================
CREATE OR ALTER VIEW dbo.vUsoServicios
AS
SELECT
    rsa.ServicioAdicionalId,
    sa.nombreServicio,
    sa.tipoServicio,
    CantidadTotal = SUM(rsa.Cantidad),
    IngresoEstimado = SUM(rsa.SubTotalAplicado)
FROM dbo.ReservaServicioAdicional rsa
JOIN dbo.ServicioAdicional sa ON sa.ServicioAdicionalId = rsa.ServicioAdicionalId
JOIN dbo.Reserva r ON r.ReservaId = rsa.ReservaId
WHERE r.EstadoReserva IN ('Confirmada')   -- típico para "ingresos"
GROUP BY rsa.ServicioAdicionalId, sa.nombreServicio, sa.tipoServicio;
GO

-- =============================================
-- VISTA: vOcupacionReservasActivas
-- Descripción: Vista de ocupación actual de habitaciones
-- Muestra habitaciones con reservas activas (Pendiente/Confirmada)
-- Uso: Dashboard de ocupación, gestión de disponibilidad en tiempo real
-- Nota: No incluye reservas canceladas
-- =============================================
CREATE OR ALTER VIEW dbo.vOcupacionReservasActivas
AS
SELECT
    dr.HabitacionId,
    h.NumeroHabitacion,
    h.Piso,
    h.CategoriaHabitacionId,
    r.ReservaId,
    r.EstadoReserva,
    r.FechaEntrada,
    r.FechaSalida
FROM dbo.DetalleReserva dr
JOIN dbo.Reserva r ON r.ReservaId = dr.ReservaId
JOIN dbo.Habitacion h ON h.HabitacionId = dr.HabitacionId
WHERE r.EstadoReserva IN ('Pendiente', 'Confirmada');
GO

-- =============================================
-- TRIGGER: TR_Reserva_Confirmada_BloquearCambioFechas
-- Descripción: Bloquea cambios de fechas en reservas ya confirmadas
-- Momento: Se ejecuta DESPUÉS de UPDATE en Reserva
-- Lógica: Si reserva estaba confirmada y cambiaron fechas, rechaza cambio
-- Razón: Las tarifas dependen de las fechas; cambiarlas invalidaría el snapshot
-- Alternativa: Permitir cancelar y crear nueva reserva
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_Reserva_Confirmada_BloquearCambioFechas
ON dbo.Reserva
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT (UPDATE(FechaEntrada) OR UPDATE(FechaSalida))
        RETURN;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted  d ON d.ReservaId = i.ReservaId
        WHERE d.EstadoReserva = 'Confirmada'
          AND (i.FechaEntrada <> d.FechaEntrada OR i.FechaSalida <> d.FechaSalida)
    )
    BEGIN
        RAISERROR('Reserva confirmada: no se permite cambiar FechaEntrada/FechaSalida. La tarifa debe conservarse.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

-- =============================================
-- TRIGGER: TR_DetalleReserva_Confirmada_BloquearCambios
-- Descripción: Bloquea modificaciones de habitaciones en reservas confirmadas
-- Momento: Se ejecuta DESPUÉS de INSERT, UPDATE, DELETE en DetalleReserva
-- Lógica: Si la reserva está confirmada, rechaza cualquier cambio
-- Razón: Los detalles son parte del snapshot de precios inmutable
-- Nota: Aplica a INSERT (agregar habitaciones), UPDATE, DELETE (quitar)
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_DetalleReserva_Confirmada_BloquearCambios
ON dbo.DetalleReserva
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    /* INSERT o UPDATE: validar reservas afectadas por inserted */
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Reserva r ON r.ReservaId = i.ReservaId
        WHERE r.EstadoReserva = 'Confirmada'
    )
    BEGIN
        RAISERROR('Reserva confirmada: no se permite agregar/modificar habitaciones (DetalleReserva).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    /* DELETE o UPDATE: validar reservas afectadas por deleted */
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN dbo.Reserva r ON r.ReservaId = d.ReservaId
        WHERE r.EstadoReserva = 'Confirmada'
    )
    BEGIN
        RAISERROR('Reserva confirmada: no se permite eliminar/modificar habitaciones (DetalleReserva).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

-- =============================================
-- TRIGGER: TR_RSA_Confirmada_BloquearCambios
-- Descripción: Bloquea modificaciones de servicios en reservas confirmadas
-- Momento: Se ejecuta DESPUÉS de INSERT, UPDATE, DELETE en ReservaServicioAdicional
-- Lógica: Si la reserva está confirmada, rechaza cualquier cambio
-- Razón: Los servicios son parte del snapshot de precios inmutable
-- Nota: Aplica a INSERT (agregar servicios), UPDATE (cambiar cantidad/precio), DELETE
-- =============================================
CREATE OR ALTER TRIGGER dbo.TR_RSA_Confirmada_BloquearCambios
ON dbo.ReservaServicioAdicional
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    /* INSERT/UPDATE */
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Reserva r ON r.ReservaId = i.ReservaId
        WHERE r.EstadoReserva = 'Confirmada'
    )
    BEGIN
        RAISERROR('Reserva confirmada: no se permite agregar/modificar servicios (ReservaServicioAdicional).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    /* DELETE/UPDATE */
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN dbo.Reserva r ON r.ReservaId = d.ReservaId
        WHERE r.EstadoReserva = 'Confirmada'
    )
    BEGIN
        RAISERROR('Reserva confirmada: no se permite eliminar/modificar servicios (ReservaServicioAdicional).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO