using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CuttleText;

namespace CuttleTextTests
{
    public class HelloWorld : InjectorBase
    {
        public override void Inject(List<string> cmdLine)
        {
            for (int q = 0; q < 3; ++q)
            {
                if (cmdLine.Count == 1)
                {
                    Write("Hello World!");
                }
                else if (cmdLine.Count == 2)
                {
                    Write(cmdLine[1]);
                }
            }
        }
    }
}
