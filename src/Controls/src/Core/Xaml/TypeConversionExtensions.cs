//
// TypeConversionExtensions.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2014 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls.Xaml.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	static class TypeConversionExtensions
	{
		internal static Dictionary<Type, Type> KnownConverters = new Dictionary<Type, Type>
		{
			{ typeof(Font), typeof(FontTypeConverter) }
		};

		static Dictionary<Type, Func<TypeConverter>> KnownConverterFactories = new()
		{
			{ typeof(Font), () => new FontTypeConverter() }
		};

		// caches both Type and MemberInfo keys to their corresponding TypeConverter
		static readonly ConcurrentDictionary<MemberInfo, TypeConverter> s_converterCache = new();

		internal static object ConvertTo(this object value, Type toType, Func<ParameterInfo> pinfoRetriever,
			IServiceProvider serviceProvider, out Exception exception)
		{
			Func<TypeConverter> getConverter = () =>
			{
				if (pinfoRetriever == null || pinfoRetriever() is not ParameterInfo pInfo)
					return null;

				var convertertype = pInfo.CustomAttributes.GetTypeConverterType();
				if (convertertype == null)
					return null;
				return (TypeConverter)Activator.CreateInstance(convertertype);
			};

			return ConvertTo(value, toType, getConverter, serviceProvider, out exception);
		}

		internal static object ConvertTo(this object value, Type toType, Func<MemberInfo> minfoRetriever,
			IServiceProvider serviceProvider, out Exception exception)
		{
			Func<TypeConverter> getConverter = () =>
			{
				TypeConverter converter = null;
				if (minfoRetriever != null && minfoRetriever() is MemberInfo memberInfo)
				{
					if (!s_converterCache.TryGetValue(memberInfo, out converter))
					{
						if (memberInfo.CustomAttributes.GetTypeConverterType() is Type converterType)
						{
							converter = (TypeConverter)Activator.CreateInstance(converterType);
						}

						// cache the result, even if it is null
						s_converterCache[memberInfo] = converter;
					}

					if (converter is not null)
					{
						return converter;
					}
				}

				if (KnownConverterFactories.TryGetValue(toType, out var converterFactory))
					return converterFactory();

				if (!s_converterCache.TryGetValue(toType, out converter))
				{
					if (toType.CustomAttributes.GetTypeConverterType() is Type converterType)
					{
						converter = (TypeConverter)Activator.CreateInstance(converterType);
					}

					// cache the result, even if it is null
					s_converterCache[toType] = converter;
				}

				return converter;
			};

			return ConvertTo(value, toType, getConverter, serviceProvider, out exception);
		}

		static Type GetTypeConverterType(this IEnumerable<CustomAttributeData> attributes)
		{
			foreach (var converterAttribute in attributes)
			{
				if (converterAttribute.AttributeType != typeof(System.ComponentModel.TypeConverterAttribute))
					continue;
				var ctor = converterAttribute.ConstructorArguments[0];
				if (ctor.ArgumentType == typeof(string))
					return Type.GetType((string)ctor.Value);
				if (ctor.ArgumentType == typeof(Type))
					return (Type)ctor.Value;
			}
			return null;
		}

		//Don't change the name or the signature of this, it's used by XamlC
		public static object ConvertTo(
			this object value,
			Type toType,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type convertertype,
			IServiceProvider serviceProvider)
		{
			Exception exception = null;
			object ret = null;
			if (convertertype == null)
			{
				ret = value.ConvertTo(toType, (Func<TypeConverter>)null, serviceProvider, out exception);
				if (exception != null)
					throw exception;
				return ret;
			}
			Func<TypeConverter> getConverter = () => (TypeConverter)Activator.CreateInstance(convertertype);
			ret = value.ConvertTo(toType, getConverter, serviceProvider, out exception);
			if (exception != null)
				throw exception;
			return ret;
		}

		internal static object ConvertTo(this object value, Type toType, Func<TypeConverter> getConverter,
			IServiceProvider serviceProvider, out Exception exception)
		{
			exception = null;
			if (value == null)
				return null;

			if (value is string str)
			{
				//If there's a [TypeConverter], use it
				TypeConverter converter;
				try
				{ //minforetriver can fail
					converter = getConverter?.Invoke();
				}
				catch (Exception e)
				{
					exception = e;
					return null;
				}
				try
				{
					if (converter is IExtendedTypeConverter xfExtendedTypeConverter)
						return xfExtendedTypeConverter.ConvertFromInvariantString(str, serviceProvider);
					if (converter is TypeConverter xfTypeConverter)
						return xfTypeConverter.ConvertFromInvariantString(str);
				}
				catch (Exception e)
				{
					exception = e as XamlParseException ?? new XamlParseException($"Type converter failed: {e.Message}", serviceProvider, e);
					return null;
				}

				if (converter != null)
				{
					try
					{
						return converter.ConvertFromInvariantString(str);
					}
					catch (Exception e)
					{
						exception = new XamlParseException("Type conversion failed", serviceProvider, e);
						return null;
					}
				}

				var ignoreCase = (serviceProvider?.GetService(typeof(IConverterOptions)) as IConverterOptions)?.IgnoreCase ?? false;

				//If the type is nullable, as the value is not null, it's safe to assume we want the built-in conversion
				if (toType.GetTypeInfo().IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
					toType = Nullable.GetUnderlyingType(toType);

				//Obvious Built-in conversions
				try
				{
					if (toType.GetTypeInfo().IsEnum)
						return Enum.Parse(toType, str, ignoreCase);
					if (toType == typeof(SByte))
						return SByte.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Int16))
						return Int16.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Int32))
						return Int32.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Int64))
						return Int64.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Byte))
						return Byte.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(UInt16))
						return UInt16.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(UInt32))
						return UInt32.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(UInt64))
						return UInt64.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Single))
						return Single.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Double))
						return Double.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Boolean))
						return Boolean.Parse(str);
					if (toType == typeof(TimeSpan))
						return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(DateTime))
						return DateTime.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Char))
					{
						Char.TryParse(str, out var c);
						return c;
					}
					if (toType == typeof(String) && str.StartsWith("{}", StringComparison.Ordinal))
						return str.Substring(2);
					if (toType == typeof(String))
						return value;
					if (toType == typeof(Decimal))
						return Decimal.Parse(str, CultureInfo.InvariantCulture);
				}
				catch (FormatException fe)
				{
					exception = fe;
					return null;
				}
			}

			//if the value is not assignable and there's an implicit conversion, convert
			if (value != null && !toType.IsAssignableFrom(value.GetType()))
			{
				var opImplicit = value.GetType().GetImplicitConversionOperator(fromType: value.GetType(), toType: toType)
								?? toType.GetImplicitConversionOperator(fromType: value.GetType(), toType: toType);

				if (opImplicit != null)
				{
					value = opImplicit.Invoke(null, new[] { value });
					return value;
				}
			}

			var platformValueConverterService = DependencyService.Get<INativeValueConverterService>();

			object platformValue = null;
			if (platformValueConverterService != null && platformValueConverterService.ConvertTo(value, toType, out platformValue))
				return platformValue;

			return value;
		}

		internal static MethodInfo GetImplicitConversionOperator(this Type onType, Type fromType, Type toType)
		{
			var bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
			IEnumerable<MethodInfo> mis = null;
			try
			{
				mis = new[] { onType.GetMethod("op_Implicit", bindingAttr, null, new[] { fromType }, null) };
			}
			catch (AmbiguousMatchException)
			{
				mis = new List<MethodInfo>();
				foreach (var mi in onType.GetMethods(bindingAttr))
				{
					if (mi.Name != "op_Implicit")
						break;
					var parameters = mi.GetParameters();
					if (parameters.Length == 0)
						continue;
					if (!parameters[0].ParameterType.IsAssignableFrom(fromType))
						continue;
					((List<MethodInfo>)mis).Add(mi);
				}
			}

			foreach (var mi in mis)
			{
				if (mi == null)
					continue;
				if (!mi.IsSpecialName)
					continue;
				if (!mi.IsPublic)
					continue;
				if (!mi.IsStatic)
					continue;
				if (!toType.IsAssignableFrom(mi.ReturnType))
					continue;

				return mi;
			}
			return null;
		}
	}
}