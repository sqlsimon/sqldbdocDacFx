﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.372
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Altairis.SqlDbDoc.Resources {


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Commands {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Commands() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Altairis.SqlDbDoc.Resources.Commands", typeof(Commands).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	C.name														AS [name],
        ///	TYPE_NAME(C.system_type_id)									AS [type],
        ///	C.max_length												AS [length],
        ///	C.precision													AS [precision], 
        ///	C.scale														AS [scale], 
        ///	C.is_nullable												AS [nullable], 
        ///	C.is_identity												AS [identity], 
        ///	C.is_computed												AS [computed], 
        ///	P.value														AS [description],
        ///	PK.object_id												AS [primaryKey:refId],
        ///	FK.constraint_object_id										AS [foreignKey:refId],
        ///	FK.referenced_object_id		 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetColumns {
            get {
                return ResourceManager.GetString("GetColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	name		AS [name], 
        ///	create_date	AS [dateCreated]
        ///FROM
        ///	sys.databases
        ///WHERE 
        ///	database_id = DB_ID().
        /// </summary>
        internal static string GetDatabase {
            get {
                return ResourceManager.GetString("GetDatabase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	O.object_id		AS [id], 
        ///	S.name			AS [schema], 
        ///	O.name			AS [name], 
        ///	O.type_desc		AS [type], 
        ///	O.create_date	AS [dateCreated], 
        ///	O.modify_date	AS [dateModified], 
        ///	P.value			AS [description]
        ///FROM 
        ///	sys.objects AS O
        ///	LEFT JOIN sys.schemas AS S on S.schema_id = o.schema_id
        ///	LEFT JOIN sys.extended_properties AS P ON P.major_id = O.object_id AND P.minor_id = 0 and P.name = &apos;MS_Description&apos; 
        ///WHERE 
        ///	is_ms_shipped = 0 
        ///	AND parent_object_id = @parent_object_id
        ///
        ///	
        ///
        ///.
        /// </summary>
        internal static string GetObjects {
            get {
                return ResourceManager.GetString("GetObjects", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT DISTINCT
        ///	S.name
        ///FROM 
        ///	sys.schemas AS S 
        ///	LEFT JOIN sys.objects AS O ON S.schema_id = O.schema_id
        ///WHERE 
        ///	O.is_ms_shipped = 0 
        ///	AND O.parent_object_id=0
        ///ORDER BY S.name.
        /// </summary>
        internal static string GetSchemas {
            get {
                return ResourceManager.GetString("GetSchemas", resourceCulture);
            }
        }
    }
}