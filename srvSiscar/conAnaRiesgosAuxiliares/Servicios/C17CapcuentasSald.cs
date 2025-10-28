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

namespace conAnaRiesgosAuxiliares
{
    public class C17CapcuentasSald
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
                    throw new Exception("C17CapcuentasSald.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prccaptaciones";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("lnkfecha", OracleDbType.Varchar2).Value = $"{sfechac.Substring(6,2)}{sfechac.Substring(4, 2)}{sfechac.Substring(0, 4)}";
//                    cmd.Parameters.Add("lnkfecha", OracleDbType.Varchar2).Value = string.Format("{0:ddMMyyyy}", sfechac);
                    cmd.Parameters.Add("CURSOR_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCCapt_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha.Substring(0, 6) + ".inp";
                    string periodo = sfecha.Substring(0, 6).Trim();
                    string modulo = "CAP";
                    string empresa = sdbconexion.Substring(4,2);
                    int conteo = 0;
                    decimal total = 0;
                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                conteo++;
                                total = total + decimal.Parse(reader["capsaldactual"].ToString().Trim());
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C17CapcuentasSald Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                    throw new Exception(string.Format("C17CapcuentasSald Error {0} Conexion{1} ", ex.Message, sdbconexion));
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
