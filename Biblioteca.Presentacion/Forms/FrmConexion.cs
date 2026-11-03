using Biblioteca.Datos.Connection;
using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Forms;

public class FrmConexion : Form
{
    private readonly ServiciosAplicacion _servicios;
    private readonly TextBox _txtServidor;
    private readonly TextBox _txtBase;
    private readonly CheckBox _chkIntegrada;
    private readonly TextBox _txtUsuario;
    private readonly TextBox _txtClave;
    private readonly Label _lblEstado;

    public FrmConexion(ServiciosAplicacion servicios)
    {
        _servicios = servicios;
        Text = "Conexión a SQL Server";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(560, 460);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = UiTema.Fondo;
        Font = UiTema.Normal;

        var titulo = new Label
        {
            Text = "Configurar conexión",
            Font = UiTema.Titulo,
            ForeColor = UiTema.Sidebar,
            AutoSize = true,
            Location = new Point(24, 20)
        };
        var ayuda = new Label
        {
            Text = "La cadena se guarda de forma local (no se publica en el repositorio).",
            AutoSize = true,
            Location = new Point(24, 58),
            ForeColor = UiTema.Texto
        };

        Controls.Add(titulo);
        Controls.Add(ayuda);

        _txtServidor = CrearCampo("Servidor / instancia", 90);
        _txtBase = CrearCampo("Base de datos", 140);
        _chkIntegrada = new CheckBox
        {
            Text = "Autenticación de Windows",
            Location = new Point(24, 190),
            AutoSize = true,
            Checked = true
        };
        _chkIntegrada.CheckedChanged += (_, _) => AlternarSqlAuth();
        Controls.Add(_chkIntegrada);

        _txtUsuario = CrearCampo("Usuario SQL", 220);
        _txtClave = CrearCampo("Contraseña", 270);
        _txtClave.UseSystemPasswordChar = true;

        _lblEstado = new Label
        {
            Location = new Point(24, 320),
            Size = new Size(500, 40),
            ForeColor = UiTema.Advertencia
        };
        Controls.Add(_lblEstado);

        var btnProbar = UiTema.CrearBoton("Probar", UiTema.Acento, 110);
        btnProbar.Location = new Point(24, 365);
        btnProbar.Click += (_, _) => Probar(false);

        var btnCrear = UiTema.CrearBoton("Crear BD", UiTema.Sidebar, 110);
        btnCrear.Location = new Point(144, 365);
        btnCrear.Click += (_, _) => CrearBase();

        var btnEntrar = UiTema.CrearBoton("Entrar", UiTema.Exito, 110);
        btnEntrar.Location = new Point(264, 365);
        btnEntrar.Click += (_, _) => Probar(true);

        Controls.AddRange(new Control[] { btnProbar, btnCrear, btnEntrar });

        CargarValores();
        AlternarSqlAuth();
    }

    private TextBox CrearCampo(string etiqueta, int y)
    {
        var lbl = UiTema.Etiqueta(etiqueta);
        lbl.Location = new Point(24, y);
        var caja = UiTema.Caja();
        caja.Location = new Point(24, y + 20);
        caja.Width = 490;
        Controls.Add(lbl);
        Controls.Add(caja);
        return caja;
    }

    private void CargarValores()
    {
        var cfg = _servicios.Fabrica.Configuracion;
        _txtServidor.Text = cfg.Server;
        _txtBase.Text = cfg.Database;
        _chkIntegrada.Checked = cfg.IntegratedSecurity;
        _txtUsuario.Text = cfg.UserId ?? string.Empty;
        _txtClave.Text = cfg.Password ?? string.Empty;
    }

    private void AlternarSqlAuth()
    {
        _txtUsuario.Enabled = !_chkIntegrada.Checked;
        _txtClave.Enabled = !_chkIntegrada.Checked;
    }

    private ConfiguracionConexion LeerConfiguracion() => new()
    {
        Server = _txtServidor.Text.Trim(),
        Database = string.IsNullOrWhiteSpace(_txtBase.Text) ? "BibliotecaDB" : _txtBase.Text.Trim(),
        IntegratedSecurity = _chkIntegrada.Checked,
        TrustServerCertificate = true,
        UserId = _txtUsuario.Text.Trim(),
        Password = _txtClave.Text
    };

    private void AplicarConfiguracion() => _servicios.Fabrica.Actualizar(LeerConfiguracion());

    private void Probar(bool cerrarSiOk)
    {
        AplicarConfiguracion();
        if (_servicios.Fabrica.Probar(out var mensaje))
        {
            _lblEstado.ForeColor = UiTema.Exito;
            _lblEstado.Text = mensaje;
            if (cerrarSiOk)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        else
        {
            _lblEstado.ForeColor = UiTema.Peligro;
            _lblEstado.Text = mensaje;
        }
    }

    private void CrearBase()
    {
        try
        {
            AplicarConfiguracion();
            if (!_servicios.Fabrica.ProbarServidor(out var mensajeServidor))
            {
                _lblEstado.ForeColor = UiTema.Peligro;
                _lblEstado.Text = mensajeServidor;
                return;
            }

            var raiz = BuscarCarpetaDatabase();
            if (raiz is null)
            {
                UiMensajes.Error("No se encontraron los scripts SQL en la carpeta Database.");
                return;
            }

            _servicios.Instalador.EjecutarScripts(
                Path.Combine(raiz, "Biblioteca.sql"),
                Path.Combine(raiz, "DatosPrueba.sql"));

            _lblEstado.ForeColor = UiTema.Exito;
            _lblEstado.Text = "Base de datos creada y datos de prueba cargados.";
            UiMensajes.Informar("BibliotecaDB quedó lista. Pulse Entrar para continuar.");
        }
        catch (Exception ex)
        {
            _lblEstado.ForeColor = UiTema.Peligro;
            _lblEstado.Text = ex.Message;
            UiMensajes.Error(ex.Message);
        }
    }

    private static string? BuscarCarpetaDatabase()
    {
        var candidatos = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Database"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Database")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Database"))
        };
        return candidatos.FirstOrDefault(Directory.Exists);
    }
}
