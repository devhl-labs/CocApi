using System;

namespace CocApi
{
    public static class Library
    {
        public static readonly Version? Version = typeof(Library).Assembly.GetName().Version;

        public const string REPOSITORY_URL = "https://github.com/devhl-labs/CocApi";
    }
}
