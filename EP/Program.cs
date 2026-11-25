using System;
using System.Diagnostics;
using System.IO;

namespace TiendaDoñaLuz
{
    class Program
    {
        static string archivoProductos = "productos.txt";
        static string archivoVentas = "ventas.txt";

        static ListaSimpleInventario inventarioSimple = new ListaSimpleInventario();
        static ArbolBinarioProductos arbolProductos = new ArbolBinarioProductos();
        static ListaDobleHistorial historialBilletera = new ListaDobleHistorial();
        static PilaPersonalizada<Venta> pilaReversiones = new PilaPersonalizada<Venta>();
        static ColaHorasPico colaClientes = new ColaHorasPico();
        static GrafoRedDonaLuz grafoAbastecimiento = new GrafoRedDonaLuz();

        static int contadorOperaciones = 1000;
        static int contadorTurnos = 1;

        static readonly string[] categoriasTienda = {
            "Granos",
            "Abarrotes",
            "Lacteos",
            "Conservas",
            "Limpieza",
            "Bebidas"
        };

        static void Main(string[] args)
        {
            Console.Title = "Sistema de Ventas e Inventario - Minimarket Doña Luz";
            InicializarRedGrafo();
            CargarDatosDesdeArchivos();

            int opcion;
            do
            {
                Console.Clear();
                Console.WriteLine("======================================================================");
                Console.WriteLine("           MINIMARKET DOÑA LUZ - SISTEMA DE GESTIÓN AUTOMATIZADO      ");
                Console.WriteLine("   Integrantes: Chuquiruna, D. | Sánchez, E. | Huingo, E. | Juape, R. ");
                Console.WriteLine("======================================================================");
                Console.WriteLine($"   [Productos: {inventarioSimple.CantidadNodos} | Ventas: {historialBilletera.TotalMovimientos} | Clientes en Cola: {colaClientes.CantidadEnCola}]");
                Console.WriteLine("----------------------------------------------------------------------");
                Console.WriteLine("1. Inventario (Lista Simple): Ver, Registrar (Cód. Automático) o Abastecer");
                Console.WriteLine("2. Caja y Billetera (Lista Doble): Venta Rápida, Abono y Auditoría");
                Console.WriteLine("3. Sincronización (Lista Circular): Simular Temporizador Automático");
                Console.WriteLine("4. Anulaciones y Descuentos (Pilas LIFO): Revertir y Calcular Monto");
                Console.WriteLine("5. Atención en Horas Pico (Colas FIFO): Encolar, Simular y Atender Fila");
                Console.WriteLine("6. Búsqueda Rápida (ABB vs. Lineal): Comparar Pasos y Prueba de Estrés");
                Console.WriteLine("7. Rutas de Reparto (Grafos - Dijkstra): Calcular Camino Más Corto");
                Console.WriteLine("0. Guardar y Salir");
                Console.WriteLine("======================================================================");
                Console.Write("Seleccione una opción (0-7): ");

                if (!int.TryParse(Console.ReadLine(), out opcion)) opcion = -1;

                switch (opcion)
                {
                    case 1: MenuTema1_ListaSimple(); break;
                    case 2: MenuTema2_ListaDoble(); break;
                    case 3: MenuTema3_ListaCircular(); break;
                    case 4: MenuTema4_Pilas(); break;
                    case 5: MenuTema5_Colas(); break;
                    case 6: MenuTema6_ArbolBinario(); break;
                    case 7: MenuTema7_Grafos(); break;
                    case 0:
                        GuardarDatosEnArchivos();
                        Console.WriteLine("\nDatos guardados correctamente en 'productos.txt' y 'ventas.txt'. ¡Hasta luego!");
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Ingrese un número del 0 al 7.");
                        break;
                }

                if (opcion != 0)
                {
                    Console.WriteLine("\nPresione cualquier tecla para volver al menú principal...");
                    Console.ReadKey();
                }

            } while (opcion != 0);
        }

        // =====================================================================
        // MÉTODOS DE LECTURA RÁPIDA Y VALIDACIÓN AMIGABLE
        // =====================================================================
        static string LeerLinea()
        {
            string texto = Console.ReadLine();
            return texto == null ? "" : texto.Trim();
        }

        static int LeerEntero(string mensaje, int minimo, int maximo)
        {
            while (true)
            {
                Console.Write(mensaje);
                string texto = LeerLinea();

                if (!int.TryParse(texto, out int numero))
                    Console.WriteLine("   -> Ingrese un número entero válido.");
                else if (numero < minimo || numero > maximo)
                    Console.WriteLine($"   -> El valor debe estar entre {minimo} y {maximo}.");
                else
                    return numero;
            }
        }

        static int LeerEnteroConDefecto(string mensaje, int valorPorDefecto, int minimo, int maximo)
        {
            while (true)
            {
                Console.Write(mensaje);
                string texto = LeerLinea();
                if (texto == "") return valorPorDefecto;

                if (int.TryParse(texto, out int numero) && numero >= minimo && numero <= maximo)
                    return numero;

                Console.WriteLine($"   -> Ingrese un número entre {minimo} y {maximo} (o presione Enter para usar {valorPorDefecto}).");
            }
        }

        static double LeerDecimal(string mensaje, double minimo, double maximo)
        {
            while (true)
            {
                Console.Write(mensaje);
                string texto = LeerLinea();

                if (!double.TryParse(texto, out double numero))
                    Console.WriteLine("   -> Ingrese un monto numérico (ej. 4.50).");
                else if (numero < minimo || numero > maximo)
                    Console.WriteLine($"   -> El monto debe estar entre {minimo:F2} y {maximo:F2}.");
                else
                    return Math.Round(numero, 2);
            }
        }

        static string LeerTexto(string mensaje, int largoMaximo)
        {
            while (true)
            {
                Console.Write(mensaje);
                string texto = LeerLinea();

                if (texto == "")
                    Console.WriteLine("   -> Este campo no puede quedar vacío.");
                else if (texto.Contains(";"))
                    Console.WriteLine("   -> No se permite usar el carácter ';'.");
                else if (texto.Length > largoMaximo)
                    Console.WriteLine($"   -> Máximo {largoMaximo} caracteres.");
                else
                    return texto;
            }
        }

        // Permite presionar Enter para asignar automáticamente un nombre rápido en mostrador
        static string LeerClienteRapido(string nombrePorDefecto)
        {
            while (true)
            {
                Console.Write($"Nombre del cliente (Enter para '{nombrePorDefecto}'): ");
                string nombre = LeerLinea();
                if (nombre == "") return nombrePorDefecto;

                bool valido = nombre.Length >= 2 && !nombre.Contains(";");
                foreach (char c in nombre)
                {
                    if (!char.IsLetter(c) && c != ' ')
                    {
                        valido = false;
                        break;
                    }
                }

                if (valido) return nombre;
                Console.WriteLine("   -> Ingrese solo letras y espacios (o presione Enter para usar el nombre rápido).");
            }
        }

        static string LeerOpcion(string mensaje, params string[] opcionesValidas)
        {
            while (true)
            {
                Console.Write(mensaje);
                string texto = LeerLinea().ToUpper();

                foreach (string op in opcionesValidas)
                {
                    if (texto == op) return texto;
                }
                Console.WriteLine($"   -> Opción no válida. Elija entre: {string.Join(", ", opcionesValidas)}.");
            }
        }

        static Producto SeleccionarProductoDelCatalogo(string mensaje)
        {
            inventarioSimple.MostrarResumenCorto();
            while (true)
            {
                int cod = LeerEntero(mensaje, 1, 99999);
                Producto prod = arbolProductos.BuscarPorCodigo(cod, out _);
                if (prod != null) return prod;

                Console.WriteLine($"   -> No existe un producto con el código {cod}.");
                if (LeerOpcion("   ¿Intentar con otro código? (S/N): ", "S", "N") == "N")
                    return null;
            }
        }

        // =====================================================================
        // MÓDULO 1: LISTA ENLAZADA SIMPLE (INVENTARIO AUTOMATIZADO)
        // =====================================================================
        static void MenuTema1_ListaSimple()
        {
            Console.Clear();
            inventarioSimple.MostrarInventario();

            Console.WriteLine("\nOpciones de Inventario:");
            Console.WriteLine("1. Registrar nuevo producto (Código y Categoría automáticos)");
            Console.WriteLine("2. Abastecer / Aumentar stock rápido de un producto existente");
            Console.WriteLine("3. Cargar lote automático de prueba (3 productos de demostración)");
            Console.WriteLine("4. Volver al menú principal");
            string sub = LeerOpcion("Seleccione (1-4): ", "1", "2", "3", "4");

            if (sub == "1")
            {
                int codigoSugerido = inventarioSimple.ObtenerSiguienteCodigo();
                int cod = LeerEnteroConDefecto($"\nCódigo del producto (Enter para usar automático [{codigoSugerido}]): ", codigoSugerido, 1, 99999);

                while (arbolProductos.BuscarPorCodigo(cod, out _) != null)
                {
                    Console.WriteLine($"   -> El código {cod} ya existe.");
                    cod = LeerEntero("Ingrese otro código libre: ", 1, 99999);
                }

                string nom = LeerTexto("Nombre del producto: ", 30);

                Console.WriteLine("Categorías: 1.Granos | 2.Abarrotes | 3.Lacteos | 4.Conservas | 5.Limpieza | 6.Bebidas");
                int idxCat = LeerEnteroConDefecto("Seleccione el número de categoría (1-6, Enter para Abarrotes): ", 2, 1, 6);
                string cat = categoriasTienda[idxCat - 1];

                double pre = LeerDecimal("Precio unitario (S/): ", 0.10, 9999.99);
                int stk = LeerEnteroConDefecto("Stock inicial (Enter para 20 unidades): ", 20, 0, 100000);

                Producto nuevo = new Producto(cod, nom, cat, pre, stk);
                inventarioSimple.Insertar(nuevo);
                arbolProductos.Insertar(nuevo);
                GuardarDatosEnArchivos();
                Console.WriteLine($"\n-> Producto [{cod}] '{nom}' ({cat}) enlazado y guardado automáticamente.");
            }
            else if (sub == "2")
            {
                Producto prod = SeleccionarProductoDelCatalogo("\nIngrese el código del producto a abastecer: ");
                if (prod == null) return;

                int ingreso = LeerEntero($"Unidades a sumar a '{prod.Nombre}' (Stock actual: {prod.Stock}): ", 1, 10000);
                prod.Stock += ingreso;
                GuardarDatosEnArchivos();
                Console.WriteLine($"-> Stock actualizado automáticamente. Nuevo stock de '{prod.Nombre}': {prod.Stock} unidades.");
            }
            else if (sub == "3")
            {
                Producto[] loteDemo = {
                    new Producto(inventarioSimple.ObtenerSiguienteCodigo(), "Avena 3 Ositos 300g", "Granos", 3.20, 35),
                    new Producto(inventarioSimple.ObtenerSiguienteCodigo() + 1, "Jabon Bolivar 190g", "Limpieza", 2.90, 40),
                    new Producto(inventarioSimple.ObtenerSiguienteCodigo() + 2, "Gaseosa Inka Kola 1.5L", "Bebidas", 6.50, 25)
                };

                foreach (Producto p in loteDemo)
                {
                    if (arbolProductos.BuscarPorCodigo(p.Codigo, out _) == null)
                    {
                        inventarioSimple.Insertar(p);
                        arbolProductos.Insertar(p);
                    }
                }
                GuardarDatosEnArchivos();
                Console.WriteLine("\n-> Se insertaron 3 productos de prueba automáticamente.");
                inventarioSimple.MostrarInventario();
            }
        }

        // =====================================================================
        // MÓDULO 2: LISTA DOBLE (VENTAS RÁPIDAS, ABONOS Y AUDITORÍA)
        // =====================================================================
        static void MenuTema2_ListaDoble()
        {
            Console.Clear();
            Console.WriteLine("--- GESTIÓN DE VENTAS, ABONOS Y AUDITORÍA (LISTA DOBLE) ---");
            Console.WriteLine("1. Venta rápida en mostrador (muestra catálogo y calcula total automático)");
            Console.WriteLine("2. Registrar abono de cuenta fiada");
            Console.WriteLine("3. Simulación rápida: Generar 3 operaciones automáticas de prueba");
            Console.WriteLine("4. Auditar historial hacia ADELANTE y hacia ATRÁS");
            string sub = LeerOpcion("Seleccione (1-4): ", "1", "2", "3", "4");

            if (sub == "1")
            {
                Producto prod = SeleccionarProductoDelCatalogo("\nIngrese el código del producto a vender: ");
                if (prod == null) return;

                if (prod.Stock == 0)
                {
                    Console.WriteLine($"-> No se puede vender '{prod.Nombre}': stock agotado.");
                    return;
                }

                int cant = LeerEnteroConDefecto($"Cantidad de '{prod.Nombre}' (Stock: {prod.Stock}, Enter para 1): ", 1, 1, prod.Stock);
                string cliente = LeerClienteRapido("Cliente Mostrador");

                Console.WriteLine("¿Aplicar descuento? (0 = Sin descuento | 1 = Descuento 5% | 2 = Descuento 10%)");
                int opDesc = LeerEnteroConDefecto("Opción de descuento (Enter para 0): ", 0, 0, 2);

                double subtotal = cant * prod.Precio;
                double descuento = opDesc == 1 ? Math.Round(subtotal * 0.05, 2) : (opDesc == 2 ? Math.Round(subtotal * 0.10, 2) : 0);

                // Evalúa automáticamente con nuestra Pila Postfija
                ValidadorExpresionesMontos.ConstruirExpresionesAutomaticas(prod.Precio, cant, descuento, out _, out string expPost, out _);
                double total = Math.Round(ValidadorExpresionesMontos.EvaluarPostfija(expPost), 2);

                prod.Stock -= cant;
                Venta nuevaVenta = new Venta(++contadorOperaciones, cliente, prod.Codigo, prod.Nombre, cant, total, "VENTA", DateTime.Now);
                historialBilletera.RegistrarMovimiento(nuevaVenta);
                pilaReversiones.Push(nuevaVenta);
                GuardarDatosEnArchivos();

                Console.WriteLine($"\n-> Venta registrada automáticamente: {nuevaVenta}");
                if (descuento > 0)
                    Console.WriteLine($"   (Descuento aplicado con Pila Postfija '{expPost}': -S/ {descuento:F2})");
            }
            else if (sub == "2")
            {
                string cliente = LeerClienteRapido("Cliente Vecino");
                double monto = LeerDecimal("Monto abonado a cuenta (S/): ", 0.50, 5000.00);
                Venta nuevoAbono = new Venta(++contadorOperaciones, cliente, 0, "Abono de Cuenta", 1, monto, "ABONO_CUENTA", DateTime.Now);
                historialBilletera.RegistrarMovimiento(nuevoAbono);
                pilaReversiones.Push(nuevoAbono);
                GuardarDatosEnArchivos();
                Console.WriteLine($"\n-> Abono registrado automáticamente: {nuevoAbono}");
            }
            else if (sub == "3")
            {
                int[] codigosPrueba = { 104, 102, 106 };
                string[] clientesPrueba = { "Carlos Perez", "Lucia Rojas", "Rosa Diaz" };
                int[] cantidadesPrueba = { 2, 1, 3 };

                Console.WriteLine("\n--- GENERANDO 3 VENTAS AUTOMÁTICAS DE DEMOSTRACIÓN ---");
                for (int i = 0; i < codigosPrueba.Length; i++)
                {
                    Producto p = arbolProductos.BuscarPorCodigo(codigosPrueba[i], out _);
                    if (p != null && p.Stock >= cantidadesPrueba[i])
                    {
                        p.Stock -= cantidadesPrueba[i];
                        double total = Math.Round(cantidadesPrueba[i] * p.Precio, 2);
                        Venta v = new Venta(++contadorOperaciones, clientesPrueba[i], p.Codigo, p.Nombre, cantidadesPrueba[i], total, "VENTA", DateTime.Now);
                        historialBilletera.RegistrarMovimiento(v);
                        pilaReversiones.Push(v);
                        Console.WriteLine($"  + {v}");
                    }
                }
                GuardarDatosEnArchivos();
                historialBilletera.RecorrerAdelanteYAtras();
            }
            else
            {
                historialBilletera.RecorrerAdelanteYAtras();
            }
        }

        // =====================================================================
        // MÓDULO 3: LISTA CIRCULAR (TEMPORIZADOR AUTOMÁTICO)
        // =====================================================================
        static void MenuTema3_ListaCircular()
        {
            Console.Clear();
            ListaCircularTemporizador temporizador = new ListaCircularTemporizador();
            temporizador.AgregarTareaCiclica("Caja Principal Doña Luz", true);
            temporizador.AgregarTareaCiclica("Agencia Corresponsal Rural", false);
            temporizador.AgregarTareaCiclica("Respaldo en Nube Inventario", false);
            temporizador.AgregarTareaCiclica("Verificador POS Bancario", false);

            int maxCiclos = LeerEnteroConDefecto("Límite máximo de ciclos (1 a 20, presione Enter para 4 automático): ", 4, 1, 20);
            temporizador.SimularTemporizadorReintentos(maxCiclos);
        }

        // =====================================================================
        // MÓDULO 4: PILAS LIFO (REVERSIÓN Y CALCULADORA AUTOMÁTICA DE MONTOS)
        // =====================================================================
        static void MenuTema4_Pilas()
        {
            Console.Clear();
            Console.WriteLine("--- PILAS (LIFO): REVERSIÓN Y CALCULADORA AUTOMÁTICA DE DESCUENTOS ---");
            Console.WriteLine("1. Revertir (Deshacer) última operación registrada (Pop en O(1))");
            Console.WriteLine("2. Calculadora automática Postfija/Prefija (sin escribir fórmulas a mano)");
            Console.WriteLine("3. Demostración inmediata con expresión de prueba");
            string op = LeerOpcion("Seleccione (1-3): ", "1", "2", "3");

            if (op == "1")
            {
                if (pilaReversiones.EstaVacia)
                {
                    Console.WriteLine("\nNo hay operaciones recientes en la pila para revertir.");
                    return;
                }

                Venta ultima = pilaReversiones.Peek();
                Console.WriteLine($"\nOperación en el tope de la pila: {ultima}");
                if (LeerOpcion("¿Confirmar reversión automática? (S/N): ", "S", "N") == "N")
                {
                    Console.WriteLine("Operación conservada.");
                    return;
                }

                ultima = pilaReversiones.Pop();
                Console.WriteLine($"\nRevirtiendo operación (LIFO): {ultima}");

                if (ultima.TipoMovimiento == "VENTA")
                {
                    Producto p = arbolProductos.BuscarPorCodigo(ultima.CodigoProducto, out _);
                    if (p != null)
                    {
                        p.Stock += ultima.Cantidad;
                        Console.WriteLine($"-> Se devolvieron {ultima.Cantidad} unidades al stock de '{p.Nombre}'. Stock actual: {p.Stock}");
                    }
                }
                historialBilletera.EliminarUltimoPorId(ultima.IdOperacion);
                GuardarDatosEnArchivos();
                Console.WriteLine("-> Operación eliminada del historial y archivos actualizados con éxito.");
            }
            else if (op == "2")
            {
                Producto prod = SeleccionarProductoDelCatalogo("\nSeleccione el código del producto para calcular su cobro: ");
                if (prod == null) return;

                int cantidad = LeerEnteroConDefecto($"Cantidad de '{prod.Nombre}' (Precio S/ {prod.Precio:F2}, Enter para 4): ", 4, 1, 1000);
                double maxDesc = Math.Round(prod.Precio * cantidad, 2);
                double descuento = LeerDecimal($"Descuento a aplicar en S/ (0.00 a {maxDesc:F2}): ", 0.0, maxDesc);

                ValidadorExpresionesMontos.ConstruirExpresionesAutomaticas(prod.Precio, cantidad, descuento, out string infija, out string postfija, out string prefija);

                Console.WriteLine("\n--- CONVERSIÓN Y EVALUACIÓN AUTOMÁTICA EN PILA ---");
                Console.WriteLine($"Fórmula normal (Infija)       : {infija}");
                Console.WriteLine($"Notación Postfija generada    : '{postfija}' -> Total: S/ {ValidadorExpresionesMontos.EvaluarPostfija(postfija):F2}");
                Console.WriteLine($"Notación Prefija generada     : '{prefija}'  -> Total: S/ {ValidadorExpresionesMontos.EvaluarPrefija(prefija):F2}");
            }
            else
            {
                ValidadorExpresionesMontos.ConstruirExpresionesAutomaticas(12.5, 4, 5, out string infija, out string expPostfija, out string expPrefija);
                Console.WriteLine($"\nDemostración automática: {infija} = S/ 45.00");
                Console.WriteLine($"Expresión Postfija : '{expPostfija}' -> Resultado: S/ {ValidadorExpresionesMontos.EvaluarPostfija(expPostfija):F2}");
                Console.WriteLine($"Expresión Prefija  : '{expPrefija}'  -> Resultado: S/ {ValidadorExpresionesMontos.EvaluarPrefija(expPrefija):F2}");
            }
        }

        // =====================================================================
        // MÓDULO 5: COLAS FIFO (ATENCIÓN AUTOMATIZADA EN HORAS PICO)
        // =====================================================================
        static void MenuTema5_Colas()
        {
            Console.Clear();
            Console.WriteLine("--- COLAS (FIFO): ATENCIÓN DE CLIENTES EN HORAS PICO ---");
            colaClientes.MostrarColaPendiente();

            Console.WriteLine("\nOpciones de Cola:");
            Console.WriteLine("1. Encolar un pedido rápido (Enqueue)");
            Console.WriteLine("2. Simular Hora Pico: Cargar 4 clientes automáticamente en la fila");
            Console.WriteLine("3. Atender al siguiente cliente en turno (Dequeue 1 a 1)");
            Console.WriteLine("4. Atender TODA la fila automáticamente en orden FIFO (Ráfaga)");
            Console.WriteLine("5. Volver al menú principal");
            string op = LeerOpcion("Seleccione (1-5): ", "1", "2", "3", "4", "5");

            if (op == "1")
            {
                Producto prod = SeleccionarProductoDelCatalogo("\nCódigo del producto solicitado: ");
                if (prod == null) return;
                if (prod.Stock == 0)
                {
                    Console.WriteLine($"-> '{prod.Nombre}' no tiene stock disponible.");
                    return;
                }

                string cli = LeerClienteRapido($"Cliente Turno {contadorTurnos}");
                int cant = LeerEnteroConDefecto($"Cantidad (Stock: {prod.Stock}, Enter para 1): ", 1, 1, prod.Stock);

                colaClientes.EncolarPedido(new PedidoCliente(contadorTurnos++, cli, prod.Codigo, cant));
                Console.WriteLine("-> Cliente agregado al final de la cola en O(1).");
                colaClientes.MostrarColaPendiente();
            }
            else if (op == "2")
            {
                string[] nombresSim = { "Maria Gomez", "Jorge Silva", "Elena Torres", "Pedro Ruiz" };
                int[] codsSim = { 102, 104, 101, 106 };
                int[] cantsSim = { 1, 2, 1, 2 };

                for (int i = 0; i < nombresSim.Length; i++)
                {
                    Producto p = arbolProductos.BuscarPorCodigo(codsSim[i], out _);
                    if (p != null && p.Stock >= cantsSim[i])
                    {
                        colaClientes.EncolarPedido(new PedidoCliente(contadorTurnos++, nombresSim[i], codsSim[i], cantsSim[i]));
                    }
                }
                Console.WriteLine("\n-> Se encolaron automáticamente 4 pedidos de hora pico.");
                colaClientes.MostrarColaPendiente();
            }
            else if (op == "3")
            {
                AtenderUnClienteDeLaCola();
            }
            else if (op == "4")
            {
                if (colaClientes.EstaVacia)
                {
                    Console.WriteLine("\nNo hay clientes en la cola para atender.");
                    return;
                }
                Console.WriteLine("\n--- ATENDIENDO AUTOMÁTICAMENTE TODA LA FILA POR ORDEN DE LLEGADA (FIFO) ---");
                while (!colaClientes.EstaVacia)
                {
                    AtenderUnClienteDeLaCola();
                }
                Console.WriteLine("\n-> Todos los clientes de la fila fueron atendidos y registrados.");
            }
        }

        static void AtenderUnClienteDeLaCola()
        {
            PedidoCliente atendido = colaClientes.AtenderSiguiente();
            if (atendido == null)
            {
                Console.WriteLine("\nNo hay clientes en la cola para atender.");
                return;
            }

            Console.WriteLine($"\nAtendiendo Turno #{atendido.Turno}: {atendido.NombreCliente}");
            Producto prod = arbolProductos.BuscarPorCodigo(atendido.CodigoProducto, out _);

            if (prod == null)
            {
                Console.WriteLine("-> No se concretó la venta: el producto no existe.");
            }
            else if (prod.Stock < atendido.Cantidad)
            {
                Console.WriteLine($"-> Stock insuficiente para '{prod.Nombre}' (Disponible: {prod.Stock}).");
            }
            else
            {
                prod.Stock -= atendido.Cantidad;
                double total = Math.Round(atendido.Cantidad * prod.Precio, 2);
                Venta v = new Venta(++contadorOperaciones, atendido.NombreCliente, prod.Codigo, prod.Nombre, atendido.Cantidad, total, "VENTA", DateTime.Now);
                historialBilletera.RegistrarMovimiento(v);
                pilaReversiones.Push(v);
                GuardarDatosEnArchivos();
                Console.WriteLine($"-> Venta completada: {v}");
            }
        }

        // =====================================================================
        // MÓDULO 6: ÁRBOL BINARIO (ABB) VS BÚSQUEDA LINEAL + EXPERIMENTOS
        // =====================================================================
        static void MenuTema6_ArbolBinario()
        {
            Console.Clear();
            Console.WriteLine("--- ÁRBOL BINARIO DE BÚSQUEDA (ABB) VS. BÚSQUEDA LINEAL ---");
            if (arbolProductos.Raiz == null)
            {
                Console.WriteLine("No hay productos registrados en el árbol.");
                return;
            }

            Console.WriteLine("Catálogo ordenado por código (Recorrido In-Orden del ABB):");
            arbolProductos.RecorridoInOrden(arbolProductos.Raiz);

            Console.WriteLine("\nOpciones de Comparación:");
            Console.WriteLine("1. Buscar un código específico y comparar pasos (Lineal vs. ABB)");
            Console.WriteLine("2. Comparar automáticamente TODOS los productos del catálogo actual");
            Console.WriteLine("3. Ejecutar prueba de escalado (450 productos y 1,000 pedidos en cola)");
            string sub = LeerOpcion("Seleccione (1-3): ", "1", "2", "3");

            if (sub == "1")
            {
                int codigoBuscar = LeerEnteroConDefecto("\nIngrese el código a buscar (Enter para probar con 108): ", 108, 1, 99999);

                Stopwatch swLineal = Stopwatch.StartNew();
                Producto pLineal = inventarioSimple.BuscarLineal(codigoBuscar, out int pasosLineales);
                swLineal.Stop();

                Stopwatch swABB = Stopwatch.StartNew();
                Producto pABB = arbolProductos.BuscarPorCodigo(codigoBuscar, out int pasosABB);
                swABB.Stop();

                Console.WriteLine("\n--- RESULTADOS DE LA MEDICIÓN ---");
                if (pABB != null)
                    Console.WriteLine($"Producto encontrado: {pABB}");
                else
                    Console.WriteLine("El código buscado no existe en el inventario.");

                Console.WriteLine($"1. Búsqueda Lineal (Lista Simple) : {pasosLineales} comparaciones | Tiempo: {swLineal.ElapsedTicks} ticks");
                Console.WriteLine($"2. Búsqueda en Árbol (ABB)        : {pasosABB} comparaciones | Tiempo: {swABB.ElapsedTicks} ticks");
            }
            else if (sub == "2")
            {
                Console.WriteLine("\n--- TABLA AUTOMÁTICA DE COMPARACIONES EN EL CATÁLOGO ACTUAL ---");
                Console.WriteLine($"{"Código",-8} | {"Producto",-20} | {"Pasos Lineal",-14} | {"Pasos ABB",-10}");
                Console.WriteLine(new string('-', 60));

                NodoSimple actual = inventarioSimple.Cabeza;
                int sumaLineal = 0;
                int sumaABB = 0;
                int n = 0;

                while (actual != null)
                {
                    inventarioSimple.BuscarLineal(actual.Dato.Codigo, out int pLin);
                    arbolProductos.BuscarPorCodigo(actual.Dato.Codigo, out int pAbb);
                    sumaLineal += pLin;
                    sumaABB += pAbb;
                    n++;
                    Console.WriteLine($"{actual.Dato.Codigo,-8} | {actual.Dato.Nombre,-20} | {pLin,-14} | {pAbb,-10}");
                    actual = actual.Siguiente;
                }

                if (n > 0)
                {
                    Console.WriteLine(new string('-', 60));
                    Console.WriteLine($"Promedio exacto ({n} productos): Lineal = {(double)sumaLineal / n:F2} pasos vs. ABB = {(double)sumaABB / n:F2} comparaciones.");
                }
            }
            else
            {
                EjecutarExperimentoEscalado450();
            }
        }

        // Prueba experimental automatizada (450 códigos SKU y 1,000 pedidos en cola)
        static void EjecutarExperimentoEscalado450()
        {
            const int N = 450;
            ListaSimpleInventario listaExp = new ListaSimpleInventario();
            ArbolBinarioProductos abbBalanceado = new ArbolBinarioProductos();
            ArbolBinarioProductos abbAleatorio = new ArbolBinarioProductos();
            ArbolBinarioProductos abbDegenerado = new ArbolBinarioProductos();

            int[] codigosOrdenados = new int[N];
            for (int i = 0; i < N; i++)
            {
                codigosOrdenados[i] = 1001 + i;
                Producto p = new Producto(codigosOrdenados[i], "Prod_" + codigosOrdenados[i], "Abarrotes", 5.0, 100);
                listaExp.Insertar(p);
                abbDegenerado.Insertar(p);
            }

            InsertarPorBiseccion(abbBalanceado, codigosOrdenados, 0, N - 1);

            int[] codigosAleatorios = (int[])codigosOrdenados.Clone();
            Random rnd = new Random(184);
            for (int i = N - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int temp = codigosAleatorios[i];
                codigosAleatorios[i] = codigosAleatorios[j];
                codigosAleatorios[j] = temp;
            }
            for (int i = 0; i < N; i++)
            {
                abbAleatorio.Insertar(new Producto(codigosAleatorios[i], "Prod_" + codigosAleatorios[i], "Abarrotes", 5.0, 100));
            }

            long sumaLin = 0, sumaBal = 0, sumaAle = 0, sumaDeg = 0;
            for (int i = 0; i < N; i++)
            {
                int cod = codigosOrdenados[i];
                listaExp.BuscarLineal(cod, out int cLin);
                abbBalanceado.BuscarPorCodigo(cod, out int cBal);
                abbAleatorio.BuscarPorCodigo(cod, out int cAle);
                abbDegenerado.BuscarPorCodigo(cod, out int cDeg);

                sumaLin += cLin;
                sumaBal += cBal;
                sumaAle += cAle;
                sumaDeg += cDeg;
            }

            // Simulación de 1,000 pedidos en Cola dinámica vs. Arreglo estático
            const int totalPedidos = 1000;
            ColaHorasPico colaPrueba = new ColaHorasPico();
            for (int i = 1; i <= totalPedidos; i++)
                colaPrueba.EncolarPedido(new PedidoCliente(i, "Cliente", 1001, 1));
            while (!colaPrueba.EstaVacia)
                colaPrueba.AtenderSiguiente();

            int copiasResize = 10 + 20 + 40 + 80 + 160 + 320 + 420; // 1,050 copias por redimensionamiento
            int copiasDesplazamiento = (totalPedidos * (totalPedidos + 1)) / 2 - 500; // 500,000 desplazamientos

            Console.WriteLine("\n--- RESULTADOS DEL EXPERIMENTO ESCALADO (N = 450 SKU y 1,000 PEDIDOS) ---");
            Console.WriteLine($"1. Búsqueda Lineal (Lista Simple)     : Promedio = {(double)sumaLin / N:F2} pasos");
            Console.WriteLine($"2. ABB Balanceado (Por mitades)       : Altura = {abbBalanceado.ObtenerAltura(abbBalanceado.Raiz),3} | Promedio = {(double)sumaBal / N:F2} comp.");
            Console.WriteLine($"3. ABB Caso Promedio (Semilla 184)    : Altura = {abbAleatorio.ObtenerAltura(abbAleatorio.Raiz),3} | Promedio = {(double)sumaAle / N:F2} comp.");
            Console.WriteLine($"4. ABB Degenerado (Orden correlativo) : Altura = {abbDegenerado.ObtenerAltura(abbDegenerado.Raiz),3} | Promedio = {(double)sumaDeg / N:F2} comp.");
            Console.WriteLine($"5. Estrés 1,000 pedidos en Cola FIFO  : 0 copias en memoria (frente a {copiasResize + copiasDesplazamiento:N0} en arreglo fijo).");
        }

        static void InsertarPorBiseccion(ArbolBinarioProductos abb, int[] codigos, int izq, int der)
        {
            if (izq > der) return;
            int medio = (izq + der) / 2;
            abb.Insertar(new Producto(codigos[medio], "Prod_" + codigos[medio], "Abarrotes", 5.0, 100));
            InsertarPorBiseccion(abb, codigos, izq, medio - 1);
            InsertarPorBiseccion(abb, codigos, medio + 1, der);
        }

        // =====================================================================
        // MÓDULO 7: GRAFOS CON DIJKSTRA (SELECCIÓN POR NÚMERO SIN TIPEAR NOMBRES)
        // =====================================================================
        static void MenuTema7_Grafos()
        {
            Console.Clear();
            grafoAbastecimiento.MostrarRed();
            string[] puntos = grafoAbastecimiento.ObtenerNombresVertices();

            Console.WriteLine("\nOpciones de Ruteo con Dijkstra:");
            Console.WriteLine("1. Elegir punto de origen y destino por número (1 al 5)");
            Console.WriteLine("2. Calcular automáticamente las rutas principales de la tienda");
            string sub = LeerOpcion("Seleccione (1-2): ", "1", "2");

            if (sub == "1")
            {
                int idxOrigen = LeerEntero($"\nSeleccione el número del punto de ORIGEN (1-{puntos.Length}): ", 1, puntos.Length) - 1;
                int idxDestino;
                while (true)
                {
                    idxDestino = LeerEntero($"Seleccione el número del punto de DESTINO (1-{puntos.Length}): ", 1, puntos.Length) - 1;
                    if (idxDestino != idxOrigen) break;
                    Console.WriteLine("   -> El destino debe ser diferente al origen.");
                }

                grafoAbastecimiento.CalcularRutaMinimaDijkstra(puntos[idxOrigen], puntos[idxDestino]);
            }
            else
            {
                grafoAbastecimiento.CalcularRutaMinimaDijkstra("AlmacenCentral", "SucursalNorte");
                grafoAbastecimiento.CalcularRutaMinimaDijkstra("AlmacenCentral", "TiendaDonaLuz");
                grafoAbastecimiento.CalcularRutaMinimaDijkstra("MercadoMayorista", "SucursalNorte");
            }
        }

        static void InicializarRedGrafo()
        {
            grafoAbastecimiento.AgregarConexionBidireccional("AlmacenCentral", "TiendaDonaLuz", 4);
            grafoAbastecimiento.AgregarConexionBidireccional("AlmacenCentral", "MercadoMayorista", 7);
            grafoAbastecimiento.AgregarConexionBidireccional("MercadoMayorista", "TiendaDonaLuz", 2);
            grafoAbastecimiento.AgregarConexionBidireccional("TiendaDonaLuz", "AgenciaBancaria", 3);
            grafoAbastecimiento.AgregarConexionBidireccional("MercadoMayorista", "SucursalNorte", 8);
            grafoAbastecimiento.AgregarConexionBidireccional("AgenciaBancaria", "SucursalNorte", 2);
        }

        static void CargarDatosDesdeArchivos()
        {
            if (!File.Exists(archivoProductos))
            {
                string[] semilla = {
                    "104;Arroz Extra 1kg;Granos;4.20;50",
                    "102;Aceite Vegetal 1L;Abarrotes;8.50;30",
                    "106;Azucar Rubia 1kg;Endulzantes;3.80;60",
                    "101;Leche Evaporada;Lacteos;4.00;45",
                    "103;Fideos Tallarin;Pastas;2.50;40",
                    "105;Atun en Conserva;Conservas;5.50;25",
                    "107;Detergente 500g;Limpieza;6.20;20",
                    "108;Legia Clorox;Limpieza;2.80;10"
                };
                File.WriteAllLines(archivoProductos, semilla);
            }

            foreach (string linea in File.ReadAllLines(archivoProductos))
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                try
                {
                    Producto p = Producto.DesdeFormatoArchivo(linea);
                    if (p.Precio <= 0 || p.Stock < 0 || arbolProductos.BuscarPorCodigo(p.Codigo, out _) != null)
                        continue;

                    inventarioSimple.Insertar(p);
                    arbolProductos.Insertar(p);
                }
                catch (Exception)
                {
                    // Se omiten líneas con formato inválido
                }
            }

            if (File.Exists(archivoVentas))
            {
                foreach (string linea in File.ReadAllLines(archivoVentas))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    try
                    {
                        Venta v = Venta.DesdeFormatoArchivo(linea);
                        historialBilletera.RegistrarMovimiento(v);
                        pilaReversiones.Push(v);
                        if (v.IdOperacion > contadorOperaciones)
                            contadorOperaciones = v.IdOperacion;
                    }
                    catch (Exception)
                    {
                        // Se omiten líneas con formato inválido
                    }
                }
            }
        }

        static void GuardarDatosEnArchivos()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(archivoProductos, false))
                {
                    NodoSimple actual = inventarioSimple.Cabeza;
                    while (actual != null)
                    {
                        sw.WriteLine(actual.Dato.AFormatoArchivo());
                        actual = actual.Siguiente;
                    }
                }

                using (StreamWriter sw = new StreamWriter(archivoVentas, false))
                {
                    NodoDoble actual = historialBilletera.Cabeza;
                    while (actual != null)
                    {
                        sw.WriteLine(actual.Movimiento.AFormatoArchivo());
                        actual = actual.Siguiente;
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error al guardar los archivos: {ex.Message}");
            }
        }
    }
}
