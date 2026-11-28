using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    namespace TiendaDoñaLuz
{
        public class Producto
        {
            public int Codigo { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }
            public double Precio { get; set; }
            public int Stock { get; set; }

            public Producto(int codigo, string nombre, string categoria, double precio, int stock)
            {
                Codigo = codigo;
                Nombre = nombre;
                Categoria = categoria;
                Precio = precio;
                Stock = stock;
            }

            public string AFormatoArchivo()
            {
                return $"{Codigo};{Nombre};{Categoria};{Precio};{Stock}";
            }

            public static Producto DesdeFormatoArchivo(string linea)
            {
                string[] partes = linea.Split(';');
                return new Producto(
                    int.Parse(partes[0]),
                    partes[1],
                    partes[2],
                    double.Parse(partes[3]),
                    int.Parse(partes[4])
                );
            }

        public override string ToString()
        {
            return $"[Cód: {Codigo}] {Nombre,-32} | Cat: {Categoria,-12} | S/ {Precio,6:F2} | Stock: {Stock,3}";
        }
    }
    }
