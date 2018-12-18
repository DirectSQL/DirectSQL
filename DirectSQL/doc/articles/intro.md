# Example1
```
public static void example()
{
    SqlLiteDatabase db = new SqlLiteDatabase("connectionString_to_yourdb");
    db.Process((connection, transaction) =>
    {
        SqlLiteDatabase.Query(
            "select TEST_VAL1,TEST_VAL2 from TEST_TABLE where TEST_VAL1 = @val1",
            new (String, object)[] {ValueTuple.Create("@val1","abcdef")},
            connection,
            transaction,
            (result) => {
                while (result.Next())
                {
                    var resultValues = result.ResultValues;
                    Console.Out.WriteLine("TEST_VAL1:" + resultValues.TEST_VAL1);
                    Console.Out.WriteLine("TEST_VAL2:" + resultValues.TEST_VAL2);
                }
            }
        );
    });
}
```

# Example2
```
public static void example2()
{
    SqlLiteDatabase db = new SqlLiteDatabase("connectionString_to_yourdb");
    db.Process((connection, transaction) =>
    {
        dynamic[] resultArray =
            SqlLiteDatabase
            .LoadSqlResult(
                "select TEST_COL1,TEST_COL2 from TEST_TABLE",
                connection,
                transaction);
        
        Console.Out.WriteLine("TEST_VAL1:" + resultArray[0].TEST_VAL1);
        Console.Out.WriteLine("TEST_VAL2:" + resultArray[0].TEST_VAL2);

    });
}
```


