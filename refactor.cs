
namespace Parcial1.Ferreteria;

// Antes: una sola interfaz "IEmpleadoDeFerreteria" con 4 metodos obligaba a
// Vendedor a implementar cosas que no puede hacer (lanzaba NotSupportedException).
// Eso violaba ISP (interfaz demasiado grande) y de paso LSP (Vendedor no era
// sustituible de forma segura donde se esperaba un IEmpleadoDeFerreteria).
//
// Ahora: se separa en dos contratos chicos segun la responsabilidad real.

public interface IGestionDePedidos
{
    void RegistrarPedido(string material, int cantidad);
}

public interface IGestionComercial
{
    void AutorizarVentaAlPorMayor(string material);
    void AjustarPrecio(string material, decimal nuevoPrecio);
    void VerReporteDeCompras();
}

// Encargado puede hacer todo, asi que implementa ambas interfaces.
public class Encargado : IGestionDePedidos, IGestionComercial
{
    public void RegistrarPedido(string material, int cantidad)
        => Console.WriteLine($"[ENC] Pedido: {cantidad} x {material}");

    public void AutorizarVentaAlPorMayor(string material)
        => Console.WriteLine($"[ENC] Venta al por mayor de {material} autorizada");

    public void AjustarPrecio(string material, decimal nuevoPrecio)
        => Console.WriteLine($"[ENC] {material} ahora cuesta {nuevoPrecio:0.00} Bs");

    public void VerReporteDeCompras()
        => Console.WriteLine("[ENC] Reporte de compras del mes");
}

// Vendedor solo implementa lo que realmente puede hacer.
// Ya no hereda metodos que tiene que "romper" con excepciones -> LSP resuelto.
public class Vendedor : IGestionDePedidos
{
    public void RegistrarPedido(string material, int cantidad)
        => Console.WriteLine($"[VEND] Pedido: {cantidad} x {material}");
}


// Antes: GestorDePedidos.ProcesarPedido hacia "new BaseDeDatosMySql()" y
// "new CorreoSmtp()" adentro del metodo. La clase de alto nivel quedaba
// atada a implementaciones concretas de bajo nivel.
//
// Ahora: GestorDePedidos depende de abstracciones (IBaseDeDatos, INotificador)
// que recibe por constructor. Cualquier implementacion que cumpla el contrato
// se puede inyectar sin tocar GestorDePedidos.

public interface IBaseDeDatos
{
    void GuardarPedido(string cliente, string material, int cantidad, decimal total);
}

public interface INotificador
{
    void Enviar(string mensaje);
}

// Implementaciones concretas: pueden vivir en otro archivo del sistema,
// se muestran aca solo para que refactor.cs quede completo y compilable.
public class BaseDeDatosMySql : IBaseDeDatos
{
    public void GuardarPedido(string cliente, string material, int cantidad, decimal total)
        => Console.WriteLine($"[MYSQL] INSERT INTO pedidos VALUES ('{cliente}', '{material}', {cantidad}, {total})");
}

public class CorreoSmtp : INotificador
{
    public void Enviar(string mensaje)
        => Console.WriteLine($"[SMTP] {mensaje}");
}

// GestorDePedidos ya no crea sus dependencias: las recibe por constructor.
public class GestorDePedidos
{
    private readonly IBaseDeDatos _baseDeDatos;
    private readonly INotificador _notificador;

    public GestorDePedidos(IBaseDeDatos baseDeDatos, INotificador notificador)
    {
        _baseDeDatos = baseDeDatos ?? throw new ArgumentNullException(nameof(baseDeDatos));
        _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
    }

    public void ProcesarPedido(string cliente, string tipoCliente, string material, int cantidad, decimal precioUnitario)
    {
        decimal total = cantidad * precioUnitario;

        // El calculo del descuento se deja tal cual estaba en el sistema original
        // (esa parte -OCP- no forma parte de las dos curas elegidas aca).
        decimal descuento;
        switch (tipoCliente)
        {
            case "particular":
                descuento = 0;
                break;
            case "contratista":
                descuento = total * 0.15m;
                break;
            case "constructora":
                descuento = total * 0.25m;
                break;
            default:
                descuento = 0;
                break;
        }
        decimal totalFinal = total - descuento;

        _baseDeDatos.GuardarPedido(cliente, material, cantidad, totalFinal);

        Console.WriteLine("----- COMPROBANTE -----");
        Console.WriteLine($"{cantidad} x {material}");
        Console.WriteLine($"Cliente: {cliente} ({tipoCliente})");
        Console.WriteLine($"TOTAL: {totalFinal:0.00} Bs");

        _notificador.Enviar($"Su pedido de {material} fue registrado, {cliente}");
    }
}


// ============================================================
// Ejemplo de uso (equivalente al Demo.Correr() del sistema original)
// ============================================================
public static class DemoRefactor
{
    public static void Correr()
    {
        // Las dependencias concretas se arman aca afuera y se inyectan
        // por constructor -> GestorDePedidos no sabe (ni le importa)
        // que hay MySQL o SMTP detras de las interfaces.
        IBaseDeDatos db = new BaseDeDatosMySql();
        INotificador correo = new CorreoSmtp();
        var gestor = new GestorDePedidos(db, correo);

        gestor.ProcesarPedido("Marco", "contratista", "Cemento 50kg", 10, 62.00m);

        // Demostracion de la cura ISP/LSP: un Vendedor solo puede
        // registrar pedidos, ya no existe la posibilidad de que
        // "implemente" AutorizarVentaAlPorMayor con una excepcion.
        IGestionDePedidos vendedor = new Vendedor();
        vendedor.RegistrarPedido("Clavos 1kg", 5);

        // Un Encargado si puede usar el contrato comercial completo.
        IGestionComercial encargado = new Encargado();
        encargado.AutorizarVentaAlPorMayor("Cemento 50kg");
        encargado.AjustarPrecio("Cemento 50kg", 65.00m);
        encargado.VerReporteDeCompras();
    }
}