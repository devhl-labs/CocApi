/*
 * Clash of Clans API
 *
 * Check out <a href=\"https://developer.clashofclans.com/#/getting-started\" target=\"_parent\">Getting Started</a> for instructions and links to other resources. Clash of Clans API uses <a href=\"https://jwt.io/\" target=\"_blank\">JSON Web Tokens</a> for authorizing the requests. Tokens are created by developers on <a href=\"https://developer.clashofclans.com/#/account\" target=\"_parent\">My Account</a> page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \"Authorization: Bearer API_TOKEN\". 
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using Xunit;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CocApi.Rest.Models;
using CocApi.Rest.Client;
using System.Reflection;

namespace CocApi.Rest.Test.Model
{
    /// <summary>
    ///  Class for testing Language
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the model.
    /// </remarks>
    public class LanguageTests : IDisposable
    {
        // TODO uncomment below to declare an instance variable for Language
        //private Language instance;

        public LanguageTests()
        {
            // TODO uncomment below to create an instance of Language
            //instance = new Language();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of Language
        /// </summary>
        [Fact]
        public void LanguageInstanceTest()
        {
            // TODO uncomment below to test "IsType" Language
            //Assert.IsType<Language>(instance);
        }

        /// <summary>
        /// Test the property 'Name'
        /// </summary>
        [Fact]
        public void NameTest()
        {
            // TODO unit test for the property 'Name'
        }

        /// <summary>
        /// Test the property 'Id'
        /// </summary>
        [Fact]
        public void IdTest()
        {
            // TODO unit test for the property 'Id'
        }

        /// <summary>
        /// Test the property 'LanguageCode'
        /// </summary>
        [Fact]
        public void LanguageCodeTest()
        {
            // TODO unit test for the property 'LanguageCode'
        }
    }
}
