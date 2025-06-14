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
    ///  Class for testing ClanWarLeagueGroup
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the model.
    /// </remarks>
    public class ClanWarLeagueGroupTests : IDisposable
    {
        // TODO uncomment below to declare an instance variable for ClanWarLeagueGroup
        //private ClanWarLeagueGroup instance;

        public ClanWarLeagueGroupTests()
        {
            // TODO uncomment below to create an instance of ClanWarLeagueGroup
            //instance = new ClanWarLeagueGroup();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of ClanWarLeagueGroup
        /// </summary>
        [Fact]
        public void ClanWarLeagueGroupInstanceTest()
        {
            // TODO uncomment below to test "IsType" ClanWarLeagueGroup
            //Assert.IsType<ClanWarLeagueGroup>(instance);
        }

        /// <summary>
        /// Test the property 'Season'
        /// </summary>
        [Fact]
        public void SeasonTest()
        {
            // TODO unit test for the property 'Season'
        }

        /// <summary>
        /// Test the property 'Clans'
        /// </summary>
        [Fact]
        public void ClansTest()
        {
            // TODO unit test for the property 'Clans'
        }

        /// <summary>
        /// Test the property 'Rounds'
        /// </summary>
        [Fact]
        public void RoundsTest()
        {
            // TODO unit test for the property 'Rounds'
        }

        /// <summary>
        /// Test the property 'State'
        /// </summary>
        [Fact]
        public void StateTest()
        {
            // TODO unit test for the property 'State'
        }
    }
}
