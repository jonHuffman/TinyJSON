using System;
using TinyJSON;
using NUnit.Framework;
using System.Collections.Generic;


[TestFixture]
public class TestStructType
{
    public static bool loadCallbackFired = false;

    struct TestStruct
    {
        public int x;
        public int y;

        [ExcludeAttribute] 
        public int z;

        [AfterDecode]
        public void OnLoad()
        {
            loadCallbackFired = true;
        }
    }


    [Test]
    public void TestDumpStruct()
    {
        var testStruct = new TestStruct() { x = 5, y = 7, z = 0 };

        Assert.AreEqual( "{\"@type\":\"" + testStruct.GetType().FullName + "\",\"x\":5,\"y\":7}", JSON.Dump( testStruct ) );
    }


    [Test]
    public void TestLoadStruct()
    {
        TestStruct testStruct = JSON.Load( "{\"x\":5,\"y\":7,\"z\":3}" ).Make<TestStruct>();

        Console.WriteLine(testStruct.x + " | " + testStruct.y + " | " + testStruct.z);

        Assert.AreEqual( 5, testStruct.x );
        Assert.AreEqual( 7, testStruct.y );
        Assert.AreEqual( 0, testStruct.z ); // should not get assigned

        Assert.IsTrue( loadCallbackFired );
    }
}

