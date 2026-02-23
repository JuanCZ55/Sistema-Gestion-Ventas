namespace SistemaGestionVentas.Models
{
    public class DashboardViewModel
    {
        public List<Grafico> Graficos { get; set; } = new List<Grafico>();
        public decimal TotalCosto => Graficos.Sum(g => g.Costo);
        public decimal TotalVenta => Graficos.Sum(g => g.Venta);
        public decimal TotalGanancia => TotalVenta - TotalCosto;
        public int TotalCantVenta => Graficos.Sum(g => g.CantV);
        public decimal TotalCantProd => Graficos.Sum(g => g.CantP);
        public List<string> UsuariosTop { get; set; } = new List<string>();
        public Dictionary<string, decimal> ProductoPocoStock { get; set; } =
            new Dictionary<string, decimal>();
        public List<string> ProductoTop { get; set; } = new List<string>();
    }

    public class Grafico
    {
        public DateTime Fecha { get; set; }
        public decimal Venta { get; set; }
        public decimal Costo { get; set; }
        public int CantV { get; set; }
        public decimal CantP { get; set; }
    }
}
