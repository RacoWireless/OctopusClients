﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Serilog;
using NSubstitute;
using NUnit.Framework;
using Octopus.Cli.Commands;
using Octopus.Cli.Repositories;
using Octopus.Cli.Util;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Cli.Tests.Commands
{
    public abstract class ApiCommandFixtureBase
    {
        private static string _previousCurrentDirectory;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            _previousCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            Directory.SetCurrentDirectory(_previousCurrentDirectory);
        }

        [SetUp]
        public void BaseSetup()
        {
            LogOutput = new StringBuilder();
            Log = new LoggerConfiguration()
                .WriteTo.TextWriter(new StringWriter(LogOutput), outputTemplate: "[{Level}] {Message}{NewLine}{Exception}", formatProvider: new StringFormatter(null))
                .CreateLogger();

            RootResource rootDocument = Substitute.For<RootResource>();
            rootDocument.ApiVersion = "2.0";
            rootDocument.Version = "2.0";
            rootDocument.Links.Add("Tenants", "http://tenants.org");

            Repository = Substitute.For<IOctopusAsyncRepository>();
            Repository.Client.RootDocument.Returns(rootDocument);

            ClientFactory = Substitute.For<IOctopusClientFactory>();

            RepositoryFactory = Substitute.For<IOctopusAsyncRepositoryFactory>();
            RepositoryFactory.CreateRepository(null).ReturnsForAnyArgs(Repository);

            FileSystem = Substitute.For<IOctopusFileSystem>();

            CommandLineArgs = new List<string>
            {
                "--server=http://the-server",
                "--apiKey=ABCDEF123456789"
            };
        }

        public StringBuilder LogOutput { get; set; }
        public string[] LogLines => LogOutput.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        public IOctopusClientFactory ClientFactory { get; set; }

        public ILogger Log { get; set; }

        public IOctopusAsyncRepositoryFactory RepositoryFactory { get; set; }

        public IOctopusAsyncRepository Repository { get; set; }

        public IOctopusFileSystem FileSystem { get; set; }

        public List<string> CommandLineArgs { get; set; }

        class StringFormatter : IFormatProvider
        {
            readonly IFormatProvider basedOn;
            public StringFormatter(IFormatProvider basedOn)
            {
                this.basedOn = basedOn;
            }

            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(string))
                {
                    return "s";
                }
                return this.basedOn;
            }
        }
    }
}
