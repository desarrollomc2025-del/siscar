using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace conAnaRiesgosAuxiliares.Servicios
{
    internal class Verificador
    {
        private static void Genera(string periodo, string modulo, string empresa, int conteo, decimal total)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString))
                {
                    connection.Open();

                    //SqlCommand cmd = connection.CreateCommand();
                    //cmd.CommandText = $"INSERT INTO [dbo].[VerificadorSiscar] values ('{periodo}','{modulo}','{empresa}',{conteo},{total}";
                    //cmd.CommandType = CommandType.Text;

                    try
                    {
                        try
                        {
                            connection.Query("[dbo].[PROC_VERIFICADOR_INS]", new { periodo = periodo, modulo = modulo, empresa = empresa, conteo= conteo, total = total }, commandType: CommandType.StoredProcedure);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static void Load(string periodo, string modulo, string empresa, int conteo, decimal total)
        {
            Genera(periodo,modulo,empresa,conteo,total);
        }
    }
}
