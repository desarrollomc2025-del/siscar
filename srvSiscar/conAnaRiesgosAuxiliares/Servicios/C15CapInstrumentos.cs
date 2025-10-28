using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;
using Dapper;

namespace conAnaRiesgosAuxiliares
{
    public class C15CapInstrumentos
    {
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
                    throw new Exception("C15InstrumentoCapta.error [No se pudo establecer conexion con la base de datos]");
                }
                try
                {
                    string query = "[AnalyticsImport].[dbo].[PROC_ARCH_DCINCA_NEW]";
                    var resultado = Oconexion.Query(query, new { fecha = DateTime.Parse($"{sfecha.Substring(0, 4)}-{sfecha.Substring(4, 2)}-{sfecha.Substring(6, 2)}"), cooperativa = sdbconexion.Substring(4, 2) }, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C15InstrumentoCapta Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        private static void Genera(string sdbconexion, string sfecha, string scarpeta)
        {
            using (OracleConnection Oconexion = new OracleConnection(ConfigurationManager.ConnectionStrings[sdbconexion].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C15CapInstrumentos.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prccapinstrument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("CURSOR_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCInCa_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    //////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sLinea = reader["capcodinstrum"].ToString().Trim() + "|" +
                                            sdbconexion.Substring(4, 2).Trim() + "|" +
                                            reader["capdesinstrum"].ToString().Trim() + "|" +
                                            reader["capcodmoneda"].ToString().Trim() + "|" +
                                            reader["capclasifinst"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C15CapInstrumentos Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta)
        {
            Genera(conexion, sfecha, scarpeta);
        }
        public static void LoadT24(string conexion, string sfecha, string scarpeta)
        {
            GeneraT24(conexion, sfecha, scarpeta);
        }
    }
}
