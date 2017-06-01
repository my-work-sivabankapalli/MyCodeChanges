using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceProviderMVC.Controllers;
using System.Web.Mvc;

namespace ServiceProviderMVC.Test.Controllers
{
    /// <summary>
    /// Summary description for HomeControllerTest
    /// </summary>
    [TestClass]
    public class HomeControllerTest
    {
        public HomeControllerTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            //Assert.AreEqual("An unsecured page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Signoff()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Signoff() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Secured()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Secured() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Setup()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Setup() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
