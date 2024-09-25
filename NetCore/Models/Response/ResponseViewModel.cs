using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetCore.Models.Response
{
    public class ResponseViewModel
    {
        public ResponseViewModel()
        {
        }

        public ResponseViewModel(int error, string message, dynamic data)
        {
            e = error;
            m = message;
            d = data;
        }

        public int e { get; set; }
        public string m { get; set; }
        public dynamic d { get; set; }

        public static ResponseViewModel CreateSuccess(object data = null, string message = null)
        {
            return new ResponseViewModel(0, message, data);
        }

        public static ResponseViewModel CreateError(string message = null)
        {
            return new ResponseViewModel(1, message, null);
        }

        public static ResponseViewModel CreateErrorObject(object data)
        {
            return new ResponseViewModel(1, null, data);
        }
    }
}
