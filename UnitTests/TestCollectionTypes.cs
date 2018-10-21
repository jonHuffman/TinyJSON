using NUnit.Framework;
using System;
using System.Collections.Generic;
using TinyJSON;


[TestFixture]
public class TestCollectionTypes
{
    [Test]
    public void TestDumpRank1Array()
    {
        var array = new int[] { 3, 1, 4 };
        Assert.AreEqual( "[3,1,4]", JSON.Dump( array ) );
    }


    [Test]
    public void TestDumpRank2Array()
    {
        var array = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
        Assert.AreEqual( "[[1,2,3],[4,5,6]]", JSON.Dump( array ) );
    }


    [Test]
    public void TestDumpRank3Array()
    {
        var array = new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } }, { { 9, 0 }, { 1, 2 } } };
        Assert.AreEqual( "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]", JSON.Dump( array ) );
    }


    [Test]
    public void TestDumpJaggedArray()
    {
        var array = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 } };
        Assert.AreEqual( "[[1,2,3],[4,5,6]]", JSON.Dump( array ) );
    }


    [Test]
    public void TestLoadRank1Array()
    {
        int[] array;
        var variant = JSON.Load( "[1,2,3]" );
        JSON.MakeInto( variant, out array );
        Assert.AreNotEqual( null, array );

        Assert.AreEqual( 3, array.Length );
        Assert.AreEqual( 1, array[0] );
        Assert.AreEqual( 2, array[1] );
        Assert.AreEqual( 3, array[2] );
    }


    [Test]
    public void TestLoadRank2Array()
    {
        int[,] array;
        var variant = JSON.Load( "[[1,2,3],[4,5,6]]" );
        JSON.MakeInto( variant, out array );
        Assert.AreNotEqual( null, array );

        Assert.AreEqual( 2, array.Rank );
        Assert.AreEqual( 2, array.GetLength( 0 ) );
        Assert.AreEqual( 3, array.GetLength( 1 ) );

        Assert.AreEqual( 1, array[0, 0] );
        Assert.AreEqual( 2, array[0, 1] );
        Assert.AreEqual( 3, array[0, 2] );

        Assert.AreEqual( 4, array[1, 0] );
        Assert.AreEqual( 5, array[1, 1] );
        Assert.AreEqual( 6, array[1, 2] );
    }


    [Test]
    public void TestLoadRank3Array()
    {
        int[,,] array;
        var variant = JSON.Load( "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]" );
        JSON.MakeInto( variant, out array );
        Assert.AreNotEqual( null, array );

        Assert.AreEqual( 3, array.Rank );
        Assert.AreEqual( 3, array.GetLength( 0 ) );
        Assert.AreEqual( 2, array.GetLength( 1 ) );
        Assert.AreEqual( 2, array.GetLength( 2 ) );

        Assert.AreEqual( 1, array[0, 0, 0] );
        Assert.AreEqual( 2, array[0, 0, 1] );

        Assert.AreEqual( 3, array[0, 1, 0] );
        Assert.AreEqual( 4, array[0, 1, 1] );

        Assert.AreEqual( 5, array[1, 0, 0] );
        Assert.AreEqual( 6, array[1, 0, 1] );

        Assert.AreEqual( 7, array[1, 1, 0] );
        Assert.AreEqual( 8, array[1, 1, 1] );

        Assert.AreEqual( 9, array[2, 0, 0] );
        Assert.AreEqual( 0, array[2, 0, 1] );

        Assert.AreEqual( 1, array[2, 1, 0] );
        Assert.AreEqual( 2, array[2, 1, 1] );
    }


    [Test]
    public void TestLoadJaggedArray()
    {
        int[][] array;
        var variant = JSON.Load( "[[1,2,3],[4,5,6]]" );
        JSON.MakeInto( variant, out array );
        Assert.AreNotEqual( null, array );

        Assert.AreEqual( 2, array.Length );

        Assert.AreEqual( 3, array[0].Length );
        Assert.AreEqual( 1, array[0][0] );
        Assert.AreEqual( 2, array[0][1] );
        Assert.AreEqual( 3, array[0][2] );

        Assert.AreEqual( 3, array[1].Length );
        Assert.AreEqual( 4, array[1][0] );
        Assert.AreEqual( 5, array[1][1] );
        Assert.AreEqual( 6, array[1][2] );
    }


    [Test]
    public void TestDumpList()
    {
        var list = new List<int>() {3 ,1 ,4};
        Assert.AreEqual( "[" + Environment.NewLine +
                        "\t3," + Environment.NewLine +
                        "\t1," + Environment.NewLine + 
                        "\t4" + Environment.NewLine +
                        "]", JSON.Dump( list, EncodeOptions.PrettyPrint ) );
    }


    [Test]
    public void TestLoadList()
    {
        List<int> list;
        var variant = JSON.Load( "[3,1,4]" );
        JSON.MakeInto( variant, out list );
        Assert.AreNotEqual( null, list );

        Assert.AreEqual( 3, list.Count );
        Assert.AreEqual( 3, list[0] );
        Assert.AreEqual( 1, list[1] );
        Assert.AreEqual( 4, list[2] );
    }


    [Test]
    public void TestDumpDict()
    {
        var dict = new Dictionary<string, float>();
        dict["foo"] = 1337f;
        dict["bar"] = 3.14f;

        Assert.AreEqual( "{\"foo\":1337,\"bar\":3.14}", JSON.Dump( dict ) );
    }


    [Test]
    public void TestLoadDict()
    {
        var variant = JSON.Load( "{\"foo\":1337,\"bar\":3.14}" );

        Dictionary<string, float> dict;
        JSON.MakeInto( variant, out dict );

        Assert.AreNotEqual( null, dict );
        Assert.AreEqual( 2, dict.Count );
        Assert.AreEqual( 1337f, (float) dict["foo"] );
        Assert.AreEqual( 3.14f, (float) dict["bar"] );
    }


    [Test]
    public void TestLoadDictIntoProxy()
    {
        var variant = JSON.Load( "{\"foo\":1337,\"bar\":3.14}" );
        var proxy = variant as ProxyObject;

        Assert.AreNotEqual( null, proxy );
        Assert.AreEqual( 2, proxy.Count );
        Assert.AreEqual( 1337f, (float) proxy["foo"] );
        Assert.AreEqual( 3.14f, (float) proxy["bar"] );
    }


    enum TestEnum
    {
        Thing1,
        Thing2,
        Thing3
    }

    [Test]
    public void TestDumpEnum()
    {
        var testEnum = TestEnum.Thing2;
        Assert.AreEqual( "\"Thing2\"", JSON.Dump( testEnum ) );
        string t = JSON.Dump(testEnum, EncodeOptions.EnumsAsInts);
        Assert.AreEqual("1", JSON.Dump(testEnum, EncodeOptions.EnumsAsInts));
    }


    public void TestLoadEnum()
    {
        TestEnum testEnum;

        testEnum = JSON.Load( "\"Thing2\"" ).Make<TestEnum>();
        Assert.AreEqual( TestEnum.Thing2, testEnum );

        testEnum = JSON.Load( "\"Thing4\"" ).Make<TestEnum>();
        Assert.AreEqual( null, testEnum );
    }


    [Test]
    public void TestDumpDictWithEnumKeys()
    {
        var dict = new Dictionary<TestEnum,String>();
        dict[TestEnum.Thing1] = "Item 1";
        dict[TestEnum.Thing2] = "Item 2";
        dict[TestEnum.Thing3] = "Item 3";
        Assert.AreEqual( "{\"Thing1\":\"Item 1\",\"Thing2\":\"Item 2\",\"Thing3\":\"Item 3\"}", JSON.Dump( dict ) );
    }


    [Test]
    public void TestLoadDictWithEnumKeys()
    {
        var json = "{\"Thing1\":\"Item 1\",\"Thing2\":\"Item 2\",\"Thing3\":\"Item 3\"}";
        var dict = JSON.Load( json ).Make<Dictionary<TestEnum,String>>();
        Assert.AreNotEqual( null, dict );
        Assert.AreEqual( 3, dict.Count );
        Assert.AreEqual( "Item 1", dict[TestEnum.Thing1] );
        Assert.AreEqual( "Item 2", dict[TestEnum.Thing2] );
        Assert.AreEqual( "Item 3", dict[TestEnum.Thing3] );
    }

    [Test]
    public void TestDumpHashSet()
    {
        const string ELEMENT_1 = "Element1";
        const string ELEMENT_2 = "Element2";
        const string ELEMENT_3 = "Element3";

        var hashSet = new HashSet<string>()
        {
            ELEMENT_1,
            ELEMENT_2,
            ELEMENT_3
        };

        var json = string.Format("[\"{0}\",\"{1}\",\"{2}\"]", ELEMENT_1, ELEMENT_2, ELEMENT_3);
        Assert.AreEqual(json, JSON.Dump(hashSet));
    }

    [Test]
    public void TestLoadHashSet()
    {
        const string ELEMENT_1 = "Element1";
        const string ELEMENT_2 = "Element2";
        const string ELEMENT_3 = "Element3";

        var json = string.Format("[\"{0}\",\"{1}\",\"{2}\"]", ELEMENT_1, ELEMENT_2, ELEMENT_3);
        var hashSet = JSON.Load(json).Make<HashSet<string>>();
        Assert.AreNotEqual(null, hashSet);
        Assert.AreEqual(3, hashSet.Count);
        Assert.AreEqual(true, hashSet.Contains(ELEMENT_1));
        Assert.AreEqual(true, hashSet.Contains(ELEMENT_2));
        Assert.AreEqual(true, hashSet.Contains(ELEMENT_3));
    }

    [Test]
    public void TestDumpHashSetWithEnumKeys()
    {
        const TestEnum ELEMENT_1 = TestEnum.Thing1;
        const TestEnum ELEMENT_2 = TestEnum.Thing2;
        const TestEnum ELEMENT_3 = TestEnum.Thing3;

        var hashSet = new HashSet<TestEnum>()
        {
            ELEMENT_1,
            ELEMENT_2,
            ELEMENT_3
        };

        var json = string.Format("[\"{0}\",\"{1}\",\"{2}\"]", ELEMENT_1, ELEMENT_2, ELEMENT_3);
        Assert.AreEqual(json, JSON.Dump(hashSet));
    }

    [Test]
    public void TestLoadHashSetWithEnum()
    {
        const int ELEMENT_1 = (int)TestEnum.Thing1;
        const int ELEMENT_2 = (int)TestEnum.Thing2;
        const int ELEMENT_3 = (int)TestEnum.Thing3;

        var json = string.Format("[\"{0}\",\"{1}\",\"{2}\"]", ELEMENT_1, ELEMENT_2, ELEMENT_3);
        var hashSet = JSON.Load(json).Make<HashSet<TestEnum>>();
        Assert.AreNotEqual(null, hashSet);
        Assert.AreEqual(3, hashSet.Count);
        Assert.AreEqual(true, hashSet.Contains((TestEnum)ELEMENT_1));
        Assert.AreEqual(true, hashSet.Contains((TestEnum)ELEMENT_2));
        Assert.AreEqual(true, hashSet.Contains((TestEnum)ELEMENT_3));
    }
}
