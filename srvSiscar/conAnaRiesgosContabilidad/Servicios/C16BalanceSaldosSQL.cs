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

namespace conAnaRiesgosContabilidad
{
    public class C16BalanceSaldosSQL
    {
        private static void Genera(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C16BalanceSaldos.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[dbo].[PROC_Siscar_concuentasconsald]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@lnkempresa",
                            Value = sdbconexion.Substring(4, 2).Trim()
                        });

                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@lnkfecha",
                            Value = string.Format("{0:yyyyMMdd}", sfechac)
                        });
                    }
                    catch (Exception ex)
                    {

                    }

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCSaCo_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0,6) + ".inp";
                    string sLinea = null;

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        using (SqlDataReader dtr = cmd.ExecuteReader())
                        {
                            while (dtr.Read())
                            {
                                sLinea = sfecha.Substring(0,6).Trim() + "|" +
                                         dtr["conempresa"].ToString().Trim() + "|" +
                                         dtr["connumecuenta"].ToString().Trim() + "|" +
                                         dtr["connivel"].ToString().Trim() + "|" +
                                         dtr["consaldactual"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C16BalanceSaldosSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
