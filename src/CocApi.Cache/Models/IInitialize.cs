using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Cache.Models
{
    public interface IInitialize
    {
        void Initialize(CocApiClient_old cocApi);
    }
}
