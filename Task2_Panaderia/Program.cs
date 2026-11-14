// Angel Liriano
// Proyecto: Registro de Productos para Panadería
// TAREA 2: Estructura base con datos en memoria (listas y diccionarios)


// ---------------------- DATOS EN MEMORIA -----------------------------
Dictionary<int, string> categorias = new Dictionary<int, string>();

// Productos: listas individuales (paralelas). El producto que está en la
// posición 0 de una lista es el mismo que está en la posición 0 de las demás.
List<int> codigos = new List<int>();
List<string> nombres = new List<string>();
List<int> categoriasDeProducto = new List<int>();
List<decimal> precios = new List<decimal>();

int siguienteCodigoProducto = 1;
int siguienteCodigoCategoria = 1;

CargarDatosIniciales();

// ---------------------- CICLO PRINCIPAL ------------------------------
bool salir = false;

while (!salir)
{
    MostrarMenu();
    string opcion = LeerTexto("Seleccione una opción: ");

    try
    {
        switch (opcion)
        {
            case "1":
                AgregarProducto();
                break;
            case "2":
                BuscarProducto();
                break;
            case "3":
                ModificarProducto();
                break;
            case "4":
                EliminarProducto();
                break;
            case "5":
                ListarProductos();
                break;
            case "6":
                AgregarCategoria();
                break;
            case "7":
                ListarCategorias();
                break;
            case "0":
                salir = Confirmar("¿Seguro que desea salir? (s/n): ");
                break;
            default:
                Console.WriteLine("Opción no válida. Intente de nuevo.");
                break;
        }
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine("No encontrado: " + ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine("Operación no permitida: " + ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ocurrió un error inesperado: " + ex.Message);
    }

    if (!salir)
    {
        Pausar();
    }
}

Console.WriteLine("Gracias por usar el sistema de la panadería. ¡Hasta luego!");

// ---------------------- MENÚ -----------------------------------------
void MostrarMenu()
{
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("   REGISTRO DE PRODUCTOS PARA PANADERÍA");
    Console.WriteLine("==============================================");
    Console.WriteLine("1. Agregar producto");
    Console.WriteLine("2. Buscar producto");
    Console.WriteLine("3. Modificar producto");
    Console.WriteLine("4. Eliminar producto");
    Console.WriteLine("5. Listar productos");
    Console.WriteLine("6. Agregar categoría");
    Console.WriteLine("7. Listar categorías");
    Console.WriteLine("0. Salir");
    Console.WriteLine("----------------------------------------------");
}

// ---------------------- AGREGAR --------------------------------------
void AgregarProducto()
{
    Console.WriteLine();
    Console.WriteLine("--- AGREGAR PRODUCTO ---");

    string nombre = LeerTextoObligatorio("Nombre del producto: ");
    if (ExisteNombreDeProducto(nombre, -1))
    {
        throw new InvalidOperationException("ya existe un producto con ese nombre.");
    }

    ListarCategorias();
    int categoria = LeerEntero("Código de la categoría: ");
    if (!categorias.ContainsKey(categoria))
    {
        throw new KeyNotFoundException("la categoría " + categoria + " no existe.");
    }

    decimal precio = LeerPrecio("Precio (RD$): ");

    codigos.Add(siguienteCodigoProducto);
    nombres.Add(nombre);
    categoriasDeProducto.Add(categoria);
    precios.Add(precio);

    Console.WriteLine("Producto agregado con el código " + siguienteCodigoProducto + ".");
    siguienteCodigoProducto++;
}

void AgregarCategoria()
{
    Console.WriteLine();
    Console.WriteLine("--- AGREGAR CATEGORÍA ---");

    string nombre = LeerTextoObligatorio("Nombre de la categoría: ");

    foreach (string existente in categorias.Values)
    {
        if (existente.ToLower() == nombre.ToLower())
        {
            throw new InvalidOperationException("ya existe una categoría con ese nombre.");
        }
    }

    categorias.Add(siguienteCodigoCategoria, nombre);
    Console.WriteLine("Categoría agregada con el código " + siguienteCodigoCategoria + ".");
    siguienteCodigoCategoria++;
}

// ---------------------- BUSCAR ---------------------------------------
void BuscarProducto()
{
    Console.WriteLine();
    Console.WriteLine("--- BUSCAR PRODUCTO ---");
    Console.WriteLine("1. Por código");
    Console.WriteLine("2. Por nombre");
    Console.WriteLine("3. Por categoría");
    Console.WriteLine("4. Por rango de precio");
    string criterio = LeerTexto("Seleccione el criterio: ");

    int encontrados = 0;

    switch (criterio)
    {
        case "1":
            int codigo = LeerEntero("Código del producto: ");
            int posicion = BuscarPosicionPorCodigo(codigo);
            if (posicion >= 0)
            {
                MostrarEncabezado();
                MostrarProducto(posicion);
                encontrados = 1;
            }
            break;

        case "2":
            string texto = LeerTextoObligatorio("Nombre o parte del nombre: ").ToLower();
            for (int i = 0; i < codigos.Count; i++)
            {
                if (nombres[i].ToLower().Contains(texto))
                {
                    if (encontrados == 0) MostrarEncabezado();
                    MostrarProducto(i);
                    encontrados++;
                }
            }
            break;

        case "3":
            ListarCategorias();
            int categoria = LeerEntero("Código de la categoría: ");
            if (!categorias.ContainsKey(categoria))
            {
                throw new KeyNotFoundException("la categoría " + categoria + " no existe.");
            }
            for (int i = 0; i < codigos.Count; i++)
            {
                if (categoriasDeProducto[i] == categoria)
                {
                    if (encontrados == 0) MostrarEncabezado();
                    MostrarProducto(i);
                    encontrados++;
                }
            }
            break;

        case "4":
            decimal minimo = LeerDecimal("Precio mínimo: ");
            decimal maximo = LeerDecimal("Precio máximo: ");
            if (minimo > maximo)
            {
                throw new InvalidOperationException("el precio mínimo no puede ser mayor que el máximo.");
            }
            for (int i = 0; i < codigos.Count; i++)
            {
                if (precios[i] >= minimo && precios[i] <= maximo)
                {
                    if (encontrados == 0) MostrarEncabezado();
                    MostrarProducto(i);
                    encontrados++;
                }
            }
            break;

        default:
            Console.WriteLine("Criterio no válido.");
            return;
    }

    if (encontrados == 0)
    {
        Console.WriteLine("No se encontraron productos con ese criterio.");
    }
    else
    {
        Console.WriteLine("Productos encontrados: " + encontrados);
    }
}

// ---------------------- MODIFICAR ------------------------------------
void ModificarProducto()
{
    Console.WriteLine();
    Console.WriteLine("--- MODIFICAR PRODUCTO ---");

    int codigo = LeerEntero("Código del producto a modificar: ");
    int posicion = BuscarPosicionPorCodigo(codigo);
    if (posicion < 0)
    {
        throw new KeyNotFoundException("no existe un producto con el código " + codigo + ".");
    }

    MostrarEncabezado();
    MostrarProducto(posicion);
    Console.WriteLine("(Presione Enter para dejar un dato como está)");

    // Nombre
    string nuevoNombre = LeerTexto("Nuevo nombre: ");
    if (nuevoNombre != "")
    {
        if (ExisteNombreDeProducto(nuevoNombre, posicion))
        {
            throw new InvalidOperationException("ya existe otro producto con ese nombre.");
        }
        nombres[posicion] = nuevoNombre;
    }

    // Categoría
    ListarCategorias();
    string textoCategoria = LeerTexto("Nuevo código de categoría: ");
    if (textoCategoria != "")
    {
        int nuevaCategoria;
        if (!int.TryParse(textoCategoria, out nuevaCategoria) || !categorias.ContainsKey(nuevaCategoria))
        {
            throw new KeyNotFoundException("la categoría indicada no existe.");
        }
        categoriasDeProducto[posicion] = nuevaCategoria;
    }

    // Precio
    string textoPrecio = LeerTexto("Nuevo precio (RD$): ");
    if (textoPrecio != "")
    {
        decimal nuevoPrecio;
        if (!decimal.TryParse(textoPrecio, out nuevoPrecio) || nuevoPrecio <= 0)
        {
            throw new InvalidOperationException("el precio debe ser un número mayor que cero.");
        }
        precios[posicion] = nuevoPrecio;
    }

    Console.WriteLine("Producto modificado correctamente.");
}

// ---------------------- ELIMINAR -------------------------------------
void EliminarProducto()
{
    Console.WriteLine();
    Console.WriteLine("--- ELIMINAR PRODUCTO ---");

    int codigo = LeerEntero("Código del producto a eliminar: ");
    int posicion = BuscarPosicionPorCodigo(codigo);
    if (posicion < 0)
    {
        throw new KeyNotFoundException("no existe un producto con el código " + codigo + ".");
    }

    MostrarEncabezado();
    MostrarProducto(posicion);

    if (Confirmar("¿Seguro que desea eliminarlo? (s/n): "))
    {
        // Se elimina la misma posición en todas las listas para mantenerlas alineadas.
        codigos.RemoveAt(posicion);
        nombres.RemoveAt(posicion);
        categoriasDeProducto.RemoveAt(posicion);
        precios.RemoveAt(posicion);
        Console.WriteLine("Producto eliminado.");
    }
    else
    {
        Console.WriteLine("Eliminación cancelada.");
    }
}

// ---------------------- LISTAR ---------------------------------------
void ListarProductos()
{
    Console.WriteLine();
    Console.WriteLine("--- LISTADO DE PRODUCTOS ---");

    if (codigos.Count == 0)
    {
        Console.WriteLine("No hay productos registrados.");
        return;
    }

    MostrarEncabezado();
    for (int i = 0; i < codigos.Count; i++)
    {
        MostrarProducto(i);
    }
    Console.WriteLine("Total de productos: " + codigos.Count);
}

void ListarCategorias()
{
    Console.WriteLine();
    Console.WriteLine("Categorías disponibles:");

    if (categorias.Count == 0)
    {
        Console.WriteLine("  (no hay categorías registradas)");
        return;
    }

    foreach (KeyValuePair<int, string> categoria in categorias)
    {
        Console.WriteLine("  " + categoria.Key + ". " + categoria.Value);
    }
}

// ---------------------- FUNCIONES DE APOYO ---------------------------
void MostrarEncabezado()
{
    Console.WriteLine($"{"Código",-8}{"Nombre",-28}{"Categoría",-18}{"Precio",12}");
    Console.WriteLine(new string('-', 66));
}

void MostrarProducto(int posicion)
{
    string nombreCategoria = categorias[categoriasDeProducto[posicion]];
    Console.WriteLine($"{codigos[posicion],-8}{nombres[posicion],-28}{nombreCategoria,-18}{"RD$ " + precios[posicion].ToString("N2"),12}");
}

// Devuelve la posición del producto en las listas, o -1 si no existe.
int BuscarPosicionPorCodigo(int codigo)
{
    for (int i = 0; i < codigos.Count; i++)
    {
        if (codigos[i] == codigo)
        {
            return i;
        }
    }
    return -1;
}

// Indica si el nombre ya lo usa otro producto (se ignora la posición indicada).
bool ExisteNombreDeProducto(string nombre, int posicionIgnorada)
{
    for (int i = 0; i < nombres.Count; i++)
    {
        if (i != posicionIgnorada && nombres[i].ToLower() == nombre.ToLower())
        {
            return true;
        }
    }
    return false;
}

string LeerTexto(string mensaje)
{
    Console.Write(mensaje);
    string? entrada = Console.ReadLine();
    if (entrada == null)
    {
        return "";
    }
    return entrada.Trim();
}

string LeerTextoObligatorio(string mensaje)
{
    while (true)
    {
        string texto = LeerTexto(mensaje);
        if (texto != "")
        {
            return texto;
        }
        Console.WriteLine("Este dato es obligatorio.");
    }
}

int LeerEntero(string mensaje)
{
    while (true)
    {
        int numero;
        if (int.TryParse(LeerTexto(mensaje), out numero))
        {
            return numero;
        }
        Console.WriteLine("Debe escribir un número entero.");
    }
}

decimal LeerDecimal(string mensaje)
{
    while (true)
    {
        decimal numero;
        if (decimal.TryParse(LeerTexto(mensaje), out numero) && numero >= 0)
        {
            return numero;
        }
        Console.WriteLine("Debe escribir un número válido (no negativo).");
    }
}

decimal LeerPrecio(string mensaje)
{
    while (true)
    {
        decimal precio = LeerDecimal(mensaje);
        if (precio > 0)
        {
            return precio;
        }
        Console.WriteLine("El precio debe ser mayor que cero.");
    }
}

bool Confirmar(string mensaje)
{
    while (true)
    {
        string respuesta = LeerTexto(mensaje).ToLower();
        if (respuesta == "s" || respuesta == "si" || respuesta == "sí")
        {
            return true;
        }
        if (respuesta == "n" || respuesta == "no")
        {
            return false;
        }
        Console.WriteLine("Responda con s o n.");
    }
}

void Pausar()
{
    Console.WriteLine();
    Console.Write("Presione Enter para continuar...");
    Console.ReadLine();
}

void CargarDatosIniciales()
{
    categorias.Add(siguienteCodigoCategoria++, "Panes");
    categorias.Add(siguienteCodigoCategoria++, "Bizcochos");
    categorias.Add(siguienteCodigoCategoria++, "Galletas");
    categorias.Add(siguienteCodigoCategoria++, "Bebidas");

    string[] nombresIniciales = { "Pan de agua", "Pan sobao", "Bizcocho dominicano (libra)", "Galletas de mantequilla", "Jugo de chinola" };
    int[] categoriasIniciales = { 1, 1, 2, 3, 4 };
    decimal[] preciosIniciales = { 10m, 15m, 450m, 75m, 60m };

    for (int i = 0; i < nombresIniciales.Length; i++)
    {
        codigos.Add(siguienteCodigoProducto++);
        nombres.Add(nombresIniciales[i]);
        categoriasDeProducto.Add(categoriasIniciales[i]);
        precios.Add(preciosIniciales[i]);
    }
}

