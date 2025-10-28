using conAnaRiesgosAuxiliares.Servicios;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conAnaRiesgosAuxiliares
{
    public class C19TarjetaCreditoSQLNew
    {
        private static void Genera(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQLTC"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C19TarjetaCreditoSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[TCreditoMicoope].[dbo].[PROC_SISCAR_DATOS_TARJETAS_SL]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 1000;
                    //EventLog.WriteEntry("SISCARDatosCooperativa ", "Empresa Tarjeta Credito "+sdbconexion.Substring(4, 2).Trim(), //EventLogEntryType.Warning, 234);
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@fecha",
                        Value = string.Format("{0:yyyyMMdd}", sfechac) //sfechac //.ToString("yyyyMMdd")
                    });

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@CodigoEmpresa",
                        Value = int.Parse(sdbconexion.Substring(4, 2).Trim())
                    });

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCTarj_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    ////EventLog.WriteEntry("SISCARdb19", $"Ruta Archivo {ConfigurationManager.AppSettings["Ruta"].ToString()}{sfile}", //EventLogEntryType.Information, 234);
                    string periodo = sfecha.Substring(0, 6);
                    string modulo = "TC";
                    string empresa = int.Parse(sdbconexion.Substring(4, 2).Trim()).ToString();
                    int conteo = 0;
                    decimal total = 0;

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (SqlDataReader dtr = cmd.ExecuteReader())
                        {
                            while (dtr.Read())
                            {
                                string[] lineas = dtr["linea"].ToString().Split('|');
                                periodo = lineas[2].ToString();
                                empresa = lineas[0].ToString();
                                conteo++;
                                total = total + decimal.Parse(lineas[3].ToString().Trim());
                                sLinea = dtr["linea"].ToString().Trim();
                                sw.WriteLine(sLinea);
                            }
                        }
                    }
                    string hostIp = ConfigurationManager.AppSettings["HostFTP"].ToString();
                    string userFtp = ConfigurationManager.AppSettings["UserFTP"].ToString();
                    string passwordFtp = ConfigurationManager.AppSettings["ClaveFTP"].ToString();
                    CFtpTraslada ftpTraslada = new CFtpTraslada(hostIp, userFtp, passwordFtp);
                    //var resp = ftpTraslada.upload(sfile, ConfigurationManager.AppSettings["Ruta"].ToString() + sfile);
                    string resp = "1";
                    if (resp == "1")
                    {
                        string sDirectoryCarga = ConfigurationManager.AppSettings["RutaDestino"];
                        File.Copy(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, sDirectoryCarga + sfile, true);
                    }
                    Verificador.Load(periodo,modulo,empresa,conteo,total);
                }
                catch (Exception ex)
                {
                    ////EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C19TarjetaCreditoSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        private static async Task GeneraTarjetaCreditoNew(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQLTC"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C19TarjetaCreditoSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    string query = "[AnalyticsImport].[dbo].[PROC_TAR_SISCAR]";
                    string modulo = "TC";
                    string empresa = int.Parse(sdbconexion.Substring(4, 2).Trim()).ToString();
                    int conteo = 0;
                    decimal total = 0;
                    Oconexion.Query(query, new { dfecha = sfechac, coope = $"A{sdbconexion.Substring(4, 2)}" }, commandTimeout: 300, commandType: CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    ////EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C19TarjetaCreditoSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        //private static void GeneraTarjetaCreditoICLO_DE_VIDA_RE(string sdbconexion, string sfechac)
        private static async Task GeneraTarjetaCreditoICLO_DE_VIDA_RE(string sdbconexion, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQLTC"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C19TarjetaCreditoSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    string query = "[TCreditoMicoope].[dbo].[PR_TAR_CICLO_DE_VIDA_RE]";
                    string empresa = int.Parse(sdbconexion.Substring(4, 2).Trim()).ToString();
                    int conteo = 0;
                    decimal total = 0;
                    Oconexion.Query(query, new { fecha1 = sfechac }, commandTimeout: 300, commandType: CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    ////EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C19TarjetaCreditoSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        private static async Task GeneraTarjetaCreditoREPORTE_XF_RE(string sdbconexion, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQLTC"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C19TarjetaCreditoSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    string query = "[TCreditoMicoope].[dbo].[PR_TAR_REPORTE_XF_RE]";

                    string modulo = "TC";
                    string empresa = int.Parse(sdbconexion.Substring(4, 2).Trim()).ToString();
                    int conteo = 0;
                    decimal total = 0;
                    Oconexion.QueryAsync(query, new {fecha = $"{sfechac.Substring(0,4)}-{sfechac.Substring(4,2)}-{sfechac.Substring(6,2)}" }, commandTimeout: 300, commandType: CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    ////EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C19TarjetaCreditoSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static async Task Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            await GeneraTarjetaCreditoNew(conexion,sfecha,scarpeta, sfechac);
        }
        public static async Task LoadREPORTE_XF(string conexion, string sfechac)
        {
            await GeneraTarjetaCreditoREPORTE_XF_RE(conexion, sfechac);
        }
        public static async Task LoadCICLO_DE_VIDA(string conexion, string sfechac)
        {
            await GeneraTarjetaCreditoICLO_DE_VIDA_RE(conexion, sfechac);
        }
    }
}
