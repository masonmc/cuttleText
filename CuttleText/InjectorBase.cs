using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuttleText
{
    public abstract class InjectorBase
    {
        int _numLinesWritten = 0;
        public int NumLinesWritten { get { return _numLinesWritten; } }
        protected string _injectorMatchedID = "";

        protected ContextBase _context;
        public ContextBase Context { get { return _context; } }

        /// <summary>
        /// easier than passing in constuctor arguments through reflection.
        /// </summary>
        /// <param name="linePrefix"></param>
        public void Setup(ContextBase context, string linePrefix, StringBuilder dest, string filename, int lineNum, string injectorMatchedID)
        {
            _context = context;
            _injectorMatchedID = injectorMatchedID;
            _linePrefix = linePrefix;
            _dest = dest;
            _lineNum = lineNum;
            _filename = filename;
        }


        StringBuilder _dest = null;
        string _linePrefix = "";

        // for error reporting
        string _filename = "";
        int _lineNum;

        public string Filename { get { return _filename; } }
        public int LineNum { get { return _lineNum + _numLinesWritten; } }

        // if this is null, the type name is used
        public virtual List<string> CodeIdentifiers { get { return null; } }
        int _indentLevel = 0;
        string _indentStr = "";

        public void Write(string line)
        {
            _dest.AppendLine(_linePrefix + _indentStr + line);
            _numLinesWritten++;
        }

        void BuildIndentString()
        {
            _indentStr = "";
            for (int q = 0; q < _indentLevel; ++q)
            {
                _indentStr += "\t";
            }
        }
        public void Indent()
        {
            _indentLevel++;
            BuildIndentString();
        }

        public void Unindent()
        {
            _indentLevel--; if (_indentLevel < 0) _indentLevel = 0;
            BuildIndentString();
        }

        public void OpenBracket()
        {
            Write("{");
            Indent();
        }

        public void OpenBracket(string rightOfBracketTextInclSlashes)
        {
            Write("{ " + rightOfBracketTextInclSlashes);
            Indent();
        }

        public void CloseBracket()
        {
            Unindent();
            Write("}");
        }

        public void CloseBracket(string rightOfBracketTextInclSlashes)
        {
            Unindent();
            Write("} " + rightOfBracketTextInclSlashes);
        }
        public void FatalError(string message)
        {
            _context.LogMessage(ContextBase.eLogCatetory.FatalError, _filename, _lineNum + _numLinesWritten, message);
        }
        public void Error(string message)
        {
            _context.LogMessage(ContextBase.eLogCatetory.Error, _filename, _lineNum + _numLinesWritten, message);
        }
        public void InternalError(string message)
        {
            _context.LogMessage(ContextBase.eLogCatetory.InternalError, _filename, _lineNum + _numLinesWritten, message);
        }
        public void Warning(string message)
        {
            _context.LogMessage(ContextBase.eLogCatetory.Warning, _filename, _lineNum + _numLinesWritten, message);
        }
        public void Info(string message)
        {
            _context.LogMessage(ContextBase.eLogCatetory.Info, _filename, _lineNum + _numLinesWritten, message);
        }
        
        public abstract void Inject(List<string> cmdLine);
    }
}
