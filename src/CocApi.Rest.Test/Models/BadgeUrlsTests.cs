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
    ///  Class for testing BadgeUrls
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the model.
    /// </remarks>
    public class BadgeUrlsTests : IDisposable
    {
        // TODO uncomment below to declare an instance variable for BadgeUrls
        //private BadgeUrls instance;

        public BadgeUrlsTests()
        {
            // TODO uncomment below to create an instance of BadgeUrls
            //instance = new BadgeUrls();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of BadgeUrls
        /// </summary>
        [Fact]
        public void BadgeUrlsInstanceTest()
        {
            // TODO uncomment below to test "IsType" BadgeUrls
            //Assert.IsType<BadgeUrls>(instance);
        }

        /// <summary>
        /// Test the property 'Small'
        /// </summary>
        [Fact]
        public void SmallTest()
        {
            // TODO unit test for the property 'Small'
        }

        /// <summary>
        /// Test the property 'Medium'
        /// </summary>
        [Fact]
        public void MediumTest()
        {
            // TODO unit test for the property 'Medium'
        }

        /// <summary>
        /// Test the property 'Large'
        /// </summary>
        [Fact]
        public void LargeTest()
        {
            // TODO unit test for the property 'Large'
        }
    }
}
