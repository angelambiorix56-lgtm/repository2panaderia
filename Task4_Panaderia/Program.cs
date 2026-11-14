// Angel Liriano
// Proyecto: Registro de Productos para Panadería
// TAREA 4: Aplicativo conectado a un motor de base de datos (SQL Server)

// ---------------------- INICIO DEL PROGRAMA --------------------------
ConexionBD conexion = new ConexionBD();

try
{
    conexion.Probar();

    Inventario inventario = new Inventario(conexion);
    Menu menu = new Menu(inventario);
    menu.Iniciar();
}
catch (Exception ex)
{
    Console.WriteLine("No se pudo conectar con la base de datos.");
    Console.WriteLine("Revise que SQL Server esté encendido, que haya ejecutado script.sql");
    Console.WriteLine("y que la cadena de conexión (clase ConexionBD) sea correcta.");
    Console.WriteLine("Detalle: " + ex.Message);
}

// CLASE: ConexionBD
public class ConexionBD
{

    private string cadenaDeConexion =
        "Server=LAPTOP-5EUO28BN\\SQLEXPRESS04;Database=PanaderiaDB;Trusted_Connection=True;TrustServerCertificate=True;";

    // recordar abrir y cierrar una conexión para comprobar que la base de datos responde. :)
    public void Probar()
    {
        Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(cadenaDeConexion);
        try
        {
            conexion.Open();
        }
        finally
        {
            conexion.Close();
        }
    }

   
    public List<object[]> Consultar(string sql, Dictionary<string, object> parametros)
    {
        List<object[]> filas = new List<object[]>();
        Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(cadenaDeConexion);

        try
        {
            conexion.Open();
            Microsoft.Data.SqlClient.SqlCommand comando = CrearComando(conexion, sql, parametros);
            Microsoft.Data.SqlClient.SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                object[] fila = new object[lector.FieldCount];
                lector.GetValues(fila);
                filas.Add(fila);
            }

            lector.Close();
        }
        finally
        {
            // ten en cuenta que la conexión se cierra siempre, aunque ocurra un error.
            conexion.Close();
        }

        return filas;
    }

    
    public int Ejecutar(string sql, Dictionary<string, object> parametros)
    {
        Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(cadenaDeConexion);

        try
        {
            conexion.Open();
            Microsoft.Data.SqlClient.SqlCommand comando = CrearComando(conexion, sql, parametros);
            return comando.ExecuteNonQuery();
        }
        finally
        {
            conexion.Close();
        }
    }

    // Ejecuta una consulta que devuelve un solo número (un conteo o el código recién creado).
    public int EjecutarNumero(string sql, Dictionary<string, object> parametros)
    {
        Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(cadenaDeConexion);

        try
        {
            conexion.Open();
            Microsoft.Data.SqlClient.SqlCommand comando = CrearComando(conexion, sql, parametros);
            return Convert.ToInt32(comando.ExecuteScalar());
        }
        finally
        {
            conexion.Close();
        }
    }

    // Arma el comando y le agrega los parámetros (@nombre, @codigo, etc.).
    private Microsoft.Data.SqlClient.SqlCommand CrearComando(
        Microsoft.Data.SqlClient.SqlConnection conexion, string sql, Dictionary<string, object> parametros)
    {
        Microsoft.Data.SqlClient.SqlCommand comando = new Microsoft.Data.SqlClient.SqlCommand(sql, conexion);

        foreach (KeyValuePair<string, object> parametro in parametros)
        {
            comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
        }

        return comando;
    }
}

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


    public override string ToString()
    {
        return $"{Codigo,-8}{Nombre,-28}{Categoria.Nombre,-18}{Precio.ToString(),12}";
    }
}


// CLASE: Inventario
// Administra las categorías y los productos. Los datos se guardan en SQL Server.
public class Inventario
{
    private ConexionBD conexionBD;

    public Inventario(ConexionBD conexionBD)
    {
        this.conexionBD = conexionBD;
    }

    // ---------------------- CATEGORÍAS -------------------------------
    public List<Categoria> ListarCategorias()
    {
        List<Categoria> categorias = new List<Categoria>();
        Dictionary<string, object> parametros = new Dictionary<string, object>();

        List<object[]> filas = conexionBD.Consultar("SELECT Id, Nombre FROM Categorias ORDER BY Id", parametros);

        foreach (object[] fila in filas)
        {
            categorias.Add(new Categoria(Convert.ToInt32(fila[0]), fila[1].ToString() ?? ""));
        }

        return categorias;
    }

    public Categoria? BuscarCategoria(int id)
    {
        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@id", id);

        List<object[]> filas = conexionBD.Consultar("SELECT Id, Nombre FROM Categorias WHERE Id = @id", parametros);

        if (filas.Count == 0)
        {
            return null;
        }

        return new Categoria(Convert.ToInt32(filas[0][0]), filas[0][1].ToString() ?? "");
    }

    public Categoria AgregarCategoria(string nombre)
    {
        // Se crea el objeto primero para aprovechar las validaciones de la clase.
        Categoria validada = new Categoria(0, nombre);

        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@nombre", validada.Nombre);

        if (conexionBD.EjecutarNumero("SELECT COUNT(*) FROM Categorias WHERE Nombre = @nombre", parametros) > 0)
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }

        int id = conexionBD.EjecutarNumero("INSERT INTO Categorias (Nombre) OUTPUT INSERTED.Id VALUES (@nombre)", parametros);
        return new Categoria(id, validada.Nombre);
    }

    // ---------------------- PRODUCTOS --------------------------------
    public Producto AgregarProducto(string nombre, Categoria categoria, decimal monto)
    {
        // Se crea el objeto primero para aprovechar las validaciones de las clases.
        Producto validado = new Producto(0, nombre, categoria, new Precio(monto));

        ValidarNombreDisponible(validado.Nombre, 0);

        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@nombre", validado.Nombre);
        parametros.Add("@categoria", categoria.Id);
        parametros.Add("@precio", validado.Precio.Monto);

        string sql = "INSERT INTO Productos (Nombre, CategoriaId, Precio) OUTPUT INSERTED.Codigo " +
                     "VALUES (@nombre, @categoria, @precio)";

        int codigo = conexionBD.EjecutarNumero(sql, parametros);
        return new Producto(codigo, validado.Nombre, categoria, validado.Precio);
    }

    public Producto? BuscarPorCodigo(int codigo)
    {
        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@codigo", codigo);

        List<Producto> productos = ConsultarProductos("WHERE p.Codigo = @codigo", parametros);

        if (productos.Count == 0)
        {
            return null;
        }

        return productos[0];
    }

    public List<Producto> BuscarPorNombre(string texto)
    {
        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@texto", "%" + texto.Trim() + "%");

        return ConsultarProductos("WHERE p.Nombre LIKE @texto", parametros);
    }

    public List<Producto> BuscarPorCategoria(int idCategoria)
    {
        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@categoria", idCategoria);

        return ConsultarProductos("WHERE p.CategoriaId = @categoria", parametros);
    }

    public List<Producto> BuscarPorRangoDePrecio(decimal minimo, decimal maximo)
    {
        if (minimo > maximo)
        {
            throw new ArgumentException("El precio mínimo no puede ser mayor que el máximo.");
        }

        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@minimo", minimo);
        parametros.Add("@maximo", maximo);

        return ConsultarProductos("WHERE p.Precio BETWEEN @minimo AND @maximo", parametros);
    }

    public void Modificar(int codigo, string nombre, Categoria categoria, decimal monto)
    {
        Producto validado = new Producto(codigo, nombre, categoria, new Precio(monto));

        ValidarNombreDisponible(validado.Nombre, codigo);

        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@nombre", validado.Nombre);
        parametros.Add("@categoria", categoria.Id);
        parametros.Add("@precio", validado.Precio.Monto);
        parametros.Add("@codigo", codigo);

        string sql = "UPDATE Productos SET Nombre = @nombre, CategoriaId = @categoria, Precio = @precio " +
                     "WHERE Codigo = @codigo";

        if (conexionBD.Ejecutar(sql, parametros) == 0)
        {
            throw new KeyNotFoundException("No existe un producto con el código " + codigo + ".");
        }
    }

    public bool Eliminar(int codigo)
    {
        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@codigo", codigo);

        return conexionBD.Ejecutar("DELETE FROM Productos WHERE Codigo = @codigo", parametros) > 0;
    }

    public List<Producto> Listar()
    {
        return ConsultarProductos("", new Dictionary<string, object>());
    }

    // ---------------------- MÉTODOS DE APOYO -------------------------
    // Ejecuta el SELECT de productos con el filtro indicado y arma los objetos.
    private List<Producto> ConsultarProductos(string filtro, Dictionary<string, object> parametros)
    {
        List<Producto> productos = new List<Producto>();

        string sql = "SELECT p.Codigo, p.Nombre, p.Precio, c.Id, c.Nombre " +
                     "FROM Productos p INNER JOIN Categorias c ON c.Id = p.CategoriaId " +
                     filtro + " ORDER BY p.Codigo";

        List<object[]> filas = conexionBD.Consultar(sql, parametros);

        foreach (object[] fila in filas)
        {
            Categoria categoria = new Categoria(Convert.ToInt32(fila[3]), fila[4].ToString() ?? "");
            Precio precio = new Precio(Convert.ToDecimal(fila[2]));
            productos.Add(new Producto(Convert.ToInt32(fila[0]), fila[1].ToString() ?? "", categoria, precio));
        }

        return productos;
    }

    // Lanza un error si otro producto (distinto al código indicado) ya usa ese nombre.
    private void ValidarNombreDisponible(string nombre, int codigoIgnorado)
    {
        Dictionary<string, object> parametros = new Dictionary<string, object>();
        parametros.Add("@nombre", nombre);
        parametros.Add("@codigo", codigoIgnorado);

        string sql = "SELECT COUNT(*) FROM Productos WHERE Nombre = @nombre AND Codigo <> @codigo";

        if (conexionBD.EjecutarNumero(sql, parametros) > 0)
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
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine("Error de base de datos: " + ex.Message);
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