## Description

TinyJSON is a simple JSON library for C# that strives for ease of use.

## Features

* Transmogrify objects into JSON and back again.
* Uses reflection to dump and load object graphs automagically.
* Supports primitives, classes, structs, enums, lists, dictionaries and arrays.
* Supports single dimensional arrays, multidimensional arrays and jagged arrays.
* Parsed data uses proxy variants that can be implicitly cast to primitive types for cleaner code.
* Numeric types are handled without fuss.
* Optional pretty printing JSON output.
* Polymorphic classes supported with a type hint encoded into the JSON.
* Optionally encode properties and private fields.
* Unit tested.
* Combine Json files


## Usage

The API is namespaced under `TinyJSON` and the primary class is `JSON`. There are really only three methods you need to know:

```csharp
namespace TinyJSON
{
	public static class JSON
	{
		public static Variant Load( string json );
		public static string Dump( object data, EncodeOptions.None );
		public static void MakeInto<T>( Variant data, out T item );
	}
}
```

`Load()` will load a string of JSON, returns `null` if invalid or a `Variant` proxy object if successful. The proxy allows for implicit casts and can convert between various C# numeric value types.

```csharp
var data = JSON.Load( "{\"foo\": 1, \"bar\": 2.34}" );
int i = data["foo"];
float f = data["bar"];
```

`Dump()` will take a C# object, list, dictionary or primitive value type and turn it into JSON.

```csharp
var data = new List<int>() { { 0 }, { 1 }, { 2 } };
Console.WriteLine( JSON.Dump( data ) ); // output: [1,2,3]
```

TinyJSON can also handle classes, structs, enums and nested objects. Given these definitions:

```csharp
enum TestEnum
{
	Thing1,
	Thing2,
	Thing3
}


struct TestStruct
{
	public int x;
	public int y;
}


class TestClass
{
	public string name;
	public TestEnum type;
	public List<TestStruct> data = new List<TestStruct>();

	[Exclude]
	public int _ignored;

    [Include, Alias("name")]
	private string m_Name;

	[BeforeEncode]
	public void BeforeEncode()
	{
		Console.WriteLine( "BeforeEncode callback fired!" );
	}

	[AfterDecode]
	public void AfterDecode()
	{
		Console.WriteLine( "AfterDecode callback fired!" );
	}

	[AfterDecode]
	public void AfterDecodeWithVariant(Variant variant)
	{
		Console.WriteLine( "AfterDecode callback fired but this type we have the Variant being loaded" );
		//You can use this variant to do any custom parsing that you want. 
	}
}
```

The following code:

```csharp
var testClass = new TestClass();
testClass.name = "Rumpelstiltskin Jones";
testClass.type = TestEnum.Thing2;
testClass.data.Add( new TestStruct() { x = 1, y = 2 } );
testClass.data.Add( new TestStruct() { x = 3, y = 4 } );
testClass.data.Add( new TestStruct() { x = 5, y = 6 } );

var testClassJson = JSON.Dump( testClass, EncodeOptions.PrettyPrint );
Console.WriteLine( testClassJson );
```

Will output:

```json
{
	"name": "Rumpelstiltskin Jones",
	"type": "Thing2",
	"data": [
		{
			"x": 1,
			"y": 2
		},
		{
			"x": 3,
			"y": 4
		},
		{
			"x": 5,
			"y": 6
		}
	]
}
```

You can use, `MakeInto()` can be used to reconstruct JSON data back into an object:

```csharp
TestClass testClass;
JSON.MakeInto( JSON.Load( testClassJson ), out testClass );
```

There are also `Make()` methods on `Variant` which provide options for slightly more natural syntax:

```csharp
TestClass testClass;

JSON.Load( json ).Make( out testClass );
// or
testClass = JSON.Load( json ).Make<Data>();
```

Finally, you'll notice that `TestClass` has the methods `BeforeEncode()` and `AfterDecode()` which have the `TinyJSON.BeforeEncode` and `TinyJSON.AfterDecode` attributes. These methods will be called *before* the object starts being serialized and *after* the object has been fully deserialized. This is useful when some further preparation or initialization logic is required.

By default, only public fields are encoded, not properties or private fields. You can tag any field or property to be included with the `TinyJSON.Include` attribute, or force a public field to be excluded with the `TinyJSON.Exclude` attribute.

## Type Hinting

When decoding polymorphic types, TinyJSON has no way of knowing which subclass to instantiate unless a type hint is included. So, by default, TinyJSON will add a key named `@type` to each encoded object with the fully qualified type of the object.

## Encode Options

Five options are currently available for JSON encoding, and can be passed in as a second parameter to `JSON.Dump()`.

* `EncodeOptions.PrettyPrint` will output nicely formatted JSON to make it more readable.
* `EncodeOptions.NoTypeHints` will disable the outputting of type hints into the JSON output. This may be desirable if you plan to read the JSON into another application that might choke on the type information. You can override this on a per-member basis with the `TinyJSON.TypeHint` attribute.
* `EncodeOptions.EncodePrivateVariables` will encode all private variables, they are ingored unless the IncludeAttribute is used. Only public ones are encoded by default. 
* `EncodeOptions.IgnoreAttributes` will encode skip the step of looking for attributes. This is a very slow part of decoding/encoding and turning this on will increase it's speed. 
* `EncodeOptions.Combinable` will encode array elements with a `@Index` key for all objects. This is used for merging arrays with `Variant.Combine()`

# Method Attributes
#### BeforeEncodeAttribute 
`BeforeEncode` will call the method before class is encoded. The method should return void and take no arguments.
```csharp
	public class Person
	{
		[BeforeEncode]
		public void BeforeEncoding()
		{
			Console.WriteLine("I am about to be encoded!"});
		}
	}
```


#### AfterDecodeAttribute
`AfterDecode` will call the method after the class has been decoded. The method should return void and take either no arguements or a Variant.
```csharp
	public class Person
	{
		[AfterDecode]
		public void AfterDecode()
		{
			Console.WriteLine("I have been decoded!");
		}

		[AfterDecode]
		public void AfterDecodeWithVariant(Variant variant)
		{
			Console.WriteLine("I have been decoded!");
		}
	}
```

# Field and Property Attributes
#### IncludeAttribute
`Include` will encode or decode this memeber. Only only has an effect on private fields and properties since public fields are encoded by default.
```csharp
	public class Person
	{
		public float height = 3.4f;
		[Include]
		private int m_Age = 23;
		private string m_WearingGlasses = false;

		[Include]
		public bool wearingGlasses
		{
			get { return  m_WearingGlasses; }
			set { m_WearingGlasses = value; }
		}
	}
```
```csharp
	JSON.Dump(new Person());
```
Will output:
```json
	{ 
		"height" : 3.4,
		"m_Age" : 23,
		"wearingGlasses" : false
	}
```


#### ExcludeAttribute
`Exlude` will skip encoding or decoding this member. Only has an effect on public fields since private fields and properties are not encoded by default.
```csharp
	public class Person
	{
		[Exclude]
		public float height = 3.4f;
		[Include]
		private int m_Age = 23;
		private string m_WearingGlasses = false;

		[Include]
		public bool wearingGlasses
		{
			get { return  m_WearingGlasses; }
			set { m_WearingGlasses = value; }
		}
	}
```
```csharp
	JSON.Dump(new Person());
```
Will output:
```json
	{ 
		"m_Age" : 23,
		"wearingGlasses" : false
	}
```


#### AliasAttribute
`Alias` will override the name of the member being encoded or decode. It will use it's alias name instead of the member name. 
```csharp
   public class Person
   {
		[Exclude]
		public float height = 3.4f;
		[Include, Alias("age")]
		private int m_Age = 23;
		private string m_WearingGlasses = false;

		[Include]
		public bool wearingGlasses
		{
			get { return  m_WearingGlasses; }
			set { m_WearingGlasses = value; }
		}
	}

```
```csharp
	JSON.Dump(new Person());
```
Will output:
```json
	{ 
		"age" : 23,
		"wearingGlasses" : false
	}
```


#### TypeHintAttribute
`TypeHint` will encode this member with the name of the type and assign it a key of `@type`. When decoded it will be created using the encoded type now. Used for polymorphism.
```csharp
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
    	[TypeHint]
        public Animal Mom = new Person() { name = "Mary" };
        [TypeHint]
        public Animal Dad = new Person() { name = "Dave" };
        [TypeHint]
        public Animal Pet = new Cat() { petName = "Mittens" };
    }
```

```csharp
    JSON.Dump(new Family());
```
Ouputs 
```json
	{
		"@type": "TestClassType+Family",
		"Mom": {
			"@type": "Person",
			"name": "Mary",
			"age": 32
		},
		"Dad": {
			"@type": "Person",
			"name": "Dave",
			"age": 32
		},
		"Pet": {
			"@type": "Cat",
			"petName": "Mittens",
			"age": 32
		}
	}
```
## Using Variants

For most use cases you can just assign, cast or make your object graph using the API outlined above, but at times you may need to work with the intermediate proxy objects to, say, dig through and iterate over a collection. To do this, cast the variant to the appropriate subclass (either `ProxyArray` or `ProxyObject`) and you're good to go:

```csharp
var list = JSON.Load( "[1,2,3]" );
foreach (var item in list as ProxyArray)
{
	int number = item;
	Console.WriteLine( number );
}

var dict = JSON.Load( "{\"x\":1,\"y\":2}" );
foreach (var pair in dict as ProxyObject)
{
	float value = pair.Value;
	Console.WriteLine( pair.Key + " = " + value );
}
```

## Combining JSON 
There are times where you want to combine one set of json data with another. This can be used to have one json file for default values and a second one to override any values that are contained in it. If any values are in the second one that are not in the first one they will be appended. In the example below we define the second person as Frank who is 32. We then override bot the name and age. The final result is Mike who is 100.

```csharp
//Define our people 
Variant people = JSON.Load("[{\"@index\":0,\"name\":\"Jenny\",\"age\":12},{\"@index\":1,\"name\":\"Frank\",\"age\":32}]");

//Redefine person[1] name to Mike from Frank
Variant nameOverride = JSON.Load("[{\"@index\":1,\"name\":\"Mike\"}]");

//Redefine person[1] age to 100 from 32
Variant ageOverride = JSON.Load("[{\"@index\":1,\"age\":100}]");

//Merge three variants into one ProxyArray
ProxyArray peopleArray = Variant.CombineInto<ProxyArray>(people, nameOverride, ageOverride);

JSON.Dump(peopleArray, EncodeOptions.PrettyPrint);
```

Will Output:
```json
[{
	"@index": 0,
	"name": "Jenny",
	"age": 12
}, {
	"@index": 1,
	"name": "Mike",
	"age": 100
}]
```
## Notes

This project was developed with pain elimination and lightweight size in mind. That said, it should be able able to handle reasonable amounts of reasonable data at reasonable speeds.

My primary use case for this library is with Mono and Unity3D (currently version 4), so compatibility is focused there, though it should work with most modern C# environments.

## Meta

Handcrafted by Patrick Hogan [[twitter](http://twitter.com/pbhogan) &bull; [github](http://github.com/pbhogan) &bull; [website](http://www.gallantgames.com)]

Extended by Byron Mayne [[twitter](https://twitter.com/byMayne) &bull; [github](https://github.com/ByronMayne)]

Based on [MiniJSON](https://gist.github.com/darktable/1411710) by Calvin Rien

Released under the [MIT License](http://www.opensource.org/licenses/mit-license.php).
