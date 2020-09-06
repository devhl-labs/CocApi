using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi
{
    public static class Library
    {
        public static readonly Version Version = typeof(Library).Assembly.GetName().Version;

        public const string RepositoryUrl = "https://github.com/devhl-labs/CocApi";
    }
}
