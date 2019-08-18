﻿using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Exceptions
{
    [Serializable]
    public class BadRequestException : ServerResponseException
    {
        public BadRequestException(ResponseMessageAPIModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }

    }
}