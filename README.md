# Configuración del proyecto

Este proyecto está desarrollado con **.NET 8** y utiliza **Entity Framework Core** para la gestión de la base de datos.

## Primeros pasos

Sigue los siguientes pasos para configurar y ejecutar el proyecto localmente.

### 1. Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
cd <NOMBRE_DEL_PROYECTO>
```

### 2. Configurar la cadena de conexión

Abre el archivo `appsettings.json` y actualiza la cadena de conexión para que apunte al servidor SQL donde deseas crear la base de datos.

Ejemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=TU_BASE_DE_DATOS;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### 3. Crear la migración

Abre la **Package Manager Console** en Visual Studio y ejecuta:

```powershell
Add-Migration InitialCreate
```

> Si la migración inicial ya existe en el proyecto, este paso puede omitirse.

### 4. Actualizar la base de datos

Ejecuta el siguiente comando para aplicar las migraciones y crear la base de datos:

```powershell
Update-Database
```

### 5. Insertar roles iniciales

Una vez creada la base de datos, ejecuta el siguiente script SQL:

```sql
INSERT INTO users_role (Name, Description, CreatedDate, IsActive)
VALUES
    ('Nurse', 'Nurse role', GETDATE(), 1),
    ('Psychologist', 'Psychologist role', GETDATE(), 1),
    ('Head Nurse', 'Head nurse role', GETDATE(), 1),
    ('Head of Psychology', 'Head of psychology role', GETDATE(), 1),
    ('Coordinator', 'Coordinator role', GETDATE(), 1),
    ('Student', 'Student role', GETDATE(), 1),
    ('Administrator', 'Administrator role', GETDATE(), 1),
    ('Teacher', 'Teacher role', GETDATE(), 1);
```

## Resumen rápido

- Clonar el repositorio
- Actualizar la cadena de conexión en `appsettings.json`
- Ejecutar `Add-Migration InitialCreate`
- Ejecutar `Update-Database`
- Ejecutar el script SQL para insertar los roles iniciales
