﻿using System;
using System.IO;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.ServiceLocation;

using GalaSoft.MvvmLight.Ioc;

using FakeHttp;

namespace BingGeoCoder.Client.UnitTests
{
    [TestClass]
    public class AssemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // set the http message handler factory to the mode we want for the entire assmebly test execution
            MessageHandlerFactory.Mode = MessageHandlerMode.Automatic;

            // setup IOC so test classes can get the shared message handler
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // folders where fake responses are stored and where captured response should be saved
            var fakeFolder = context.DeploymentDirectory; // the folder where the unit tests are running
            var captureFolder = Path.Combine(context.TestRunDirectory, @"..\..\FakeResponses\"); // kinda hacky but this should be the solution folder

            // here we don't want to serialize or include our api key in response lookups so
            // pass a lambda that will indicate to the serialzier to filter that param out
            var store = new FileSystemResponseStore(fakeFolder, captureFolder, (name, value) => name.Equals("key", StringComparison.InvariantCultureIgnoreCase));

            SimpleIoc.Default.Register<HttpMessageHandler>(() => MessageHandlerFactory.CreateMessageHandler(store));
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (SimpleIoc.Default.IsRegistered<HttpMessageHandler>())
            {
                SimpleIoc.Default.GetInstance<HttpMessageHandler>().Dispose();
            }
        }
    }
}