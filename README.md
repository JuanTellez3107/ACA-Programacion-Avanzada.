# Manual de instalación y ejecución

Este paquete permite instalar y ejecutar el **Sistema de Gestión de Biblioteca** en otro equipo Windows, sin Visual Studio.

## 1. Qué incluye el paquete

| Carpeta / archivo | Contenido |
| --- | --- |
| `Aplicacion/SistemaBiblioteca.exe` | Ejecutable autónomo (incluye el runtime de .NET 8) |
| `Aplicacion/Database/` | Scripts SQL copiados junto al exe |
| `Database/` | Scripts para SQL Server Management Studio o `sqlcmd` |
| `Configuracion/connection.example.json` | Ejemplo de parámetros de conexión (sin contraseñas) |
| `CodigoFuente/` | Solución completa para abrir o recompilar |
| `Documentacion/` | Documento técnico, ER, diccionario y arquitectura |
| `Evidence/` | Capturas de la aplicación |

## 2. Requisitos del otro computador

- Windows 10 u 11 de 64 bits.
- **SQL Server local**: Express (`.\SQLEXPRESS`) o LocalDB (`(localdb)\MSSQLLocalDB`).
- No hace falta instalar el SDK de .NET ni Visual Studio para usar el `.exe`.
- Sí hace falta SQL Server: la aplicación no incluye el motor de base de datos.

### Instalar SQL Server Express LocalDB (recomendado)

1. Descargue el instalador oficial: [SQL Server 2022 Express](https://www.microsoft.com/en-us/download/details.aspx?id=108838).
2. En el asistente elija **Download Media** y **LocalDB**, o instale Express con instancia `SQLEXPRESS`.
3. LocalDB también puede instalarse con el MSI `SqlLocalDB.msi`:
   `msiexec /i SqlLocalDB.msi IACCEPTSQLLOCALDBLICENSETERMS=YES`
4. Compruebe la instancia:

```powershell
& "C:\Program Files\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe" start MSSQLLocalDB
& "C:\Program Files\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe" info MSSQLLocalDB
```

### Instalar SQL Server Express

Si prefiere una instancia de servicio:

- Instale **SQL Server 2022 Express**.
- Instancia típica: `.\SQLEXPRESS`.
- Autenticación de Windows.

## 3. Ejecutar la aplicación

1. Copie la carpeta `SistemaGestionBiblioteca` completa al equipo destino.
2. Abra `Aplicacion\SistemaBiblioteca.exe`.
3. Si no hay base de datos todavía, aparecerá **Conexión a SQL Server**.

### Parámetros de conexión

| Campo | LocalDB | Express |
| --- | --- | --- |
| Servidor / instancia | `(localdb)\MSSQLLocalDB` | `.\SQLEXPRESS` |
| Base de datos | `BibliotecaDB` | `BibliotecaDB` |
| Autenticación | Windows (recomendada) | Windows |

4. Pulse **Crear BD**. Eso ejecuta `Biblioteca.sql` y `DatosPrueba.sql` sin borrar una base ya existente: solo crea lo que falte e inserta datos de prueba si las tablas están vacías.
5. Pulse **Probar** y luego **Entrar**.

La configuración se guarda en:

`%AppData%\SistemaBiblioteca\connection.json`

Ese archivo **no** viaja en el paquete a propósito: cada equipo guarda su servidor local.

Puede copiar `Configuracion\connection.example.json` como guía. No escriba contraseñas reales en el repositorio ni en el ZIP si va a compartirlo.

## 4. Instalar la base con scripts (opcional)

Si usa SQL Server Management Studio o `sqlcmd`:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "Database\Biblioteca.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "Database\DatosPrueba.sql"
```

Para Express:

```powershell
sqlcmd -S ".\SQLEXPRESS" -i "Database\Biblioteca.sql"
sqlcmd -S ".\SQLEXPRESS" -i "Database\DatosPrueba.sql"
```

Los scripts son idempotentes: no recrean tablas que ya existen y no duplican los datos de prueba.

## 5. Compilar desde el código fuente (opcional)

Solo si quiere modificar el sistema. Requiere [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
cd CodigoFuente
dotnet restore SistemaBiblioteca.sln
dotnet build SistemaBiblioteca.sln -c Release
dotnet run --project Biblioteca.Presentacion -c Release
```

## 6. Módulos que debe ver al entrar

Inicio, Libros, Autores, Editoriales, Usuarios, Préstamos, Devoluciones y Reportes.

## 7. Limitación técnica

El `.exe` es portátil respecto de .NET, pero **SQL Server debe existir en el equipo destino**. No es posible incrustar el motor SQL Server dentro del ejecutable de WinForms sin sustituir la tecnología exigida por el proyecto.
