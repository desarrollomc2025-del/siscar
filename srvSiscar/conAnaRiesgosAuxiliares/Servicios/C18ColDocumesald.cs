using conAnaRiesgosAuxiliares.Servicios;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace conAnaRiesgosAuxiliares
{
    public class C18ColDocumesald
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
                    throw new Exception("C18ColDocumesald.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prccolocaciones";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("lnkfecha", OracleDbType.Varchar2).Value = $"{sfechac.Substring(6, 2)}{sfechac.Substring(4, 2)}{sfechac.Substring(0, 4)}";
                    cmd.Parameters.Add("CURSOR_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCColo_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    string periodo = sfecha.Substring(0, 6).Trim();
                    string modulo = "COL";
                    string empresa = sdbconexion.Substring(4, 2);
                    int conteo = 0;
                    decimal total = 0;

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //empresa = reader["fincodempresa"].ToString().Trim();
                                //periodo = sfecha.Substring(0, 6).Trim();
                                conteo++;
                                total = total + decimal.Parse(reader["colsalactuahy"].ToString().Trim());
                                sLinea = reader["fincodempresa"].ToString().Trim() + "|" +
                                            sfecha.Substring(0, 6).Trim() + "|" +
                                            reader["colcodcliente"].ToString().Trim() + "|" +
                                            reader["colnumdocumen"].ToString().Trim() + "|" +
                                            reader["colcodareafin"].ToString().Trim() + "|" +
                                            reader["colnuminstrum"].ToString().Trim() + "|" +
                                            reader["colnumlincred"].ToString().Trim() + "|" +
                                            reader["colcodtipogar"].ToString().Trim() + "|" +
                                            reader["colplazo"].ToString().Trim() + "|" +
                                            reader["colfrecuepago"].ToString().Trim() + "|" +
                                            reader["colflgtipcuot"].ToString().Trim() + "|" +
                                            reader["coltasaintere"].ToString().Trim() + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["colfch1erdese"]) + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["colfchvencimi"]) + "|" +
                                            reader["colcuotasngracia"].ToString().Trim() + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["colfchprxcuot"]) + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["colfch1ercuot"]) + "|" +
                                            reader["colcodcategor"].ToString().Trim() + "|" +
                                            reader["coldiasmora"].ToString().Trim() + "|" +
                                            reader["colmntodocume"].ToString().Trim() + "|" +
                                            reader["colcapdesembo"].ToString().Trim() + "|" +
                                            reader["colsalactuahy"].ToString().Trim() + "|" +
                                            reader["colflgreestru"].ToString().Trim() + "|" +
                                            reader["colnumecuenta"].ToString().Trim() + "|" +
                                            reader["colestadodoc"].ToString().Trim() + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["colfchcancela"]) + "|" +
                                            reader["colcuotacapita"].ToString().Trim() + "|" +
                                            reader["coltasapactad"].ToString().Trim() +"|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["colfchsaneami"]) + "|" +
                                            reader["colflgestdepu"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C18ColDocumesald Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
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
                    string query = "[AnalyticsImport].[dbo].[PROC_ARCH_DCCOLO_NEW]";
                    var resultado = Oconexion.Query(query, new { dfecha = DateTime.Parse($"{sfecha.Substring(0, 4)}-{sfecha.Substring(4, 2)}-{sfecha.Substring(6, 2)}"), coope = $"A{sdbconexion.Substring(4, 2)}" }, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C15InstrumentoCapta Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        private static void GeneraT24Cancelados(string sdbconexion, string sfecha, string scarpeta)
        {
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionT24"].ConnectionString.ToString()))
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
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCColoCan_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + "a.inp";
                    string query = "[MicoopeReportesT24].[dbo].[PROC_PRESTAMOS_CANCELADOS_SL]";
                    var resultado = Oconexion.Query(query, new { CodigoEmpresa = $"{sdbconexion.Substring(4, 2)}", Fecha = $"{sfecha.Substring(0, 4)}{sfecha.Substring(4, 2)}{sfecha.Substring(6, 2)}" }, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                    int conteo = 0;
                    decimal total = 0;
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        foreach (var item in resultado)
                        {
                            string sLinea = null;
                            //empresa = reader["fincodempresa"].ToString().Trim();
                            //periodo = sfecha.Substring(0, 6).Trim();
                            conteo++;
                            //total = total + decimal.Parse(reader["colsalactuahy"].ToString().Trim());
                            sLinea = item.empresa + "|" +
                                     item.periodo + "|" +
                                     item.customer + "|" +
                                     item.numcuenta + "|" +
                                     item.fecha;
                            sw.WriteLine(sLinea);
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C15InstrumentoCapta Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
        public static void LoadT24(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            //GeneraT24(conexion, sfecha, scarpeta);
            GeneraT24Cancelados(conexion,sfecha,scarpeta);
        }
    }
}
