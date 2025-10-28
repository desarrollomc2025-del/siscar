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

namespace conAnaRiesgosAuxiliares
{
    public class C17CapcuentasSaldSQL
    {
        private static void Genera(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL188"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C17CapcuentasSald.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[fen].[PROC_siscar_capcuentassald]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@lnkempresa",
                        Value = sdbconexion.Substring(4, 2).Trim()
                    });

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@lnkfecha",
                        Value = string.Format("{0:ddMMyyyy}", sfechac)
                    });


                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCCapt_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha + ".inp";
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (SqlDataReader dtr = cmd.ExecuteReader())
                        {
                            while (dtr.Read())
                            {
                                sLinea = dtr["fincodempresa"].ToString().Trim() + "|" +
                                                    dtr["capfchactuali"].ToString().Trim() + "|" +
                                                    dtr["capcodcliente"].ToString().Trim() + "|" +
                                                    dtr["capcodarea"].ToString().Trim() + "|" +
                                                    dtr["cifsexo"].ToString().Trim() + "|" +
                                                    dtr["capnumcuenta"].ToString().Trim() + "|" +
                                                    dtr["capcodinstrum"].ToString().Trim() + "|" +
                                                    string.Format("{0:dd/MM/yyyy}", dtr["capfchemision"]) + "|" +
                                                    string.Format("{0:dd/MM/yyyy}", dtr["capfchvencimi"]) + "|" +
                                                    dtr["captasaactual"].ToString().Trim() + "|" +
                                                    string.Format("{0:dd/MM/yyyy}", dtr["capfchultiren"]) + "|" +
                                                    dtr["capsaldactual"].ToString().Trim() + "|" +
                                                    dtr["capcodmoneda"].ToString().Trim() + "|" +
                                                    dtr["capagrupador"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C17CapcuentasSald Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
