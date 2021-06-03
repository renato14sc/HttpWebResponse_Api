using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class ConsultaUsuarios
    {
        private static void Consultar(HttpClient client, int valor)
        {
            //HttpResponseMessage response = client.GetAsync(_urlBase + "values/" + valor.ToString()).Result;

            //Console.WriteLine("");
            //Console.WriteLine($"{_urlBase} + 'values/' + {valor.ToString()}");
            //Console.WriteLine("");

            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            //}
            //else
            //{
            //    Console.WriteLine("Token provavelmente expirado!");
            //}

            Console.ReadKey();
        }


    }
}
