using System;
using TinyJSON;
using System.Collections.Generic;
using NUnit.Framework;
namespace UnitTests
{
	
	public abstract class MyBase
	{
		[Include]
		protected int ID; 
	}

	public class MyClass : MyBase
	{
		public struct MyStruct
		{
			public char c;
			public int i;
		}

		[Include]
		private string m_Key = "key";

		[Include]
		private MyStruct[] m_Structs;

		public string key
		{
			get 
			{
				return m_Key;
			}
			set
			{
				m_Key = value;
			}
		}

		public MyStruct[] structs 
		{
			get
			{
				return m_Structs;
			}

			set
			{

				m_Structs = value;
			}
		}
	}

	[TestFixture]
	public class ComplexObjectTest
	{
		[Test]
		public static void DumpComplexObject()
		{
			MyClass complexClass = new MyClass();
			List<MyClass.MyStruct> struts = new List<MyClass.MyStruct>(); 
			var s1 = new MyClass.MyStruct();
			s1.c = 'a';
			s1.i = 1;
			var s2 = new MyClass.MyStruct();
			s2.c = 'b';
			s2.i = 2;
			struts.Add(s1);
			struts.Add(s2);
			complexClass.structs = struts.ToArray();
			string json = JSON.Dump(complexClass);
			Console.WriteLine(json);

			var decodedClass = JSON.Load(json).Make<MyClass>();
			Assert.AreEqual(decodedClass.structs.Length, 2);
			Assert.AreEqual(decodedClass.structs[0].c, 'a');
			Assert.AreEqual(decodedClass.structs[1].c, 'b');
			Assert.AreEqual(decodedClass.structs[0].i, 1);
			Assert.AreEqual(decodedClass.structs[1].i, 2);
			Assert.AreEqual(decodedClass.key, "key");
		}

		[Test]
		public static void DumpAbstractClassWithNoTypeHint()
		{
			string json = "{\"m_Key\":\"key\",\"m_Structs\":[{\"@type\":\"UnitTests.MyClass+MyStruct\",\"c\":\"a\",\"i\":1},{\"@type\":\"UnitTests.MyClass+MyStruct\",\"c\":\"b\",\"i\":2}],\"ID\":0}";
			var parsedClass = JSON.Load(json).Make<MyClass>();

			Console.WriteLine(parsedClass.GetType().FullName);
		}
	}
}

