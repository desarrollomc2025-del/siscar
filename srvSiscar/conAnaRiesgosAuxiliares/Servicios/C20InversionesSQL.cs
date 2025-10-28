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
    public class C20InversionesSQL
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
                    throw new Exception("C20InversionesSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "dbo.PROC_Siscar_invcuentassald";
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

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCInve_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    string periodo = sfecha.Substring(0, 6).Trim();
                    string modulo = "INV";
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
                                //empresa = dtr["fincodempresa"].ToString().Trim();
                                //periodo = dtr["fecha"].ToString().Trim();
                                conteo++;
                                total = total + decimal.Parse(dtr["capsaldactual"].ToString().Trim());
                                sLinea = dtr["fecha"].ToString().Trim() + "|" +
                                            dtr["fincodempresa"].ToString().Trim() + "|" +
                                            dtr["capcodbanctaj"].ToString().Trim() + "|" +
                                            dtr["capnumcuenta"].ToString().Trim() + "|" +
                                            dtr["capnomcuenta"].ToString().Trim() + "|" +
                                            dtr["capnumdocumen"].ToString().Trim() + "|" +
                                            dtr["capfchemision"] + "|" +
                                            dtr["capfchultiren"] + "|" +
                                            dtr["capfchvencimi"] + "|" +
                                            dtr["capplazomeses"].ToString().Trim() + "|" +
                                            dtr["captasareal"].ToString().Trim() + "|" +
                                            dtr["capsaldactual"].ToString().Trim() + "|" +
                                            dtr["capcodmoneda"].ToString().Trim() + "|" +
                                            dtr["capcodinstrum"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C20Inversiones Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
