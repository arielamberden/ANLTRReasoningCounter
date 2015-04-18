using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ANTLRReasoningCounter
{
    class SourceCodeInfoInterfaceClass
    {

        public SourceCodeInfoInterfaceClass() { }

        public static void ResetDB()
        {
            string result = "\n\n";

            result += DropTable("File_Package");
            result += DropTable("File_Class");
            result += DropTable("File_Method");

            result += DropTable("Method_Keyword");
            result += DropTable("Method_UserDefinedIdentifier");
            result += DropTable("Method_Constant");
            result += DropTable("Method_Special_Character");
            result += DropTable("Methods");

            result += DropTable("Class_Keyword");
            result += DropTable("Class_UserDefinedIdentifier");
            result += DropTable("Class_Constant");
            result += DropTable("Class_Special_Character");
            result += DropTable("Classes");
 
            result += DropTable("Package_Keyword");
            result += DropTable("Package_UserDefinedIdentifier");
            result += DropTable("Package_Constant");
            result += DropTable("Package_Special_Character");
            result += DropTable("Packages");

            result += DropTable("File_Keyword");
            result += DropTable("File_UserDefinedIdentifier");
            result += DropTable("File_Constant");
            result += DropTable("File_Special_Character");
            result += DropTable("Files");

            result += CreateTable("Packages", CREATE_TABLE_PACKAGES);
            result += CreateTable("Package_Keyword", CREATE_TABLE_PACKAGE_KEYWORD);
            result += CreateTable("Package_UserDefinedIdentifier", CREATE_TABLE_PACKAGE_USERDEFINEDIDENTIFIER);
            result += CreateTable("Package_Constant", CREATE_TABLE_PACKAGE_CONSTANT);
            result += CreateTable("Package_Special_Character", CREATE_TABLE_PACKAGE_SPECIALCHARACTER);

            result += CreateTable("Classes", CREATE_TABLE_CLASSES);
            result += CreateTable("Class_Keyword", CREATE_TABLE_CLASS_KEYWORD);
            result += CreateTable("Class_UserDefinedIdentifier", CREATE_TABLE_CLASS_USERDEFINEDIDENTIFIER);
            result += CreateTable("Class_Constant", CREATE_TABLE_CLASS_CONSTANT);
            result += CreateTable("Class_Special_Character", CREATE_TABLE_CLASS_SPECIALCHARACTER);

            result += CreateTable("Methods", CREATE_TABLE_METHODS);
            result += CreateTable("Method_Keyword", CREATE_TABLE_METHOD_KEYWORD);
            result += CreateTable("Method_UserDefinedIdentifier", CREATE_TABLE_METHOD_USERDEFINEDIDENTIFIER);
            result += CreateTable("Method_Constant", CREATE_TABLE_METHOD_CONSTANT);
            result += CreateTable("Method_Special_Character", CREATE_TABLE_METHOD_SPECIALCHARACTER);

            result += CreateTable("Files", CREATE_TABLE_FILES);
            result += CreateTable("File_Keyword", CREATE_TABLE_FILE_KEYWORD);
            result += CreateTable("File_UserDefinedIdentifier", CREATE_TABLE_FILE_USERDEFINEDIDENTIFIER);
            result += CreateTable("File_Constant", CREATE_TABLE_FILE_CONSTANT);
            result += CreateTable("File_Special_Character", CREATE_TABLE_FILE_SPECIALCHARACTER);

            result += CreateTable("File_Package", CREATE_TABLE_FILE_PACKAGE);
            result += CreateTable("File_Class", CREATE_TABLE_FILE_CLASS);
            result += CreateTable("File_Method", CREATE_TABLE_FILE_METHOD);

            Console.Write(result + "\n\n");
        }

        private static bool TableExists(string table_name)
        {
            bool exists = false;
            string cmdStr = "select case when exists((select * from information_schema.tables where table_name = '" + table_name + "')) then 1 else 0 end";
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(cmdStr, conn);

            try
            {
                conn.Open();
                exists = (int)cmd.ExecuteScalar() == 1;
            }
            catch (System.Exception ex)
            {
                exists = false;
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return exists;
        }

        private static string DropTable(string table_name)
        {
            string result = "\n" + table_name + " does not exist.";

            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand("DROP TABLE " + table_name + ";", conn);

            if (TableExists(table_name) == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    result = "\n" + table_name + " dropped successfully.";
                }
                catch (System.Exception ex)
                {
                    result = "\n" + ex.ToString();
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
            return result;
        }

        private static string CreateTable(string table_name, string creation_string)
        {
            string result = "";

            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(creation_string, conn);

            if (TableExists(table_name) != true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    result += "\n" + table_name + " added successfully.";
                }
                catch (System.Exception ex)
                {
                    result += "\n" + ex.ToString();
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
            return result;
        }

        public static List<SourceCodeFile> GetFileData()
        {
            List < SourceCodeFile > results = new List<SourceCodeFile>();
            List<string> packagesInFile = new List<string>();
            List<string> classesInFile = new List<string>();
            List<string> methodsInFile = new List<string>();

            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(FILES_QUERY, conn);
            SqlDataReader reader;
            //Console.WriteLine("\n" + FILES_QUERY + "\n");
            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord) reader;
                    packagesInFile = GetPackagesInFile(r[1].ToString());
                    classesInFile = GetClassesInFile(r[1].ToString());
                    methodsInFile = GetMethodsInFile(r[1].ToString());
                    results.Add(new SourceCodeFile((int)r.GetValue(0), r[1].ToString(), (int)r.GetValue(3), (int)r.GetValue(4), (int)r.GetValue(5), r[2].ToString(), packagesInFile, classesInFile, methodsInFile));
                }
                reader.Close();
                //Console.WriteLine("\nFile data retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetFileData.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return results;
        }

        public static List<string> GetPackagesInFile(string filename)
        {
            List<string> results = new List<string>();

            string query = @"SELECT P.Name 
                        FROM Packages as P 
                        JOIN File_Package as FP ON P.Package_ID = FP.Package_ID 
                        JOIN Files as F ON F.File_ID = FP.File_ID
                        WHERE F.Name = '" + filename + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nPackages in file retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetPackagesInFile.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetClassesInFile(string filename)
        {
            List<string> results = new List<string>();

            string query = @"SELECT C.Name 
                        FROM Classes as C 
                        JOIN File_Class as FC ON C.Class_ID = FC.Class_ID 
                        JOIN Files as F ON F.File_ID = FC.File_ID
                        WHERE F.Name = '" + filename + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close(); 
                //Console.WriteLine("\nClasses in file retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetClassesInFile.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetMethodsInFile(string filename)
        {
            List<string> results = new List<string>();

            string query = @"SELECT M.Name 
                        FROM Methods as M 
                        JOIN File_Method as FM ON M.Method_ID = FM.Method_ID 
                        JOIN Files as F ON F.File_ID = FM.File_ID
                        WHERE F.Name = '" + filename + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nMethods in file retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetMethodsInFile.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<PackageInfo> GetPackageData()
        {
            List<PackageInfo> results = new List<PackageInfo>();
            List<string> filesPresentIn = new List<string>();
            List<string> classesInPackage = new List<string>();
            List<string> methodsInPackage = new List<string>();

            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(PACKAGES_QUERY, conn);
            SqlDataReader reader;
            //Console.WriteLine("\n" + PACKAGES_QUERY + "\n");
            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    filesPresentIn = GetFilesContainingPackage(r[1].ToString());
                    classesInPackage = GetClassesInPackage(r[1].ToString());
                    methodsInPackage = GetMethodsInPackage(r[1].ToString());
                    if (r[1].ToString().CompareTo("OutsideOfPackage") != 0)
                    {
                        PackageInfo temp = new PackageInfo((int)r.GetValue(0), r[1].ToString(), (int)r.GetValue(2), (int)r.GetValue(3), (int)r.GetValue(4), (int)r.GetValue(5), (int)r.GetValue(6), (int)r.GetValue(7), (int)r.GetValue(8), (int)r.GetValue(9), filesPresentIn, classesInPackage, methodsInPackage);
                        results.Add(temp);
                    }
                    //Console.WriteLine(temp.ToString());
                }
                reader.Close();
                //Console.WriteLine("\nPackage data retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetPackageData.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetFilesContainingPackage(string package_name)
        {
            List<string> results = new List<string>();

            string query = @"SELECT F.Name 
                        FROM Files as F 
                        JOIN File_Package as FP ON F.File_ID = FP.File_ID 
                        JOIN Packages as P ON P.Package_ID = FP.Package_ID
                        WHERE P.Name = '" + package_name + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nFiles containing package retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetFilesContainingPackage.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetClassesInPackage(string package_name)
        {
            List<string> results = new List<string>();

            string query = @"SELECT C.Name 
                        FROM Classes as C 
                        JOIN Packages as P ON P.Package_ID = C.Package_ID
                        WHERE P.Name = '" + package_name + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nClasses in package retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetClassesInPackage.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetMethodsInPackage(string package_name)
        {
            List<string> results = new List<string>();

            string query = @"SELECT M.Name 
                        FROM Methods as M 
                        JOIN Packages as P ON P.Package_ID = M.Package_ID
                        WHERE P.Name = '" + package_name + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nMethods in package retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetMethodsInPackage.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<ClassInfo> GetClassData()
        {
            List<ClassInfo> results = new List<ClassInfo>();
            List<string> filesPresentIn = new List<string>();
            List<string> methodsInClass = new List<string>();

            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(CLASSES_QUERY, conn);
            SqlDataReader reader;
            //Console.WriteLine("\n" + CLASSES_QUERY + "\n");
            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    filesPresentIn = GetFilesContainingClass(r[1].ToString());
                    methodsInClass = GetMethodsInClass(r[1].ToString());
                    if (r[1].ToString().CompareTo("OutsideOfClass") != 0)
                    {
                        ClassInfo temp = new ClassInfo((int)r.GetValue(0), r[1].ToString(), (int)r.GetValue(2), (int)r.GetValue(3), (int)r.GetValue(4), (int)r.GetValue(5), (int)r.GetValue(6), (int)r.GetValue(7), (int)r.GetValue(8), (int)r.GetValue(9), r[10].ToString(), methodsInClass, filesPresentIn);
                        results.Add(temp);
                    }
                    //Console.WriteLine(temp.ToString());
                }
                reader.Close();
                //Console.WriteLine("\nClass data retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetClassData.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetFilesContainingClass(string class_name)
        {
            List<string> results = new List<string>();

            string query = @"SELECT F.Name 
                        FROM Files as F 
                        JOIN File_Class as FC ON F.File_ID = FC.File_ID 
                        JOIN Classes as C ON C.Class_ID = FC.Class_ID
                        WHERE C.Name = '" + class_name + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nFiles containing class retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetFilesContainingClass.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetMethodsInClass(string class_name)
        {
            List<string> results = new List<string>();

            string query = @"SELECT M.Name 
                        FROM Methods as M 
                        JOIN Classes as C ON C.Class_ID = M.Class_ID
                        WHERE C.Name = '" + class_name + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nMethods in class retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetMethodsInClass.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<MethodInfo> GetMethodData()
        {
            List<MethodInfo> results = new List<MethodInfo>();
            List<string> filesPresentIn = new List<string>();

            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(METHODS_QUERY, conn);
            SqlDataReader reader;
            //Console.WriteLine("\n" + METHODS_QUERY + "\n");
            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    filesPresentIn = GetFilesContainingMethod(r[1].ToString());
                    if (r[1].ToString().Equals("OutsideOfMethod") == false)
                    {
                        MethodInfo temp = new MethodInfo((int)r.GetValue(0), r[1].ToString(), (int)r.GetValue(2), (int)r.GetValue(3), (int)r.GetValue(4), (int)r.GetValue(5), (int)r.GetValue(6), (int)r.GetValue(7), (int)r.GetValue(8), (int)r.GetValue(9), r[10].ToString(), r[11].ToString(), filesPresentIn);
                        results.Add(temp);
                    }
                    //Console.WriteLine(temp.ToString());
                }
                reader.Close();
                //Console.WriteLine("\nMethod data retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetMethodData.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static List<string> GetFilesContainingMethod(string method_name)
        {
            List<string> results = new List<string>();

            string query = @"SELECT F.Name 
                        FROM Files as F 
                        JOIN File_Method as FM ON F.File_ID = FM.File_ID 
                        JOIN Methods as M ON M.Method_ID = FM.Method_ID
                        WHERE M.Name = '" + method_name + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[0].ToString());
                }
                reader.Close();
                //Console.WriteLine("\nFiles containing method retrieved successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetFilesContainingMethod.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return results;
        }

        public static void AddScanToDB(Scanner scanner)
        {
            string result = "\n\n";

            result += InsertIntoFiles(scanner);
            result += InsertIntoFileTables(scanner.GetPackages(), scanner.filename);

            result += InsertIntoPackages(scanner.GetPackages());
            result += InsertIntoFile_Package(GetFileID(scanner.filename), scanner.GetPackages());
            result += InsertIntoPackageTables(scanner.GetPackages(), scanner.filename);

            result += InsertIntoClasses(scanner.GetClasses());
            result += InsertIntoFile_Class(GetFileID(scanner.filename), scanner.GetClasses());
            result += InsertIntoClassTables(scanner.GetClasses(), scanner.filename);

            result += InsertIntoMethods(scanner.GetMethods());
            result += InsertIntoFile_Method(GetFileID(scanner.filename), scanner.GetMethods());
            result += InsertIntoMethodTables(scanner.GetMethods(), scanner.filename);

            Console.Write(result + "\n\n");
        }

        public static string InsertIntoFiles(Scanner scanner)
        {
            string results = "";

            string sql = "INSERT INTO [dbo].[Files] ([Name], [Path], [CharCount], [WhiteSpaceCount], [CommentCharCount]) VALUES (N'" + scanner.filename + "',N'" + scanner.path + "'," + scanner.CharCt + "," + scanner.WhitespaceCt + "," + scanner.CommentCharCt + ");";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if (TableExists("Files") == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    //Console.Write("\nFiles table populated successfully.");
                    results += "\nFiles table populated successfully.";
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoFiles.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static string InsertIntoFileTables(Dictionary<string, Package> packages, string filename)
        {
            string results = "";

            Dictionary<string, int> package_name_id = GetPackagesTable(filename);

            string sql = "";
            Package p = packages["OutsideOfPackage"];
            foreach (KeyValuePair<string, int> k in p.getKeywords())
            {
                sql += "INSERT INTO [dbo].[File_Keyword] ([File_ID],[Keyword],[Count]) VALUES (" + GetFileID(filename) + ",N'" + k.Key + "'," + k.Value.ToString() + ")\n";
            }
            foreach (KeyValuePair<string, int> udi in p.getUserDefinedIdentifiers())
            {
                sql += "INSERT INTO [dbo].[File_UserDefinedIdentifier] ([File_ID],[UserDefinedIdentifier],[Count]) VALUES (" + GetFileID(filename) + ",N'" + udi.Key + "'," + udi.Value.ToString() + ")\n";
            }
            foreach (KeyValuePair<string, int> c in p.getConstants())
            {
                sql += "INSERT INTO [dbo].[File_Constant] ([File_ID],[Constant],[Count]) VALUES (" + GetFileID(filename) + ",N'" + c.Key + "'," + c.Value.ToString() + ")\n";
            }
            foreach (KeyValuePair<char, int> sc in p.getSpecialChars())
            {
                sql += "INSERT INTO [dbo].[File_Special_Character] ([File_ID],[SpecialCharacter],[Count]) VALUES (" + GetFileID(filename) + ",N'" + sc.Key.ToString() + "'," + sc.Value.ToString() + ")\n";
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");

            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if ((TableExists("File_Keyword") == true) && (TableExists("File_UserDefinedIdentifier") == true) && (TableExists("File_Constant") == true) && (TableExists("File_Special_Character") == true))
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nFile tables populated successfully.";
                    //Console.WriteLine("\nFile tables populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoFileTables.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static int GetFileID(string filename)
        {
            int result = -1;
            string query = @"SELECT F.File_ID FROM Files AS F WHERE F.Name = '" + filename + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    result = (int)r.GetValue(0);
                }
                reader.Close();
                //Console.Write("\nRetrieved File ID successfully.");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetFileID.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }

        public static string InsertIntoPackages(Dictionary<string, Package> packages)
        {
            string results = "";

            string sql = "";
            foreach(KeyValuePair<string, Package> p in packages)
            {
                sql += "INSERT INTO [dbo].[Packages] ([Name]) VALUES (N'" + p.Value.name + "')\n";
                
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if (TableExists("Packages") == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nPackages table populated successfully.";
                    //Console.WriteLine("\n" + "\nPackages table populated successfully." + "\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoPackages.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static string InsertIntoPackageTables(Dictionary<string, Package> packages, string filename)
        {
            string results = "";

            Dictionary<string, int> package_name_id = GetPackagesTable(filename);

            string sql = "";
            foreach (KeyValuePair<string, Package> p in packages)
            {
                if (p.Value.getKeywords().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Package_Keyword] ([Package_ID],[Keyword],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'No Keywords'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> k in p.Value.getKeywords())
                    {
                        sql += "INSERT INTO [dbo].[Package_Keyword] ([Package_ID],[Keyword],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'" + k.Key + "'," + k.Value.ToString() + ")\n";
                    }
                }
                if (p.Value.getUserDefinedIdentifiers().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Package_UserDefinedIdentifier] ([Package_ID],[UserDefinedIdentifier],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'No UDIs'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> udi in p.Value.getUserDefinedIdentifiers())
                    {
                        sql += "INSERT INTO [dbo].[Package_UserDefinedIdentifier] ([Package_ID],[UserDefinedIdentifier],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'" + udi.Key + "'," + udi.Value.ToString() + ")\n";
                    }
                }
                if (p.Value.getConstants().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Package_Constant] ([Package_ID],[Constant],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'No Constants'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> c in p.Value.getConstants())
                    {
                        sql += "INSERT INTO [dbo].[Package_Constant] ([Package_ID],[Constant],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'" + c.Key + "'," + c.Value.ToString() + ")\n";
                    }
                }
                if (p.Value.getSpecialChars().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Package_Special_Character] ([Package_ID],[SpecialCharacter],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'No Special Characters'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<char, int> sc in p.Value.getSpecialChars())
                    {
                        sql += "INSERT INTO [dbo].[Package_Special_Character] ([Package_ID],[SpecialCharacter],[Count]) VALUES (" + package_name_id[p.Value.name] + ",N'" + sc.Key.ToString() + "'," + sc.Value.ToString() + ")\n";
                    }
                }
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");

            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if ((TableExists("Package_Keyword") == true) && (TableExists("Package_UserDefinedIdentifier") == true) && (TableExists("Package_Constant") == true) && (TableExists("Package_Special_Character") == true))
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nPackage tables populated successfully.";
                    //Console.WriteLine("\nPackage tables populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoPackageTables.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static Dictionary<string, int> GetPackagesTable(string filename)
        {
            Dictionary<string, int> results = new Dictionary<string, int>(); // package name, package id
            //string query = "SELECT * FROM Packages;";
            string query = @"SELECT * 
                        FROM Packages as P 
                        JOIN File_Package as FP ON P.Package_ID = FP.Package_ID
                        JOIN Files as F ON FP.File_ID = F.File_ID
                        WHERE F.Name = '" + filename + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[1].ToString(),(int) r.GetValue(0));
                }
                reader.Close();
                //Console.WriteLine("\nRetrieved Packages table successfully.\n");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetPackagesTable.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return results;
        }

        public static string InsertIntoClasses(Dictionary<string, Class> classes)
        {
            string results = "";

            string sql = "";
            foreach (KeyValuePair<string, Class> c in classes)
            {
                sql += "INSERT INTO [dbo].[Classes] ([Name],[Package_ID]) VALUES (N'" + c.Value.name + "',N'" + GetPackageID(c.Value.containingPackage) + "')\n";
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if (TableExists("Classes") == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nClasses table populated successfully.";
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoClasses.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static string InsertIntoClassTables(Dictionary<string, Class> classes, string filename)
        {
            string results = "";

            Dictionary<string, int> class_name_id = GetClassesTable(filename);

            string sql = "";
            foreach (KeyValuePair<string, Class> c in classes)
            {
                if (c.Value.getKeywords().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Class_Keyword] ([Class_ID],[Keyword],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'No Keywords'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> k in c.Value.getKeywords())
                    {
                        sql += "INSERT INTO [dbo].[Class_Keyword] ([Class_ID],[Keyword],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'" + k.Key + "'," + k.Value.ToString() + ")\n";
                    }
                }
                if (c.Value.getUserDefinedIdentifiers().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Class_UserDefinedIdentifier] ([Class_ID],[UserDefinedIdentifier],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'No UDIs'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> udi in c.Value.getUserDefinedIdentifiers())
                    {
                        sql += "INSERT INTO [dbo].[Class_UserDefinedIdentifier] ([Class_ID],[UserDefinedIdentifier],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'" + udi.Key + "'," + udi.Value.ToString() + ")\n";
                    }
                }
                if (c.Value.getConstants().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Class_Constant] ([Class_ID],[Constant],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'No Constants'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> c1 in c.Value.getConstants())
                    {
                        sql += "INSERT INTO [dbo].[Class_Constant] ([Class_ID],[Constant],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'" + c1.Key + "'," + c1.Value.ToString() + ")\n";
                    }
                }
                if (c.Value.getSpecialChars().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Class_Special_Character] ([Class_ID],[SpecialCharacter],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'No Special Characters'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<char, int> sc in c.Value.getSpecialChars())
                    {
                        sql += "INSERT INTO [dbo].[Class_Special_Character] ([Class_ID],[SpecialCharacter],[Count]) VALUES (" + class_name_id[c.Value.name] + ",N'" + sc.Key.ToString() + "'," + sc.Value.ToString() + ")\n";
                    }
                }
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if ((TableExists("Class_Keyword") == true) && (TableExists("Class_UserDefinedIdentifier") == true) && (TableExists("Class_Constant") == true) && (TableExists("Class_Special_Character") == true))
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nClass tables populated successfully.";
                    //Console.WriteLine("\nClass tables populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoClassTables.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static Dictionary<string, int> GetClassesTable(string filename)
        {
            Dictionary<string, int> results = new Dictionary<string, int>(); // class name, class id
            //string query = @"SELECT * FROM Classes;";
            string query = @"SELECT * 
                        FROM Classes as C 
                        JOIN File_Class as FC ON C.Class_ID = FC.Class_ID
                        JOIN Files as F ON FC.File_ID = F.File_ID
                        WHERE F.Name = '" + filename + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[1].ToString(), (int)r.GetValue(0));
                }
                reader.Close();
                //Console.WriteLine("\nClasses table retrieved successfully.\n");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetClassesTable.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return results;
        }

        public static string InsertIntoMethods(Dictionary<string, Method> methods)
        {
            string results = "";

            string sql = "";
            foreach (KeyValuePair<string, Method> m in methods)
            {
                sql += "INSERT INTO [dbo].[Methods] ([Name],[Package_ID],[Class_ID]) VALUES (N'" + m.Value.name + "'," + GetPackageID(m.Value.containingPackage) + "," + GetClassID(m.Value.containingClass) + ")\n";
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if (TableExists("Methods") == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nMethods table populated successfully.";
                    //Console.WriteLine("\nMethods table populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoMethods.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static string InsertIntoMethodTables(Dictionary<string, Method> methods, string filename)
        {
            string results = "";

            Dictionary<string, int> method_name_id = GetMethodsTable(filename);

            string sql = "";
            foreach (KeyValuePair<string, Method> m in methods)
            {
                if (m.Value.getKeywords().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Method_Keyword] ([Method_ID],[Keyword],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'No Keywords'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> k in m.Value.getKeywords())
                    {
                        sql += "INSERT INTO [dbo].[Method_Keyword] ([Method_ID],[Keyword],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'" + k.Key + "'," + k.Value.ToString() + ")\n";
                    }
                }
                if (m.Value.getUserDefinedIdentifiers().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Method_UserDefinedIdentifier] ([Method_ID],[UserDefinedIdentifier],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'No UDIs'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> udi in m.Value.getUserDefinedIdentifiers())
                    {
                        sql += "INSERT INTO [dbo].[Method_UserDefinedIdentifier] ([Method_ID],[UserDefinedIdentifier],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'" + udi.Key + "'," + udi.Value.ToString() + ")\n";
                    }
                }
                if (m.Value.getConstants().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Method_Constant] ([Method_ID],[Constant],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'No Constants'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<string, int> c in m.Value.getConstants())
                    {
                        sql += "INSERT INTO [dbo].[Method_Constant] ([Method_ID],[Constant],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'" + c.Key + "'," + c.Value.ToString() + ")\n";
                    }
                }
                if (m.Value.getSpecialChars().Count == 0)
                {
                    sql += "INSERT INTO [dbo].[Method_Special_Character] ([Method_ID],[SpecialCharacter],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'No Special Characters'," + 0 + ")\n";
                }
                else
                {
                    foreach (KeyValuePair<char, int> sc in m.Value.getSpecialChars())
                    {
                        sql += "INSERT INTO [dbo].[Method_Special_Character] ([Method_ID],[SpecialCharacter],[Count]) VALUES (" + method_name_id[m.Value.name] + ",N'" + sc.Key.ToString() + "'," + sc.Value.ToString() + ")\n";
                    }
                }
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if ((TableExists("Method_Keyword") == true) && (TableExists("Method_UserDefinedIdentifier") == true) && (TableExists("Method_Constant") == true) && (TableExists("Method_Special_Character") == true))
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nMethod tables populated successfully.";
                    //Console.WriteLine("\nMethod tables populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoMethodTables.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static Dictionary<string, int> GetMethodsTable(string filename)
        {
            Dictionary<string, int> results = new Dictionary<string, int>(); // class name, class id
            string query = @"SELECT * 
                        FROM Methods as M 
                        JOIN File_Method as FM ON M.Method_ID = FM.Method_ID
                        JOIN Files as F ON FM.File_ID = F.File_ID
                        WHERE F.Name = '" + filename + "';";

            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    results.Add(r[1].ToString(), (int)r.GetValue(0));
                }
                reader.Close();
                //Console.WriteLine("\nRetrieved methods table successfully.\n");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetMethodsTable.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return results;
        }

        public static string InsertIntoFile_Package(int fid, Dictionary<string, Package> packages)
        {
            string results = "";

            string sql = "";
            foreach (KeyValuePair<string, Package> p in packages)
            {
                sql += "INSERT INTO [dbo].[File_Package] ([File_ID], [Package_ID]) VALUES (" + fid.ToString() + "," + GetPackageID(p.Value.name).ToString() + ")\n";
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if (TableExists("File_Package") == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nFile_Package table populated successfully.";
                    //Console.WriteLine("\nFile_Package table populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoFile_Package.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static int GetPackageID(string packagename)
        {
            int result = -1;
            string query = @"SELECT P.Package_ID FROM Packages AS P WHERE P.Name = '" + packagename + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    result = (int)r.GetValue(0);
                }
                reader.Close();
                //Console.WriteLine("\nRetrieved package ID successfully.\n");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetPackageID.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }

        public static string InsertIntoFile_Class(int fid, Dictionary<string, Class> classes)
        {
            string results = "";

            string sql = "";
            foreach (KeyValuePair<string, Class> c in classes)
            {
                sql += "INSERT INTO [dbo].[File_Class] ([File_ID], [Class_ID]) VALUES (" + fid.ToString() + "," + GetClassID(c.Value.name).ToString() + ")\n";
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if (TableExists("File_Class") == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nFile_Class table populated successfully.";
                    //Console.WriteLine("\nFile_Class table populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoFile_Class.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static int GetClassID(string classname)
        {
            int result = -1;
            string query = @"SELECT C.Class_ID FROM Classes AS C WHERE C.Name = '" + classname + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    result = (int)r.GetValue(0);
                }
                reader.Close();
                //Console.WriteLine("\nRetrieved class ID successfully.\n");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetClassID.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }

        public static string InsertIntoFile_Method(int fid, Dictionary<string, Method> methods)
        {
            string results = "";

            string sql = "";
            foreach (KeyValuePair<string, Method> m in methods)
            {
                sql += "INSERT INTO [dbo].[File_Method] ([File_ID], [Method_ID]) VALUES (" + fid.ToString() + "," + GetMethodID(m.Value.name).ToString() + ")\n";
            }
            sql += ";";
            //Console.WriteLine("\n" + sql + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Check if table exists, perform insert, then close the connection
            if (TableExists("File_Method") == true)
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    results += "\nFile_Method table populated successfully.";
                    //Console.WriteLine("\nFile_Method table populated successfully.\n");
                }
                catch (System.Exception ex)
                {
                    Console.Write("\n\nThere was a problem with the SQL syntax in InsertIntoFile_Method.\n\n");
                    Console.Write(ex);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return results;
        }

        public static int GetMethodID(string methodname)
        {
            int result = -1;
            string query = @"SELECT M.Method_ID FROM Methods AS M WHERE M.Name = '" + methodname + "';";
            //Console.WriteLine("\n" + query + "\n");
            // Connect to DB
            SqlConnection conn = new SqlConnection(CONNECTION);
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;

            // Perform query and add returned records to the result string, then close the connection
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IDataRecord r = (IDataRecord)reader;
                    result = (int)r.GetValue(0);
                }
                reader.Close();
                //Console.WriteLine("\nRetrieved method ID successfully.\n");
            }
            catch (System.Exception ex)
            {
                Console.Write("\n\nThere was a problem with the SQL syntax in GetMethodID.\n\n");
                Console.Write(ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }

        // Conncetion static string
        private static readonly string CONNECTION = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=\"c:\\users\\ariel\\documents\\visual studio 2013\\Projects\\ANTLRReasoningCounter\\ANTLRReasoningCounter\\SourceCodeInformation.mdf\";Integrated Security=True";
        
        // Table creation static strings
        private static readonly string CREATE_TABLE_PACKAGES = @"CREATE TABLE Packages (
            Package_ID  INT          IDENTITY(1,1) NOT NULL,
            Name        VARCHAR (50) NOT NULL,
            PRIMARY KEY CLUSTERED (Package_ID ASC),
            UNIQUE NONCLUSTERED (Package_ID ASC)
            );";
        private static readonly string CREATE_TABLE_PACKAGE_KEYWORD = @"CREATE TABLE Package_Keyword (
            Keyword_ID  INT          NOT NULL IDENTITY(1,1),
            Package_ID  INT          NOT NULL,
            Keyword     VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Keyword_ID ASC),
            UNIQUE NONCLUSTERED (Keyword_ID ASC),
            FOREIGN KEY ([Package_ID]) REFERENCES [dbo].[Packages] ([Package_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_PACKAGE_USERDEFINEDIDENTIFIER = @"CREATE TABLE Package_UserDefinedIdentifier (
            UserDefinedIdentifier_ID    INT          NOT NULL IDENTITY(1,1),
            Package_ID                  INT          NOT NULL,
            UserDefinedIdentifier       VARCHAR (50) NOT NULL,
            Count                       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (UserDefinedIdentifier_ID ASC),
            UNIQUE NONCLUSTERED (UserDefinedIdentifier_ID ASC),
            FOREIGN KEY ([Package_ID]) REFERENCES [dbo].[Packages] ([Package_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_PACKAGE_CONSTANT = @"CREATE TABLE Package_Constant (
            Constant_ID INT          NOT NULL IDENTITY(1,1),
            Package_ID  INT          NOT NULL,
            Constant    VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Constant_ID ASC),
            UNIQUE NONCLUSTERED (Constant_ID ASC),
            FOREIGN KEY ([Package_ID]) REFERENCES [dbo].[Packages] ([Package_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_PACKAGE_SPECIALCHARACTER = @"CREATE TABLE Package_Special_Character (
            SpecialCharacter_ID     INT          NOT NULL IDENTITY(1,1),
            Package_ID              INT          NOT NULL,
            SpecialCharacter        CHAR (1)     NOT NULL,
            Count                   INT          NOT NULL,
            PRIMARY KEY CLUSTERED (SpecialCharacter_ID ASC),
            UNIQUE NONCLUSTERED (SpecialCharacter_ID ASC),
            FOREIGN KEY ([Package_ID]) REFERENCES [dbo].[Packages] ([Package_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_CLASSES = @"CREATE TABLE Classes (
            Class_ID    INT          NOT NULL IDENTITY(1,1),
            Name        VARCHAR (50) NOT NULL,
            Package_ID  INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Class_ID ASC),
            UNIQUE NONCLUSTERED (Class_ID ASC),
            FOREIGN KEY ([Package_ID]) REFERENCES [dbo].[Packages] ([Package_ID]),
            );";
        private static readonly string CREATE_TABLE_CLASS_KEYWORD = @"CREATE TABLE Class_Keyword (
            Keyword_ID  INT          NOT NULL IDENTITY(1,1),
            Class_ID    INT          NOT NULL,
            Keyword     VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Keyword_ID ASC),
            UNIQUE NONCLUSTERED (Keyword_ID ASC),
            FOREIGN KEY ([Class_ID]) REFERENCES [dbo].[Classes] ([Class_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_CLASS_USERDEFINEDIDENTIFIER = @"CREATE TABLE Class_UserDefinedIdentifier (
            UserDefinedIdentifier_ID    INT          NOT NULL IDENTITY(1,1),
            Class_ID                    INT          NOT NULL,
            UserDefinedIdentifier       VARCHAR (50) NOT NULL,
            Count                       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (UserDefinedIdentifier_ID ASC),
            UNIQUE NONCLUSTERED (UserDefinedIdentifier_ID ASC),
            FOREIGN KEY ([Class_ID]) REFERENCES [dbo].[Classes] ([Class_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_CLASS_CONSTANT = @"CREATE TABLE Class_Constant (
            Constant_ID INT          NOT NULL IDENTITY(1,1),
            Class_ID    INT          NOT NULL,
            Constant    VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Constant_ID ASC),
            UNIQUE NONCLUSTERED (Constant_ID ASC),
            FOREIGN KEY ([Class_ID]) REFERENCES [dbo].[Classes] ([Class_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_CLASS_SPECIALCHARACTER = @"CREATE TABLE Class_Special_Character (
            SpecialCharacter_ID     INT          NOT NULL IDENTITY(1,1),
            Class_ID                INT          NOT NULL,
            SpecialCharacter        CHAR (1)     NOT NULL,
            Count                   INT          NOT NULL,
            PRIMARY KEY CLUSTERED (SpecialCharacter_ID ASC),
            UNIQUE NONCLUSTERED (SpecialCharacter_ID ASC),
            FOREIGN KEY ([Class_ID]) REFERENCES [dbo].[Classes] ([Class_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_METHODS = @"CREATE TABLE Methods (
            Method_ID    INT          NOT NULL IDENTITY(1,1),
            Name         VARCHAR (50) NOT NULL,
            Package_ID   INT          NOT NULL,
            Class_ID     INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Method_ID ASC),
            UNIQUE NONCLUSTERED (Method_ID ASC),
            FOREIGN KEY ([Package_ID]) REFERENCES [dbo].[Packages] ([Package_ID]),
            FOREIGN KEY ([Class_ID]) REFERENCES [dbo].[Classes] ([Class_ID]),
            );";
        private static readonly string CREATE_TABLE_METHOD_KEYWORD = @"CREATE TABLE Method_Keyword (
            Keyword_ID  INT          NOT NULL IDENTITY(1,1),
            Method_ID    INT          NOT NULL,
            Keyword     VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Keyword_ID ASC),
            UNIQUE NONCLUSTERED (Keyword_ID ASC),
            FOREIGN KEY ([Method_ID]) REFERENCES [dbo].[Methods] ([Method_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_METHOD_USERDEFINEDIDENTIFIER = @"CREATE TABLE Method_UserDefinedIdentifier (
            UserDefinedIdentifier_ID    INT          NOT NULL IDENTITY(1,1),
            Method_ID                    INT          NOT NULL,
            UserDefinedIdentifier       VARCHAR (50) NOT NULL,
            Count                       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (UserDefinedIdentifier_ID ASC),
            UNIQUE NONCLUSTERED (UserDefinedIdentifier_ID ASC),
            FOREIGN KEY ([Method_ID]) REFERENCES [dbo].[Methods] ([Method_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_METHOD_CONSTANT = @"CREATE TABLE Method_Constant (
            Constant_ID INT          NOT NULL IDENTITY(1,1),
            Method_ID    INT          NOT NULL,
            Constant    VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Constant_ID ASC),
            UNIQUE NONCLUSTERED (Constant_ID ASC),
            FOREIGN KEY ([Method_ID]) REFERENCES [dbo].[Methods] ([Method_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_METHOD_SPECIALCHARACTER = @"CREATE TABLE Method_Special_Character (
            SpecialCharacter_ID     INT          NOT NULL IDENTITY(1,1),
            Method_ID                INT          NOT NULL,
            SpecialCharacter        CHAR (1)     NOT NULL,
            Count                   INT          NOT NULL,
            PRIMARY KEY CLUSTERED (SpecialCharacter_ID ASC),
            UNIQUE NONCLUSTERED (SpecialCharacter_ID ASC),
            FOREIGN KEY ([Method_ID]) REFERENCES [dbo].[Methods] ([Method_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_FILES = @"CREATE TABLE Files (
            File_ID             INT             NOT NULL IDENTITY(1,1),
            Name                VARCHAR (50)    NOT NULL,
            Path                VARCHAR (150)   NOT NULL,
            CharCount           INT             NOT NULL,
            WhiteSpaceCount     INT             NOT NULL,
            CommentCharCount    INT             NOT NULL,
            PRIMARY KEY CLUSTERED (File_ID ASC),
            UNIQUE NONCLUSTERED (File_ID ASC)
            );";
        private static readonly string CREATE_TABLE_FILE_KEYWORD = @"CREATE TABLE File_Keyword (
            Keyword_ID  INT          NOT NULL IDENTITY(1,1),
            File_ID     INT          NOT NULL,
            Keyword     VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Keyword_ID ASC),
            UNIQUE NONCLUSTERED (Keyword_ID ASC),
            FOREIGN KEY ([File_ID]) REFERENCES [dbo].[Files] ([File_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_FILE_USERDEFINEDIDENTIFIER = @"CREATE TABLE File_UserDefinedIdentifier (
            UserDefinedIdentifier_ID    INT          NOT NULL IDENTITY(1,1),
            File_ID                     INT          NOT NULL,
            UserDefinedIdentifier       VARCHAR (50) NOT NULL,
            Count                       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (UserDefinedIdentifier_ID ASC),
            UNIQUE NONCLUSTERED (UserDefinedIdentifier_ID ASC),
            FOREIGN KEY ([File_ID]) REFERENCES [dbo].[Files] ([File_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_FILE_CONSTANT = @"CREATE TABLE File_Constant (
            Constant_ID INT          NOT NULL IDENTITY(1,1),
            File_ID     INT          NOT NULL,
            Constant    VARCHAR (50) NOT NULL,
            Count       INT          NOT NULL,
            PRIMARY KEY CLUSTERED (Constant_ID ASC),
            UNIQUE NONCLUSTERED (Constant_ID ASC),
            FOREIGN KEY ([File_ID]) REFERENCES [dbo].[Files] ([File_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_FILE_SPECIALCHARACTER = @"CREATE TABLE File_Special_Character (
            SpecialCharacter_ID     INT          NOT NULL IDENTITY(1,1),
            File_ID                 INT          NOT NULL,
            SpecialCharacter        CHAR (1)     NOT NULL,
            Count                   INT          NOT NULL,
            PRIMARY KEY CLUSTERED (SpecialCharacter_ID ASC),
            UNIQUE NONCLUSTERED (SpecialCharacter_ID ASC),
            FOREIGN KEY ([File_ID]) REFERENCES [dbo].[Files] ([File_ID]),
            CHECK ([Count]>=(0))
            );";
        private static readonly string CREATE_TABLE_FILE_PACKAGE = @"CREATE TABLE File_Package (
            File_ID     INT          NOT NULL,
            Package_ID  INT          NOT NULL,
            FOREIGN KEY ([File_ID]) REFERENCES [dbo].[Files] ([File_ID]),
            FOREIGN KEY ([Package_ID]) REFERENCES [dbo].[Packages] ([Package_ID]),
            );";
        private static readonly string CREATE_TABLE_FILE_CLASS = @"CREATE TABLE File_Class (
            File_ID     INT          NOT NULL,
            Class_ID    INT          NOT NULL,
            FOREIGN KEY ([File_ID]) REFERENCES [dbo].[Files] ([File_ID]),
            FOREIGN KEY ([Class_ID]) REFERENCES [dbo].[Classes] ([Class_ID]),
            );";
        private static readonly string CREATE_TABLE_FILE_METHOD = @"CREATE TABLE File_Method (
            File_ID     INT          NOT NULL,
            Method_ID    INT          NOT NULL,
            FOREIGN KEY ([File_ID]) REFERENCES [dbo].[Files] ([File_ID]),
            FOREIGN KEY ([Method_ID]) REFERENCES [dbo].[Methods] ([Method_ID]),
            );";

        // Stored Queries
        private static readonly string FILES_QUERY = @"SELECT * FROM Files;";

        private static readonly string PACKAGES_QUERY = @"SELECT P.Package_ID, P.Name, PK.UniqueKeywordCt, PU.UniqueUDICt, PC.UniqueConstantCt, PS.UniqueSpecialCharCt, PK.TotalKeywordCt, PU.TotalUDICt, PC.TotalConstantCt, PS.TotalSpecialCharCt
                                                            FROM Packages AS P
                                                            JOIN (SELECT Package_ID, COUNT(*) AS UniqueKeywordCt, SUM(Count) AS TotalKeywordCt FROM Package_Keyword GROUP BY Package_ID) AS PK
                                                                ON P.Package_ID = PK.Package_ID
                                                            JOIN (SELECT Package_ID, COUNT(*) AS UniqueUDICt, SUM(Count) AS TotalUDICt FROM Package_UserDefinedIdentifier GROUP BY Package_ID) AS PU
                                                                ON P.Package_ID = PU.Package_ID
                                                            JOIN (SELECT Package_ID, COUNT(*) AS UniqueConstantCt, SUM(Count) AS TotalConstantCt FROM Package_Constant GROUP BY Package_ID) AS PC
                                                                ON P.Package_ID = PC.Package_ID
                                                            JOIN (SELECT Package_ID, COUNT(*) AS UniqueSpecialCharCt, SUM(Count) AS TotalSpecialCharCt FROM Package_Special_Character GROUP BY Package_ID) AS PS
                                                                ON P.Package_ID = PS.Package_ID
                                                            ;";
        private static readonly string TEST_QUERY = @"SELECT Package_ID, COUNT(*) AS UniqueKeywordCt, SUM(Count) AS TotalKeywordCt FROM Package_Keyword GROUP BY Package_ID;";

        private static readonly string CLASSES_QUERY = @"SELECT C.Class_ID, C.Name, CK.UniqueKeywordCt, CU.UniqueUDICt, CC.UniqueConstantCt, CS.UniqueSpecialCharCt, CK.TotalKeywordCt, CU.TotalUDICt, CC.TotalConstantCt, CS.TotalSpecialCharCt, P.Name
                                                            FROM Classes AS C
                                                            JOIN (SELECT ck.Class_ID, COUNT(*) AS UniqueKeywordCt, SUM(Count) AS TotalKeywordCt FROM Class_Keyword as ck GROUP BY ck.Class_ID) AS CK
                                                                ON C.Class_ID = CK.Class_ID
                                                            JOIN (SELECT cu.Class_ID, COUNT(*) AS UniqueUDICt, SUM(Count) AS TotalUDICt FROM Class_UserDefinedIdentifier as cu GROUP BY cu.Class_ID) AS CU
                                                                ON C.Class_ID = CU.Class_ID
                                                            JOIN (SELECT cc.Class_ID, COUNT(*) AS UniqueConstantCt, SUM(Count) AS TotalConstantCt FROM Class_Constant as cc GROUP BY cc.Class_ID) AS CC
                                                                ON C.Class_ID = CC.Class_ID
                                                            JOIN (SELECT cs.Class_ID, COUNT(*) AS UniqueSpecialCharCt, SUM(Count) AS TotalSpecialCharCt FROM Class_Special_Character as cs GROUP BY cs.Class_ID) AS CS
                                                                ON C.Class_ID = CS.Class_ID
                                                            JOIN (SELECT p.Package_ID, p.Name FROM Packages as p) AS P
                                                                ON C.Package_ID = P.Package_ID
                                                            ;";
        private static readonly string METHODS_QUERY = @"SELECT M.Method_ID, M.Name, MK.UniqueKeywordCt, MU.UniqueUDICt, MC.UniqueConstantCt, MS.UniqueSpecialCharCt, MK.TotalKeywordCt, MU.TotalUDICt, MC.TotalConstantCt, MS.TotalSpecialCharCt, P.Name, C.Name
                                                            FROM Methods AS M
                                                            JOIN (SELECT mk.Method_ID, COUNT(*) AS UniqueKeywordCt, SUM(Count) AS TotalKeywordCt FROM Method_Keyword as mk GROUP BY mk.Method_ID) AS MK
                                                                ON M.Method_ID = MK.Method_ID
                                                            JOIN (SELECT mu.Method_ID, COUNT(*) AS UniqueUDICt, SUM(Count) AS TotalUDICt FROM Method_UserDefinedIdentifier as mu GROUP BY mu.Method_ID) AS MU
                                                                ON M.Method_ID = MU.Method_ID
                                                            JOIN (SELECT mc.Method_ID, COUNT(*) AS UniqueConstantCt, SUM(Count) AS TotalConstantCt FROM Method_Constant as mc GROUP BY mc.Method_ID) AS MC
                                                                ON M.Method_ID = MC.Method_ID
                                                            JOIN (SELECT ms.Method_ID, COUNT(*) AS UniqueSpecialCharCt, SUM(Count) AS TotalSpecialCharCt FROM Method_Special_Character as ms GROUP BY ms.Method_ID) AS MS
                                                                ON M.Method_ID = MS.Method_ID
                                                            JOIN (SELECT p.Package_ID, p.Name FROM Packages as p) AS P
                                                                ON M.Package_ID = P.Package_ID
                                                            JOIN (SELECT c.Class_ID, c.Name FROM Classes as c) AS C
                                                                ON M.Class_ID = C.Class_ID
                                                            ;";
    }

    class SourceCodeFile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CharCount { get; set; }
        public int WhitespaceCount { get; set; }
        public int CommentCharCount { get; set; }
        public double PercentCharActive { get; set; }
        public string Path { get; set; }
        public List<string> Packages { get; set; }
        public List<string> Classes { get; set; }
        public List<string> Methods { get; set; }

        public SourceCodeFile(int id, string name, int charct, int wsct, int ccc, string path, List<string> plist, List<string> clist, List<string> mlist)
        {
            this.ID = id;
	        this.Name = name;
            this.CharCount = charct;
            this.WhitespaceCount = wsct;
            this.CommentCharCount = ccc;
            if ((charct + wsct + ccc) == 0) { this.PercentCharActive = 0; }
            else { this.PercentCharActive = 1.0 * charct / (charct + wsct + ccc); }
            this.Path = path;
            this.Packages = plist;
            this.Classes = clist;
            this.Methods = mlist;
        }

        public override string ToString()
        {
            return String.Format("ID={0},Name={1},CharCt={2},WhiteSpaceCt={3},Files length={4},Package={5},Class={6}", ID, Name, CharCount, WhitespaceCount, Packages.Count, Classes.Count, Methods.Count);
        }

    }

    class PackageInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UniqueKeywordCt { get; set; }
        public int UniqueUserDefinedIdentifierCt { get; set; }
        public int UniqueConstantCt { get; set; }
        public int UniqueSpecialCharCt { get; set; }
        public int TotalKeywordCt { get; set; }
        public int TotalUserDefinedIdentifierCt { get; set; }
        public int TotalConstantCt { get; set; }
        public int TotalSpecialCharCt { get; set; }
        public List<string> Files { get; set; }
        public List<string> Classes { get; set; }
        public List<string> Methods { get; set; }

        public PackageInfo(int id, string name, int uk, int uudi, int uc, int us, int tk, int tudi, int tc, int ts, List<string> flist, List<string> clist, List<string> mlist)
        {
            this.ID = id;
            this.Name = name;
            this.UniqueKeywordCt = uk;
            this.UniqueUserDefinedIdentifierCt = uudi;
            this.UniqueConstantCt = uc;
            this.UniqueSpecialCharCt = us;
            this.TotalKeywordCt = tk;
            this.TotalUserDefinedIdentifierCt = tudi;
            this.TotalConstantCt = tc;
            this.TotalSpecialCharCt = ts;
            this.Files = flist;
            this.Classes = clist;
            this.Methods = mlist;
        }

        public override string ToString()
        {
            return String.Format("ID={0},Name={1},UK={2},TK={3},Files length={4},Package={5},Class={6}", ID, Name, UniqueKeywordCt, TotalKeywordCt, Files.Count, Classes.Count, Methods.Count);
        }
    }

    class ClassInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UniqueKeywordCt { get; set; }
        public int UniqueUserDefinedIdentifierCt { get; set; }
        public int UniqueConstantCt { get; set; }
        public int UniqueSpecialCharCt { get; set; }
        public int TotalKeywordCt { get; set; }
        public int TotalUserDefinedIdentifierCt { get; set; }
        public int TotalConstantCt { get; set; }
        public int TotalSpecialCharCt { get; set; }
        public List<string> Files { get; set; }
        public string Package { get; set; }
        public List<string> Methods { get; set; }

        public ClassInfo(int id, string name, int uk, int uudi, int uc, int us, int tk, int tudi, int tc, int ts, string pname, List<string> mlist, List<string> flist)
        {
            this.ID = id;
            this.Name = name;
            this.UniqueKeywordCt = uk;
            this.UniqueUserDefinedIdentifierCt = uudi;
            this.UniqueConstantCt = uc;
            this.UniqueSpecialCharCt = us;
            this.TotalKeywordCt = tk;
            this.TotalUserDefinedIdentifierCt = tudi;
            this.TotalConstantCt = tc;
            this.TotalSpecialCharCt = ts;
            this.Files = flist;
            this.Package = pname;
            this.Methods = mlist;
        }

        public override string ToString()
        {
            return String.Format("ID={0},Name={1},UK={2},TK={3},Files length={4},Package={5},Methods length={6}", ID, Name, UniqueKeywordCt, TotalKeywordCt, Files.Count, Package, Methods.Count);
        }
    }

    class MethodInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UniqueKeywordCt { get; set; }
        public int UniqueUserDefinedIdentifierCt { get; set; }
        public int UniqueConstantCt { get; set; }
        public int UniqueSpecialCharCt { get; set; }
        public int TotalKeywordCt { get; set; }
        public int TotalUserDefinedIdentifierCt { get; set; }
        public int TotalConstantCt { get; set; }
        public int TotalSpecialCharCt { get; set; }
        public List<string> Files { get; set; }
        public string Package { get; set; }
        public string Class { get; set; }

        public MethodInfo(int id, string name, int uk, int uudi, int uc, int us, int tk, int tudi, int tc, int ts, string pname, string cname, List<string> flist)
        {
            this.ID = id;
            this.Name = name;
            this.UniqueKeywordCt = uk;
            this.UniqueUserDefinedIdentifierCt = uudi;
            this.UniqueConstantCt = uc;
            this.UniqueSpecialCharCt = us;
            this.TotalKeywordCt = tk;
            this.TotalUserDefinedIdentifierCt = tudi;
            this.TotalConstantCt = tc;
            this.TotalSpecialCharCt = ts;
            this.Files = flist;
            this.Package = pname;
            this.Class = cname;
        }

        public override string ToString()
        {
            return String.Format("ID={0},Name={1},UK={2},TK={3},Files length={4},Package={5},Class={6}", ID, Name, UniqueKeywordCt, TotalKeywordCt, Files.Count, Package, Class);
        }
    }
}
