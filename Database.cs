using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CodeHelper.Core.Database.Attributes;
namespace CodeHelper.Core.Database.SqlServer
{
    public static class Database
    {
        #region Static Public Methods
        /// <summary>
        /// Execute a storeprocedure and fill in automatically the data in the corresponding fields of the given object
        /// The method can base used as static method or as an extension
        /// </summary>
        /// <param name="MyObject">The object with the fields to be filled in.</param>
        /// <param name="DBConnString">string: Database connection string, when black or the Envronment variable containing the database string</param>
        /// <param name="storedProcedure">string: string: Name of the stored procedure to call, include the schema. [ex. dbo.UserGetByID]</param>
        /// <param name="parameters"></param>
        public static void GetData(this object MyObject, string DBConnString, string storedProcedure, object[] parameters)
        {
            GetDBConnString(ref DBConnString);
            using SqlConnection connection = new SqlConnection(DBConnString);
            connection.Open();

            using SqlCommand command = new SqlCommand() { CommandText = storedProcedure, Connection = connection, CommandType = CommandType.StoredProcedure };
            command.AddParameters(parameters);

            SqlDataReader reader = command.ExecuteReader();
            MyObject.SetData(reader);
            reader.Close();
            
            connection.Close();
        }

        /// <summary>
        /// Saves the given object into the database, using the DVBInfo Attrubute added to the MObject Class
        /// The methods can base used as extension of an object or as a static methods
        /// </summary>
        /// <param name="MyObject">object: Object to be saved</param>
        /// <param name="DBConnString">string: The database connectionstring</param>
        /// <param name="storedProcedure">string: The Stored Procedure Name, include the schema  (ex. dbo.UserInfoSave)</param>
        /// <returns>object: The methods will execute a Scalar operation, return a field value and place it into the right MyObject Property (Ex. the newly saved ID) (can be null)</returns>
        public static void Save(this object MyObject, string DBConnString)
        {
            GetDBConnString(ref DBConnString);
            DBInfo _dbInfo = (DBInfo)MyObject.GetType().GetCustomAttribute(typeof(DBInfo), false);
            if (_dbInfo != null && !string.IsNullOrEmpty(_dbInfo.SaveSP))
            {
                object returnValue = Database.Save(MyObject, DBConnString, _dbInfo.SaveSP);

                if (returnValue != null && !string.IsNullOrEmpty(_dbInfo.SaveReturnID))
                {
                    //.single throws an error when property doesn't exist
                    try
                    {
                        PropertyInfo property = MyObject.GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(DBFieldAttribute)) != null).Single(p => p.DBField().FieldName == _dbInfo.SaveReturnID);
                        if (property != null)
                            property.SetValue(MyObject, Convert.ChangeType(returnValue, property.PropertyType));
                    }
                    catch { }
                }
            }
        }
        
        /// <summary>
        /// Saves the given object into the database, using the given storedprocedure.
        /// The methods can base used as extension of an object or as a static methods
        /// </summary>
        /// <param name="MyObject">object: Object to be saved</param>
        /// <param name="DBConnString">string: The database connectionstring</param>
        /// <param name="storedProcedure">string: The Stored Procedure Name, include the schema  (ex. dbo.UserInfoSave)</param>
        /// <returns>object: The methods will execute a Scalar operation, return a field (Ex. the newly saved ID) (can be null)</returns>
        public static object Save(this object MyObject, string DBConnString, string storedProcedure)
        {
            GetDBConnString(ref DBConnString);
            object _returnValue = null;
            if (MyObject != null)
            {
                using SqlConnection connection = new SqlConnection(DBConnString);
                connection.Open();

                using SqlCommand command = new SqlCommand() { CommandText = storedProcedure, Connection = connection, CommandType = CommandType.StoredProcedure };

                foreach (PropertyInfo property in MyObject.GetType().GetProperties())
                {
                    DBFieldAttribute p = property.DBField();
                    if(p!=null && p.SaveToDB)
                        command.Parameters.Add(new SqlParameter(p.FieldName, property.GetValue(MyObject)));
                }                                                                    
                _returnValue = command.ExecuteScalar();
                connection.Close();
            }
            return _returnValue;
        }


        public static List<object> GetList(Type objectType, string DBConnString, string storedProcedure, string[] parameters, bool addEmptyObject = false)
        {
            GetDBConnString(ref DBConnString);
            var list = new List<object>();

            using SqlConnection connection = new SqlConnection(DBConnString);
            connection.Open();

            using SqlCommand command = new SqlCommand() { CommandText = storedProcedure, Connection = connection, CommandType = CommandType.StoredProcedure };
            command.AddParameters(parameters);

            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                object instance;
                while (reader.Read())
                {
                    instance = Activator.CreateInstance(objectType);
                    instance.SetData(reader, false);
                    list.Add(instance);
                }
            }
            reader.Close();                
            connection.Close();

            if (addEmptyObject)
                list.Add(Activator.CreateInstance(objectType));

            return list;
        }

        
        public static object ExecuteScalar(string storedProcedure, string DBConnString, string[] parameters)
        {
            GetDBConnString(ref DBConnString);

            using SqlConnection connection = new SqlConnection(DBConnString);
            connection.Open();

            using SqlCommand command = new SqlCommand() { CommandText = storedProcedure, Connection = connection, CommandType = CommandType.StoredProcedure };
            command.AddParameters(parameters);

            object _returnValue = command.ExecuteScalar();                
             connection.Close();
            
            return _returnValue;
        }
        #endregion

        #region Static Private Metjods
        private static void GetDBConnString(ref string DBConnString)
        {
            if(!string.IsNullOrEmpty(DBConnString))
                DBConnString = Environment.GetEnvironmentVariable(DBConnString) ?? DBConnString;
        }

        private static void AddParameters(this SqlCommand command, object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                for (int p = 0; p < parameters.Length; p += 2)
                    command.Parameters.Add(new SqlParameter(parameters[p].ToString(), parameters[p + 1]));
            }            
        }
        
        /// <summary>
        /// Must Set record must be set to false when using SetData in a while(reader.read()) loop%
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="MyObject"></param>
        /// <param name="mustReadNextRecord"></param>
        private static void SetData(this object MyObject, SqlDataReader reader, bool mustReadNextRecord = true)
        {
            if (MyObject != null)
            {
                List<PropertyInfo> properties = MyObject.GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(DBFieldAttribute)) != null).ToList();
                if (properties != null && reader.HasRows && reader.FieldCount > 0)
                {
                    if (mustReadNextRecord)
                        reader.Read();
                    PropertyInfo field;
                    reader.GetColumnSchema().ToList().ForEach(c => {
                        try { 
                        field = properties.Single(p => p.DBField().FieldName == c.ColumnName);
                        if (field != null && reader.GetValue(c.ColumnName) != DBNull.Value)
                            field.SetValue(MyObject, reader.GetValue(c.ColumnName));
                        }
                        catch { }
                    });                    
                }
            }
            
        }

        private static DBFieldAttribute DBField(this PropertyInfo property)
        {
            return (DBFieldAttribute)property.GetCustomAttribute(typeof(DBFieldAttribute));
        }
        #endregion
    }
}
