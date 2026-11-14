// Angel Liriano
// Proyecto: Registro de Productos para Panadería
// TAREA 3: Proyecto transformado a Programación Orientada a Objetos

Inventario inventario = new Inventario();
inventario.CargarDatosIniciales();

Menu menu = new Menu(inventario);
menu.Iniciar();

// CLASE: Categoria

public class Categoria
{
    private string nombre = "";

    public int Id { get; private set; }

    public string Nombre
    {
        get { return nombre; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El nombre de la categoría es obligatorio.");
            }
            nombre = value.Trim();
        }
    }

    public Categoria(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }

    public override string ToString()
    {
        return Id + ". " + Nombre;
    }
}

// CLASE: Precio
public class Precio
{
    private decimal monto;

    public decimal Monto
    {
        get { return monto; }
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("El precio debe ser mayor que cero.");
            }
            monto = value;
        }
    }

    public string Moneda { get; private set; }

    public Precio(decimal monto)
    {
        Monto = monto;
        Moneda = "RD$";
    }

    public override string ToString()
    {
        return Moneda + " " + Monto.ToString("N2");
    }
}

// CLASE: Producto

public class Producto
{
    private string nombre = "";

    public int Codigo { get; private set; }

    public string Nombre
    {
        get { return nombre; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("El nombre del producto es obligatorio.");
            }
            nombre = value.Trim();
        }
    }

    public Categoria Categoria { get; set; }

    public Precio Precio { get; set; }

    public Producto(int codigo, string nombre, Categoria categoria, Precio precio)
    {
        Codigo = codigo;
        Nombre = nombre;
        Categoria = categoria;
        Precio = precio;
    }

    // Devuelve el producto como una fila de la tabla que se muestra en consola.
    public override string ToString()
    {
        return $"{Codigo,-8}{Nombre,-28}{Categoria.Nombre,-18}{Precio.ToString(),12}";
    }
}


// CLASE: Inventario
public class Inventario
{
    private List<Categoria> categorias = new List<Categoria>();
    private List<Producto> productos = new List<Producto>();
    private int siguienteIdCategoria = 1;
    private int siguienteCodigoProducto = 1;

    // ---------------------- CATEGORÍAS -------------------------------
    public List<Categoria> ListarCategorias()
    {
        return new List<Categoria>(categorias);
    }

    public Categoria? BuscarCategoria(int id)
    {
        return categorias.FirstOrDefault(c => c.Id == id);
    }

    public Categoria AgregarCategoria(string nombre)
    {
        if (categorias.Any(c => c.Nombre.ToLower() == nombre.Trim().ToLower()))
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }

        Categoria categoria = new Categoria(siguienteIdCategoria, nombre);
        categorias.Add(categoria);
        siguienteIdCategoria++;
        return categoria;
    }

    // ---------------------- PRODUCTOS --------------------------------
    public Producto AgregarProducto(string nombre, Categoria categoria, decimal monto)
    {
        ValidarNombreDisponible(nombre, 0);

        Producto producto = new Producto(siguienteCodigoProducto, nombre, categoria, new Precio(monto));
        productos.Add(producto);
        siguienteCodigoProducto++;
        return producto;
    }

    public Producto? BuscarPorCodigo(int codigo)
    {
        return productos.FirstOrDefault(p => p.Codigo == codigo);
    }

    public List<Producto> BuscarPorNombre(string texto)
    {
        return productos.Where(p => p.Nombre.ToLower().Contains(texto.Trim().ToLower())).ToList();
    }

    public List<Producto> BuscarPorCategoria(int idCategoria)
    {
        return productos.Where(p => p.Categoria.Id == idCategoria).ToList();
    }

    public List<Producto> BuscarPorRangoDePrecio(decimal minimo, decimal maximo)
    {
        if (minimo > maximo)
        {
            throw new ArgumentException("El precio mínimo no puede ser mayor que el máximo.");
        }
        return productos.Where(p => p.Precio.Monto >= minimo && p.Precio.Monto <= maximo).ToList();
    }

    public void Modificar(int codigo, string nombre, Categoria categoria, decimal monto)
    {
        Producto? producto = BuscarPorCodigo(codigo);
        if (producto == null)
        {
            throw new KeyNotFoundException("No existe un producto con el código " + codigo + ".");
        }

        ValidarNombreDisponible(nombre, codigo);

        // Se crea primero el precio: si el monto no es válido, no se cambia nada.
        Precio nuevoPrecio = new Precio(monto);
        producto.Nombre = nombre;
        producto.Categoria = categoria;
        producto.Precio = nuevoPrecio;
    }

    public bool Eliminar(int codigo)
    {
        Producto? producto = BuscarPorCodigo(codigo);
        if (producto == null)
        {
            return false;
        }
        return productos.Remove(producto);
    }

    public List<Producto> Listar()
    {
        return new List<Producto>(productos);
    }

    public void CargarDatosIniciales()
    {
        Categoria panes = AgregarCategoria("Panes");
        Categoria bizcochos = AgregarCategoria("Bizcochos");
        Categoria galletas = AgregarCategoria("Galletas");
        Categoria bebidas = AgregarCategoria("Bebidas");

        AgregarProducto("Pan de agua", panes, 10m);
        AgregarProducto("Pan sobao", panes, 15m);
        AgregarProducto("Bizcocho dominicano (libra)", bizcochos, 450m);
        AgregarProducto("Galletas de mantequilla", galletas, 75m);
        AgregarProducto("Jugo de chinola", bebidas, 60m);
    }

    // Lanza un error si otro producto (distinto al código indicado) ya usa ese nombre.
    private void ValidarNombreDisponible(string nombre, int codigoIgnorado)
    {
        if (productos.Any(p => p.Codigo != codigoIgnorado && p.Nombre.ToLower() == nombre.Trim().ToLower()))
        {
            throw new InvalidOperationException("Ya existe un producto con ese nombre.");
        }
    }
}

// CLASE: Menu
public class Menu
{
    private Inventario inventario;

    public Menu(Inventario inventario)
    {
        this.inventario = inventario;
    }

    // El programa no se cierra hasta que el usuario elija salir y lo confirme.
    public void Iniciar()
    {
        bool salir = false;

        while (!salir)
        {
            MostrarOpciones();
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
            catch (ArgumentException ex)
            {
                Console.WriteLine("Dato no válido: " + ex.Message);
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
    }

    private void MostrarOpciones()
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

    // ---------------------- AGREGAR ----------------------------------
    private void AgregarProducto()
    {
        Console.WriteLine();
        Console.WriteLine("--- AGREGAR PRODUCTO ---");

        string nombre = LeerTextoObligatorio("Nombre del producto: ");
        Categoria categoria = SeleccionarCategoria("Código de la categoría: ");
        decimal monto = LeerDecimal("Precio (RD$): ");

        Producto producto = inventario.AgregarProducto(nombre, categoria, monto);
        Console.WriteLine("Producto agregado con el código " + producto.Codigo + ".");
    }

    private void AgregarCategoria()
    {
        Console.WriteLine();
        Console.WriteLine("--- AGREGAR CATEGORÍA ---");

        string nombre = LeerTextoObligatorio("Nombre de la categoría: ");
        Categoria categoria = inventario.AgregarCategoria(nombre);
        Console.WriteLine("Categoría agregada con el código " + categoria.Id + ".");
    }

    // ---------------------- BUSCAR -----------------------------------
    private void BuscarProducto()
    {
        Console.WriteLine();
        Console.WriteLine("--- BUSCAR PRODUCTO ---");
        Console.WriteLine("1. Por código");
        Console.WriteLine("2. Por nombre");
        Console.WriteLine("3. Por categoría");
        Console.WriteLine("4. Por rango de precio");
        string criterio = LeerTexto("Seleccione el criterio: ");

        List<Producto> resultados = new List<Producto>();

        switch (criterio)
        {
            case "1":
                Producto? encontrado = inventario.BuscarPorCodigo(LeerEntero("Código del producto: "));
                if (encontrado != null)
                {
                    resultados.Add(encontrado);
                }
                break;
            case "2":
                resultados = inventario.BuscarPorNombre(LeerTextoObligatorio("Nombre o parte del nombre: "));
                break;
            case "3":
                Categoria categoria = SeleccionarCategoria("Código de la categoría: ");
                resultados = inventario.BuscarPorCategoria(categoria.Id);
                break;
            case "4":
                decimal minimo = LeerDecimal("Precio mínimo: ");
                decimal maximo = LeerDecimal("Precio máximo: ");
                resultados = inventario.BuscarPorRangoDePrecio(minimo, maximo);
                break;
            default:
                Console.WriteLine("Criterio no válido.");
                return;
        }

        if (resultados.Count == 0)
        {
            Console.WriteLine("No se encontraron productos con ese criterio.");
            return;
        }

        MostrarTabla(resultados);
        Console.WriteLine("Productos encontrados: " + resultados.Count);
    }

    // ---------------------- MODIFICAR --------------------------------
    private void ModificarProducto()
    {
        Console.WriteLine();
        Console.WriteLine("--- MODIFICAR PRODUCTO ---");

        int codigo = LeerEntero("Código del producto a modificar: ");
        Producto? producto = inventario.BuscarPorCodigo(codigo);
        if (producto == null)
        {
            throw new KeyNotFoundException("No existe un producto con el código " + codigo + ".");
        }

        MostrarTabla(new List<Producto> { producto });
        Console.WriteLine("(Presione Enter para dejar un dato como está)");

        // Nombre
        string nombre = LeerTexto("Nuevo nombre: ");
        if (nombre == "")
        {
            nombre = producto.Nombre;
        }

        // Categoría
        Categoria categoria = producto.Categoria;
        ListarCategorias();
        string textoCategoria = LeerTexto("Nuevo código de categoría: ");
        if (textoCategoria != "")
        {
            int idCategoria;
            Categoria? nuevaCategoria = null;
            if (int.TryParse(textoCategoria, out idCategoria))
            {
                nuevaCategoria = inventario.BuscarCategoria(idCategoria);
            }
            if (nuevaCategoria == null)
            {
                throw new KeyNotFoundException("La categoría indicada no existe.");
            }
            categoria = nuevaCategoria;
        }

        // Precio
        decimal monto = producto.Precio.Monto;
        string textoPrecio = LeerTexto("Nuevo precio (RD$): ");
        if (textoPrecio != "")
        {
            if (!decimal.TryParse(textoPrecio, out monto))
            {
                throw new ArgumentException("El precio debe ser un número.");
            }
        }

        inventario.Modificar(codigo, nombre, categoria, monto);
        Console.WriteLine("Producto modificado correctamente.");
    }

    // ---------------------- ELIMINAR ---------------------------------
    private void EliminarProducto()
    {
        Console.WriteLine();
        Console.WriteLine("--- ELIMINAR PRODUCTO ---");

        int codigo = LeerEntero("Código del producto a eliminar: ");
        Producto? producto = inventario.BuscarPorCodigo(codigo);
        if (producto == null)
        {
            throw new KeyNotFoundException("No existe un producto con el código " + codigo + ".");
        }

        MostrarTabla(new List<Producto> { producto });

        if (Confirmar("¿Seguro que desea eliminarlo? (s/n): "))
        {
            inventario.Eliminar(codigo);
            Console.WriteLine("Producto eliminado.");
        }
        else
        {
            Console.WriteLine("Eliminación cancelada.");
        }
    }

    // ---------------------- LISTAR -----------------------------------
    private void ListarProductos()
    {
        Console.WriteLine();
        Console.WriteLine("--- LISTADO DE PRODUCTOS ---");

        List<Producto> productos = inventario.Listar();
        if (productos.Count == 0)
        {
            Console.WriteLine("No hay productos registrados.");
            return;
        }

        MostrarTabla(productos);
        Console.WriteLine("Total de productos: " + productos.Count);
    }

    private void ListarCategorias()
    {
        Console.WriteLine();
        Console.WriteLine("Categorías disponibles:");

        List<Categoria> categorias = inventario.ListarCategorias();
        if (categorias.Count == 0)
        {
            Console.WriteLine("  (no hay categorías registradas)");
            return;
        }

        foreach (Categoria categoria in categorias)
        {
            Console.WriteLine("  " + categoria);
        }
    }

    // ---------------------- MÉTODOS DE APOYO -------------------------
    private void MostrarTabla(List<Producto> productos)
    {
        Console.WriteLine($"{"Código",-8}{"Nombre",-28}{"Categoría",-18}{"Precio",12}");
        Console.WriteLine(new string('-', 66));
        foreach (Producto producto in productos)
        {
            Console.WriteLine(producto);
        }
    }

    // Muestra las categorías y devuelve la que el usuario elija.
    private Categoria SeleccionarCategoria(string mensaje)
    {
        ListarCategorias();
        int id = LeerEntero(mensaje);
        Categoria? categoria = inventario.BuscarCategoria(id);
        if (categoria == null)
        {
            throw new KeyNotFoundException("La categoría " + id + " no existe.");
        }
        return categoria;
    }

    private string LeerTexto(string mensaje)
    {
        Console.Write(mensaje);
        string? entrada = Console.ReadLine();
        if (entrada == null)
        {
            return "";
        }
        return entrada.Trim();
    }

    private string LeerTextoObligatorio(string mensaje)
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

    private int LeerEntero(string mensaje)
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

    private decimal LeerDecimal(string mensaje)
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

    private bool Confirmar(string mensaje)
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

    private void Pausar()
    {
        Console.WriteLine();
        Console.Write("Presione Enter para continuar...");
        Console.ReadLine();
    }
}