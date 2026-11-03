# Sistema de Gestión de Biblioteca

Aplicación de escritorio en **C#**, **Windows Forms** y **SQL Server** para administrar libros, autores, editoriales, usuarios, préstamos, devoluciones y reportes.

Proyecto final de Programación Avanzada 2026. Arquitectura en tres capas más un proyecto de entidades. Acceso a datos con **ADO.NET** (`Microsoft.Data.SqlClient`) y consultas SQL parametrizadas. No se utiliza Entity Framework.

## Requisitos

- Windows 10 u 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (incluye el runtime Windows Desktop para WinForms)
- SQL Server local: **SQL Server Express** (`.\SQLEXPRESS`) o **SQL Server Express LocalDB** (`(localdb)\MSSQLLocalDB`)
- Git

## Estructura

```
SistemaBiblioteca/
├── SistemaBiblioteca.sln
├── Biblioteca.Presentacion/     # Windows Forms
├── Biblioteca.Negocio/          # Reglas y validaciones
├── Biblioteca.Datos/            # ADO.NET, repositorios, scripts
├── Biblioteca.Entidades/        # Modelos
├── Database/                    # Biblioteca.sql y DatosPrueba.sql
├── Documentation/               # Documentación técnica
└── Evidence/                    # Capturas de evidencia
```

## Cómo ejecutar

```powershell
cd C:\Users\perav\Projects\SistemaBiblioteca
dotnet restore
dotnet build SistemaBiblioteca.sln -c Release
dotnet run --project Biblioteca.Presentacion -c Release
```

Al iniciar, si no hay conexión:

1. Indique el servidor (por ejemplo `(localdb)\MSSQLLocalDB` o `.\SQLEXPRESS`).
2. Pulse **Crear BD** para ejecutar `Database/Biblioteca.sql` y `Database/DatosPrueba.sql`.
3. Pulse **Entrar**.

La cadena de conexión se guarda solo en `%AppData%\SistemaBiblioteca\connection.json`. No se publica en el repositorio.

## Módulos

| Módulo | Operaciones |
| --- | --- |
| Inicio | Dashboard con indicadores y préstamos activos |
| Libros | CRUD, búsqueda, ISBN único, autores y editorial, existencias |
| Autores | CRUD; no se elimina si tiene libros |
| Editoriales | CRUD; no se elimina si tiene libros |
| Usuarios | CRUD; documento y correo únicos |
| Préstamos | Registro, consulta, edición de activos, eliminación |
| Devoluciones | Cambia estado a Devuelto y restaura disponibilidad |
| Reportes | Libros, autores, editoriales, usuarios, activos, devueltos, historial |

## Documentación

La documentación académica completa está en [Documentation/DocumentoTecnico.md](Documentation/DocumentoTecnico.md).
