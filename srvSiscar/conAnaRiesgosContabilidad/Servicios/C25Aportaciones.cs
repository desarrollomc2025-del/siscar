using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace conAnaRiesgosContabilidad
{
    public class C25Aportaciones
    {
        private static void Genera(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (OracleConnection Oconexion = new OracleConnection(ConfigurationManager.ConnectionStrings[sdbconexion].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C25Aportaciones.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prcfgcrl003varaportes";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("lnkfchactuali", OracleDbType.Varchar2).Value = $"{sfechac.Substring(6, 2)}{sfechac.Substring(4, 2)}{sfechac.Substring(0, 4)}";
                        //string.Format("{0:ddMMyyyy}", sfechac);
                    cmd.Parameters.Add("cursor_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCCaAp_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";

                    int Correlativo = 0;
                    decimal SaldoFinal = 0;
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Correlativo = Correlativo + 1;

                                SaldoFinal += (reader.GetDecimal(1) + reader.GetDecimal(2)) - reader.GetDecimal(3);

                                sLinea = string.Format("{1}{0}{2}{0}{3}{0}{4:dd/MM/yyyy}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}A", "|",
                                        sdbconexion.Substring(4, 2).Trim(),
                                        sfecha.Substring(0,6).Trim(),
                                        Correlativo,
                                        reader.GetDateTime(0),
                                        reader.GetDecimal(2),
                                        reader.GetDecimal(3),
                                        SaldoFinal);
                                sw.WriteLine(sLinea);
                            }
                        }
                    }
                    string hostIp = ConfigurationManager.AppSettings["HostFTP"].ToString();
                    string userFtp = ConfigurationManager.AppSettings["UserFTP"].ToString();
                    string passwordFtp = ConfigurationManager.AppSettings["ClaveFTP"].ToString();
                    CFtpTraslada ftpTraslada = new CFtpTraslada(hostIp, userFtp, passwordFtp);
                    var resp = "1";
                    try
                    {
                        resp = ftpTraslada.upload(sfile, ConfigurationManager.AppSettings["Ruta"].ToString() + sfile);
                    }
                    catch (Exception ex)
                    {
                        resp = "1";
                    }

                    if (resp == "1")
                    {
                        string sDirectoryCarga = ConfigurationManager.AppSettings["RutaDestino"];
                        File.Copy(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, sDirectoryCarga + sfile, true);
                    }

                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C25Aportaciones Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        private static void GeneraT24(string sdbconexion, string sfecha)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionT24"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C23AhorroCorriente.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    string query = "[dbo].[PROC_ARCH_DCCACO_NEW]";
                    string select = $"select coope, COUNT(1) as conteo, sum(debitos) as debitos, SUM(creditos) as creditos, SUM(saldo) as saldo from analyticsImport.dbo.t_archivo_DCCAAP where periodo = {sfecha.Substring(0, 6)} and   coope = {sdbconexion.Substring(4, 2)} group by coope";
                    int Correlativo = 0;
                    decimal SaldoFinal = 0;
                    Oconexion.Query(query, new { Cooperativa = $"A{sdbconexion.Substring(4, 2)}", fecha = sfecha }, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C23AhorroCorriente Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
        public static void LoadT24(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            GeneraT24(conexion, sfechac);
        }
    }
}
