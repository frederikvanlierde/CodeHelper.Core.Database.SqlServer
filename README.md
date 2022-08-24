# CodeHelper.Core.Database.SqlServer
CodeHelper.Core.Database.SqlServer is a modern lightweight database mapper for .NET
Reduce the code and map easily your stored procedures to a object, set value of an object from the database, get lists from database and more....

## Versions
6.0.2 : .net6 : Generic Delete function has been added (backwards compatibl). See documentation to link KeyFields
6.0.1 : .net6
5.0.0 : .net5
1.0.0 : .net Core 3.1

## Easy explanation
1. Give a property in your Class a DBField Name
2. Give your Class a DBInfo attribute to easily save the object
2. Use the given DBField Names in your stored procedures as column name
3. Use the methods Database.GetData, Database.GetList, Database.ExecuteScalar and Database.Save in all your classes for clean code.
4. Database.Save saves you a lot of coding.

## Advantages
1. .Net developer team and SQL Server Team can work independent.
2. The attributes makes sure all teams use the same way of Naming
3. Reduce code on how to link your query result with your object.

Ex. Adding a field to a table, the DB team updated the Stored Procedures/Views, gives the columnname to the development team.
The devlopment team adds the property to the class and add a DBField attribute with the given columnname
Result: Everything works.

## An example of a stored procedure

```C#
    SELECT dbo.offers.ID as OfferID, OfferStart as OfferFrom ,... FROM Offers....
```

## An example of code
```C#
    using CodeHelper.Core.Database.Attributes;
    using CodeHelper.Core.Database.SqlServer;

    public class BaseClass
    {
        #region Properties
        public static string DBConnString { get { return Environment.GetEnvironmentVariable("DbConnString"); } }
        #endregion

        #region Public Methods
        public virtual void Save()
        {
            Database.Save(this, DBConnString);            
        }
        #endregion        
    }

    [DBInfo("dbo.OfferSave", "OfferID")]
    public class Offer : BaseClass
    {
        #region Properties
        [DBField("OfferID")]            public Int64 ID { get; set; }
        [DBField("LocationID")]         public Int64 LocationID { get; set; }
        [DBField("BusinessTypeID",false)]     public Int64 BusinessTypeID { get; set; } = 1;
        [DBField("OfferFrom")]          public DateTime OfferStart { get; set; } = System.DateTime.Today;
        [DBField("OfferTo")]            public DateTime OfferEnd { get; set; } = System.DateTime.Today.AddMonths(1);
        [DBField("OfferTitle")]         public string Title { get; set; } = "";
        [DBField("OfferDescription")]   public string Description { get; set; } = "";
        #endregion

        #region Constructors
        public Offer() { }
        public Offer(Int64 offerId)
        {
            Database.GetData(this, DBConnString, "dbo.OfferGetById", new object[] { "OfferID", offerId });
        }
        #endregion

        #region Public Methods
        static public Int32 GetNbDealsForUrl(string url)
        {
            return (Int32)Database.ExecuteScalar("dbo.OfferGetNbForUrl", DBConnString, new string[] { "Url", url });
        }
        #endregion

        #region Static Public Methods

        static public List<Offer> SearchOffers(string searchString, int offetX =0, int rowsX = 25)
        {
            return Database.GetList(typeof(Offer), DBConnString, "[dbo].[OffersSearch]", new object[] { "SearchString", searchString, "OffsetX", offetX, "RowsX", rowsX }).Cast<Offer>().ToList();
        }
        #endregion
    }
```
The value `{CONTACTNAME}` can be anything.  This value will be used in your text

### DBInfo
The DBInfo attribute on the Class level accepts the Save Stored Procedure and the return value (In case the save stored procedure returns a value).
The DBField attributes sets the Column name the stored procedure returns.
The saveToDB properties of the DBField (true or false) indicates if the Save method will use the Object property to save or not.

The Function Save (often placed in a base class), will take automatically the DBInfo Attribute values, check the DBField Properties of the Object and execute the stored procedure with the parameters
The Save function in the base class is virtual, this way you override the function, add extra functionalities and call base.Save()

In this example, when calling the MyObject.Save() method, the Stored Procedure "dbo.OfferSave" will be executed with the 6 parameters and return the new OfferID

### Database.GetData()
Gets the data from the database, and set the object value with the query result, using the DBField attribute
DBConnString: the database connection string or the name of the environment variable containing the database connection string
myObject: the object you want to fill in (can be an object or this)
new object[]: Contains the parameters you like to send to the stored procedure.  "FieldName1", FieldValue1, "FieldName2", FieldValue2,... or null 

```C#
    Database.GetData(myObject, DBConnString, "dbo.OfferGetById", new object[] { "OfferID", offerId });
```


### Database.GetList()
Optimize your database stored procedure and use the GetList to return the query results.
DBConnString: the database connection string or the name of the environment variable containing the database connection string
new object[]: Contains the parameters you like to send to the stored procedure.  "FieldName1", FieldValue1, "FieldName2", FieldValue2,... or null 

```C#
    static public List<Offer> SearchOffers(string searchString, int offetX =0, int rowsX = 25)
    {
        return Database.GetList(typeof(Offer), DBConnString, "[dbo].[OffersSearch]", new object[] { "SearchString", searchString, "OffsetX", offetX, "RowsX", rowsX }).Cast<Offer>().ToList();
    }
```


### Database.ExecuteScalar
Optimize your database stored procedure and use the ExecuteScalar to easily execute and get a value.
DBConnString: the database connection string or the name of the environment variable containing the database connection string
new object[]: Contains the parameters you like to send to the stored procedure.  "FieldName1", FieldValue1, "FieldName2", FieldValue2,... or null 

```C#
    static public Int32 GetNbDealsForUrl(string url)
    {
        return (Int32)Database.ExecuteScalar("dbo.OfferGetNbForUrl", DBConnString, new string[] { "Url", url });
    }
```


## Question?
Frederik van Lierde <https://twitter.com/@frederik_vl/>
