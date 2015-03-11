using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuttleText
{
    public abstract class TokenMorpherBase
    {
        public abstract string MorphToken(string curToken, int pos, string unmorphed, StringBuilder partiallyBuiltOutput);
    }
}
