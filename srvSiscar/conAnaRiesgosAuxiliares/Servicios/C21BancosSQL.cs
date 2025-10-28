using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Data.SqlClient;
using conAnaRiesgosAuxiliares.Servicios;

namespace conAnaRiesgosAuxiliares
{
    public class C21BancosSQL
    {
        private static void Genera(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C21BancosSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[dbo].[PROC_GEN_BANCOS]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    //EventLog.WriteEntry("SISCARDatosCooperativa ", string.Format("{0:yyyyMMdd}", sfechac),//EventLogEntryType.Error, 234);

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@empresa",
                        Value = sdbconexion.Substring(4, 2).Trim()
                    });
                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@fchactuali",
                        Value = sfechac
                    });
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCSaBa_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    string periodo = sfecha.Substring(0, 6).Trim();
                    string modulo = "BAN";
                    string empresa = sdbconexion.Substring(4, 2);
                    int conteo = 0;
                    decimal total = 0;
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (SqlDataReader dtr = cmd.ExecuteReader())
                        {
                            while (dtr.Read())
                            {
                                string[] lineas = dtr["LINEA"].ToString().Trim().Split('|');
                                //periodo = lineas[0].Trim();
                                //empresa = lineas[1].Trim();
                                conteo++;
                                total = total + decimal.Parse(lineas[9].ToString().Trim());
                                sLinea = dtr["LINEA"].ToString().Trim();
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
                    Verificador.Load(periodo, modulo, empresa, conteo, total);
                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C21BancosSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
