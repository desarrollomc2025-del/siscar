using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using Dapper;
using System.Data.SqlClient;

namespace conAnaRiesgosAuxiliares
{
    public class C13AreaFinanciera
    {
        private static void GeneraBkw(string sdbconexion, string sfecha, string scarpeta)
        {
            using (OracleConnection Oconexion = new OracleConnection(ConfigurationManager.ConnectionStrings[sdbconexion].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C13AreaFinanciera.error [No se pudo establecer conexion con la base de datos]");
                }
                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prcfinareafin";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("CURSOR_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCAgen_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    //////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sLinea = reader["fincodareafin"].ToString().Trim() + "|" +
                                            reader["findesareafin"].ToString().Trim() + "|" +
                                            reader["findepareafin"].ToString().Trim() + "|" +
                                            reader["finciuareafin"].ToString().Trim() + "|" +
                                            reader["fincodempresa"].ToString().Trim();
                                sw.WriteLine(sLinea);
                            }
                        }
                    }
                    
                    string hostIp = ConfigurationManager.AppSettings["HostFTP"].ToString();
                    string userFtp = ConfigurationManager.AppSettings["UserFTP"].ToString();
                    string passwordFtp = ConfigurationManager.AppSettings["ClaveFTP"].ToString();
                    //CFtpTraslada ftpTraslada = new CFtpTraslada(hostIp, userFtp, passwordFtp);
                    //var resp = ftpTraslada.upload(sfile, ConfigurationManager.AppSettings["Ruta"].ToString() + sfile);
                    string resp = "1";
                    if (resp == "1")
                    {
                        string sDirectoryCarga = ConfigurationManager.AppSettings["RutaDestino"];
                        File.Copy(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, sDirectoryCarga + sfile, true);
                    }
                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C13AreaFinanciera Error {0} Conexion{1} ",ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        private static void GeneraT24(string sdbconexion, string sfecha, string scarpeta)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionAnalytics"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C13AreaFinanciera.error [No se pudo establecer conexion con la base de datos]");
                }
                try
                {
                    string query = "[AnalyticsImport].[dbo].[PROC_ARCH_DCAGEN_NEW]";
                    var resultado = Oconexion.Query(query, new {fecha=sfecha,cooperativa=sdbconexion.Substring(4,2)}, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C13AreaFinanciera Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void LoadBkw(string conexion, string sfecha, string scarpeta)
        {
            GeneraBkw(conexion, sfecha, scarpeta);
        }
        public static void LoadT24(string conexion, string sfecha, string scarpeta)
        {
            GeneraT24(conexion, sfecha, scarpeta);
        }
    }
}
