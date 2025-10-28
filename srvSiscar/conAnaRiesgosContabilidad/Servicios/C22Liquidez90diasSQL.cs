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
    public class C22Liquidez90diasSQL
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
                    throw new Exception("C22Liquidez90diasSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[dbo].[proc_liquidez90dias]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    //EventLog.WriteEntry("SISCARDatosCooperativa ", "Empresa Inversiones "+sdbconexion.Substring(4, 2).Trim(), //EventLogEntryType.Warning, 234);
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@empresa",
                        Value = sdbconexion.Substring(4, 2).Trim()
                    });

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@vfchactuali",
                        Value = sfecha
                    });

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCLiPr_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (SqlDataReader dtr = cmd.ExecuteReader())
                        {
                            while (dtr.Read())
                            {
                                sLinea = 
                                    //string.Format("{1}{0}{2}{0}{3:dd/MM/yyyy}{0}{4:f2}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}{8:f2}{0}{9:f2}{0}{10:f2}", "|",
                                        dtr["conempresa"].ToString().Trim() + "|" +
                                            sfechac.Substring(0, 6) + "|" +
                                            dtr["fecha"].ToString().Trim() + "|" +
                                            dtr["disponibilidades"].ToString().Trim() + "|" +
                                            dtr["inversionesliquidas"].ToString().Trim() + "|" +
                                            dtr["inversionesfinancieras"].ToString().Trim() + "|" +
                                            dtr["estimacionesinversiones"].ToString().Trim() + "|" +
                                            dtr["cuentasporpagar"].ToString().Trim() + "|" +
                                            dtr["depositos"].ToString().Trim() + "|" +
                                            dtr["aportaciones"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C22Liquidez90diasSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
