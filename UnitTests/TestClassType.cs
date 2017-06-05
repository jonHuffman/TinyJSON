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

		public int p1 { get; set; }
		public int p2 { get; private set; }
		public int p3 { get; }


		public TestClass()
		{
			p1 = 1;
			p2 = 2;
			p3 = 3;
		}


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

    public class AliasClass
    {
        [Include, Alias("Name")]
        private string m_StringValue;

        [Include, Alias("height")]
        public float floatValue;

        public string stringValue
        {
            get { return m_StringValue; }
            set { m_StringValue = value; }
        }

        private int m_IntValue;

        [Include, Alias("age")]
        public int IntValue
        {
            get { return m_IntValue; }
            set { m_IntValue = value; }
        }

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
	public void TestDumpClassIncludePublicProperties()
	{
		var testClass = new TestClass() { x = 5, y = 7, z = 0 };
		Console.WriteLine( JSON.Dump( testClass, EncodeOptions.NoTypeHints | EncodeOptions.IncludePublicProperties ) );
		Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":null,\"p1\":1,\"p2\":2,\"p3\":3}", JSON.Dump( testClass, EncodeOptions.NoTypeHints | EncodeOptions.IncludePublicProperties ) );
	}


    [Test]
    public void TestDumpClass()
    {
        var testClass = new TestClass() { x = 5, y = 7, z = 0 };
        testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

        Assert.AreEqual("{\"@type\":\"" + testClass.GetType().FullName + "\",\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump(testClass));

        Assert.IsTrue(beforeEncodeCallbackFired);
    }



    [Test]
    public void TestEncoderOptions()
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
    public void TestAliasAttribute()
    {

        //Encoding
        {
            AliasClass aClass = new AliasClass() { IntValue = 10, floatValue = 31.5f, stringValue = "Hamburger" };

            // Normal Dump
            Assert.AreEqual("{\"@type\":\"TestClassType+AliasClass\",\"Name\":\"Hamburger\",\"height\":31.5,\"age\":10}", JSON.Dump(aClass));

            // Ignore Attributes
            Assert.AreEqual("{\"@type\":\"TestClassType+AliasClass\",\"floatValue\":31.5}", JSON.Dump(aClass, EncodeOptions.IgnoreAttributes));
        }

        //Decoding
        {
            AliasClass aClass = null;

            JSON.MakeInto<AliasClass>(JSON.Load("{\"@type\":\"TestClassType+AliasClass\",\"Name\":\"Hamburger\",\"height\":31.5,\"age\":10}"), out aClass);

            Assert.AreEqual("Hamburger", aClass.stringValue);

            Assert.AreEqual(31.5, aClass.floatValue);

            Assert.AreEqual(10, aClass.IntValue);
        }
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

        // Looks weird but this works on both iOS and Windows 
        Assert.AreEqual(@"{" + Environment.NewLine +
                         "\t\"x\": 5," + Environment.NewLine +
                         "\t\"y\": 7," + Environment.NewLine +
                         "\t\"list\": [" + Environment.NewLine +
                         "\t\t3," + Environment.NewLine +
                         "\t\t1," + Environment.NewLine +
                         "\t\t4" + Environment.NewLine +
                         "\t]" + Environment.NewLine +
                         "}",
        JSON.Dump(testClass, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
    }


    [Test]
    public void TestLoadClass()
    {
        TestClass testClass = JSON.Load("{\"x\":5,\"y\":7,\"z\":3,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}").Make<TestClass>();

        Assert.AreEqual(5, testClass.x);
        Assert.AreEqual(7, testClass.y);
        Assert.AreEqual(0, testClass.z); // should not get assigned

        Assert.AreEqual(3, testClass.list.Count);
        Assert.AreEqual(3, testClass.list[0]);
        Assert.AreEqual(1, testClass.list[1]);
        Assert.AreEqual(4, testClass.list[2]);

        Assert.AreEqual(1, testClass.p1);
        Assert.AreEqual(2, testClass.p2);
        Assert.AreEqual(3, testClass.p3);

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

    public class Animal
    {
        public int age = 32;
    }

    public class Person : Animal
    {
        public string name = "Frank";
    }

    public class Cat : Animal
    {
        public string petName = "Mittens";
    }

    public class Family
    {
        public Animal Mom = new Person() { name = "Mary" };
        public Animal Dad = new Person() { name = "Dave" };
        public Animal Pet = new Cat() { petName = "Mittens" };
    }


    [Test]
    public void TestDumpOuterNoTypeHint()
    {
        var outerClass = new OuterClass();
        outerClass.inner = new InnerClass();
        Console.WriteLine(JSON.Dump(new Family()));
        Assert.AreEqual("{\"inner\":{\"@type\":\"" + typeof(InnerClass).FullName + "\"}}", JSON.Dump(outerClass, EncodeOptions.NoTypeHints));
    }
}

