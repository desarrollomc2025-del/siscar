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
using System.Diagnostics;
using conAnaRiesgosAuxiliares.Servicios;
using System.IO;

namespace conAnaRiesgosAuxiliares
{
    internal class Program
    {
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
        public static void ActualizaPeriodo(string sdbconexion, string periodo, string conflganarieConta , string conflganarieAuxiliares, string conflganarieCatalogos)
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
        public static void ActualizaPeriodo_Catalogo(string sdbconexion, string periodo, string conflganarieConta, string conflganarieAuxiliares, string conflganarieCatalogos)
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
                string query = $"update [MicoopeReportesNav].[dbo].[conperiodos] set [conflganarieCatalogos] = '{conflganarieCatalogos}' WHERE [conperiodo] = '{periodo}'";
                Oconexion.Execute(query, null, commandType: CommandType.Text);
            }
        }
        public static void ActualizaPeriodo_Auxiliar(string sdbconexion, string periodo, string conflganarieConta, string conflganarieAuxiliares, string conflganarieCatalogos)
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
                string query = $"update [MicoopeReportesNav].[dbo].[conperiodos] set [conflganarieAuxiliares] = '{conflganarieAuxiliares}' WHERE [conperiodo] = '{periodo}' AND [conempresa] = 1{sdbconexion.Substring(4, 2).Trim()}";
                Oconexion.Execute(query, null, commandType: CommandType.Text);
            }
        }
        static void Main(string[] args)
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(0).AddDays(-1);
            

            using (StreamWriter sw = new StreamWriter(@"C:\Servicios\SISCAR\install\Auxiliares\"+ $"LogSISCAR-{DateTime.Now:yyyyMM}-{date:yyyyMM}.log"))
            {
                string periodo = $"{date:yyyyMM}";
                string carpeta = string.Empty;
                string[] listacoopes = ConfigurationManager.AppSettings["ListaCooperativas"].Split(',');
                foreach (string s in listacoopes)
                {
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
                    var Sistema = Con_fn_ObtenerSistemaCooperativas($"BANK{s}");
                    var ParametrosNav = Con_fn_ObtenerParametrosConPeriodos($"BANK{s}", periodo);

                    

                    if (ParametrosNav.conflganarieCatalogos != "S")
                    {
                        Console.WriteLine($"Inicia Generacion de Archivos Cooperativa {s}");
                        try
                        {
                            Console.WriteLine($"Inicia Generacion de Archivos Catalogos Cooperativa {s}");
                            C01Entidades.Load($"BANK{s}", $"{date:yyyyMM}");
                            C04FinDeptos.Load($"BANK{s}", $"{date:yyyyMM}");
                            C05FinCiudades.Load($"BANK{s}", $"{date:yyyyMM}");
                            C06FinBancos.Load($"BANK{s}", $"{date:yyyyMM}");
                            C08LineaCredito.Load($"BANK{s}", $"{date:yyyyMM}");
                            C09TiposGarantias.Load($"BANK{s}", $"{date:yyyyMM}");
                            C10FinMonedas.Load($"BANK{s}", $"{date:yyyyMM}");
                            C11ColFrecuePag.Load($"BANK{s}", $"{date:yyyyMM}");
                            C12ConCuentasconSQL.Load($"BANK{s}", $"{date:yyyyMM}");
                            ActualizaPeriodo_Catalogo("BANK00", $"{date:yyyyMM}", "N", "N", "S");
                            //Console.WriteLine($"Finaliza Generacion de Archivos Catalogos Cooperativa {s}");
                            C19TarjetaCreditoSQLNew.LoadREPORTE_XF($"BANK{s}", $"{date:yyyyMMdd}");
                            C19TarjetaCreditoSQLNew.LoadCICLO_DE_VIDA($"BANK{s}", $"{date:yyyyMMdd}");
                        }
                        catch (Exception ex)
                        {
                            sw.WriteLine($"Error Catalogos {ex.Message}");
                        }
                    }
                    if (ParametrosNav.conflganarieAuxiliares != "S")
                    {
                        Console.WriteLine($"Inicia Generacion de Archivos Auxiliares Cooperativa {s}");
                        try
                        {
                            string sCoope = $"BANK{s}";
                            string sFecha = $"{date:yyyyMMdd}";
                            string sCarpeta = carpeta;
                            string DFecha = $"{date:yyyyMMdd}";
                            
                            string generaBancos = ConfigurationManager.AppSettings["GeneraBancos"];
                            
                            if(generaBancos=="true")
                            {
                                try
                                {
                                    GeneraBancos.Load(sCoope, $"{date:yyyyMM}01", $"{date:yyyyMMdd}");
                                }
                                catch (Exception ex) 
                                {
                                    sw.WriteLine($"Error Genera Bancos {ex.Message}");
                                }
                            }
                            if(Sistema.Sistema=="BKW")
                            {
                                try
                                {
                                    C13AreaFinanciera.LoadBkw(sCoope, sFecha, sCarpeta);
                                }
                                catch (Exception af)
                                {
                                    sw.WriteLine($"Error AreaFinanciera {af.Message}");
                                }

                                try
                                {
                                    C14ColInstrumentos.Load(sCoope, sFecha, sCarpeta);
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error ColInstrumentos {e.Message}");
                                }

                                try
                                {
                                    C15CapInstrumentos.Load(sCoope, sFecha, sCarpeta);
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error CapInstrumentos {e.Message}");
                                }

                                //hugo con validacion por migracion
                                try
                                {
                                    C17CapcuentasSald.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error CapcuentasSald {e.Message}");
                                }
                                //hugo con validacion por migracion
                                try
                                {
                                    C18ColDocumesald.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error Coldocumesald {e.Message}");
                                }
                                //hugo con validacion por migracion
                                try
                                {
                                    C27CapcuentasSaldenc.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error CapcuentasSaldEncaje {e.Message}");
                                }
                            }
                            else
                            {
                                
                                try
                                {
                                    C13AreaFinanciera.LoadT24(sCoope, sFecha, sCarpeta);
                                }
                                catch (Exception af)
                                {
                                    sw.WriteLine($"Error AreaFinanciera {af.Message}");
                                }

                                try
                                {
                                    C14ColInstrumentos.LoadT24(sCoope, sFecha, sCarpeta);
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error ColInstrumentos {e.Message}");
                                }

                                try
                                {
                                    C15CapInstrumentos.LoadT24(sCoope, sFecha, sCarpeta);
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error CapInstrumentos {e.Message}");
                                }
                                
                                try
                                {
                                    C18ColDocumesald.LoadT24(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error CapInstrumentos {e.Message}");
                                }
                                
                                try
                                {
                                    C27CapcuentasSaldenc.LoadT24(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                }
                                catch (Exception e)
                                {
                                    sw.WriteLine($"Error CapInstrumentos {e.Message}");
                                }
                            }
                            
                            try
                            {
                                C19TarjetaCreditoSQL.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                                C19TarjetaCreditoSQLNew.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                            }
                            catch (Exception tc)
                            {
                                sw.WriteLine($"Error TarjetaDeCredito {tc.Message}");
                            }
                            try
                            {
                                C20InversionesSQL.Load(sCoope, sFecha, sCarpeta, string.Format("{0:yyyyMMdd}", DFecha));
                            }
                            catch (Exception In)
                            {
                                sw.WriteLine($"Error Inversiones {In.Message}");
                            }
                            
                            try
                            {
                                C21BancosSQL.Load(sCoope, sFecha, sCarpeta, DFecha);
                            }
                            catch (Exception ba)
                            {
                                sw.WriteLine($"Error Bancos {ba.Message}");
                            }
                            
                            try
                            {
                               C26DetalleControl.Load(sCoope, sFecha, sCarpeta, string.Format("{0:ddMMyyyy}", DFecha));
                            }
                            catch (Exception e)
                            {
                                sw.WriteLine($"Error DetalleControl {e.Message}");
                            }
                            
                            try
                            {
                                ActualizaPeriodo_Auxiliar($"BANK{s}", $"{date:yyyyMM}", "N", "S", "S");
                            }
                            catch (Exception ex)
                            {
                                sw.WriteLine($"Error Actualiza Periodo {ex.Message}");
                            }
                            Console.WriteLine($"Finaliza Generacion de Archivos Auxiliares Cooperativa {s}");
                        }
                        catch (Exception ex)
                        {
                            sw.WriteLine($"Error Archivos Auxilialres {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
