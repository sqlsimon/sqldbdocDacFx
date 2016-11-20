using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;

namespace Altairis.SqlDbDoc
{
    class SqlDbDocDacFx
    {
        private static readonly string[] FORMATS = { "html", "wikiplex", "xml" };
        private static readonly string[] HTML_EXTENSIONS = { ".htm", ".html", ".xhtml" };
        private static readonly string[] WIKI_EXTENSIONS = { ".txt", ".wiki" };
        private static readonly string[] SOURCE_FORMAT = { "database", "dacpac" };

        private static string connectionString;
        private static string dacpacFile;

        private static TSqlTypedModel model;


        //todo: have it take a dacpac as well as a connection string as an input file

        //TODO: IN EACH RENDER METHOD PASS AN ADDITIONAL PARAMETER THAT LETS YOU DICATATE
        // WHETHER THE CONNECTION IS USED OR THE DAC MODEL. IN THE CODE SWITCH ON THE 
        // PARAMETER AND CALL THE APPROPRIATE METHOD TO GET THE LIST OF OBJECTS TO RENDER.

        // Actions

        public static void CreateDocumentation(
            string connection,
            string dacpac,
            string fileName,
            bool overwrite,
            string format,
            bool debug
            )
        {

            //todo: HERE WE NEED TO CHECK TO SEE IF EITHER A DACPAC OR A CONNECTION HAS BEEN PASSED. IF NEITHER THEN FAIL 

            // Validate arguments
            if ((connection != null) && (dacpac != null)) throw new ArgumentException("specify either a connection string or dacpac file, but not both");
            if ((connection == null) && (dacpac == null)) throw new ArgumentNullException("connection and dacpac");
            if ((dacpac == null) && (string.IsNullOrWhiteSpace(connection))) throw new ArgumentException("Value cannot be empty or whitespace only string.", "connection");
            if ((connection == null) && (string.IsNullOrWhiteSpace(dacpac))) throw new ArgumentException("value cannot be empty or whitespace only string", "dacpac");

            if (fileName == null) throw new ArgumentNullException("fileName");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "fileName");

            // if we were passed a dacpac, check that it exists
            if ((dacpac != null) && !File.Exists(dacpac))
            {
                throw new FileNotFoundException("File does not exist", dacpac);
            }

            // Validate output file
            if (File.Exists(fileName) && !overwrite)
            {
                Console.WriteLine("ERROR: Target file already exists. Use /y to overwrite.");
                return;
            }

            // Get output format
            if (string.IsNullOrWhiteSpace(format))
            {
                Console.WriteLine("Autodetecting output format...");
                if (Array.IndexOf(HTML_EXTENSIONS, Path.GetExtension(fileName)) > -1)
                {
                    format = "html";
                }
                else if (Array.IndexOf(WIKI_EXTENSIONS, Path.GetExtension(fileName)) > -1)
                {
                    format = "wikiplex";
                }
                else
                {
                    format = "xml";
                }
            }
            else
            {
                format = format.ToLower().Trim();
                if (Array.IndexOf(FORMATS, format) == -1) throw new ArgumentOutOfRangeException("format", "Unknown format string.");
            }
            Console.WriteLine("Output format: {0}", format);

            try
            {

                // Prepare XML document
                var doc = new XmlDocument();
                string sourceFormat = null;


                if (dacpac != null)
                {
                    sourceFormat = "dacpac";
                    // losd up the dacpac
                    model = new TSqlTypedModel(dacpacFile);
                    
                }                  
                else if (connection != null)
                {
                    sourceFormat = "database";
                }

                // Process database info
                connectionString = connection;
                dacpacFile = dacpac;

                doc.AppendChild(doc.CreateElement("database"));
                doc.DocumentElement.SetAttribute("dateGenerated", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                RenderDatabase(doc.DocumentElement, sourceFormat);

                // Process schemas
                RenderSchemas(doc.DocumentElement, sourceFormat);

                // Process top-level objects
                RenderChildObjects(0, doc.DocumentElement, sourceFormat);

                if (format.Equals("xml"))
                {
                    // Save raw XML
                    Console.Write("Saving raw XML...");
                    doc.Save(fileName);
                    Console.WriteLine("OK");
                    return;
                }

                // Read XSL template code
                string xslt;
                if (format.Equals("html"))
                {
                    xslt = Resources.Templates.Html;
                }
                else
                {
                    xslt = Resources.Templates.WikiPlex;
                }

                // Prepare XSL transformation
                Console.Write("Preparing XSL transformation...");
                using (var sr = new StringReader(xslt))
                using (var xr = XmlReader.Create(sr))
                {
                    var tran = new XslCompiledTransform();
                    tran.Load(xr);
                    Console.WriteLine("OK");

                    Console.Write("Performing XSL transformation...");
                    using (var fw = File.CreateText(fileName))
                    {
                        tran.Transform(doc, null, fw);
                    }
                    Console.WriteLine("OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed!");
                Console.WriteLine(ex.Message);
                if (debug) Console.WriteLine(ex.ToString());
            }
        }

        // Helper methods

        static void RenderSchemas(XmlElement parentElement, string sourceFormat)
        {

            var dt = new DataTable();

            if (sourceFormat == "database")
            { 
                    // Get list of schemas
                using (var da = new SqlDataAdapter(Resources.Commands.GetSchemas, connectionString))
                {
                    da.Fill(dt);
                }
            }
            else if (sourceFormat == "dacpac")
            {
                //dac get schemas

                dt.Columns.Add("name", typeof(string));

                var schemas = model.GetObjects<TSqlSchema>(DacQueryScopes.UserDefined);
                    foreach (var schema in schemas)
                    {
                        dt.Rows.Add(schema.Name.ToString());
                        
                    }
             }

            // Populate schemas
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var e = parentElement.AppendChild(parentElement.OwnerDocument.CreateElement("schema")) as XmlElement;
                e.SetAttribute("name", (string)dt.Rows[i][0]);
            }
        }

        static void RenderDatabase(XmlElement parentElement, string sourceFormat)
        {
            // Get current database info
            var dt = new DataTable();

            if (sourceFormat == "database")
            { 
                using (var da = new SqlDataAdapter(Resources.Commands.GetDatabase, connectionString))
                {
                    da.Fill(dt);
                }
            }
            else if (sourceFormat == "dacpac")
            {
                dt.Columns.Add("name", typeof(string));
                dt.Columns.Add("dateCreated",typeof(DateTime));

                //todo: get these properties from dacpac
                dt.Rows.Add(dacpacFile, DateTime.Now.ToString());

            }


            // Display database info
            foreach (DataColumn col in dt.Columns)
            {
                var value = dt.Rows[0].ToXmlString(col);
                if (!string.IsNullOrWhiteSpace(value)) parentElement.SetAttribute(col.ColumnName, value);
            }
        }

        static void RenderChildObjects(int parentObjectId, XmlElement parentElement, string sourceFormat)
        {
            // Get all database objects with given parent
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(Resources.Commands.GetObjects, connectionString))
            {
                da.SelectCommand.Parameters.Add("@parent_object_id", SqlDbType.Int).Value = parentObjectId;
                da.Fill(dt);
            }

            // Process all objects
            foreach (DataRow row in dt.Rows)
            {
                var objectId = (int)row["id"];
                Trace.WriteLine(string.Format("{0}.{1}", row["schema"], row["name"]));

                // Create object element
                var e = parentElement.AppendChild(parentElement.OwnerDocument.CreateElement("object")) as XmlElement;
                foreach (DataColumn col in dt.Columns)
                {
                    var value = row.ToXmlString(col);
                    if (!string.IsNullOrWhiteSpace(value)) e.SetAttribute(col.ColumnName, value);
                }

                Trace.Indent();
                // Process columns
                RenderColumns(objectId, e, sourceFormat);

                // Process child objects
                RenderChildObjects(objectId, e, sourceFormat);
                Trace.Unindent();
            }
        }

        static void RenderColumns(int objectId, XmlElement parentElement, string sourceFormat)
        {
            // Get all columns object with given parent
            var dt = new DataTable();

            if (sourceFormat == "database")
            {

                using (var da = new SqlDataAdapter(Resources.Commands.GetColumns, connectionString))
                {
                    da.SelectCommand.Parameters.Add("@object_id", SqlDbType.Int).Value = objectId;
                    da.Fill(dt);
                }
            }
            else if (sourceFormat == "dacpac")
            {
                //dac get columns

                dt.Columns.Add("type", typeof(string));
                dt.Columns.Add("length", typeof(string));
                dt.Columns.Add("precision", typeof(string));
                dt.Columns.Add("name", typeof(string));
                dt.Columns.Add("scale", typeof(string));
                dt.Columns.Add("nullable", typeof(string));
                dt.Columns.Add("identity", typeof(string));
                dt.Columns.Add("computed", typeof(string));
                dt.Columns.Add("description", typeof(string));
                dt.Columns.Add("primaryKey: refId", typeof(string));
                dt.Columns.Add("foreignKey: refId", typeof(string));
                dt.Columns.Add("foreignKey: tableId", typeof(string));
                dt.Columns.Add("foreignKey: column", typeof(string));
                dt.Columns.Add("default: refId", typeof(string));
                dt.Columns.Add("default: value"typeof(string));

                

                var columns = model.GetObjects<TSqlColumn>(DacQueryScopes.UserDefined);
                foreach (var column in columns)
                {
                    dt.Rows.Add(column.Name.ToString());

                }

            }


            // Process all columns
            foreach (DataRow row in dt.Rows)
            {
                Trace.WriteLine(string.Format("{0} {1}", row["name"], row["type"]));

                // Create object element
                var e = parentElement.AppendChild(parentElement.OwnerDocument.CreateElement("column")) as XmlElement;
                foreach (DataColumn col in dt.Columns)
                {
                    var value = row.ToXmlString(col);
                    if (string.IsNullOrWhiteSpace(value)) continue;

                    if (col.ColumnName.IndexOf(':') == -1)
                    {
                        // Plain attribute
                        e.SetAttribute(col.ColumnName, value);
                    }
                    else
                    {
                        // Nested element/attribute
                        var names = col.ColumnName.Split(':');
                        var se = (e.SelectSingleNode(names[0]) ?? e.AppendChild(e.OwnerDocument.CreateElement(names[0]))) as XmlElement;
                        se.SetAttribute(names[1], value);
                    }
                }
            }
        }


    }
}
