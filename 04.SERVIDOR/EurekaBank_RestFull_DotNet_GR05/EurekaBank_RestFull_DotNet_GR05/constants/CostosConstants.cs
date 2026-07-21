namespace EurekaBank_RestFull_DotNet_GR05.Constants
{
    /// <summary>
    /// Constantes de costos y parámetros del sistema bancario
    /// </summary>
    public static class CostosConstants
    {
        // ITF (Impuesto a las Transacciones Financieras)
        public const decimal TASA_ITF = 0.0008m; // 0.08%
        
        // Operaciones gratuitas por cuenta
        public const int OPERACIONES_GRATUITAS = 15;
        
        // Costos por movimiento según moneda
        public const decimal COSTO_MOVIMIENTO_SOLES = 2.00m;
        public const decimal COSTO_MOVIMIENTO_DOLARES = 0.60m;
        
        // Códigos de moneda
        public const string MONEDA_SOLES = "01";
        public const string MONEDA_DOLARES = "02";
        
        // Cargo de mantenimiento
        public const decimal MONTO_MINIMO_SOLES = 3500.00m;
        public const decimal MONTO_MINIMO_DOLARES = 1200.00m;
        public const decimal CARGO_MANTENIMIENTO_SOLES = 7.00m;
        public const decimal CARGO_MANTENIMIENTO_DOLARES = 2.50m;
        
        // Intereses mensuales
        public const decimal INTERES_MENSUAL_SOLES = 0.70m; // 0.70%
        public const decimal INTERES_MENSUAL_DOLARES = 0.60m; // 0.60%
    }
}
