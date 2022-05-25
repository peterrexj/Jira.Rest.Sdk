using System;
using System.Collections.Generic;
using System.Text;
using TestAny.Essentials.Api;

namespace Jira.Rest.Sdk
{
    internal static class TestApiRequestEx
    {
        public static TestApiRequest WithJsonResponse(this TestApiRequest request)
        {
            request.RemoveHeader("X-Requested-With");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            request.RemoveHeader("Accept");
            request.RemoveHeader("Content-Type");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Content-Type", "application/json");
            return request;
        }
    }
}
