using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CuttleText
{
    public class Hydrator
    {
        public Hydrator(ContextBase context)
        {
            _context = context;
        }

        ContextBase _context;

        const string INJECTOR_TOKEN = "// --> ";

        List<TokenMorpherBase> _tokenMorphers = new List<TokenMorpherBase>();
        List<InjectorBase> _injectors = new List<InjectorBase>();
        Dictionary<string, string> _stringSubsts = new Dictionary<string, string>();

        public void AddInjector(InjectorBase injector)
        {
            _injectors.Add(injector);
        }

        public void AddTokenMorpher(TokenMorpherBase tm)
        {
            _tokenMorphers.Add(tm);
        }
        
        public void AddStringSubst(string srcCaseInsen, string dest)
        {
            if (_stringSubsts.ContainsKey(srcCaseInsen)) return;
            _stringSubsts.Add(srcCaseInsen, dest);
        }

        /// <summary>
        /// default hydrators are set up, usually with 1.0 --> 1.0f conversion... but for project files
        /// and maybe other files, that makes no sense... so in those "more special" cases, clear them manually using this.
        /// </summary>
        public void Clear()
        {
            _tokenMorphers.Clear();
            _injectors.Clear();
            _stringSubsts.Clear();
        }

       

        int ProcessInjectorLine(string filenameforErrors, int lineNum, StringBuilder dest, string line)
        {
            // grab the whitespace to the left of the token
            int injectorTokenNdx = line.IndexOf(INJECTOR_TOKEN);

            string whiteSpace = line.Substring(0, injectorTokenNdx);

            string removedToken = line.Replace(INJECTOR_TOKEN, "");
            removedToken = removedToken.Trim();
            string[] args = removedToken.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (args.Length == 0) return 0; // weird, they put //--> on a line all by itself.

            string injectorName = args[0];

            InjectorBase injector = null;
            string injectorMatchedID = "";

            foreach (InjectorBase inj in _injectors)
            {
                List<string> ids = inj.CodeIdentifiers;
                if (ids == null || ids.Count < 1)
                {
                    ids = new List<string>();
                    ids.Add(inj.GetType().Name);
                }
                foreach (string thisID in ids)
                {
                    if (string.Compare(injectorName, thisID, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        injector = inj;
                        injectorMatchedID = thisID;
                        break;
                    }
                }
                if (injector != null) break;
            }

            if (injector == null)
            {
                if (_context != null) _context.LogMessage(ContextBase.eLogCatetory.InternalError, filenameforErrors, lineNum, "Unknown injector: " + injectorName);
                dest.AppendLine(line + " INJECTOR NOT FOUND ???????? ");
                return 1;
            }

            injector.Setup(_context, whiteSpace, dest, filenameforErrors, lineNum, injectorMatchedID);

            List<string> argList = new List<string>();
            argList.AddRange(args);
            injector.Inject(argList);
            return injector.NumLinesWritten;
        }

        public string HydrateIntoString(string template, string fakeFileName = "MemoryFile.cs")
        {
            byte[] templateByteArray = Encoding.UTF8.GetBytes(template);
            MemoryStream templateMemStream = new MemoryStream(templateByteArray);
            return HydrateIntoString(templateMemStream, fakeFileName);
        }

        public string HydrateIntoString(Stream templateStream, string fakeFileName = "MemoryFile.cs")
        {
            if (templateStream == null) { return ""; }
            StringWriter memStreamWriter = new StringWriter();
            HydrateStream(templateStream, memStreamWriter, fakeFileName, true);
            return memStreamWriter.ToString();
        }

        public void HydrateStream(Stream templateStream, TextWriter dest, string fullPathToDestForErrorLogging, bool flushOnFinish)
        {
            StringBuilder instream = new StringBuilder();
            StreamReader templateStreamReader = new StreamReader(templateStream);
            int lineNum = 0;
            int destLineNum = 0; // line numbers in generated file will be diff after 1st injector.
            while (!templateStreamReader.EndOfStream)
            {
                string line = templateStreamReader.ReadLine();
                if (line.Contains(INJECTOR_TOKEN))
                {
                    destLineNum += ProcessInjectorLine(fullPathToDestForErrorLogging, destLineNum, instream, line);
                }
                else
                {
                    foreach (string key in _stringSubsts.Keys)
                    {
                        line = line.Replace(key, _stringSubsts[key]);
                    }
                    instream.AppendLine(line);
                    destLineNum++;
                }
                lineNum++;
            }

            // now run the token morphers against the fully injected code.
            string unmorphed = instream.ToString();

            string morphed = ApplyTokenMorphers(unmorphed);
            dest.Write(morphed);
            if (flushOnFinish) dest.Flush();
        }

        private string ApplyTokenMorphers(string unmorphed)
        {
            if (_tokenMorphers.Count == 0)
            {
                return unmorphed;
            }
            List<char> punctuators = new List<char> { '{', '}', '[', ']', '(', ')', '.', ',', ':', ';', '+', '-', '*', '/', '%', '&', '|', '^', '!', '~',
                                                      '=', '<', '>', '?', '\'', '\"' };

            StringBuilder sb = new StringBuilder(8192);

            char[] chars = unmorphed.ToCharArray();
            int startToken = 0;
            bool inQuotes = false;
            for (int q = 0; q < chars.Length; ++q)
            {
                char curchar = chars[q];
                if (curchar == '\"')
                {
                    if (q == 0 || chars[q - 1] != '\\')
                    {
                        inQuotes = !inQuotes;
                        if (!inQuotes)
                        {
                            // we just hit close quote, move up startToken
                            startToken = q+1;
                        }
                        else
                        {
                            // we just hit an open quote, write out everything up to this point
                            // end of a token
                            if ((q - startToken) > 0)
                            {
                                string curToken = unmorphed.Substring(startToken, (q - startToken));

                                // morph it!
                                sb.Append(ApplyAllTokenizersToToken(curToken, startToken, unmorphed, sb));
                            }
                        }
                        sb.Append(curchar);
                        continue;
                    }
                }
                if (inQuotes)
                {
                    sb.Append(curchar);
                    continue;
                }

                if (punctuators.Contains(curchar))
                {

                    if ((q - startToken) > 0)
                    {
                        // end of a token
                        string curToken = unmorphed.Substring(startToken, (q - startToken));

                        // morph it!
                        sb.Append(ApplyAllTokenizersToToken(curToken, startToken, unmorphed, sb));
                    }

                    // append the actual token seperator char (this is never morphed)
                    sb.Append(curchar);

                    startToken = q+1;
                }
            }

            if (chars.Length - 1 > startToken)
            {
                string lastToken = unmorphed.Substring(startToken, (chars.Length - 1) - startToken);
                // morph it!
                
                sb.Append(ApplyAllTokenizersToToken(lastToken, startToken, unmorphed, sb));
            }
            return sb.ToString();
        }

        string ApplyAllTokenizersToToken(string token, int startPos, string unmorphed, StringBuilder partiallyAssembledOutput)
        {
            string finalToken = token;
            foreach (var tokenizer in _tokenMorphers)
            {
                finalToken = tokenizer.MorphToken(finalToken, startPos, unmorphed, partiallyAssembledOutput);
            }
            return finalToken;
        }
    }
}
