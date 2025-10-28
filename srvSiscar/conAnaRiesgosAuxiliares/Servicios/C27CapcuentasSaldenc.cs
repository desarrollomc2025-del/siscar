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
using conAnaRiesgosAuxiliares.Servicios;
using Dapper;
using System.Data.SqlClient;

namespace conAnaRiesgosAuxiliares
{
    public class C27CapcuentasSaldenc
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
                    throw new Exception("C27CapcuentasSaldenc.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prccaptacionesenc";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("lnkfchactuali", OracleDbType.Varchar2).Value = $"{sfechac.Substring(6, 2)}{sfechac.Substring(4, 2)}{sfechac.Substring(0, 4)}";
                    cmd.Parameters.Add("cursor_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCCaptENC_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    string periodo = sfecha.Substring(0, 6).Trim();
                    string modulo  = "CAPENC";
                    string empresa = sdbconexion.Substring(4,2);
                    int conteo     = 0;
                    decimal total  = 0;
                    //EventLog.WriteEntry("SISCARDatosCooperativa ", string.Format("Fecha Encaje {0:ddMMyyyy}", sfechac), //EventLogEntryType.Error, 234);
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //empresa = reader["fincodempresa"].ToString().Trim();
                                conteo++;
                                total = total + decimal.Parse(reader["captasaactual"].ToString().Trim());

                                sLinea = reader["fincodempresa"].ToString().Trim() + "|" +
                                            sfecha.Substring(0, 6).Trim() + "|" +
                                            reader["capcodcliente"].ToString().Trim() + "|" +
                                            reader["capcodarea"].ToString().Trim() + "|" +
                                            reader["cifsexo"].ToString().Trim() + "|" +
                                            reader["capnumcuenta"].ToString().Trim() + "|" +
                                            reader["capcodinstrum"].ToString().Trim() + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["capfchemision"]) + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["capfchvencimi"]) + "|" +
                                            reader["captasaactual"].ToString().Trim() + "|" +
                                            string.Format("{0:dd/MM/yyyy}", reader["capfchultiren"]) + "|" +
                                            reader["capsaldactual"].ToString().Trim() + "|" +
                                            reader["capcodmoneda"].ToString().Trim() + "|" +
                                            reader["capagrupador"].ToString().Trim();
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C27CapcuentasSaldenc Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        private static void GeneraCaptacionesEncDapper(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            CultureInfo cultura = new CultureInfo("es-ES");
            using (OracleConnection Oconexion = new OracleConnection(ConfigurationManager.ConnectionStrings[sdbconexion].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C27CapcuentasSaldenc.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    string query = $"select d.fincodempresa, a.capcodcliente, a.capcodarea, c.cifsexo, " +
                        $" a.capnumcuenta, a.capcodinstrum, to_char(a.capfchemision,'dd/mm/yyyy') as capfchemision, " +
                        $" to_char(a.capfchvencimi,'dd/mm/yyyy') as capfchvencimi, a.captasaactual, " +
                        $" to_char(a.capfchultiren,'dd/mm/yyyy') as capfchultiren, a.capsaldactual, a.capagrupador, b.capcodmoneda " +
                        $" from capcuentassald a inner join capinstrument b on a.capcodinstrum = b.capcodinstrum " +
                        $" inner join cifgenerales c on a.capcodcliente = c.cifcodcliente, finparametros d " +
                        $" where a.capfchactuali = (select cisfecha from cissaldencaje_view@dblink2bank90 where  ciscodempresa = d.fincodempresa " +
                        $" and    cisfecha >= to_date('01012021','ddmmyyyy') " +
                        $" and    to_char(cisfecha,'MMYYYY') = '{sfechac.Substring(4, 2)}{sfechac.Substring(0, 4)}' " +
                        $" and    rownum <=1) " +
                        $" and b.capsistinstru  = 'CAP' " +
                        $" and a.capestadocuen!= 'S' " +
                        $" and a.capcodinstrum <> 9999 ";
                    var resultado = Oconexion.Query<CaptacionesEncaje>(query, null, commandType: CommandType.Text);
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCCaptENC_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string periodo = sfecha.Substring(0, 6).Trim();
                        string modulo = "CAPENC";
                        string empresa = string.Empty;
                        int conteo = 0;
                        decimal total = 0;
                        //EventLog.WriteEntry("SISCARDatosCooperativa ", string.Format("Fecha Encaje {0:ddMMyyyy}", sfechac), //EventLogEntryType.Error, 234);
                        string sLinea = null;
                        foreach (var line in resultado)
                        {
                            conteo++;
                            total = total + decimal.Parse(line.capsaldactual.ToString().Trim());
                            DateTime dfechaemision;
                            DateTime dfechavencimi;
                            DateTime dfechaultiren;
                            bool bfechaemision = DateTime.TryParse(line.capfchemision, out dfechaemision);
                            bool bfechavencimi = DateTime.TryParse(line.capfchvencimi, out dfechavencimi);
                            bool bfechaultiren = DateTime.TryParse(line.capfchultiren, out dfechaultiren);
                            string fechaemision = !bfechaemision ? string.Empty : $"{dfechaemision:dd/MM/yyyy}".Trim();
                            string fechavencimi = !bfechavencimi ? string.Empty : $"{dfechavencimi:dd/MM/yyyy}".Trim();
                            string fechaultiren = !bfechaultiren ? string.Empty : $"{dfechaultiren:dd/MM/yyyy}".Trim();

                            try
                            {
                                sLinea = line.fincodempresa + "|" +
                                         sfecha.Substring(0, 6) + "|" +
                                         line.capcodcliente + "|" +
                                         line.capcodarea + "|" +
                                         line.cifsexo + "|" +
                                         line.capnumcuenta + "|" +
                                         line.capcodinstrum + "|" +
                                         fechaemision + "|" +
                                         fechavencimi + "|" +
                                         line.captasaactual + "|" +
                                         fechaultiren + "|" +
                                         line.capsaldactual + "|" +
                                         line.capcodmoneda + "|" +
                                         line.capagrupador;
                                sw.WriteLine(sLinea);
                            }
                            catch (Exception ex) { Console.WriteLine($"Error en Linea [{sLinea}] - {ex.Message}"); }
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C27CapcuentasSaldenc Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera
        private static void GeneraCaptacionesEncDapperSQL(string sdbconexion, string sfecha, string scarpeta, string sfechac)
        {
            CultureInfo cultura = new CultureInfo("es-ES");
            using (SqlConnection Oconexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionAnalytics"].ConnectionString.ToString()))
            {
                try
                {
                    Oconexion.Open();
                }
                catch
                {
                    throw new Exception("C27CapcuentasSaldenc.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    string query = $"[AnalyticsImport].[dbo].[PROC_ARCH_DCCAPTENC_FILE]";
                    var resultado = Oconexion.Query<CaptacionesEncajeT24>(query, new { dfecha = DateTime.Parse($"{sfechac.Substring(6, 2)}/{sfechac.Substring(4, 2)}/{sfechac.Substring(0, 4)}"), coope = $"A{sdbconexion.Substring(4, 2).Trim()}"}, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCCaptENC_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string periodo = sfecha.Substring(0, 6).Trim();
                        string modulo = "CAPENC";
                        string empresa = string.Empty;
                        int conteo = 0;
                        decimal total = 0;
                        //EventLog.WriteEntry("SISCARDatosCooperativa ", string.Format("Fecha Encaje {0:ddMMyyyy}", sfechac), //EventLogEntryType.Error, 234);
                        string sLinea = null;
                        foreach (var line in resultado)
                        {
                            conteo++;
                            total = total + decimal.Parse(line.capsaldactual.ToString().Trim());
                            DateTime dfechaemision;
                            DateTime dfechavencimi;
                            DateTime dfechaultiren;
                            bool bfechaemision = DateTime.TryParse(line.capfchemision, out dfechaemision);
                            bool bfechavencimi = DateTime.TryParse(line.capfchvencimi, out dfechavencimi);
                            bool bfechaultiren = DateTime.TryParse(line.capfchultiren, out dfechaultiren);
                            string fechaemision = !bfechaemision ? string.Empty : $"{dfechaemision:dd/MM/yyyy}".Trim();
                            string fechavencimi = !bfechavencimi ? string.Empty : $"{dfechavencimi:dd/MM/yyyy}".Trim();
                            string fechaultiren = !bfechaultiren ? string.Empty : $"{dfechaultiren:dd/MM/yyyy}".Trim();

                            try
                            {
                                sLinea = line.fincodempresa + "|" +
                                         sfecha.Substring(0, 6) + "|" +
                                         line.capcodcliente + "|" +
                                         line.capcodarea + "|" +
                                         line.cifsexo + "|" +
                                         line.capnumcuenta + "|" +
                                         line.capcodinstrum + "|" +
                                         fechaemision + "|" +
                                         fechavencimi + "|" +
                                         line.captasaactual + "|" +
                                         fechaultiren + "|" +
                                         line.capsaldactual + "|" +
                                         line.capcodmoneda + "|" +
                                         line.capagrupador + "|" +
                                         line.tipocliente + "|" +
                                         line.clientet24 + "|" +
                                         line.document24 + "|" +
                                         line.agt24 + "|" +
                                         line.prodt24 + "|" +
                                         line.moneda + "|" +
                                         line.encaje;
                                sw.WriteLine(sLinea);
                            }
                            catch (Exception ex) { Console.WriteLine($"Error en Linea [{sLinea}] - {ex.Message}"); }
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C27CapcuentasSaldenc Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            //Genera(conexion, sfecha, scarpeta, sfechac);
            GeneraCaptacionesEncDapper(conexion, sfecha, scarpeta, sfechac);
        }
        public static void LoadT24(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            //Genera(conexion, sfecha, scarpeta, sfechac);
            GeneraCaptacionesEncDapperSQL(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
