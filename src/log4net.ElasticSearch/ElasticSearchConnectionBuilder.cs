﻿using System;
using System.Collections.Specialized;
using log4net.ElasticSearch.Models;

namespace log4net.ElasticSearch
{
    public static class ElasticSearchConnectionBuilder
    {
        public static ElasticSearchConnection Build(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("ConnectionString is null or empty", "connectionString");
            }

            try
            {
                var builder = new System.Data.Common.DbConnectionStringBuilder
                    {
                        ConnectionString = connectionString.Replace("{", "\"").Replace("}", "\"")
                    };

                var lookup = new StringDictionary();
                foreach (string key in builder.Keys)
                {
                    lookup[key] = Convert.ToString(builder[key]);
                }

                var index = lookup["Index"];

                if (!string.IsNullOrEmpty(lookup["rolling"]))
                    if (lookup["rolling"] == "true")
                        index = string.Format("{0}-{1}", index, DateTime.Now.ToString("yyyy.MM.dd"));

                return
                    new ElasticSearchConnection
                    {
                        Server = lookup["Server"],
                        Port = lookup["Port"],
                        Index = index
                    };
            }
            catch
            {
                throw new InvalidOperationException("Not a valid connection string");
            }
        }
    }
}
