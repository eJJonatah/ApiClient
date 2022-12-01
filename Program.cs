using static ApiClient.App.Models.WebContext.SavedRequests;
using static ApiClient.App.Models.AppContext;
using System.Runtime.CompilerServices;
using ApiClient.App.Controllers;
using System.Linq.Expressions;
using ApiClient.App.Models;
using System;

namespace ApiClient
{

    internal class Program
    {
        static void Main()
        {

        }
    }
    public class OriginObject
    {
        public NestedObject nestedObject { get; set; }
        public Instructions instruct { get; set; }
        public class NestedObject
        {
            public string NestedName { get; set; }
            public void GetName()
            {
                NestedName = GetType().DeclaringType
                        .GetProperty("instruct")
                        .GetValue(GetType().DeclaringType)
                        .GetType()
                        .GetProperty("Name")
                        .GetValue(wtf im doing);
            }
        }
        public struct Instructions
        {
            public string Name { get; set; }
        }
    }
}
