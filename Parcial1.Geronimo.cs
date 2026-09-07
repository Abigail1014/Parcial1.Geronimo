namespace Parcial1.Geronimo;

public interface IEmpleadoDeFerreteria
{
    void RegistrarPedido(string material, int cantidad);
    void AutorizarVentaAlPorMayor(string material);
    void AjustarPrecio(string material, decimal nuevoPrecio);
    void VerReporteDeCompras();
}

public class Encargado : IEmpleadoDeFerreteria
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

public class Vendedor : IEmpleadoDeFerreteria
{
    public void RegistrarPedido(string material, int cantidad)
        => Console.WriteLine($"[VEND] Pedido: {cantidad} x {material}");
    public void AutorizarVentaAlPorMayor(string material)
        => throw new NotSupportedException("Un vendedor no autoriza ventas al por mayor.");
    public void AjustarPrecio(string material, decimal nuevoPrecio)
        => throw new NotSupportedException("Un vendedor no ajusta precios.");
    public void VerReporteDeCompras()
        => throw new NotSupportedException("Un vendedor no ve reportes.");
}

public class GestorDePedidos
{
    public void ProcesarPedido(string cliente, string tipoCliente, string material, int cantidad, decimal precioUnitario)
    {
        decimal total = cantidad * precioUnitario;

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

        var baseDeDatos = new BaseDeDatosMySql();
        baseDeDatos.GuardarPedido(cliente, material, cantidad, totalFinal);

        Console.WriteLine("----- COMPROBANTE -----");
        Console.WriteLine($"{cantidad} x {material}");
        Console.WriteLine($"Cliente: {cliente} ({tipoCliente})");
        Console.WriteLine($"TOTAL: {totalFinal:0.00} Bs");

        var correo = new CorreoSmtp();
        correo.Enviar($"Su pedido de {material} fue registrado, {cliente}");
    }
}

public class BaseDeDatosMySql
{
    public void GuardarPedido(string cliente, string material, int cantidad, decimal total)
        => Console.WriteLine($"[MYSQL] INSERT INTO pedidos VALUES ('{cliente}', '{material}', {cantidad}, {total})");
}

public class CorreoSmtp
{
    public void Enviar(string mensaje)
        => Console.WriteLine($"[SMTP] {mensaje}");
}

public static class Demo
{
    public static void Correr()
    {
        new GestorDePedidos().ProcesarPedido("Marco", "contratista", "Cemento 50kg", 10, 62.00m);
    }
}