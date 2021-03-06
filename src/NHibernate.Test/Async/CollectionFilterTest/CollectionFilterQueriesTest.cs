﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using NHibernate.Hql.Ast.ANTLR;
using NUnit.Framework;
using System.Collections;
using NHibernate.DomainModel;

namespace NHibernate.Test.CollectionFilterTest
{
	using System.Threading.Tasks;
	[TestFixture]
	public class CollectionFilterQueriesTestAsync : TestCase
	{
		protected override IList Mappings
		{
			get
			{
				return new string[] { "One.hbm.xml", "Many.hbm.xml" };
			}
		}

		private One one;

		protected override void OnSetUp()
		{
			base.OnSetUp();

			// create the objects to search on		
			one = new One();
			one.X = 20;
			one.Manies = new HashSet<Many>();

			Many many1 = new Many();
			many1.X = 10;
			many1.One = one;
			one.Manies.Add( many1 );

			Many many2 = new Many();
			many2.X = 20;
			many2.One = one;
			one.Manies.Add( many2 );

			using( ISession s = OpenSession() )
			using( ITransaction t = s.BeginTransaction() )
			{
				s.Save( one );
				s.Save( many1 );
				s.Save( many2 );
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using( ISession session = OpenSession() )
			using( ITransaction tx = session.BeginTransaction() )
			{
				session.Delete( "from Many" );
				session.Delete( "from One" );
				tx.Commit();
			}
			base.OnTearDown();
		}

		[Test]
		public async Task UpdateShouldBeDisallowedAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				One one2 = (One)await (s.CreateQuery("from One").UniqueResultAsync());

				Assert.ThrowsAsync<QuerySyntaxException>(async () =>
				{
					await ((await (s.CreateFilterAsync(one2.Manies, "update Many set X = 1")))
						.ExecuteUpdateAsync());
					// Collection filtering disallows DML queries
				});

				await (t.RollbackAsync());
			}
		}

		[Test]
		public async Task DeleteShouldBeDisallowedAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				One one2 = (One)await (s.CreateQuery("from One").UniqueResultAsync());

				Assert.ThrowsAsync<QuerySyntaxException>(async () =>
				{
					await ((await (s.CreateFilterAsync(one2.Manies, "delete from Many")))
						.ExecuteUpdateAsync());
					// Collection filtering disallows DML queries
				});

				await (t.RollbackAsync());
			}
		}

		[Test]
		public async Task InsertShouldBeDisallowedAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				One one2 = (One)await (s.CreateQuery("from One").UniqueResultAsync());

				Assert.ThrowsAsync<QuerySyntaxException>(async () =>
				{
					await ((await (s.CreateFilterAsync(one2.Manies, "insert into Many (X) select t0.X from Many t0")))
						.ExecuteUpdateAsync());
					// Collection filtering disallows DML queries
				});

				await (t.RollbackAsync());
			}
		}

		[Test]
		public async Task InnerSubqueryShouldNotBeFilteredAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				One one2 = (One)await (s.CreateQuery("from One").UniqueResultAsync());

				await ((await (s.CreateFilterAsync(one2.Manies, "where this.X in (select t0.X from Many t0)")))
					.ListAsync());
				// Filter should only affect outer query, not inner

				await (t.RollbackAsync());
			}
		}

		[Test]
		public async Task InnerSubqueryMustHaveFromClauseAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				One one2 = (One)await (s.CreateQuery("from One").UniqueResultAsync());

				Assert.ThrowsAsync<QuerySyntaxException>(async () =>
				{
					await ((await (s.CreateFilterAsync(one2.Manies, "where this.X in (select X)")))
						.ListAsync());
					// Inner query for filter query should have FROM clause 
				});

				await (t.RollbackAsync());
			}
		}
	}
}
