using System;
using CuttleText;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttleTextTests
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public void BasicHelloWorldOutputsCorrectly()
        {
            // uses the HelloWorld injector to hydrate the given template into a final hello world string.
            string template = @"
                {
                    // --> HelloWorld
                }
            ";

            Hydrator hydra = new Hydrator(null);
            hydra.AddInjector(new HelloWorld());

            string result = hydra.HydrateIntoString(template);

            const string expectedResult = @"
                {
                    Hello World!
                    Hello World!
                    Hello World!
                }
            ";

            Assert.AreEqual(result.Trim(), expectedResult.Trim());
        }

        [TestMethod]
        public void HelloWorldSeesArgumentsCorrectly()
        {
            // uses the HelloWorld injector to hydrate the given template into a final hello world string.
            string template = @"
                {
                    // --> HelloWorld ThisIsAnInjectorArgument
                }
            ";

            Hydrator hydra = new Hydrator(null);
            hydra.AddInjector(new HelloWorld());

            string result = hydra.HydrateIntoString(template);

            const string expectedResult = @"
                {
                    ThisIsAnInjectorArgument
                    ThisIsAnInjectorArgument
                    ThisIsAnInjectorArgument
                }
            ";

            Assert.AreEqual(result.Trim(), expectedResult.Trim());
        }

        [TestMethod]
        public void HelloWorldUsingACustomContext()
        {
            // uses the HelloWorld injector to hydrate the given template into a final hello world string.
            string template = @"
                {
                    // --> TestContextInjector
                }
            ";

            Hydrator hydra = new Hydrator(new TestContext() { MyCustomField = "customFieldSetInContext" } );
            hydra.AddInjector(new TestContextInjector());

            string result = hydra.HydrateIntoString(template);

            const string expectedResult = @"
                {
                    customFieldSetInContext
                }
            ";

            Assert.AreEqual(result.Trim(), expectedResult.Trim());
        }

        [TestMethod]
        public void SimpleTextSubstitution()
        {
            // uses the HelloWorld injector to hydrate the given template into a final hello world string.
            string template = @"
                {
                    this is the original string.
                }
            ";

            Hydrator hydra = new Hydrator(null);
            hydra.AddStringSubst("original", "modified");
            string result = hydra.HydrateIntoString(template);

            const string expectedResult = @"
                {
                    this is the modified string.
                }
            ";

            Assert.AreEqual(result.Trim(), expectedResult.Trim());
        }
    }
}
