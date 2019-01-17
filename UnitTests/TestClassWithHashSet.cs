using NUnit.Framework;
using System.Collections.Generic;
using TinyJSON;

[TestFixture]
public class TestClassWithHashSet
{
    private class TestClass
    {
        public HashSet<bool> hashSet;

        public TestClass()
        {
            hashSet = new HashSet<bool>
            {
                true,
                false
            };
        }
    }

    [Test]
    public void DumpClassWithHashSet()
    {
        var json = "{\"@type\":\"TestClassWithHashSet+TestClass\",\"hashSet\":[true,false]}";
        var testClass = new TestClass();
        Assert.AreEqual(json, JSON.Dump(testClass));
    }

    [Test]
    public void LoadClassWithHashSet()
    {
        var json = "{\"@type\":\"TestClassWithHashSet+TestClass\",\"hashSet\":[true,false]}";
        var testClass = JSON.Load(json).Make<TestClass>();
        Assert.AreEqual(json, JSON.Dump(testClass));
    }
}
