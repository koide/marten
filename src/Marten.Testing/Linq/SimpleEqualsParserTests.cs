﻿using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Marten.Services;
using Marten.Linq;

namespace Marten.Testing.Linq
{
	public class SimpleEqualsParserTests : DocumentSessionFixture<NulloIdentityMap>
	{
		class QueryTarget
		{
			public int IntProp { get; set; }
			public long LongProp { get; set; }
			public decimal DecimalProp { get; set; }
			public bool BoolProp { get; set; }
			public System.Guid Id { get; set; }
		}

		[Fact]
		public void CanTranslateEqualsToQueries()
		{
			var queryTarget = new QueryTarget
			{
				IntProp = 1,
				LongProp = 2,
				DecimalProp = 1.1m,
				BoolProp = true,
				Id = Guid.NewGuid()
			};

			theSession.Store(queryTarget);

			theSession.SaveChanges();

			var itemFromDb = theSession.Query<QueryTarget>()
				.Where(x => x.IntProp.Equals(queryTarget.IntProp))
				.Where(x => x.LongProp.Equals(queryTarget.LongProp))
				.Where(x => x.DecimalProp.Equals(queryTarget.DecimalProp))
				.Where(x => x.BoolProp.Equals(queryTarget.BoolProp))
				.Where(x => x.Id.Equals(queryTarget.Id))
				.FirstOrDefault();

			Assert.NotNull(itemFromDb);
		}

		[Fact]
		public void ThrowsWhenValueNotConvertibleToComparandType()
		{
			var queryTarget = new QueryTarget
			{
				Id = System.Guid.NewGuid()
			};
			theSession.Store(queryTarget);

			theSession.SaveChanges();

			object notInt = "not int";

			Assert.Throws<BadLinqExpressionException>(() =>
			{
				theSession.Query<QueryTarget>()
					.Where(x => x.IntProp.Equals(notInt))
					.FirstOrDefault();
			});
		}


		public class TestData : IEnumerable<object[]>
		{
			private readonly List<object[]> _data = new List<object[]>
			{				
				new object[] { Guid.NewGuid() },
				new object[] { 0 },
				new object[] { null },
				new object[] { false },
				new object[] { 32m },
				new object[] { 0L },
			};

			public IEnumerator<object[]> GetEnumerator()
			{
				return _data.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		[Theory]
		[ClassData(typeof(TestData))]
		public void EqualsGeneratesSameSqlAsEqualityOperator(object value)
		{
			string queryEquals = null, queryEqualOperator = null;
			switch (value)
			{
				case int intValue:
					queryEquals = theSession.Query<QueryTarget>()
						.Where(x => x.IntProp.Equals(intValue)).ToCommand().CommandText;
					queryEqualOperator = theSession.Query<QueryTarget>()
						.Where(x => x.IntProp == intValue).ToCommand().CommandText;
					break;
				case Guid guidValue:
					queryEquals = theSession.Query<QueryTarget>()
						.Where(x => x.Id.Equals(guidValue)).ToCommand().CommandText;
					queryEqualOperator = theSession.Query<QueryTarget>()
						.Where(x => x.Id == guidValue).ToCommand().CommandText;
					break;
				case decimal decimalValue:
					queryEquals = theSession.Query<QueryTarget>()
						.Where(x => x.DecimalProp.Equals(decimalValue)).ToCommand().CommandText;
					queryEqualOperator = theSession.Query<QueryTarget>()
						.Where(x => x.DecimalProp == decimalValue).ToCommand().CommandText;
					break;
				case long longValue:
					queryEquals = theSession.Query<QueryTarget>()
						.Where(x => x.LongProp.Equals(longValue)).ToCommand().CommandText;
					queryEqualOperator = theSession.Query<QueryTarget>()
						.Where(x => x.LongProp == longValue).ToCommand().CommandText;
					break;
				// Null
				default:
					queryEquals = theSession.Query<QueryTarget>()
						.Where(x => x.BoolProp.Equals(null)).ToCommand().CommandText;
					queryEqualOperator = theSession.Query<QueryTarget>()
						.Where(x => x.BoolProp == null).ToCommand().CommandText;
					break;
			}
			Assert.Equal(queryEqualOperator, queryEquals);
		}
	}
}