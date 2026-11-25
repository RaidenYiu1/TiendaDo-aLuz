using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace TiendaDoñaLuz
{
        public class Venta
        {
            public int IdOperacion { get; set; }
            public string Cliente { get; set; }
            public int CodigoProducto { get; set; }
            public string NombreProducto { get; set; }
            public int Cantidad { get; set; }
            public double MontoTotal { get; set; }
            public string TipoMovimiento { get; set; }
            public DateTime Fecha { get; set; }

            public Venta(int idOperacion, string cliente, int codigoProducto, string nombreProducto, int cantidad, double montoTotal, string tipoMovimiento, DateTime fecha)
            {
                IdOperacion = idOperacion;
                Cliente = cliente;
                CodigoProducto = codigoProducto;
                NombreProducto = nombreProducto;
                Cantidad = cantidad;
                MontoTotal = montoTotal;
                TipoMovimiento = tipoMovimiento;
                Fecha = fecha;
            }

            public string AFormatoArchivo()
            {
                return $"{IdOperacion};{Cliente};{CodigoProducto};{NombreProducto};{Cantidad};{MontoTotal};{TipoMovimiento};{Fecha:yyyy-MM-dd HH:mm:ss}";
            }

            public static Venta DesdeFormatoArchivo(string linea)
            {
                string[] p = linea.Split(';');
                return new Venta(
                    int.Parse(p[0]),
                    p[1],
                    int.Parse(p[2]),
                    p[3],
                    int.Parse(p[4]),
                    double.Parse(p[5]),
                    p[6],
                    DateTime.Parse(p[7])
                );
            }

            public override string ToString()
            {
                return $"Op #{IdOperacion} [{Fecha:dd/MM HH:mm}] - {TipoMovimiento}: {Cliente} | {NombreProducto} (x{Cantidad}) -> S/ {MontoTotal:F2}";
            }
        }
    }
