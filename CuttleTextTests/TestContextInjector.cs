using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CuttleText;

namespace CuttleTextTests
{
    class TestContextInjector : InjectorBase
    {
        public override void Inject(List<string> cmdLine)
        {
            TestContext ctx = (TestContext)_context;
            Write(ctx.MyCustomField);
        }
    }
}
