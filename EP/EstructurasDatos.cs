using System;

namespace TiendaDoñaLuz
{
    // =========================================================================
    // 1. LISTA ENLAZADA SIMPLE (Catálogo base de inventario)
    // =========================================================================
    public class NodoSimple
    {
        public Producto Dato;
        public NodoSimple Siguiente;

        public NodoSimple(Producto dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    public class ListaSimpleInventario
    {
        public NodoSimple Cabeza { get; private set; }
        public int CantidadNodos { get; private set; }

        public void Insertar(Producto nuevo)
        {
            NodoSimple nuevoNodo = new NodoSimple(nuevo);
            if (Cabeza == null)
            {
                Cabeza = nuevoNodo;
            }
            else
            {
                NodoSimple actual = Cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }
            CantidadNodos++;
        }

        public Producto BuscarLineal(int codigo, out int pasos)
        {
            pasos = 0;
            NodoSimple actual = Cabeza;
            while (actual != null)
            {
                pasos++;
                if (actual.Dato.Codigo == codigo)
                    return actual.Dato;
                actual = actual.Siguiente;
            }
            return null;
        }

        // Genera automáticamente el siguiente código disponible (ej. 109, 110...)
        public int ObtenerSiguienteCodigo()
        {
            if (Cabeza == null) return 101;
            int max = 100;
            NodoSimple actual = Cabeza;
            while (actual != null)
            {
                if (actual.Dato.Codigo > max)
                    max = actual.Dato.Codigo;
                actual = actual.Siguiente;
            }
            return max + 1;
        }

        public void MostrarInventario()
        {
            Console.WriteLine("\n--- INVENTARIO DINÁMICO (LISTA ENLAZADA SIMPLE) ---");
            if (Cabeza == null)
            {
                Console.WriteLine("El inventario está vacío.");
                return;
            }
            NodoSimple actual = Cabeza;
            while (actual != null)
            {
                string sig = actual.Siguiente != null ? actual.Siguiente.Dato.Codigo.ToString() : "NULL";
                Console.WriteLine($"{actual.Dato}  --> [Ptr Sig: {sig}]");
                actual = actual.Siguiente;
            }
            Console.WriteLine($"Total de nodos en memoria dinámica: {CantidadNodos}");
        }

        public void MostrarResumenCorto()
        {
            Console.WriteLine("\n--- CATÁLOGO DISPONIBLE EN TIENDA DOÑA LUZ ---");
            if (Cabeza == null)
            {
                Console.WriteLine("No hay productos registrados.");
                return;
            }
            NodoSimple actual = Cabeza;
            while (actual != null)
            {
                Console.WriteLine($"  [{actual.Dato.Codigo}] {actual.Dato.Nombre,-20} | Precio: S/ {actual.Dato.Precio,6:F2} | Stock: {actual.Dato.Stock}");
                actual = actual.Siguiente;
            }
        }
    }

    // =========================================================================
    // 2. LISTA DOBLEMENTE ENLAZADA (Historial de ventas y auditoría inversa)
    // =========================================================================
    public class NodoDoble
    {
        public Venta Movimiento;
        public NodoDoble Anterior;
        public NodoDoble Siguiente;

        public NodoDoble(Venta movimiento)
        {
            Movimiento = movimiento;
            Anterior = null;
            Siguiente = null;
        }
    }

    public class ListaDobleHistorial
    {
        public NodoDoble Cabeza { get; private set; }
        public NodoDoble Cola { get; private set; }
        public int TotalMovimientos { get; private set; }

        public void RegistrarMovimiento(Venta v)
        {
            NodoDoble nuevo = new NodoDoble(v);
            if (Cabeza == null)
            {
                Cabeza = Cola = nuevo;
            }
            else
            {
                Cola.Siguiente = nuevo;
                nuevo.Anterior = Cola;
                Cola = nuevo;
            }
            TotalMovimientos++;
        }

        public bool EliminarUltimoPorId(int idOperacion)
        {
            NodoDoble actual = Cola;
            while (actual != null)
            {
                if (actual.Movimiento.IdOperacion == idOperacion)
                {
                    if (actual.Anterior != null)
                        actual.Anterior.Siguiente = actual.Siguiente;
                    else
                        Cabeza = actual.Siguiente;

                    if (actual.Siguiente != null)
                        actual.Siguiente.Anterior = actual.Anterior;
                    else
                        Cola = actual.Anterior;

                    TotalMovimientos--;
                    return true;
                }
                actual = actual.Anterior;
            }
            return false;
        }

        public void RecorrerAdelanteYAtras()
        {
            if (Cabeza == null)
            {
                Console.WriteLine("\nNo hay movimientos registrados en la caja de Doña Luz.");
                return;
            }

            Console.WriteLine("\n>>> RECORRIDO HACIA ADELANTE (Cronológico: del primero al último) >>>");
            NodoDoble actual = Cabeza;
            double saldoAcumulado = 0;
            while (actual != null)
            {
                saldoAcumulado += actual.Movimiento.MontoTotal;
                string ant = actual.Anterior != null ? actual.Anterior.Movimiento.IdOperacion.ToString() : "NULL";
                string sig = actual.Siguiente != null ? actual.Siguiente.Movimiento.IdOperacion.ToString() : "NULL";
                Console.WriteLine($"[Ant: {ant}] {actual.Movimiento} [Sig: {sig}]");
                actual = actual.Siguiente;
            }

            Console.WriteLine("\n<<< RECORRIDO HACIA ATRÁS (Auditoría inversa: del último al primero) <<<");
            actual = Cola;
            while (actual != null)
            {
                Console.WriteLine($"<- {actual.Movimiento}");
                actual = actual.Anterior;
            }
            Console.WriteLine($"\nTotal de operaciones: {TotalMovimientos} | Saldo acumulado en caja: S/ {saldoAcumulado:F2}");
        }
    }

    // =========================================================================
    // 3. LISTA ENLAZADA CIRCULAR (Temporizador cíclico de sincronización)
    // =========================================================================
    public class NodoCircular
    {
        public string ModuloSincronizacion;
        public bool Conectado;
        public NodoCircular Siguiente;

        public NodoCircular(string modulo, bool conectado)
        {
            ModuloSincronizacion = modulo;
            Conectado = conectado;
            Siguiente = null;
        }
    }

    public class ListaCircularTemporizador
    {
        private NodoCircular cabeza;
        private NodoCircular cola;
        private int totalNodos = 0;

        public void AgregarTareaCiclica(string modulo, bool estadoInicial)
        {
            NodoCircular nuevo = new NodoCircular(modulo, estadoInicial);
            if (cabeza == null)
            {
                cabeza = cola = nuevo;
                nuevo.Siguiente = cabeza;
            }
            else
            {
                cola.Siguiente = nuevo;
                nuevo.Siguiente = cabeza;
                cola = nuevo;
            }
            totalNodos++;
        }

        public void SimularTemporizadorReintentos(int maxCiclosPermitidos)
        {
            if (cabeza == null || maxCiclosPermitidos <= 0)
            {
                Console.WriteLine("Configuración de temporizador no válida.");
                return;
            }

            Console.WriteLine($"\n--- SIMULACIÓN DE TEMPORIZADOR CIRCULAR (Máx. {maxCiclosPermitidos} ciclos) ---");
            NodoCircular actual = cabeza;
            int ciclosCompletados = 0;
            int pasosEnCiclo = 0;
            Random rnd = new Random();

            while (ciclosCompletados < maxCiclosPermitidos)
            {
                if (!actual.Conectado)
                {
                    actual.Conectado = rnd.Next(0, 2) == 1;
                    Console.WriteLine($"[Ciclo {ciclosCompletados + 1}] Reintentando '{actual.ModuloSincronizacion}' -> {(actual.Conectado ? "CONECTADO CON ÉXITO" : "Pendiente...")}");
                }
                else
                {
                    Console.WriteLine($"[Ciclo {ciclosCompletados + 1}] '{actual.ModuloSincronizacion}' -> Sincronización OK.");
                }

                actual = actual.Siguiente;
                pasosEnCiclo++;

                if (pasosEnCiclo == totalNodos)
                {
                    ciclosCompletados++;
                    pasosEnCiclo = 0;

                    bool todosOk = true;
                    NodoCircular temp = cabeza;
                    for (int i = 0; i < totalNodos; i++)
                    {
                        if (!temp.Conectado) todosOk = false;
                        temp = temp.Siguiente;
                    }

                    if (todosOk)
                    {
                        Console.WriteLine($"-> Todos los módulos se sincronizaron en el ciclo {ciclosCompletados}. Temporizador detenido sin bucle infinito.");
                        return;
                    }
                }
            }
            Console.WriteLine("-> Límite máximo de ciclos alcanzado. Se detiene el recorrido circular de forma segura.");
        }
    }

    // =========================================================================
    // 4. PILA LIFO GENÉRICA Y CALCULADORA AUTOMÁTICA POSTFIJA / PREFIJA
    // =========================================================================
    public class NodoPila<T>
    {
        public T Dato;
        public NodoPila<T> Siguiente;

        public NodoPila(T dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    public class PilaPersonalizada<T>
    {
        private NodoPila<T> tope;
        public bool EstaVacia => tope == null;

        public void Push(T item)
        {
            NodoPila<T> nuevo = new NodoPila<T>(item);
            nuevo.Siguiente = tope;
            tope = nuevo;
        }

        public T Pop()
        {
            if (EstaVacia) throw new InvalidOperationException("La pila está vacía.");
            T valor = tope.Dato;
            tope = tope.Siguiente;
            return valor;
        }

        public T Peek()
        {
            if (EstaVacia) throw new InvalidOperationException("La pila está vacía.");
            return tope.Dato;
        }
    }

    public static class ValidadorExpresionesMontos
    {
        // Genera automáticamente las expresiones Postfija y Prefija a partir de Precio, Cantidad y Descuento
        // para que el usuario no tenga que escribirlas a mano.
        public static void ConstruirExpresionesAutomaticas(double precio, int cantidad, double descuento, out string infija, out string postfija, out string prefija)
        {
            string p = precio.ToString("0.##");
            string c = cantidad.ToString();
            string d = descuento.ToString("0.##");

            infija = $"({p} * {c}) - {d}";
            postfija = $"{p} {c} * {d} -";
            prefija = $"- * {p} {c} {d}";
        }

        public static double EvaluarPostfija(string expresion)
        {
            PilaPersonalizada<double> pila = new PilaPersonalizada<double>();
            string[] tokens = expresion.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int cantidadEnPila = 0;

            if (tokens.Length == 0)
                throw new InvalidOperationException("La expresión está vacía.");

            foreach (string token in tokens)
            {
                if (double.TryParse(token, out double numero))
                {
                    pila.Push(numero);
                    cantidadEnPila++;
                }
                else
                {
                    if (!EsOperador(token))
                        throw new InvalidOperationException($"'{token}' no es un número ni un operador (+ - * /).");
                    if (cantidadEnPila < 2)
                        throw new InvalidOperationException($"Faltan números antes del operador '{token}'.");

                    double b = pila.Pop();
                    double a = pila.Pop();
                    pila.Push(Operar(a, b, token));
                    cantidadEnPila--;
                }
            }

            if (cantidadEnPila != 1)
                throw new InvalidOperationException("Sobran números o falta algún operador.");

            return pila.Pop();
        }

        public static double EvaluarPrefija(string expresion)
        {
            PilaPersonalizada<double> pila = new PilaPersonalizada<double>();
            string[] tokens = expresion.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int cantidadEnPila = 0;

            if (tokens.Length == 0)
                throw new InvalidOperationException("La expresión está vacía.");

            for (int i = tokens.Length - 1; i >= 0; i--)
            {
                string token = tokens[i];
                if (double.TryParse(token, out double numero))
                {
                    pila.Push(numero);
                    cantidadEnPila++;
                }
                else
                {
                    if (!EsOperador(token))
                        throw new InvalidOperationException($"'{token}' no es un número ni un operador (+ - * /).");
                    if (cantidadEnPila < 2)
                        throw new InvalidOperationException($"Faltan números después del operador '{token}'.");

                    double a = pila.Pop();
                    double b = pila.Pop();
                    pila.Push(Operar(a, b, token));
                    cantidadEnPila--;
                }
            }

            if (cantidadEnPila != 1)
                throw new InvalidOperationException("Sobran números o falta algún operador.");

            return pila.Pop();
        }

        private static bool EsOperador(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/";
        }

        private static double Operar(double a, double b, string operador)
        {
            switch (operador)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "*": return a * b;
                default:
                    if (b == 0)
                        throw new InvalidOperationException("No se puede dividir entre cero.");
                    return a / b;
            }
        }
    }

    // =========================================================================
    // 5. COLA FIFO DINÁMICA (Atención de clientes por orden de llegada)
    // =========================================================================
    public class PedidoCliente
    {
        public int Turno { get; set; }
        public string NombreCliente { get; set; }
        public int CodigoProducto { get; set; }
        public int Cantidad { get; set; }

        public PedidoCliente(int turno, string cliente, int codProd, int cant)
        {
            Turno = turno;
            NombreCliente = cliente;
            CodigoProducto = codProd;
            Cantidad = cant;
        }
    }

    public class NodoCola
    {
        public PedidoCliente Pedido;
        public NodoCola Siguiente;

        public NodoCola(PedidoCliente p)
        {
            Pedido = p;
            Siguiente = null;
        }
    }

    public class ColaHorasPico
    {
        private NodoCola frente;
        private NodoCola final;
        public int CantidadEnCola { get; private set; }

        public bool EstaVacia => frente == null;

        public void EncolarPedido(PedidoCliente pedido)
        {
            NodoCola nuevo = new NodoCola(pedido);
            if (EstaVacia)
            {
                frente = final = nuevo;
            }
            else
            {
                final.Siguiente = nuevo;
                final = nuevo;
            }
            CantidadEnCola++;
        }

        public PedidoCliente AtenderSiguiente()
        {
            if (EstaVacia) return null;
            PedidoCliente atendido = frente.Pedido;
            frente = frente.Siguiente;
            if (frente == null) final = null;
            CantidadEnCola--;
            return atendido;
        }

        public void MostrarColaPendiente()
        {
            Console.WriteLine($"\n--- COLA DE ATENCIÓN EN HORA PICO (Clientes en espera: {CantidadEnCola}) ---");
            if (EstaVacia)
            {
                Console.WriteLine("No hay clientes esperando en la fila.");
                return;
            }
            NodoCola actual = frente;
            while (actual != null)
            {
                Console.WriteLine($"[Turno #{actual.Pedido.Turno}] Cliente: {actual.Pedido.NombreCliente,-16} | Producto Cód: {actual.Pedido.CodigoProducto} | Cant: {actual.Pedido.Cantidad}");
                actual = actual.Siguiente;
            }
        }
    }

    // =========================================================================
    // 6. ÁRBOL BINARIO DE BÚSQUEDA - ABB (Búsqueda rápida por código)
    // =========================================================================
    public class NodoABB
    {
        public Producto Dato;
        public NodoABB Izquierdo;
        public NodoABB Derecho;

        public NodoABB(Producto dato)
        {
            Dato = dato;
            Izquierdo = Derecho = null;
        }
    }

    public class ArbolBinarioProductos
    {
        public NodoABB Raiz { get; private set; }

        public void Insertar(Producto prod)
        {
            Raiz = InsertarRecursivo(Raiz, prod);
        }

        private NodoABB InsertarRecursivo(NodoABB actual, Producto prod)
        {
            if (actual == null) return new NodoABB(prod);

            if (prod.Codigo < actual.Dato.Codigo)
                actual.Izquierdo = InsertarRecursivo(actual.Izquierdo, prod);
            else if (prod.Codigo > actual.Dato.Codigo)
                actual.Derecho = InsertarRecursivo(actual.Derecho, prod);

            return actual;
        }

        public Producto BuscarPorCodigo(int codigo, out int comparaciones)
        {
            comparaciones = 0;
            NodoABB actual = Raiz;
            while (actual != null)
            {
                comparaciones++;
                if (codigo == actual.Dato.Codigo)
                    return actual.Dato;
                else if (codigo < actual.Dato.Codigo)
                    actual = actual.Izquierdo;
                else
                    actual = actual.Derecho;
            }
            return null;
        }

        public int ObtenerAltura(NodoABB nodo)
        {
            if (nodo == null) return 0;
            int izq = ObtenerAltura(nodo.Izquierdo);
            int der = ObtenerAltura(nodo.Derecho);
            return 1 + (izq > der ? izq : der);
        }

        public void RecorridoInOrden(NodoABB nodo)
        {
            if (nodo != null)
            {
                RecorridoInOrden(nodo.Izquierdo);
                Console.WriteLine(nodo.Dato);
                RecorridoInOrden(nodo.Derecho);
            }
        }
    }

    // =========================================================================
    // 7. GRAFO PONDERADO CON NODOS ENLAZADOS PROPIOS (Algoritmo de Dijkstra)
    // =========================================================================
    public class Arista
    {
        public string Destino { get; set; }
        public int PesoKm { get; set; }
        public Arista Siguiente { get; set; }

        public Arista(string destino, int pesoKm)
        {
            Destino = destino;
            PesoKm = pesoKm;
            Siguiente = null;
        }
    }

    public class NodoVertice
    {
        public string Nombre;
        public Arista PrimeraArista;
        public NodoVertice Siguiente;

        public NodoVertice(string nombre)
        {
            Nombre = nombre;
            PrimeraArista = null;
            Siguiente = null;
        }
    }

    public class GrafoRedDonaLuz
    {
        private NodoVertice cabezaVertices;
        public int TotalVertices { get; private set; }

        public void AgregarNodo(string nombrePunto)
        {
            if (BuscarVertice(nombrePunto) != null) return;

            NodoVertice nuevo = new NodoVertice(nombrePunto);
            if (cabezaVertices == null)
            {
                cabezaVertices = nuevo;
            }
            else
            {
                NodoVertice actual = cabezaVertices;
                while (actual.Siguiente != null)
                    actual = actual.Siguiente;
                actual.Siguiente = nuevo;
            }
            TotalVertices++;
        }

        private NodoVertice BuscarVertice(string nombre)
        {
            NodoVertice actual = cabezaVertices;
            while (actual != null)
            {
                if (string.Equals(actual.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                    return actual;
                actual = actual.Siguiente;
            }
            return null;
        }

        private void AgregarAristaUnidireccional(string origen, string destino, int pesoKm)
        {
            NodoVertice vOrigen = BuscarVertice(origen);
            if (vOrigen == null) return;

            Arista nueva = new Arista(destino, pesoKm);
            if (vOrigen.PrimeraArista == null)
            {
                vOrigen.PrimeraArista = nueva;
            }
            else
            {
                Arista act = vOrigen.PrimeraArista;
                while (act.Siguiente != null)
                    act = act.Siguiente;
                act.Siguiente = nueva;
            }
        }

        public void AgregarConexionBidireccional(string origen, string destino, int distanciaKm)
        {
            AgregarNodo(origen);
            AgregarNodo(destino);
            AgregarAristaUnidireccional(origen, destino, distanciaKm);
            AgregarAristaUnidireccional(destino, origen, distanciaKm);
        }

        public string[] ObtenerNombresVertices()
        {
            string[] nombres = new string[TotalVertices];
            NodoVertice actual = cabezaVertices;
            int i = 0;
            while (actual != null)
            {
                nombres[i++] = actual.Nombre;
                actual = actual.Siguiente;
            }
            return nombres;
        }

        public string BuscarNombreNodo(string nombre)
        {
            NodoVertice v = BuscarVertice(nombre);
            return v != null ? v.Nombre : null;
        }

        private int ObtenerIndice(string[] nombres, string buscado)
        {
            for (int i = 0; i < nombres.Length; i++)
            {
                if (string.Equals(nombres[i], buscado, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        public void CalcularRutaMinimaDijkstra(string inicio, string fin)
        {
            string[] nombres = ObtenerNombresVertices();
            int n = nombres.Length;
            int idxInicio = ObtenerIndice(nombres, inicio);
            int idxFin = ObtenerIndice(nombres, fin);

            if (idxInicio == -1 || idxFin == -1)
            {
                Console.WriteLine("Uno de los puntos seleccionados no existe en la red.");
                return;
            }

            int[] distancias = new int[n];
            int[] previos = new int[n];
            bool[] visitados = new bool[n];

            for (int i = 0; i < n; i++)
            {
                distancias[i] = int.MaxValue;
                previos[i] = -1;
                visitados[i] = false;
            }
            distancias[idxInicio] = 0;

            for (int paso = 0; paso < n; paso++)
            {
                int u = -1;
                int menorDist = int.MaxValue;

                for (int i = 0; i < n; i++)
                {
                    if (!visitados[i] && distancias[i] < menorDist)
                    {
                        menorDist = distancias[i];
                        u = i;
                    }
                }

                if (u == -1 || u == idxFin) break;
                visitados[u] = true;

                NodoVertice verticeActual = BuscarVertice(nombres[u]);
                Arista arista = verticeActual.PrimeraArista;
                while (arista != null)
                {
                    int v = ObtenerIndice(nombres, arista.Destino);
                    if (v != -1 && !visitados[v])
                    {
                        int nuevaDist = distancias[u] + arista.PesoKm;
                        if (nuevaDist < distancias[v])
                        {
                            distancias[v] = nuevaDist;
                            previos[v] = u;
                        }
                    }
                    arista = arista.Siguiente;
                }
            }

            Console.WriteLine("\n--- RUTA ÓPTIMA DE REPARTO (ALGORITMO DE DIJKSTRA) ---");
            if (distancias[idxFin] == int.MaxValue)
            {
                Console.WriteLine($"No existe ruta disponible entre {inicio} y {fin}.");
                return;
            }

            // Reconstruir el camino usando nuestra PilaPersonalizada
            PilaPersonalizada<string> pilaCamino = new PilaPersonalizada<string>();
            int actualIdx = idxFin;
            while (actualIdx != -1)
            {
                pilaCamino.Push(nombres[actualIdx]);
                actualIdx = previos[actualIdx];
            }

            Console.Write("Recorrido óptimo: ");
            while (!pilaCamino.EstaVacia)
            {
                Console.Write(pilaCamino.Pop());
                if (!pilaCamino.EstaVacia) Console.Write(" -> ");
            }
            Console.WriteLine($"\nDistancia total mínima: {distancias[idxFin]} km.");
        }

        public void MostrarRed()
        {
            Console.WriteLine("\n--- RED DE ALMACENES, SUCURSALES Y AGENCIAS 'DOÑA LUZ' ---");
            NodoVertice actual = cabezaVertices;
            int num = 1;
            while (actual != null)
            {
                Console.Write($"  {num}. [{actual.Nombre,-17}] conecta con: ");
                Arista a = actual.PrimeraArista;
                while (a != null)
                {
                    Console.Write($"{a.Destino} ({a.PesoKm} km) | ");
                    a = a.Siguiente;
                }
                Console.WriteLine();
                actual = actual.Siguiente;
                num++;
            }
        }
    }
}
