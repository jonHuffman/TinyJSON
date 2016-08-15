using System;
using System.Collections.Generic;

#if NETCORE
using System.Reflection;
#endif


namespace TinyJSON
{
	public static class Extensions
	{
		public static bool AnyOfType<TSource>( this IEnumerable<TSource> source, Type expectedType )
		{
			if (source == null)
			{
				throw new ArgumentNullException( "source" );
			}

			if (expectedType == null)
			{
				throw new ArgumentNullException( "expectedType" );
			}

			foreach (var item in source)
			{
#if !NETCORE
				if (expectedType.IsAssignableFrom(item.GetType()))
#else
				if (expectedType.GetTypeInfo().IsAssignableFrom(item.GetType().GetTypeInfo()))
#endif
				{
					return true;
				}
			}

			return false;
		}
	}
}

