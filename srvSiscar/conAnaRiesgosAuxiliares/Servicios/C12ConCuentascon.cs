using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace conAnaRiesgosAuxiliares
{
    public class C12ConCuentascon
    {
        private static void Genera(string sdbconexion, string sfecha)
        {
            using (OracleConnection Oconexion = new OracleConnection(ConfigurationManager.ConnectionStrings[sdbconexion].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C12ConCuentascon.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prcconcuentascon";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("lnkempresa", OracleDbType.Int32).Value = ConfigurationManager.AppSettings["CoopCatalogo"].ToString();
                    cmd.Parameters.Add("CURSOR_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    string sfile = "CatalogosGenerales/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "CGCuen_" + sfecha + ".inp";
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
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
                    var resp = ftpTraslada.upload(sfile, ConfigurationManager.AppSettings["Ruta"].ToString() + sfile);
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
