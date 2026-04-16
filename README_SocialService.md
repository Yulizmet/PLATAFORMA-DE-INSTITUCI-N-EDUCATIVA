# Módulo: SocialService

Este documento describe en detalle el módulo `SocialService` del proyecto. Está orientado a desarrolladores que necesitan entender, mantener o extender las funcionalidades del servicio social (asignación de alumnos a asesores, bitácoras, asistencias, control de horas, generación de reportes y gestión de evidencias).

## 1. Visión general

El módulo `SocialService` gestiona el flujo completo del servicio social: creación y gestión de asignaciones alumno↔asesor, registro de bitácoras semanales (actividades y horas), validación y aprobación de bitácoras por parte de asesores/coordinadores, registro de asistencias y generación de reportes (Excel/PDF). La UI está implementada con vistas Razor dentro del área `SocialService` y la lógica en controladores ASP.NET Core.

## 2. Arquitectura y patrón

Se utiliza arquitectura cliente–servidor y el patrón MVC. El cliente (navegador) consume vistas Razor y recursos estáticos (`wwwroot`), mientras que el servidor implementa la lógica en controladores y servicios en C# (.NET 8). Las entidades se persisten con Entity Framework Core en `AppDbContext`.

## 3. Rutas y área

Todas las rutas del módulo están bajo el área `SocialService` y se registran en `Program.cs` con:

- `app.MapAreaControllerRoute(name: "social_service", areaName: "SocialService", pattern: "SocialService/{controller=Account}/{action=Index}/{id?}")`

Esto agrupa controladores y vistas en `Areas/SocialService`.

## 4. Componentes principales

- Controladores (lógica):
  - `CoordinatorController` — panel y operaciones administrativas (dashboard, gestión de maestros/alumnos, aprobaciones/reevaluaciones, exportes Excel/PDF).
  - `TeacherController` — funciones del asesor: revisar y aprobar/rechazar bitácoras, asignar alumnos, ver asistencias y generar reportes por grupo.
  - `StudentController` — funciones del alumno: crear/editar bitácoras, descargar evidencias, ver horas y estado.
  - `AccountController` — autenticación/gestión de sesión dentro del área.

- ViewModels: contienen los formularios y estructuras necesarias para las vistas, p. ej. `BitacoraViewModel`, `AsignarHorasViewModel`, `AvailableStudentViewModel`, `CoordinatorViewModels`.

- Vistas: Razor pages y parciales para pantalla y generación de PDF (`_AvancePdf.cshtml`, `_AlumnosPdf.cshtml`).

- Servicios: `AzureStorageService` (almacenamiento/URLs seguras), `OutlookEmailSender` (envío de correos), `IConverter` (DinkToPdf) registrado en `Program.cs`.

## 5. Persistencia y modelo de datos (muy importante)

Las tablas y entidades centrales son:

- `users_user` (Usuarios): representa a alumnos, docentes, coordinadores y otros. Contiene navegación a `users_person`, roles e inscripciones.
  - Archivo ejemplo: `Models/users_user.cs`.

- `social_service_log` (Bitácoras): cada registro de bitácora incluye `StudentId`, `Week`, `Activities`, `HoursPracticas`, `HoursServicioSocial`, `CreatedAt`, campos de aprobación (`IsApproved`, `ApprovedHoursPracticas`, `ApprovedHoursServicioSocial`, `ApprovedBy`, `ApprovedAt`) y almacenamiento de PDF (`PdfFileData`, `PdfFileName`, `PdfContentType`).
  - Archivo ejemplo: `Models/social_service_log.cs`.

- `social_service_attendance` (Asistencias): registro por fecha de la presencia del alumno en actividades de servicio social (`AttendanceId`, `StudentId`, `Date`, `IsPresent`, `Tipo`, `Notes`).
  - Archivo ejemplo: `Models/social_service_attendance.cs`.

- `social_service_assignment` (Asignaciones): vincula `TeacherId` y `StudentId`, indica `AssignedDate` y campo `IsActive` para desactivar asignaciones.
  - Archivo ejemplo: `Models/social_service_assignment.cs`.

Relaciones importantes:
- alumno → bitácoras: `social_service_log.StudentId` → `users_user.UserId`; navegación `social_service_log.Student`. Ejemplos de consultas: `CoordinatorController.VerBitacorasAlumno`, `StudentController.Bitacoras`.
- docente → alumnos: `social_service_assignment` modela la relación; `TeacherId` → `users_user.UserId` y `StudentId` → `users_user.UserId`. Ejemplos: `TeacherController.Alumnos`, `CoordinatorController.Maestros`.

El contexto EF está en `Data/AppDbContext.cs` y expone `DbSet<social_service_log> SocialServiceLogs`, `DbSet<social_service_assignment> SocialServiceAssignments`, `DbSet<social_service_attendance> SocialServiceAttendances`, y `DbSet<social_service_rejection> SocialServiceRejections`.

## 6. Lógica clave y flujos

- Registro de bitácoras (Alumno): el formulario y las validaciones están en `Areas/SocialService/ViewModels/BitacoraViewModel.cs`. La creación se ejecuta en `StudentController.CrearBitacora(BitacoraViewModel vm)` donde se valida unicidad por semana, se captura snapshot de la inscripción activa y se guarda PDF en la entidad.

- Edición de bitácoras (Alumno): `StudentController.EditarBitacora` restringe modificación cuando la bitácora está aprobada.

- Aprobación (Asesor/Coordinador): `TeacherController.AprobarBitacora` y variantes en `CoordinatorController` aplican límites acumulados (240 horas prácticas, 480 horas servicio social), ajustan horas aprobadas y registran meta-información de aprobación (`ApprovedBy`, `ApprovedAt`).

- Rechazo: `TeacherController.RechazarBitacora` y `CoordinatorController.RechazarBitacoraAdmin` crean entradas de `social_service_rejection` y eliminan la bitácora original.

- Asistencias: registro y edición en `TeacherController.GuardarAsistencia` y `CoordinatorController.VerAsistenciasAlumno`.

- Exportes: Excel con ClosedXML (`ExportAlumnosExcel`, `ExportAvanceExcel`) y PDFs generados con DinkToPdf (`ExportAlumnosPdf`, `ExportAvancePdf`). Ver `TeacherController` y `CoordinatorController`.

## 7. Manejo de archivos y PDF

- PDFs adjuntos a bitácoras se validan y guardan en la BD (`PdfFileData`, `PdfFileName`). Validación de tamaño y extensión está en `ValidateAndReadPdfFile` dentro de `StudentController`.
- Exportes a PDF utilizan plantillas parciales (por ejemplo `_AvancePdf.cshtml`) renderizadas a HTML y convertidas con `IConverter` (DinkToPdf). `IConverter` se registra en `Program.cs` como singleton.
- Para la generación de PDF en servidor de producción se debe instalar/asegurar la dependencia nativa (wkhtmltopdf) compatible.

## 8. Autenticación y autorización

- La aplicación usa autenticación por cookies y políticas globales configuradas en `Program.cs`. Roles relevantes: `Student`, `Teacher`, `Coordinator`, `Master`.
- Controles de acceso y validaciones de rol se encuentran en múltiples métodos (ej. `if (!User.IsInRole("Master")) ...`) y en las comprobaciones de asignación que impiden acciones de usuarios no autorizados.

## 9. Servicios y configuración

- `AzureStorageService` maneja upload/delete y generación de SAS URLs para archivos externos; ejemplo: `Services/AzureStorageService.cs`.
- `OutlookEmailSender` se usa para notificaciones (registro en `Program.cs`).
- Parámetros sensibles (connection strings, credenciales) deben definirse en `appsettings` o variables de entorno; `Program.cs` consume `DefaultConnection` para `AppDbContext`.

## Instrucciones de instalación y dependencias

Requisitos previos:
- Tener instalado .NET 8 SDK y un IDE compatible (por ejemplo Visual Studio con soporte para .NET 8).
- Tener acceso a una instancia de SQL Server para desarrollo o pruebas (local o en Azure).

Paquetes NuGet principales (ejemplos de instalación):
- `DinkToPdf` (librería gestionada) — `dotnet add package DinkToPdf`.
- `ClosedXML` — `dotnet add package ClosedXML` (para exportes Excel).
- `Azure.Storage.Blobs` — `dotnet add package Azure.Storage.Blobs` (para `AzureStorageService`).
- `Microsoft.EntityFrameworkCore.SqlServer` y `Microsoft.EntityFrameworkCore.Tools` — para EF Core y migraciones.
- `Microsoft.AspNetCore.Authentication.Cookies` — para autenticación por cookies.

Dependencias nativas para generación de PDF:
- `DinkToPdf` depende de la utilidad nativa `wkhtmltopdf`. En desarrollo/producción debe instalarse la versión nativa adecuada para la plataforma (Windows, Linux x64/ARM). En Windows se recomienda descargar el instalador oficial y añadir la ruta de `wkhtmltopdf.exe` al `PATH`. En Linux se puede instalar desde paquetes o incluir el binario en la imagen del contenedor.
- Alternativa: desplegar en un contenedor Docker que incluya `wkhtmltopdf` en la imagen.

Configuración de entorno y secretos:
- Definir la cadena de conexión usada por EF Core: `ConnectionStrings:DefaultConnection`.
- Definir la cadena de storage usada por `AzureStorageService` (nombre por defecto en el código: `AzureStorageProcedures`) como `ConnectionStrings:AzureStorageProcedures` o en `Azure` como `Application setting` con la misma clave.
- Variables recomendadas (ejemplos):
  - `ConnectionStrings__DefaultConnection` — cadena de conexión SQL Server.
  - `ConnectionStrings__AzureStorageProcedures` — cadena de conexión a Azure Blob Storage.
  - `Email__Username`, `Email__Password` — credenciales para `OutlookEmailSender` si aplica.

Comandos útiles de configuración local:
- Inicializar secretos del proyecto: `dotnet user-secrets init` y luego `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<cadena>"`.
- Instalar herramientas EF si no están: `dotnet tool install --global dotnet-ef`.
- Ejecutar migraciones: `dotnet ef database update`.

Despliegue en Azure (puntos clave):
- En App Service, configurar las `Application settings` con las mismas claves usadas en `appsettings` (usar `__` para anidar en variables de entorno).
- Para `wkhtmltopdf` en Azure App Service se recomienda usar App Service en Linux con una imagen personalizada que incluya `wkhtmltopdf`, o usar una VM/Container donde sea posible instalar binarios nativos.
- Para mayor seguridad usar `Azure Key Vault` y Managed Identity para recuperar secretos en tiempo de ejecución.

Notas adicionales:
- Verificar que `IConverter` (DinkToPdf) se registre correctamente en `Program.cs` (`builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()))`). La librería nativa debe ser compatible con el runtime del servidor.
- Mantener las restricciones de archivos (PDF máximo 10 MB) y validar tipos antes de almacenar en la BD.

## 10. Archivos y ubicaciones más importantes (ejemplos de código)

- Modelo bitácora: `Models/social_service_log.cs` (estructura y campos de horas/archivo/aprobación).
- Controladores:
  - `Areas/SocialService/Controllers/StudentController.cs` — crear/editar/listado/descarga de bitácoras.
  - `Areas/SocialService/Controllers/TeacherController.cs` — aprobar/rechazar bitácoras, asignar alumnos, asistencia, exportes.
  - `Areas/SocialService/Controllers/CoordinatorController.cs` — administración y reportes globales.
- ViewModels: `Areas/SocialService/ViewModels/BitacoraViewModel.cs`, `AsignarHorasViewModel.cs`, `AvailableStudentViewModel.cs`, `CoordinatorViewModels.cs`.
- Vistas: `Areas/SocialService/Views/Student/CrearBitacora.cshtml`, `Bitacoras.cshtml`, `Horas.cshtml`; `Areas/SocialService/Views/Teacher/RevisarBitacorasAlumno.cshtml`, `_AvancePdf.cshtml`.
- Contexto EF: `Data/AppDbContext.cs` (DbSets y configuración de entidades).
- Servicios: `Services/AzureStorageService.cs` (upload/delete/SAS), `Program.cs` (registro DI y middleware).

## 11. Cómo ejecutar y probar localmente (resumen rápido)

1. Configurar `appsettings.json` o variables de entorno con `ConnectionStrings:DefaultConnection` apuntando a una instancia SQL accesible.
2. Ejecutar migraciones EF si aplica: `dotnet ef database update` (o ejecutar el proyecto y aplicar las migraciones en el pipeline de CI si ya existen). 
3. Iniciar la aplicación en el entorno de desarrollo desde Visual Studio.
4. Acceder a rutas del área: `/SocialService/Student/Dashboard`, `/SocialService/Teacher/Dashboard`, `/SocialService/Coordinator/Dashboard` según rol.
5. Probar creación de bitácoras, subida de PDF y flujo de aprobación/rechazo con cuentas que tengan roles `Student`/`Teacher`/`Master`.

## 12. Recomendaciones y puntos críticos

- Revisar consultas que materializan listas completas y filtran en memoria; para volúmenes grandes, trasladar paginación y filtros a la consulta SQL para mejorar rendimiento.
- Asegurar que wkhtmltopdf (u otra dependencia nativa requerida por DinkToPdf) esté instalada en servidores donde se generen PDFs.
- En producción, establecer `Cookie.SecurePolicy = Always` y `ASPNETCORE_ENVIRONMENT=Production`.
- Mantener las restricciones de tamaño y tipo de archivo para PDFs (10 MB, `.pdf`) y evitar almacenar archivos sin validar.