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
using conAnaRiesgosAuxiliares.Servicios;
using Dapper;
using System.Globalization;

namespace conAnaRiesgosAuxiliares
{
    public class C19TarjetaCreditoSQL
    {
        private static void Genera(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQLTC"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C19TarjetaCreditoSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[TCreditoMicoope].[dbo].[PROC_SISCAR_DATOS_TARJETAS_SL]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 1000;
                    //EventLog.WriteEntry("SISCARDatosCooperativa ", "Empresa Tarjeta Credito "+sdbconexion.Substring(4, 2).Trim(), //EventLogEntryType.Warning, 234);
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@fecha",
                        Value = string.Format("{0:yyyyMMdd}", sfechac) //sfechac //.ToString("yyyyMMdd")
                    });

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@CodigoEmpresa",
                        Value = int.Parse(sdbconexion.Substring(4, 2).Trim())
                    });

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCTarj_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    ////EventLog.WriteEntry("SISCARdb19", $"Ruta Archivo {ConfigurationManager.AppSettings["Ruta"].ToString()}{sfile}", //EventLogEntryType.Information, 234);
                    string periodo = sfecha.Substring(0, 6);
                    string modulo = "TC";
                    string empresa = int.Parse(sdbconexion.Substring(4, 2).Trim()).ToString();
                    int conteo = 0;
                    decimal total = 0;

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (SqlDataReader dtr = cmd.ExecuteReader())
                        {
                            while (dtr.Read())
                            {
                                string[] lineas = dtr["linea"].ToString().Split('|');
                                periodo = lineas[2].ToString();
                                empresa = lineas[0].ToString();
                                conteo++;
                                total = total + decimal.Parse(lineas[3].ToString().Trim());
                                sLinea = dtr["linea"].ToString().Trim();
                                sw.WriteLine(sLinea);
                            }
                        }
                    }
                    string hostIp = ConfigurationManager.AppSettings["HostFTP"].ToString();
                    string userFtp = ConfigurationManager.AppSettings["UserFTP"].ToString();
                    string passwordFtp = ConfigurationManager.AppSettings["ClaveFTP"].ToString();
                    CFtpTraslada ftpTraslada = new CFtpTraslada(hostIp, userFtp, passwordFtp);
                    //var resp = ftpTraslada.upload(sfile, ConfigurationManager.AppSettings["Ruta"].ToString() + sfile);
                    string resp = "1";
                    if (resp == "1")
                    {
                        string sDirectoryCarga = ConfigurationManager.AppSettings["RutaDestino"];
                        File.Copy(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, sDirectoryCarga + sfile, true);
                    }
                    Verificador.Load(periodo,modulo,empresa,conteo,total);
                }
                catch (Exception ex)
                {
                    ////EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C19TarjetaCreditoSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        private static void GeneraTarjetaCredito(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQLTC"].ConnectionString))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C19TarjetaCreditoSQL.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    SqlCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "[TCreditoMicoope].[dbo].[PROC_SISCAR_DATOS_TARJETAS_SL]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 1000;
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", "Empresa Tarjeta Credito " + sdbconexion.Substring(4, 2).Trim(), //EventLogEntryType.Warning, 234);
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@fecha",
                        Value = string.Format("{0:yyyyMMdd}", sfechac) //sfechac //.ToString("yyyyMMdd")
                    });

                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@CodigoEmpresa",
                        Value = int.Parse(sdbconexion.Substring(4, 2).Trim())
                    });

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCTarj_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + "a.inp";
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    ////EventLog.WriteEntry("SISCARdb19", $"Ruta Archivo {ConfigurationManager.AppSettings["Ruta"].ToString()}{sfile}", //EventLogEntryType.Information, 234);
                    string periodo = sfecha.Substring(0, 6);
                    string modulo = "TC";
                    string empresa = int.Parse(sdbconexion.Substring(4, 2).Trim()).ToString();
                    int conteo = 0;
                    decimal total = 0;

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (SqlDataReader dtr = cmd.ExecuteReader())
                        {
                            while (dtr.Read())
                            {
                                string[] lineas = dtr["linea"].ToString().Split('|');
                                periodo = lineas[2].ToString();
                                empresa = lineas[0].ToString();
                                conteo++;
                                total = total + decimal.Parse(lineas[3].ToString().Trim());
                                sLinea = dtr["linea"].ToString().Trim();
                                sw.WriteLine(sLinea);
                            }
                        }
                    }
                    string hostIp = ConfigurationManager.AppSettings["HostFTP"].ToString();
                    string userFtp = ConfigurationManager.AppSettings["UserFTP"].ToString();
                    string passwordFtp = ConfigurationManager.AppSettings["ClaveFTP"].ToString();
                    CFtpTraslada ftpTraslada = new CFtpTraslada(hostIp, userFtp, passwordFtp);
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
                    ////EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C19TarjetaCreditoSQL Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            //Genera(conexion, sfecha, scarpeta, sfechac);
            GeneraTarjetaCredito(conexion,sfecha,scarpeta, sfechac);
        }
    }
}
