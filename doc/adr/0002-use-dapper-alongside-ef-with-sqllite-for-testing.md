# 1. Use Dapper Light ORM alongside EF Core. Also use SQLLite in memory db, instead of EF Core In Memory for related unit tests

Date: 2023-08-14

## Status

Accepted

## Context

By default, we use EF Core as ORM for our projects. But in some cases, EF Core is not able to translate advanced LINQ queries to efficient SQL, and some times it gives an error stating that it cannot be translated at all. 
EF Core does have a limited feature that allows execution of hand written SQL queries, but with huge limitations. When we get to the point where we need to execute raw sql, we have allready exceeded the limits of EF Core. 
Therefore, we need an alternative way of executing hand written advanced queries against the database. Dapper can solve this.
In addition, when we write unit tests, we currently use EF Core in-memory DB. But there is no way for other frameworks to reach this database. 
The suggestion is to use SQLLite in-memory DB for those cases.


## Decision

We use Dapper Light ORM alongside EF Core, for advanced queryes. Dapper connects to the database using the same connection object as EF Core.
When writing unit tests for code that uses Dapper, we use SQLLite in-memory DB. This allows both EF Core and Dapper to read/write to the same in-memory DB.


## Consequences

### Hand written sql queries are harder to understand and maintain than LINQ queries.
Therefore EF Core remains the default ORM, and most queries will still be executed through EF Core.
Dapper is used when on an existing or new query is not performing well, and we need to write the query by hand.

### EF Core in-memory DB does not enforce Foreign Key (FK) relationships.
Therefore, most existing test are not respecting FK relations, as this has not been a demand.
When adding Dapper to existing code, the setup part for tests that covers that code will have to be re-written.
This means changing to use SQL Lite in-memory DB, and adding all related entities that are referenced via a FK relation.

For that reason, we will not not re-write existing tests as long as they are not covering code using Dapper.   