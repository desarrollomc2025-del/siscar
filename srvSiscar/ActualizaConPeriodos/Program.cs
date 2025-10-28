using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Dapper;
using System.Threading;

namespace ActualizaConPeriodos
{
    internal class Program
    {
        public static FechaCierreNav Con_fn_ObtenerParametrosCierreContabilidadNav(string sdbconexion)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    Console.WriteLine("Con_fn_ObtenerParametrosCierreContabilidadNav Error");
                    throw new Exception("Con_fn_ObtenerParametrosCierreContabilidadNav.error [No se pudo establecer conexion con la base de datos]");
                }

                return Oconexion.QueryFirstOrDefault<FechaCierreNav>("[MicoopeReportesNav].[dbo].[PROC_FECHA_CONTA]", new { Cooperativa = $"1{sdbconexion.Substring(4, 2).Trim()}" }, commandType: CommandType.StoredProcedure);
            }
        }
        public static void UpdateConPeriodos(string sdbconexion)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    Console.WriteLine("Con_fn_ObtenerParametrosCierreContabilidadNav Error");
                    throw new Exception("Con_fn_ObtenerParametrosCierreContabilidadNav.error [No se pudo establecer conexion con la base de datos]");
                }

                Oconexion.QueryAsync("[MicoopeReportesNav].[dbo].[PROC_FECHA_CONTA]", new { Cooperativa = $"1{sdbconexion.Substring(4, 2).Trim()}" }, commandType: CommandType.StoredProcedure);
            }
        }
        static void Main(string[] args)
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(0).AddDays(-1);

            string[] ListaCooperativas = ConfigurationManager.AppSettings["ListaCooperativas"].Split(',');
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    Console.WriteLine("Con_fn_ObtenerParametrosCierreContabilidadNav Error");
                    throw new Exception("Con_fn_ObtenerParametrosCierreContabilidadNav.error [No se pudo establecer conexion con la base de datos]");
                }

                int espera = int.Parse(ConfigurationManager.AppSettings["Espera"]);
                int repeticiones = int.Parse(ConfigurationManager.AppSettings["Repeticiones"]);
                for (int i=0; i<=repeticiones; i++)
                {
                    Console.WriteLine($"Iteracion No. {i}");
                    foreach (var s in ListaCooperativas)
                    {
                        Console.WriteLine($"Cooperativa No. {s}");
                        var FechaCierre = Con_fn_ObtenerParametrosCierreContabilidadNav($"BANK{s}");
                        Console.WriteLine($"FechaCierre {FechaCierre.FechaContabilidad}");
                        if (int.Parse(FechaCierre.FechaContabilidad) >= int.Parse($"{date.AddDays(1):yyyyMMdd}"))
                        {
                            string query = $"update conperiodos set conestado = 'C', confechcierre = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}' where conempresa = 1{s} and conperiodo = {date:yyyyMM} and conestado = 'A'";
                            Oconexion.Query(query, null, commandTimeout: 180, commandType: CommandType.Text);
                        }
                    }
                    Thread.Sleep(espera * 60 * 1000);
                }
            }
        }
    }
}
