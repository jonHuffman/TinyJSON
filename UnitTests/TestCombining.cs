using NUnit.Framework;
using System;
using System.Collections.Generic;
using TinyJSON;

namespace UnitTests
{
    [TestFixture]
    public class TestCombining
    {
        public class Person
        {
            public Person()
            {
                this.name = null;
                this.age = 0;
            }

            public string name;
            public int age;

            public Person(string name, int age)
            {
                this.age = age;
                this.name = name;
            }
        }

        [Test]
        public void IndexCreationTest()
        {
            List<Person> m_JsonArray = new List<Person>();
            m_JsonArray.Add(new Person("Jenny", 12));
            m_JsonArray.Add(new Person("Frank", 32));

            //Check to see if index was added and incremented correctly. 
            {
                string json = JSON.Dump(m_JsonArray, EncodeOptions.Combinable | EncodeOptions.NoTypeHints);
                Assert.AreEqual("[{\"@index\":\"0\",\"name\":\"Jenny\",\"age\":12},{\"@index\":\"1\",\"name\":\"Frank\",\"age\":32}]", json);
            }

            //This should not have any @index values. 
            {
                string json = JSON.Dump(m_JsonArray, EncodeOptions.NoTypeHints);
                Assert.AreEqual("[{\"name\":\"Jenny\",\"age\":12},{\"name\":\"Frank\",\"age\":32}]", json);
                Console.WriteLine(json);
            }
        }

        [Test]
        public void CombineTest()
        {
            Variant var1 = JSON.Load("[{\"@index\":0,\"name\":\"Jenny\",\"age\":12},{\"@index\":1,\"name\":\"Frank\",\"age\":32}]");

            //Redefine person[1] name to Mike from Frank
            Variant var2 = JSON.Load("[{\"@index\":1,\"name\":\"Mike\"}]");

            //Redefine person[1] age to 100 from 32
            Variant var3 = JSON.Load("[{\"@index\":1,\"age\":100}]");

            ProxyArray combined = (ProxyArray)Variant.Combine(var1, var2);
            combined = (ProxyArray)Variant.Combine(combined, var3);

            Person[] persons = combined.Make<Person[]>();

            Assert.AreEqual(2, persons.Length);
            Assert.AreEqual("Jenny", persons[0].name);
            Assert.AreEqual(12, persons[0].age);
            Assert.AreEqual("Mike", persons[1].name);
            Assert.AreEqual(100, persons[1].age);

        }
    }

}
