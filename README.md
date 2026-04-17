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

### 6. Actualizar el proyecto desde Git

Cuando existan cambios nuevos en el repositorio, descarga la última versión con el siguiente comando:

```bash
git pull
```

Después de actualizar el proyecto, revisa si existen cambios en la estructura de la base de datos. Si se agregaron nuevas migraciones, ejecuta:

```powershell
Update-Database
```

Si también hay cambios en la configuración, valida nuevamente el archivo `appsettings.json` para confirmar que la cadena de conexión siga apuntando al servidor correcto.

### 7. Publicar el proyecto

Una vez configurado y probado el sistema, publica el proyecto desde Visual Studio.

Pasos generales:

- Haz clic derecho sobre el proyecto
- Selecciona **Publish**
- Elige la opción de publicación deseada

Por ejemplo:

- **Folder**, para generar los archivos compilados en una carpeta
- **IIS**, **Azure** o el método de despliegue que se utilice en tu entorno

### 8. Desplegar en IIS o en otro servicio de hosting

Publica los archivos generados en un servidor **IIS** o en cualquier otro servicio de hosting que tenga acceso a la base de datos configurada.

Asegúrate de que:

- El servidor tenga instalado el runtime de **.NET 8**
- El servicio de hosting tenga acceso al servidor de base de datos
- La cadena de conexión en `appsettings.json` sea válida para el entorno de publicación
- El pool de aplicaciones o el usuario del servicio tenga los permisos necesarios

## Resumen rápido

- Clonar el repositorio
- Actualizar la cadena de conexión en `appsettings.json`
- Ejecutar `Add-Migration InitialCreate`
- Ejecutar `Update-Database`
- Ejecutar el script SQL para insertar los roles iniciales
- Cuando haya cambios en Git, ejecutar `git pull`
- Si hay nuevas migraciones, ejecutar `Update-Database`
- Publicar el proyecto
- Desplegar en IIS o en otro servicio de hosting con acceso a la base de datos
