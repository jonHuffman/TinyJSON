using System;
using TinyJSON;
using NUnit.Framework;
using System.Collections.Generic;


[TestFixture]
public class TestClassType
{
    public static bool afterDecodeCallbackFired = false;
    public static bool beforeEncodeCallbackFired = false;


    class TestClass
    {
        public int x;
        public int y;

        [Exclude]
        public int z;

        public List<int> list;


        [AfterDecode]
        public void AfterDecode()
        {
            TestClassType.afterDecodeCallbackFired = true;
        }


        [BeforeEncode]
        public void BeforeDecode()
        {
            TestClassType.beforeEncodeCallbackFired = true;
        }
    }


    [Test]
    public void TestDumpClass()
    {
        var testClass = new TestClass() { x = 5, y = 7, z = 0 };
        testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

        Assert.AreEqual("{\"@type\":\"" + testClass.GetType().FullName + "\",\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump(testClass));

        Assert.IsTrue(beforeEncodeCallbackFired);
    }

    public class AttributeClass
    {
        [Exclude]
        public int excludedField = 0;

        private int m_PrivateField = 0;

        [Exclude]
        private int m_PropertyValue = 0;

        [Include]
        public int propertyValue
        {
            get { return m_PropertyValue; }
            set { m_PropertyValue = value; }
        }

        public int privateField
        {
            get { return m_PrivateField; }
            set { m_PrivateField = value; }
        }
    }

    [Test]
    public void TestEncoderOptions_EncoderOptions()
    {
        AttributeClass aClass = new AttributeClass() { excludedField = 4, propertyValue = 10, privateField = 4 };

        //Should only encode the property value 
        Assert.AreEqual("{\"@type\":\"TestClassType+AttributeClass\",\"propertyValue\":10}", JSON.Dump(aClass));

        //Should only encode the field value
        Assert.AreEqual("{\"@type\":\"TestClassType+AttributeClass\",\"excludedField\":4}", JSON.Dump(aClass, EncodeOptions.IgnoreAttributes));

        //Should encode excludedField and propertyValue
        Assert.AreEqual("{\"@type\":\"TestClassType+AttributeClass\",\"m_PrivateField\":4,\"propertyValue\":10}", JSON.Dump(aClass, EncodeOptions.EncodePrivateVariables));

        //Should encode m_PrivateField, excludedField, and m_PropertyValue
        Assert.AreEqual("{\"@type\":\"TestClassType+AttributeClass\",\"excludedField\":4,\"m_PrivateField\":4,\"m_PropertyValue\":10}", JSON.Dump(aClass, EncodeOptions.IgnoreAttributes | EncodeOptions.EncodePrivateVariables));
    }



    [Test]
    public void TestDumpClassNoTypeHint()
    {
        var testClass = new TestClass() { x = 5, y = 7, z = 0 };
        testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

        Assert.AreEqual("{\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump(testClass, EncodeOptions.NoTypeHints));
    }


    [Test]
    public void TestDumpClassPrettyPrint()
    {
        var testClass = new TestClass() { x = 5, y = 7, z = 0 };
        testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

        Assert.AreEqual(@"{
    ""x"": 5,
    ""y"": 7,
    ""list"": [
        3,
        1,
        4
    ]
}", JSON.Dump(testClass, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
    }


    [Test]
    public void TestLoadStruct()
    {
        TestClass testClass = JSON.Load("{\"x\":5,\"y\":7,\"z\":3,\"list\":[3,1,4]}}").Make<TestClass>();

        Assert.AreEqual(5, testClass.x);
        Assert.AreEqual(7, testClass.y);
        Assert.AreEqual(0, testClass.z); // should not get assigned

        Assert.AreEqual(3, testClass.list.Count);
        Assert.AreEqual(3, testClass.list[0]);
        Assert.AreEqual(1, testClass.list[1]);
        Assert.AreEqual(4, testClass.list[2]);

        Assert.IsTrue(afterDecodeCallbackFired);
    }


    class InnerClass
    {
    }

    class OuterClass
    {
        [TypeHint]
        public InnerClass inner;
    }

    [Test]
    public void TestDumpOuterNoTypeHint()
    {
        var outerClass = new OuterClass();
        outerClass.inner = new InnerClass();
        Assert.AreEqual("{\"inner\":{\"@type\":\"" + typeof(InnerClass).FullName + "\"}}", JSON.Dump(outerClass, EncodeOptions.NoTypeHints));
    }
}

