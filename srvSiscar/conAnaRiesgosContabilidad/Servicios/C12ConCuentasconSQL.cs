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
    public class C12ConCuentasconSQL
    {
        private static void Genera(string sdbconexion, string sfecha)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C12ConCuentasconSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[dbo].[PROC_concuentascon]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@Cooperativa",
                            Value = sdbconexion.Substring(4, 2).Trim()
                        });

                    }
                    catch (Exception ex)
                    {

                    }

                    string sfile = "CatalogosGenerales/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "CGCuen_" + sfecha.Substring(0, 6) + ".inp";
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sLinea = reader["connumecuenta"].ToString().Trim() + "|" +
                                            sfecha.Trim() + "|" +
                                            reader["concuentaedit"].ToString().Trim() + "|" +
                                            reader["condescrcuent"].ToString().Trim() + "|" +
                                            reader["concargoabono"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARCatalogos", "C12ConCuentascon Error " + ex.Message, //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha)
        {
            Genera(conexion, sfecha);
        }
    }
}
