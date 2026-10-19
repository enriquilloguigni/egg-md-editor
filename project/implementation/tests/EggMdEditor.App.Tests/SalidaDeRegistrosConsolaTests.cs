using System.IO;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Salida de registros formateada a la salida estándar (T-007): una línea por
// registro, con el token literal del nivel delante del mensaje ya compuesto
// con su marca de tiempo y su sesión.
public class SalidaDeRegistrosConsolaTests
{
    [Fact]
    public void Cada_registro_sale_como_una_linea_con_el_token_del_nivel()
    {
        var escritor = new StringWriter();
        var salida = new SalidaDeRegistrosConsola(escritor);

        salida.Escribir(NivelRegistro.Info, "LOG-001: inicio del proceso");
        salida.Escribir(NivelRegistro.Debug, "LOG-002: conmutación completada");
        salida.Escribir(NivelRegistro.Warning, "LOG-002: claves sin resolver");
        salida.Escribir(NivelRegistro.Error, "LOG-003: cierre fallido");

        Assert.Equal(
            "[Info] LOG-001: inicio del proceso\n" +
            "[Debug] LOG-002: conmutación completada\n" +
            "[Warning] LOG-002: claves sin resolver\n" +
            "[Error] LOG-003: cierre fallido\n",
            escritor.ToString());
    }

    [Fact]
    public void El_destino_por_defecto_es_la_salida_estandar_del_proceso()
    {
        var salidaEstandarOriginal = Console.Out;
        try
        {
            var captura = new StringWriter();
            Console.SetOut(captura);

            new SalidaDeRegistrosConsola().Escribir(NivelRegistro.Info, "registro por la salida estándar");

            Assert.Equal("[Info] registro por la salida estándar\n", captura.ToString());
        }
        finally
        {
            Console.SetOut(salidaEstandarOriginal);
        }
    }

    [Fact]
    public void Un_mensaje_nulo_es_un_error_de_programacion()
    {
        var salida = new SalidaDeRegistrosConsola(new StringWriter());

        Assert.Throws<ArgumentNullException>(() => salida.Escribir(NivelRegistro.Info, null!));
    }
}
