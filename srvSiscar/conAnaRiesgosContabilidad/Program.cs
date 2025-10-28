using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using conAnaRiesgosContabilidad.Servicios;
using System.Diagnostics;
using System.IO;

namespace conAnaRiesgosContabilidad
{
    internal class Program
    {
        public static string Con_fn_ObtenerParametrosCierreContabilidadBKW(string sdbconexion, string cooperativa, string speriodo)
        {
            using (OracleConnection Oconexion = new OracleConnection(ConfigurationManager.ConnectionStrings[sdbconexion].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CProceso88.Con_fn_ObtenerParametrosCierreContabilidadBKW Error" + ex.Message);
                    throw new Exception("Con_fn_ObtenerParametrosCierreContabilidadBKW.error [No se pudo establecer conexion con la base de datos]");
                }
                string estado = ConfigurationManager.AppSettings["Estado"].ToString();
                OracleCommand cmd = Oconexion.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE CONPERIODOS SET CONFLGANARIE = 'N' WHERE CONEMPRESA = " + cooperativa + " AND CONPERIODO = '" + speriodo + "'  AND CONESTADO = '" + estado + "'";
                Console.WriteLine(cmd.CommandText);
                Console.ReadKey();

                cmd.ExecuteNonQuery();

                return "S";
            }
        }
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
        public static CierreNav Con_fn_ObtenerParametrosConPeriodos(string sdbconexion, string speriodo)
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

                return Oconexion.QueryFirstOrDefault<CierreNav>("[MicoopeReportesNav].[dbo].[PROC_CONPERIODOS_SEL]", new { Cooperativa = $"1{sdbconexion.Substring(4, 2).Trim()}", Periodo = speriodo }, commandType: CommandType.StoredProcedure);
            }
        }
        public static Cooperativa Con_fn_ObtenerSistemaCooperativas(string sdbconexion)
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

                return Oconexion.QueryFirstOrDefault<Cooperativa>("[MicoopeReportesNav].[dbo].[proc_Sistema_Cooperativa]", new { idCooperativa = $"1{sdbconexion.Substring(4, 2).Trim()}" }, commandType: CommandType.StoredProcedure);
            }
        }
        public static void ActualizaPeriodo(string sdbconexion, string periodo, string conflganarieConta, string conflganarieAuxiliares, string conflganarieCatalogos)
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

                Oconexion.Execute("[MicoopeReportesNav].[dbo].[PROC_CONPERIODOS_UPD]", new { ConEmpresa = $"1{sdbconexion.Substring(4, 2).Trim()}", ConPeriodo = periodo, conflganarieConta = conflganarieConta, conflganarieAuxiliares = conflganarieAuxiliares, conflganarieCatalogos = conflganarieCatalogos }, commandType: CommandType.StoredProcedure);
            }
        }
        public static void ActualizaPeriodo_especifica(string sdbconexion, string periodo, string conflganarieConta, string conflganarieAuxiliares, string conflganarieCatalogos)
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
                string query= $"update [MicoopeReportesNav].[dbo].[conperiodos] set [conflganarieConta] = '{conflganarieConta}' WHERE [conperiodo] = '{periodo}' AND [conempresa] = 1{sdbconexion.Substring(4, 2).Trim()}";
                Oconexion.Execute(query,null, commandType: CommandType.Text);
            }
        }

        static void Main(string[] args)
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(0).AddDays(-1);
            //date = DateTime.Parse("30/06/2024");
            using (StreamWriter sw = new StreamWriter(@"C:\Servicios\SISCAR\install\Contables\" + $"LogSISCAR-{DateTime.Now:yyyyMM}-{date:yyyyMM}.log"))
            {
                string periodo = $"{date:yyyyMM}"; 
                string carpeta = string.Empty;
                string[] listacoopes = ConfigurationManager.AppSettings["ListaCooperativas"].Split(',');
                foreach (string s in listacoopes)
                {
                    Console.WriteLine($"Cooperativa {s}");
                    switch (int.Parse(s))
                    {
                        case 2:
                            carpeta = "02-Guayacan";
                            break;
                        case 4:
                            carpeta = "04-Coosajo";
                            break;
                        case 5:
                            carpeta = "05-UnionPopular";
                            break;
                        case 6:
                            carpeta = "06-UPA";
                            break;
                        case 7:
                            carpeta = "07-Gualan";
                            break;
                        case 8:
                            carpeta = "08-Coban";
                            break;
                        case 9:
                            carpeta = "09-Teculutan";
                            break;
                        case 10:
                            carpeta = "10-Guadalupana";
                            break;
                        case 11:
                            carpeta = "11-Tonantel";
                            break;
                        case 12:
                            carpeta = "12-Horizontes";
                            break;
                        case 13:
                            carpeta = "13-Copecom";
                            break;
                        case 14:
                            carpeta = "14-Bienestar";
                            break;
                        case 15:
                            carpeta = "15-Moyutan";
                            break;
                        case 16:
                            carpeta = "16-Chiquimulja";
                            break;
                        case 17:
                            carpeta = "17-Cosami";
                            break;
                        case 18:
                            carpeta = "18-Salcaja";
                            break;
                        case 19:
                            carpeta = "19-Acredicom";
                            break;
                        case 20:
                            carpeta = "20-Colua";
                            break;
                        case 21:
                            carpeta = "21-Coosanjer";
                            break;
                        case 22:
                            carpeta = "22-Coopsama";
                            break;
                        case 23:
                            carpeta = "23-Soloma";
                            break;
                        case 24:
                            carpeta = "24-Encarnacion";
                            break;
                        case 25:
                            carpeta = "25-Ecosaba";
                            break;
                        case 26:
                            carpeta = "26-YamanKutz";
                            break;
                        case 27:
                            carpeta = "27-Cotoneb";
                            break;
                        default:
                            carpeta = null;
                            break;
                    }

                    var FechaCierre = Con_fn_ObtenerParametrosCierreContabilidadNav($"BANK{s}");
                    var SistemaCooperativa = Con_fn_ObtenerSistemaCooperativas($"BANK{s}");

                    if (int.Parse(FechaCierre.FechaContabilidad) >= int.Parse($"{date.AddDays(1):yyyyMMdd}"))
                    {var ParametrosNav = Con_fn_ObtenerParametrosConPeriodos($"BANK{s}", periodo);
                        if (ParametrosNav.conestado == "C" && ParametrosNav.conflganarieConta != "S")
                        {
                            string sCoope = $"BANK{s}";
                            string sFecha = $"{date:yyyyMMdd}";
                            string sCarpeta = carpeta;
                            string DFecha = $"{date:yyyyMMdd}";
                            try
                            {
                                try
                                {
                                    string genera = ConfigurationManager.AppSettings["GeneraConta"];
                                    if (genera=="true")
                                    {
                                        GeneraContabilidad.Load(s, $"{periodo}01", $"{date:yyyyMMdd}");
                                    }                                    
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error Genera Contabilidad {e.Message}");
                                }
                                try
                                {
                                    C16BalanceSaldosSQL.Load(sCoope, sFecha, sCarpeta, string.Format("{0:yyyyMMdd}", DFecha));
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error Genera Balance de Saldos {e.Message}");
                                }
                                
                                try
                                {
                                    C22Liquidez90diasSQL.Load(sCoope, sFecha, sCarpeta, DFecha);
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error Genera Liquidez Promedio {e.Message}");
                                }

                                //hugo
                                try
                                {
                                    if (SistemaCooperativa.Sistema == "BKW")
                                    {
                                        C23AhorroCorriente.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                    }
                                    else
                                    {
                                        C23AhorroCorriente.LoadT24(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                    }
                                    
                                    
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error Genera Ahorro Corriente {e.Message}");
                                }

                                //hugo
                                try
                                {
                                    if(SistemaCooperativa.Sistema=="BKW")
                                    {
                                        C24PlazoFijo.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                    }
                                    else
                                    {
                                        C24PlazoFijo.LoadT24(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                    }
                                    
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error Genera Plazo Fijo {e.Message}");
                                }

                                //hugo
                                try
                                {
                                    if(SistemaCooperativa.Sistema=="BKW")
                                    {
                                        C25Aportaciones.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                    }
                                    else 
                                    {
                                        C25Aportaciones.LoadT24(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                    }
                                    
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error Genera Aportaciones {e.Message}");
                                }

                            }
                            catch (Exception ex)
                            {
                                sw.WriteLine($"Error Genera Archivos Contables {ex.Message}");
                            }
                            try
                            {
                                ActualizaPeriodo_especifica($"BANK{s}", $"{date:yyyyMM}", "S", "S", "S");
                            }
                            catch (Exception ex)
                            {
                                sw.WriteLine($"Error Actualiza Periodo {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
    }
}
