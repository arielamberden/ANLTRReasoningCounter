using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Antlr.Runtime;
using Antlr.Runtime.Misc;

namespace ANTLRReasoningCounter
{
    class Scanner
    {
        public string filename { get; private set; }
        public string path { get; private set; }
        public int CharCt { get; private set; }
        public int WhitespaceCt { get; private set; }
        public int CommentCharCt { get; private set; }

        private Dictionary<string, Package> packages = new Dictionary<string, Package>();
        private Dictionary<string, Class> classes = new Dictionary<string, Class>();
        private Dictionary<string, Method> methods = new Dictionary<string, Method>();

        public Scanner(string p)
        {
            string[] temp = p.Split('\\');
            filename = temp.Last();
            temp = filename.Split('.');
            filename = temp[0];
            path = p;
            CharCt = 0;
            WhitespaceCt = 0;
            CommentCharCt = 0;
        }

        public void Scan()
        {
            Stream inputStream = File.OpenRead(path);
            //Stream inputStream = Console.OpenStandardInput();
            ANTLRInputStream input = new ANTLRInputStream(inputStream);
            Java_MITLexer lexer = new Java_MITLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            Java_MITParser parser = new Java_MITParser(tokens);
            parser.compilationUnit();

            // Get information
            CharCt = lexer.getCharCount();
            WhitespaceCt = lexer.getWhiteSpaceCount();
            CommentCharCt = lexer.getCommentCharCount();
            int PercentActiveChar = 0;
            if ((CharCt + WhitespaceCt + CommentCharCt) != 0) { PercentActiveChar = CharCt / (CharCt + WhitespaceCt + CommentCharCt); }

            packages = parser.getPackages();
            classes = parser.getClasses();
            methods = parser.getMethods();


            // Show File information
            Console.WriteLine("\n\n\nValues pulled from  " + path + ":");
            Console.WriteLine("File character count = " + lexer.getCharCount() + ".");
            Console.WriteLine("File white space count = " + lexer.getWhiteSpaceCount() + ".");
            Console.WriteLine("File comment character count = " + lexer.getCommentCharCount() + ".");
            Console.WriteLine("File percent active characters = " + PercentActiveChar.ToString() + ".");

            // Show Package information
            ShowPackageInfo();
            ShowClassInfo();
            ShowMethodInfo();

        }

        public void ShowPackageInfo()
        {
            Console.WriteLine("File Packages:");
            string p_out = "\t\tKeywords: ";
            foreach (KeyValuePair<string, Package> p in packages)
            {
                Console.WriteLine("\t" + p.Value.name);
                foreach (KeyValuePair<string, int> k in p.Value.getKeywords())
                {
                    p_out += "(" + k.Key + "," + k.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tUser Defined Identifiers: ";
                foreach (KeyValuePair<string, int> udi in p.Value.getUserDefinedIdentifiers())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tConstants: ";
                foreach (KeyValuePair<string, int> udi in p.Value.getConstants())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tSpecial Characters: ";
                foreach (KeyValuePair<char, int> udi in p.Value.getSpecialChars())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tKeywords: ";
            }
        }

        public void ShowClassInfo()
        {
            Console.WriteLine("File Classes:");
            string p_out = "\t\tKeywords: ";
            foreach (KeyValuePair<string, Class> c in classes)
            {
                Console.WriteLine("\t" + c.Value.name + " in package " + c.Value.containingPackage);
                foreach (KeyValuePair<string, int> k in c.Value.getKeywords())
                {
                    p_out += "(" + k.Key + "," + k.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tUser Defined Identifiers: ";
                foreach (KeyValuePair<string, int> udi in c.Value.getUserDefinedIdentifiers())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tConstants: ";
                foreach (KeyValuePair<string, int> udi in c.Value.getConstants())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tSpecial Characters: ";
                foreach (KeyValuePair<char, int> udi in c.Value.getSpecialChars())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tKeywords: ";
            }
        }

        public void ShowMethodInfo()
        {
            Console.WriteLine("File Methods:");
            string p_out = "\t\tKeywords: ";
            foreach (KeyValuePair<string, Method> m in methods)
            {
                Console.WriteLine("\t" + m.Value.name + " in class " + m.Value.containingClass + " in package " + m.Value.containingPackage);
                foreach (KeyValuePair<string, int> k in m.Value.getKeywords())
                {
                    p_out += "(" + k.Key + "," + k.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tUser Defined Identifiers: ";
                foreach (KeyValuePair<string, int> udi in m.Value.getUserDefinedIdentifiers())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tConstants: ";
                foreach (KeyValuePair<string, int> udi in m.Value.getConstants())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tSpecial Characters: ";
                foreach (KeyValuePair<char, int> udi in m.Value.getSpecialChars())
                {
                    p_out += "(" + udi.Key + "," + udi.Value + "), ";
                }
                Console.WriteLine(p_out);
                p_out = "\t\tKeywords: ";
            }
        }

        public List<SourceCodeFile> GetFileGrid()
        {
            List<SourceCodeFile> results = new List<SourceCodeFile>();

            List<string> packageNames = new List<string>(packages.Keys);
            List<string> classNames = new List<string>(classes.Keys);
            List<string> methodNames = new List<string>(methods.Keys);

            results.Add(new SourceCodeFile(-1, filename, CharCt, WhitespaceCt, CommentCharCt, path, packageNames, classNames, methodNames));

            return results;
        }

        public List<PackageInfo> GetPackageGrid()
        {
            List<PackageInfo> results = new List<PackageInfo>();

            int uniqueKeyword, uniqueUserDefinedIdentifier, uniqueConstant, uniqueSpecialChar, totalKeywords, totalUDIs, totalConstants, totalSpecialChars;
            List<string> filenames = new List<string>() { filename };
            List<string> classnames = new List<string>();
            List<string> methodnames = new List<string>();

            foreach (KeyValuePair<string, Package> p in packages)
            {
                uniqueKeyword = p.Value.getKeywords().Count;
                uniqueUserDefinedIdentifier = p.Value.getUserDefinedIdentifiers().Count;
                uniqueConstant = p.Value.getConstants().Count;
                uniqueSpecialChar = p.Value.getSpecialChars().Count;

                totalKeywords = GetSumOfCounts(p.Value.getKeywords());
                totalUDIs = GetSumOfCounts(p.Value.getUserDefinedIdentifiers());
                totalConstants = GetSumOfCounts(p.Value.getConstants());
                totalSpecialChars = GetSumOfCounts(p.Value.getSpecialChars());

                foreach (KeyValuePair<string, Class> c in classes)
                {
                    if (c.Value.containingPackage == p.Value.name) { classnames.Add(c.Value.name); }
                }

                foreach (KeyValuePair<string, Method> m in methods)
                {
                    if (m.Value.containingPackage == p.Value.name) { methodnames.Add(m.Value.name); }
                }

                if (p.Value.name.ToString().Equals("OutsideOfPackage") == false)
                {
                    results.Add(new PackageInfo(-1, p.Value.name, uniqueKeyword, uniqueUserDefinedIdentifier, uniqueConstant, uniqueSpecialChar, totalKeywords, totalUDIs, totalConstants, totalSpecialChars, filenames, classnames, methodnames));
                }
            }

            return results;
        }

        public List<ClassInfo> GetClassGrid()
        {
            List<ClassInfo> results = new List<ClassInfo>();

            int uniqueKeyword, uniqueUserDefinedIdentifier, uniqueConstant, uniqueSpecialChar, totalKeywords, totalUDIs, totalConstants, totalSpecialChars;
            List<string> filenames = new List<string>() { filename };
            string packagename;
            List<string> methodnames = new List<string>();

            foreach (KeyValuePair<string, Class> c in classes)
            {
                uniqueKeyword = c.Value.getKeywords().Count;
                uniqueUserDefinedIdentifier = c.Value.getUserDefinedIdentifiers().Count;
                uniqueConstant = c.Value.getConstants().Count;
                uniqueSpecialChar = c.Value.getSpecialChars().Count;

                totalKeywords = GetSumOfCounts(c.Value.getKeywords());
                totalUDIs = GetSumOfCounts(c.Value.getUserDefinedIdentifiers());
                totalConstants = GetSumOfCounts(c.Value.getConstants());
                totalSpecialChars = GetSumOfCounts(c.Value.getSpecialChars());

                packagename = c.Value.containingPackage;

                foreach (KeyValuePair<string, Method> m in methods)
                {
                    if (m.Value.containingPackage == c.Value.name) { methodnames.Add(m.Value.name); }
                }

                if (c.Value.name.ToString().Equals("OutsideOfClass") == false)
                {
                    results.Add(new ClassInfo(-1, c.Value.name, uniqueKeyword, uniqueUserDefinedIdentifier, uniqueConstant, uniqueSpecialChar, totalKeywords, totalUDIs, totalConstants, totalSpecialChars, packagename, methodnames, filenames));
                }
            }

            return results;
        }

        public List<MethodInfo> GetMethodGrid()
        {
            List<MethodInfo> results = new List<MethodInfo>();

            int uniqueKeyword, uniqueUserDefinedIdentifier, uniqueConstant, uniqueSpecialChar, totalKeywords, totalUDIs, totalConstants, totalSpecialChars;
            List<string> filenames = new List<string>() { filename };
            string packagename, classname;

            foreach (KeyValuePair<string, Method> m in methods)
            {
                uniqueKeyword = m.Value.getKeywords().Count;
                uniqueUserDefinedIdentifier = m.Value.getUserDefinedIdentifiers().Count;
                uniqueConstant = m.Value.getConstants().Count;
                uniqueSpecialChar = m.Value.getSpecialChars().Count;

                totalKeywords = GetSumOfCounts(m.Value.getKeywords());
                totalUDIs = GetSumOfCounts(m.Value.getUserDefinedIdentifiers());
                totalConstants = GetSumOfCounts(m.Value.getConstants());
                totalSpecialChars = GetSumOfCounts(m.Value.getSpecialChars());

                packagename = m.Value.containingPackage;
                classname = m.Value.containingClass;

                if (m.Value.name.ToString().Equals("OutsideOfMethod") == false)
                {
                    results.Add(new MethodInfo(-1, m.Value.name, uniqueKeyword, uniqueUserDefinedIdentifier, uniqueConstant, uniqueSpecialChar, totalKeywords, totalUDIs, totalConstants, totalSpecialChars, packagename, classname, filenames));
                }
            }

            return results;
        }

        public int GetSumOfCounts(Dictionary<string, int> dict)
        {
            int sum = 0;
            foreach (KeyValuePair<string, int> p in dict)
            {
                sum += p.Value;
            }
            return sum;
        }

        public int GetSumOfCounts(Dictionary<char, int> dict)
        {
            int sum = 0;
            foreach (KeyValuePair<char, int> p in dict)
            {
                sum += p.Value;
            }
            return sum;
        }

        public Dictionary<string, Package> GetPackages() { return packages; }
        public Dictionary<string, Class> GetClasses() { return classes; }
        public Dictionary<string, Method> GetMethods() { return methods; }
    }

    public class Package
    {
        public string name { get; set; }
        private Dictionary<string, int> keywords = new Dictionary<string, int>();
        private Dictionary<string, int> udis = new Dictionary<string, int>();
        private Dictionary<string, int> constants = new Dictionary<string, int>();
        private Dictionary<char, int> special_chars = new Dictionary<char, int>();

        public Package(string n)
        {
            name = n;
        }

        public void addKeyword(string k)
        {
            if (this.keywords.Keys.Contains(k)) { keywords[k]++; }
            else { keywords[k] = 1; }
        }

        public void addUserDefinedIdentifier(string u)
        {
            if (u != null)
            {
                if (this.udis.Keys.Contains(u)) { udis[u]++; }
                else { udis[u] = 1; }
            }
        }

        public void addConstant(string c)
        {
            if (this.constants.Keys.Contains(c)) { constants[c]++; }
            else { constants[c] = 1; }
        }

        public void addSpecialCharacter(char s)
        {
            if (this.special_chars.Keys.Contains(s)) { special_chars[s]++; }
            else { special_chars[s] = 1; }
        }

        public Dictionary<string, int> getKeywords() { return keywords; }
        public Dictionary<string, int> getUserDefinedIdentifiers() { return udis; }
        public Dictionary<string, int> getConstants() { return constants; }
        public Dictionary<char, int> getSpecialChars() { return special_chars; }
    }

    public class Class
    {
        public string name { get; set; }
        public string containingPackage { get; set; }
        private Dictionary<string, int> keywords = new Dictionary<string, int>();
        private Dictionary<string, int> udis = new Dictionary<string, int>();
        private Dictionary<string, int> constants = new Dictionary<string, int>();
        private Dictionary<char, int> special_chars = new Dictionary<char, int>();

        public Class(string n, string p)
        {
            name = n;
            containingPackage = p;
        }

        public void addKeyword(string k)
        {
            if (this.keywords.Keys.Contains(k)) { keywords[k]++; }
            else { keywords[k] = 1; }
        }

        public void addUserDefinedIdentifier(string u)
        {
            if (u != null)
            {
                if (this.udis.Keys.Contains(u)) { udis[u]++; }
                else { udis[u] = 1; }
            }
        }

        public void addConstant(string c)
        {
            if (this.constants.Keys.Contains(c)) { constants[c]++; }
            else { constants[c] = 1; }
        }

        public void addSpecialCharacter(char s)
        {
            if (this.special_chars.Keys.Contains(s)) { special_chars[s]++; }
            else { special_chars[s] = 1; }
        }

        public Dictionary<string, int> getKeywords() { return keywords; }
        public Dictionary<string, int> getUserDefinedIdentifiers() { return udis; }
        public Dictionary<string, int> getConstants() { return constants; }
        public Dictionary<char, int> getSpecialChars() { return special_chars; }
    }

    public class Method
    {
        public string name { get; set; }
        public string containingPackage { get; set; }
        public string containingClass { get; set; }
        private Dictionary<string, int> keywords = new Dictionary<string, int>();
        private Dictionary<string, int> udis = new Dictionary<string, int>();
        private Dictionary<string, int> constants = new Dictionary<string, int>();
        private Dictionary<char, int> special_chars = new Dictionary<char, int>();

        public Method(string n, string p, string c)
        {
            name = n;
            containingPackage = p;
            containingClass = c;
        }

        public void addKeyword(string k)
        {
            if (this.keywords.Keys.Contains(k)) { keywords[k]++; }
            else { keywords[k] = 1; }
        }

        public void addUserDefinedIdentifier(string u)
        {
            if (u != null)
            {
                if (this.udis.Keys.Contains(u)) { udis[u]++; }
                else { udis[u] = 1; }
            }
        }

        public void addConstant(string c)
        {
            if (this.constants.Keys.Contains(c)) { constants[c]++; }
            else { constants[c] = 1; }
        }

        public void addSpecialCharacter(char s)
        {
            if (this.special_chars.Keys.Contains(s)) { special_chars[s]++; }
            else { special_chars[s] = 1; }
        }

        public Dictionary<string, int> getKeywords() { return keywords; }
        public Dictionary<string, int> getUserDefinedIdentifiers() { return udis; }
        public Dictionary<string, int> getConstants() { return constants; }
        public Dictionary<char, int> getSpecialChars() { return special_chars; }
    }

}
