using System;
using EurekaBank_RestFull_DotNet_GR05.Constants;

namespace EurekaBank_RestFull_DotNet_GR05.Validators
{
    /// <summary>
    /// Validador para operaciones de transacciones bancarias
    /// </summary>
    public static class TransaccionValidator
    {
        /// <summary>
        /// Verifica si el importe es válido (mayor a 0 y con máximo 2 decimales)
        /// </summary>
        public static bool ImporteValido(decimal importe)
        {
            return importe > 0 && decimal.Round(importe, 2) == importe;
        }
        
        /// <summary>
        /// Verifica si debe aplicarse el costo por movimiento
        /// </summary>
        public static bool DebeAplicarCostoPorMovimiento(int numeroMovimientos)
        {
            return numeroMovimientos >= CostosConstants.OPERACIONES_GRATUITAS;
        }
        
        /// <summary>
        /// Calcula el ITF sobre un importe
        /// </summary>
        public static decimal CalcularITF(decimal importe)
        {
            return decimal.Round(importe * CostosConstants.TASA_ITF, 2);
        }
        
        /// <summary>
        /// Obtiene el costo por movimiento según la moneda
        /// </summary>
        public static decimal ObtenerCostoPorMovimiento(string codigoMoneda)
        {
            if (codigoMoneda == CostosConstants.MONEDA_SOLES)
                return CostosConstants.COSTO_MOVIMIENTO_SOLES;
            else if (codigoMoneda == CostosConstants.MONEDA_DOLARES)
                return CostosConstants.COSTO_MOVIMIENTO_DOLARES;
            else
                return 0m;
        }
        
        /// <summary>
        /// Calcula el costo total de una transacción (ITF + costo por movimiento si aplica)
        /// </summary>
        public static decimal CalcularCostoTotal(decimal importe, string codigoMoneda, int numeroMovimientos)
        {
            decimal itf = CalcularITF(importe);
            decimal costoPorMovimiento = DebeAplicarCostoPorMovimiento(numeroMovimientos) 
                ? ObtenerCostoPorMovimiento(codigoMoneda) 
                : 0m;
            
            return itf + costoPorMovimiento;
        }
    }
}
