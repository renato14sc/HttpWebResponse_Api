using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ConsoleApp1
{
    public class Authentication
    {
        //public string tokenKey {
        //    get { 
        //        return httpc Session["sdfs"].ToString()
        //            }
        //    set { }
        
        //}



        public static void SetHeadersToken(HttpWebRequest objHttpwr)
        {
            //token
            objHttpwr.PreAuthenticate = true;
            objHttpwr.Headers.Add("Authorization", "Bearer " + "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEsImlhdCI6MTYyMjczMTkzNSwiZXhwIjoxNjIzMzM2NzM1fQ.vf5aHuGxLUmEkiNYDq2p8Ajjvuu_SnBRGp9bX4qG4Ho");
            objHttpwr.Accept = "application/json";
        }

    }





}
