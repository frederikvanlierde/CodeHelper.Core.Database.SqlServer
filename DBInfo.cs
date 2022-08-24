namespace CodeHelper.Core.Database.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DBInfo : System.Attribute
    {
        #region Properties        
        public string DeleteSP { get; set; } = "";
        public string SaveSP { get; set; } = "";
        public string SaveReturnID { get; set; } = "";
        #endregion

        #region Constructors
        /// <param name="storedProcedureSave">string: Name of the stored procedure to call, include the schema. [ex. dbo.UserSave]</param>
        /// /// <param name="saveReturnID">string</param>
        public DBInfo(string storedProcedureSave, string saveReturnID, string storedProcedureDelete="")
        {
            this.SaveSP = storedProcedureSave;
            this.SaveReturnID = saveReturnID;
            this.DeleteSP = storedProcedureDelete;
        }
        #endregion
    }
}
