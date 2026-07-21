using System;
using System.Data.SqlClient;
using EurekaBank_RestFull_DotNet_GR05.DAL;
using EurekaBank_RestFull_DotNet_GR05.Models;
using EurekaBank_RestFull_DotNet_GR05.Models.DTOs;
using EurekaBank_RestFull_DotNet_GR05.Validators;
using EurekaBank_RestFull_DotNet_GR05.Constants;

namespace EurekaBank_RestFull_DotNet_GR05.Services
{
    /// <summary>
    /// Servicio de l�gica de negocio para transacciones bancarias
    /// </summary>
    public class TransaccionService
    {
        private readonly CuentaDAO cuentaDAO;
        private readonly MovimientoDAO movimientoDAO;

        public TransaccionService()
        {
            cuentaDAO = new CuentaDAO();
            movimientoDAO = new MovimientoDAO();
        }

        /// <summary>
        /// Realiza un dep�sito en una cuenta
        /// </summary>
        public RespuestaDTO RealizarDeposito(TransaccionDTO datos)
        {
            try
            {
                // 1. Validar importe
                if (!TransaccionValidator.ImporteValido(datos.Importe))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_IMPORTE_INVALIDO,
                        CodigoError = "VAL005"
                    };
                }

                // 2. Obtener cuenta
                Cuenta cuenta = cuentaDAO.ObtenerPorCodigo(datos.CodigoCuenta);
                if (!CuentaValidator.Existe(cuenta))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CUENTA_NO_EXISTE,
                        CodigoError = "CTA001"
                    };
                }

                // 3. Validar que la cuenta est� activa
                if (!CuentaValidator.EsActiva(cuenta))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CUENTA_INACTIVA,
                        CodigoError = "CTA002"
                    };
                }

                // 4. Generar n�mero de movimiento
                int numeroMovimiento = movimientoDAO.ObtenerUltimoNumero(datos.CodigoCuenta) + 1;

                // 5. Crear movimiento de dep�sito
                Movimiento movimiento = new Movimiento
                {
                    CodigoCuenta = datos.CodigoCuenta,
                    Numero = numeroMovimiento,
                    Fecha = DateTime.Now,
                    CodigoEmpleado = datos.CodigoEmpleado,
                    CodigoTipo = TipoMovimientoConstants.DEPOSITO,
                    Importe = datos.Importe,
                    CuentaReferencia = null
                };

                // 6. Insertar movimiento
                bool movimientoInsertado = movimientoDAO.Insertar(movimiento);
                if (!movimientoInsertado)
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_OPERACION_FALLIDA,
                        CodigoError = "DB002"
                    };
                }

                // 7. Actualizar saldo
                decimal nuevoSaldo = cuenta.Saldo + datos.Importe;
                bool saldoActualizado = cuentaDAO.ActualizarSaldo(datos.CodigoCuenta, nuevoSaldo);
                
                // 8. Incrementar contador de movimientos
                cuentaDAO.IncrementarContadorMovimientos(datos.CodigoCuenta);

                return new RespuestaDTO
                {
                    Exitoso = true,
                    Mensaje = MensajesConstants.DEPOSITO_EXITOSO,
                    Datos = new DepositoResultDTO
                    {
                        NumeroMovimiento = numeroMovimiento,
                        SaldoAnterior = cuenta.Saldo,
                        SaldoNuevo = nuevoSaldo,
                        Importe = datos.Importe
                    }
                };
            }
            catch (Exception ex)
            {
                return new RespuestaDTO
                {
                    Exitoso = false,
                    Mensaje = $"{MensajesConstants.ERROR_BASE_DATOS}: {ex.Message}",
                    CodigoError = "DB001"
                };
            }
        }

        /// <summary>
        /// Realiza un retiro de una cuenta
        /// </summary>
        public RespuestaDTO RealizarRetiro(TransaccionDTO datos)
        {
            SqlConnection conn = null;
            SqlTransaction transaction = null;
            
            try
            {
                // 1-6. Validaciones (igual que antes)
                // ...existing validation code...
                
                if (!TransaccionValidator.ImporteValido(datos.Importe))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_IMPORTE_INVALIDO,
                        CodigoError = "VAL005"
                    };
                }

                Cuenta cuenta = cuentaDAO.ObtenerPorCodigo(datos.CodigoCuenta);
                if (!CuentaValidator.Existe(cuenta))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CUENTA_NO_EXISTE,
                        CodigoError = "CTA001"
                    };
                }

                if (!CuentaValidator.EsActiva(cuenta))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CUENTA_INACTIVA,
                        CodigoError = "CTA002"
                    };
                }

                if (!CuentaValidator.ClaveCorrecta(cuenta, datos.ClaveCuenta))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CLAVE_INCORRECTA,
                        CodigoError = "CTA003"
                    };
                }

                decimal itf = TransaccionValidator.CalcularITF(datos.Importe);
                decimal costoPorMovimiento = TransaccionValidator.DebeAplicarCostoPorMovimiento(cuenta.ContadorMovimientos)
                    ? TransaccionValidator.ObtenerCostoPorMovimiento(cuenta.CodigoMoneda)
                    : 0m;

                decimal totalADescontar = datos.Importe + itf + costoPorMovimiento;

                if (!CuentaValidator.TieneSaldoSuficiente(cuenta.Saldo, totalADescontar))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = $"{MensajesConstants.ERROR_SALDO_INSUFICIENTE}. Requiere: {totalADescontar:F2}, Disponible: {cuenta.Saldo:F2}",
                        CodigoError = "CTA004"
                    };
                }

                // 7. Usar transacción SQL
                conn = ConexionDB.ObtenerConexion();
                conn.Open();
                transaction = conn.BeginTransaction();

                try
                {
                    // USAR LA CONEXIÓN Y TRANSACCIÓN ACTUALES
                    int numeroMovimiento = movimientoDAO.ObtenerUltimoNumero(conn, transaction, datos.CodigoCuenta);

                    // Movimientos usando la misma transacción
                    numeroMovimiento++;
                    EjecutarInsertMovimiento(conn, transaction, new Movimiento
                    {
                        CodigoCuenta = datos.CodigoCuenta,
                        Numero = numeroMovimiento,
                        Fecha = DateTime.Now,
                        CodigoEmpleado = datos.CodigoEmpleado,
                        CodigoTipo = TipoMovimientoConstants.RETIRO,
                        Importe = datos.Importe,
                        CuentaReferencia = null
                    });

                    numeroMovimiento++;
                    EjecutarInsertMovimiento(conn, transaction, new Movimiento
                    {
                        CodigoCuenta = datos.CodigoCuenta,
                        Numero = numeroMovimiento,
                        Fecha = DateTime.Now,
                        CodigoEmpleado = datos.CodigoEmpleado,
                        CodigoTipo = TipoMovimientoConstants.ITF,
                        Importe = itf,
                        CuentaReferencia = null
                    });

                    if (costoPorMovimiento > 0)
                    {
                        numeroMovimiento++;
                        EjecutarInsertMovimiento(conn, transaction, new Movimiento
                        {
                            CodigoCuenta = datos.CodigoCuenta,
                            Numero = numeroMovimiento,
                            Fecha = DateTime.Now,
                            CodigoEmpleado = datos.CodigoEmpleado,
                            CodigoTipo = TipoMovimientoConstants.CARGO_MOVIMIENTO,
                            Importe = costoPorMovimiento,
                            CuentaReferencia = null
                        });
                    }

                    decimal nuevoSaldo = cuenta.Saldo - totalADescontar;
                    EjecutarUpdateSaldo(conn, transaction, datos.CodigoCuenta, nuevoSaldo);

                    int movimientosRegistrados = costoPorMovimiento > 0 ? 3 : 2;
                    for (int i = 0; i < movimientosRegistrados; i++)
                    {
                        EjecutarIncrementarContador(conn, transaction, datos.CodigoCuenta);
                    }

                    transaction.Commit();

                    return new RespuestaDTO
                    {
                        Exitoso = true,
                        Mensaje = MensajesConstants.RETIRO_EXITOSO,
                        Datos = new RetiroResultDTO
                        {
                            SaldoAnterior = cuenta.Saldo,
                            SaldoNuevo = nuevoSaldo,
                            ImporteRetiro = datos.Importe,
                            ITF = itf,
                            CostoPorMovimiento = costoPorMovimiento,
                            TotalDescontado = totalADescontar
                        }
                    };
                }
                catch (Exception)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new RespuestaDTO
                {
                    Exitoso = false,
                    Mensaje = $"{MensajesConstants.ERROR_BASE_DATOS}: {ex.Message}",
                    CodigoError = "DB001"
                };
            }
            finally
            {
                if (transaction != null)
                    transaction.Dispose();
                if (conn != null)
                    conn.Dispose();
            }
        }

        /// <summary>
        /// Realiza una transferencia entre dos cuentas
        /// </summary>
        public RespuestaDTO RealizarTransferencia(TransferenciaDTO datos)
        {
            SqlConnection conn = null;
            SqlTransaction transaction = null;
            
            try
            {
                // Validaciones
                if (!TransaccionValidator.ImporteValido(datos.Importe))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_IMPORTE_INVALIDO,
                        CodigoError = "VAL005"
                    };
                }

                Cuenta cuentaOrigen = cuentaDAO.ObtenerPorCodigo(datos.CuentaOrigen);
                Cuenta cuentaDestino = cuentaDAO.ObtenerPorCodigo(datos.CuentaDestino);

                if (!CuentaValidator.Existe(cuentaOrigen))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = "Cuenta origen no existe",
                        CodigoError = "CTA001"
                    };
                }

                if (!CuentaValidator.Existe(cuentaDestino))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = "Cuenta destino no existe",
                        CodigoError = "CTA001"
                    };
                }

                if (!CuentaValidator.EsActiva(cuentaOrigen))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = "Cuenta origen no está activa",
                        CodigoError = "CTA002"
                    };
                }

                if (!CuentaValidator.EsActiva(cuentaDestino))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = "Cuenta destino no está activa",
                        CodigoError = "CTA002"
                    };
                }

                if (!CuentaValidator.ClaveCorrecta(cuentaOrigen, datos.ClaveCuentaOrigen))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CLAVE_INCORRECTA,
                        CodigoError = "CTA003"
                    };
                }

                if (!CuentaValidator.MismaMoneda(cuentaOrigen, cuentaDestino))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_MONEDA_DIFERENTE,
                        CodigoError = "CTA005"
                    };
                }

                decimal itf = TransaccionValidator.CalcularITF(datos.Importe);
                decimal costoPorMovimiento = TransaccionValidator.DebeAplicarCostoPorMovimiento(cuentaOrigen.ContadorMovimientos)
                    ? TransaccionValidator.ObtenerCostoPorMovimiento(cuentaOrigen.CodigoMoneda)
                    : 0m;

                decimal totalADescontar = datos.Importe + itf + costoPorMovimiento;

                if (!CuentaValidator.TieneSaldoSuficiente(cuentaOrigen.Saldo, totalADescontar))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = $"{MensajesConstants.ERROR_SALDO_INSUFICIENTE}. Requiere: {totalADescontar:F2}, Disponible: {cuentaOrigen.Saldo:F2}",
                        CodigoError = "CTA004"
                    };
                }

                // Transacción SQL
                conn = ConexionDB.ObtenerConexion();
                conn.Open();
                transaction = conn.BeginTransaction();

                try
                {
                    // CUENTA ORIGEN - USAR LA CONEXIÓN Y TRANSACCIÓN ACTUALES
                    int numMovOrigen = movimientoDAO.ObtenerUltimoNumero(conn, transaction, datos.CuentaOrigen);

                    numMovOrigen++;
                    EjecutarInsertMovimiento(conn, transaction, new Movimiento
                    {
                        CodigoCuenta = datos.CuentaOrigen,
                        Numero = numMovOrigen,
                        Fecha = DateTime.Now,
                        CodigoEmpleado = datos.CodigoEmpleado,
                        CodigoTipo = TipoMovimientoConstants.TRANSFERENCIA_SALIDA,
                        Importe = datos.Importe,
                        CuentaReferencia = datos.CuentaDestino
                    });

                    numMovOrigen++;
                    EjecutarInsertMovimiento(conn, transaction, new Movimiento
                    {
                        CodigoCuenta = datos.CuentaOrigen,
                        Numero = numMovOrigen,
                        Fecha = DateTime.Now,
                        CodigoEmpleado = datos.CodigoEmpleado,
                        CodigoTipo = TipoMovimientoConstants.ITF,
                        Importe = itf,
                        CuentaReferencia = null
                    });

                    if (costoPorMovimiento > 0)
                    {
                        numMovOrigen++;
                        EjecutarInsertMovimiento(conn, transaction, new Movimiento
                        {
                            CodigoCuenta = datos.CuentaOrigen,
                            Numero = numMovOrigen,
                            Fecha = DateTime.Now,
                            CodigoEmpleado = datos.CodigoEmpleado,
                            CodigoTipo = TipoMovimientoConstants.CARGO_MOVIMIENTO,
                            Importe = costoPorMovimiento,
                            CuentaReferencia = null
                        });
                    }

                    decimal nuevoSaldoOrigen = cuentaOrigen.Saldo - totalADescontar;
                    EjecutarUpdateSaldo(conn, transaction, datos.CuentaOrigen, nuevoSaldoOrigen);

                    int movimientosOrigen = costoPorMovimiento > 0 ? 3 : 2;
                    for (int i = 0; i < movimientosOrigen; i++)
                    {
                        EjecutarIncrementarContador(conn, transaction, datos.CuentaOrigen);
                    }

                    // CUENTA DESTINO - USAR LA CONEXIÓN Y TRANSACCIÓN ACTUALES
                    int numMovDestino = movimientoDAO.ObtenerUltimoNumero(conn, transaction, datos.CuentaDestino);

                    numMovDestino++;
                    EjecutarInsertMovimiento(conn, transaction, new Movimiento
                    {
                        CodigoCuenta = datos.CuentaDestino,
                        Numero = numMovDestino,
                        Fecha = DateTime.Now,
                        CodigoEmpleado = datos.CodigoEmpleado,
                        CodigoTipo = TipoMovimientoConstants.TRANSFERENCIA_INGRESO,
                        Importe = datos.Importe,
                        CuentaReferencia = datos.CuentaOrigen
                    });

                    decimal nuevoSaldoDestino = cuentaDestino.Saldo + datos.Importe;
                    EjecutarUpdateSaldo(conn, transaction, datos.CuentaDestino, nuevoSaldoDestino);
                    EjecutarIncrementarContador(conn, transaction, datos.CuentaDestino);

                    transaction.Commit();

                    return new RespuestaDTO
                    {
                        Exitoso = true,
                        Mensaje = MensajesConstants.TRANSFERENCIA_EXITOSA,
                        Datos = new TransferenciaResultDTO
                        {
                            CuentaOrigen = new CuentaResumenDTO
                            {
                                Codigo = datos.CuentaOrigen,
                                SaldoAnterior = cuentaOrigen.Saldo,
                                SaldoNuevo = nuevoSaldoOrigen
                            },
                            CuentaDestino = new CuentaResumenDTO
                            {
                                Codigo = datos.CuentaDestino,
                                SaldoAnterior = cuentaDestino.Saldo,
                                SaldoNuevo = nuevoSaldoDestino
                            },
                            ImporteTransferido = datos.Importe,
                            ITF = itf,
                            CostoPorMovimiento = costoPorMovimiento,
                            TotalDescontado = totalADescontar
                        }
                    };
                }
                catch (Exception)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new RespuestaDTO
                {
                    Exitoso = false,
                    Mensaje = $"{MensajesConstants.ERROR_BASE_DATOS}: {ex.Message}",
                    CodigoError = "DB001"
                };
            }
            finally
            {
                if (transaction != null)
                    transaction.Dispose();
                if (conn != null)
                    conn.Dispose();
            }
        }

        // M�todos auxiliares para ejecutar comandos dentro de una transacci�n
        private void EjecutarInsertMovimiento(SqlConnection conn, SqlTransaction transaction, Movimiento movimiento)
        {
            string query = @"INSERT INTO Movimiento 
                            (chr_cuencodigo, int_movinumero, dtt_movifecha, 
                            chr_emplcodigo, chr_tipocodigo, dec_moviimporte, 
                            chr_cuenreferencia)
                            VALUES 
                            (@CodigoCuenta, @Numero, @Fecha, @CodigoEmpleado, 
                            @CodigoTipo, @Importe, @CuentaReferencia)";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@CodigoCuenta", movimiento.CodigoCuenta);
                cmd.Parameters.AddWithValue("@Numero", movimiento.Numero);
                cmd.Parameters.AddWithValue("@Fecha", movimiento.Fecha);
                cmd.Parameters.AddWithValue("@CodigoEmpleado", movimiento.CodigoEmpleado);
                cmd.Parameters.AddWithValue("@CodigoTipo", movimiento.CodigoTipo);
                cmd.Parameters.AddWithValue("@Importe", movimiento.Importe);
                cmd.Parameters.AddWithValue("@CuentaReferencia", (object)movimiento.CuentaReferencia ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        private void EjecutarUpdateSaldo(SqlConnection conn, SqlTransaction transaction, string codigoCuenta, decimal nuevoSaldo)
        {
            string query = @"UPDATE Cuenta 
                            SET dec_cuensaldo = @NuevoSaldo 
                            WHERE chr_cuencodigo = @CodigoCuenta";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@CodigoCuenta", codigoCuenta);
                cmd.Parameters.AddWithValue("@NuevoSaldo", nuevoSaldo);
                cmd.ExecuteNonQuery();
            }
        }

        private void EjecutarIncrementarContador(SqlConnection conn, SqlTransaction transaction, string codigoCuenta)
        {
            string query = @"UPDATE Cuenta 
                            SET int_cuencontmov = int_cuencontmov + 1 
                            WHERE chr_cuencodigo = @CodigoCuenta";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@CodigoCuenta", codigoCuenta);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
