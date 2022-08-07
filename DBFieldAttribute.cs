using System;

namespace CodeHelper.Core.Database.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field |
                       System.AttributeTargets.Property)
    ]
    public class DBFieldAttribute : System.Attribute
    {
        #region Properties
        public string FieldName { get; set; } = "";
        public bool SaveToDB { get; set; } = true;
        #endregion

        #region Constructors
        /// <param name="fieldName">string: Name of the field in the database. (Aliaseses are alloweed)</param>
        /// <param name="saveToDB">bool: indicates if the field shoud be used in the Save function</param>
        public DBFieldAttribute(string fieldName, bool saveToDB = true)
        {
            this.FieldName = fieldName;
            this.SaveToDB = saveToDB;
        }
        #endregion
    }
}
