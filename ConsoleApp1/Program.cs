using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;

namespace ConsoleApp1
{
    class Program
    {



        public static string tokenSTR { get; set; }

        static void Main(string[] args)
        {

            Crypto crypt1 = new Crypto();

            //  string key1 = "uexDPnPr";

            //string key1 = "1ue$&*nP";  //  chave de 64 bits = 8 bytes


            //var encripta1 = crypt1.EncryptData("renato 123", key1);
            //var dencripta1 = crypt1.DecryptData(encripta1, key1);

            //Console.WriteLine(dencripta1);



            HttpWebResponse response = null;
            try
            {
                tokenSTR = consultaToken();


                string visitsURL2 = "http://localhost:4000/users/userpost";
                var httpWebRequest2 = (HttpWebRequest)WebRequest.Create(visitsURL2);
                httpWebRequest2.ContentType = "application/json";
                httpWebRequest2.Method = "POST";

                var obj = new VisitDataArgs("test", "test");

                //Logger.WriteToFile("Enviando para " + visitsURL);

                Authentication.SetHeadersToken(httpWebRequest2);
                //httpWebRequest2.PreAuthenticate = true;
                //httpWebRequest2.Headers.Add("Authorization", "Bearer " + "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEsImlhdCI6MTYyMjczMzAwOCwiZXhwIjoxNjIzMzM3ODA4fQ.TDPE5Y9vIA-hkFPXmbjIbWc1a4ZmOOvWzxbxsZibD4s");
                //httpWebRequest2.Accept = "application/json";

                using (var streamWriter = new StreamWriter(httpWebRequest2.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                HttpWebResponse response2 = null;

                response2 = (HttpWebResponse)httpWebRequest2.GetResponse();
                string conteudo2 = "";
                using (var streamReader = new StreamReader(response2.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    conteudo2 = result;

                    List<Token> toke2n = JsonConvert.DeserializeObject<List<Token>>(conteudo2);


                    conteudo2 = result;
                    Console.WriteLine("success: " + result);
                }


            }
            catch (Exception ex)
            {
                var str1 = ex.Message;
                Console.WriteLine("erro: " + ex.Message);

            }
        }


        public static string consultaToken()
        {
            HttpWebResponse response = null;

            var obj = new VisitDataArgs("test", "test");

            string visitsURL = "http://localhost:4000/users/authenticate";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(visitsURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            //Logger.WriteToFile("Enviando para " + visitsURL);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            response = (HttpWebResponse)httpWebRequest.GetResponse();
            string conteudo = "";
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                conteudo = result;
                //Logger.WriteToFile(result);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //obtem o token gerado
                //conteudo = response.ReadAsStringAsync().Result;

                Token token = JsonConvert.DeserializeObject<Token>(conteudo);



                return token.token;
            }

            return "";
        }




    }








}

