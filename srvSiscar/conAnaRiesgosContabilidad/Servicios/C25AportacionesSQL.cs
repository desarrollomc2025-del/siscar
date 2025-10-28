using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace conAnaRiesgosContabilidad
{
    public class C25AportacionesSQL
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
                    throw new Exception("C25Aportaciones.error [No se pudo establecer conexion con la base de datos]");
                }

                try
                {
                    OracleCommand cmd = Oconexion.CreateCommand();
                    cmd.CommandText = "bkwanaries_pkg.prcfgcrl003varaportes";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("lnkfchactuali", OracleDbType.Varchar2).Value = string.Format("{0:ddMMyyyy}", sfechac);
                    cmd.Parameters.Add("cursor_", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    string sfile = "DatosCooperativas/" + scarpeta.Trim() + "/" + sfecha.Substring(0, 4).Trim() + "-" + sfecha.Substring(4, 2).Trim() + "/" + "DCCaAp_" + sdbconexion.Substring(4, 2).Trim() + "_" + sfecha + ".inp";

                    int Correlativo = 0;
                    decimal SaldoFinal = 0;
                    ////EventLog.WriteEntry("SISCARDatosCooperativa ", ConfigurationManager.AppSettings["Ruta"].ToString() + sfile, //EventLogEntryType.Warning, 234);

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Ruta"].ToString() + sfile))
                    {
                        string sLinea = null;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Correlativo = Correlativo + 1;

                                SaldoFinal += (reader.GetDecimal(1) + reader.GetDecimal(2)) - reader.GetDecimal(3);

                                sLinea = string.Format("{1}{0}{2}{0}{3}{0}{4:dd/MM/yyyy}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}A", "|",
                                        sdbconexion.Substring(4, 2).Trim(),
                                        sfecha.Trim(),
                                        Correlativo,
                                        reader.GetDateTime(0),
                                        reader.GetDecimal(2),
                                        reader.GetDecimal(3),
                                        SaldoFinal);
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
                    //EventLog.WriteEntry("SISCARDatosCooperativa", string.Format("C25Aportaciones Error {0} Conexion{1} ", ex.Message, sdbconexion), //EventLogEntryType.Error, 234);
                }
            }
        }//Genera

        public static void Load(string conexion, string sfecha, string scarpeta, string sfechac)
        {
            Genera(conexion, sfecha, scarpeta, sfechac);
        }
    }
}
