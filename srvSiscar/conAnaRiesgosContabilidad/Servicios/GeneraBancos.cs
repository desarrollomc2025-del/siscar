using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conAnaRiesgosContabilidad.Servicios
{
    internal class GeneraBancos
    {
        private static void Genera(string fechaInicial, string fechaFinal, string cooperativa)
        {
            CultureInfo provider = CultureInfo.CurrentUICulture;

            while (int.Parse($"{fechaInicial}") <= int.Parse(fechaFinal))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString))
                    {
                        connection.Open();

                        SqlCommand cmd = connection.CreateCommand();
                        cmd.CommandText = "[dbo].[PROC_BANCOS]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        try
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = "@codigocooperativa",
                                Value = cooperativa
                            });
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = "@FechaFinal",
                                Value = string.Format("{0:yyyyMMdd}", fechaInicial)

                                //Value = string.Format("{0:yyyyMMdd}", DateTime.Now.AddDays(-1))
                            });

//                            if (ConfigurationManager.AppSettings[string.Format("Coope{0}", cooperativa)].ToString() == "True")
//                            {
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
//                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        fechaInicial = $"{DateTime.Parse($"{fechaInicial.Substring(0, 4)}/{fechaInicial.Substring(4, 2)}/{fechaInicial.Substring(6, 2)}").AddDays(1):yyyyMMdd}";
                        Console.WriteLine("Fecha inicia " + fechaInicial);
                        //Console.ReadKey();

                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        private static void BorraBancos(string fechaInicial, string fechaFinal, string cooperativa)
        {
            CultureInfo provider = CultureInfo.CurrentUICulture;

            while (int.Parse($"{fechaInicial}") <= int.Parse(fechaFinal))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString))
                    {
                        connection.Open();

                        SqlCommand cmd = connection.CreateCommand();
                        cmd.CommandText = "[dbo].[PROC_BANCOS_DEL]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        try
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = "@codigocooperativa",
                                Value = cooperativa
                            });
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = "@FechaInicial",
                                Value = string.Format("{0:yyyyMMdd}", fechaInicial)

                                //Value = string.Format("{0:yyyyMMdd}", DateTime.Now.AddDays(-1))
                            });
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = "@FechaFinal",
                                Value = string.Format("{0:yyyyMMdd}", fechaInicial)

                            });

                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            //                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        fechaInicial = $"{DateTime.Parse($"{fechaInicial.Substring(0, 4)}/{fechaInicial.Substring(4, 2)}/{fechaInicial.Substring(6, 2)}").AddDays(1):yyyyMMdd}";
                        Console.WriteLine("Fecha inicia " + fechaInicial);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        public static void Load(string cooperativa, string fechaInicial, string fechaFinal)
        {
            BorraBancos(fechaInicial, fechaFinal,cooperativa);
            Genera(fechaInicial, fechaFinal, cooperativa);
        }
    }
}                
