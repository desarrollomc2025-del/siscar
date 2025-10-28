using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace conAnaRiesgosContabilidad.Servicios
{
    internal class GeneraContabilidad
    {
        private static void Genera(string fechaInicial, string fechaFinal, string cooperativa)
        {
            CultureInfo provider = CultureInfo.CurrentUICulture;

            string empresa_de = "2";
            string empresa_a = "28";

            DateTime.Parse($"{fechaInicial.Substring(0, 4)}/{fechaInicial.Substring(4, 2)}/{fechaInicial.Substring(6, 2)}").AddDays(1).ToString();
            DateTime fecha_final = DateTime.Parse($"{fechaInicial.Substring(0, 4)}/{fechaInicial.Substring(4, 2)}/{fechaInicial.Substring(6, 2)}");
            while (fecha_final <= DateTime.Parse($"{fechaFinal.Substring(0, 4)}/{fechaFinal.Substring(4, 2)}/{fechaFinal.Substring(6, 2)}"))
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString))
                {
                    connection.Open();

                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "[dbo].[PROC_CT_CONCUENTASCONSALDAF_SISCAR]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@cooperativa",
                            Value = cooperativa
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@FechaFinal",
                            Value = string.Format("{0:yyyyMMdd}", fecha_final)
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@CuentaInicial",
                            Value = string.Format("{0}", 1)
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@CuentaFinal",
                            Value = string.Format("{0}", 9999999999)
                        });
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@Nivel",
                            Value = string.Format("{0}", 10)
                        });

                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error Proceso General " + ex.Message);
                    }
                    connection.Close();
                }
                fecha_final = fecha_final.AddDays(1);
            }
        }
        private static void Con_BorraContabilidad(string fechaInicial, string fechaFinal, string cooperativa)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    Console.WriteLine("Con_fn_ObtenerParametrosConPeriodos Error");
                    throw new Exception("Con_fn_ObtenerParametrosConPeriodos.error [No se pudo establecer conexion con la base de datos]");
                }

                Oconexion.Query("[dbo].[PROC_CONTA_DEL_SISCAR]", new { codigocooperativa = $"{cooperativa.Trim()}", FechaInicial = fechaInicial, FechaFinal = fechaFinal }, commandType: CommandType.StoredProcedure);
            }
        }

        public static void Load(string cooperativa, string fechaInicial, string fechaFinal)
        {
            Con_BorraContabilidad(fechaInicial, fechaFinal,cooperativa);
            Genera(fechaInicial, fechaFinal, cooperativa);
        }
    }
}


