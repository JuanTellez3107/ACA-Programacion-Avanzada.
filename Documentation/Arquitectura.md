# Arquitectura del sistema

El sistema se organiza en **cuatro proyectos** que materializan una arquitectura en tres capas más el modelo de dominio.

```mermaid
flowchart TB
    subgraph presentacion [Presentación - Windows Forms]
        UI[FrmPrincipal y paneles de módulo]
    end
    subgraph negocio [Negocio]
        S[Servicios y validadores]
    end
    subgraph datos [Datos]
        R[Repositorios ADO.NET]
        C[FabricaConexion / SqlExecutor]
    end
    subgraph dominio [Entidades]
        M[Modelos POCO]
    end
    subgraph sql [SQL Server - BibliotecaDB]
        DB[(Tablas y restricciones)]
    end
    UI --> S
    S --> R
    S --> M
    R --> C
    R --> M
    C --> DB
```

## Responsabilidades

| Proyecto | Responsabilidad |
| --- | --- |
| `Biblioteca.Presentacion` | Formularios, navegación, captura, mensajes y presentación |
| `Biblioteca.Negocio` | Reglas de préstamos/devoluciones, validaciones y orquestación |
| `Biblioteca.Datos` | Conexión, transacciones, SQL parametrizado y repositorios |
| `Biblioteca.Entidades` | Clases del dominio compartidas entre capas |

La presentación **no** ejecuta SQL. Los repositorios **no** muestran interfaces. Las reglas de integridad de negocio (disponibilidad, duplicados, borrado protegido) viven en servicios.
