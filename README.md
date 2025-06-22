# DapperSqlConstructor

This project was created for reduce the amount of routine work (mapping) for .Net developers who use Dapper to work with databases.
This resource give an opportunity to construct methods for simple related table, and to do work with mapping between columns and properties less.

Application parse sql scripts for creating tables and models which related for those tables and build methods for SELECT, UPDATE and INSERT.

# Rules for sending files with scripts.
- Scripts for table should be simple without writed constrains in script of table without other rules for work with data like "CASCADE", better have only table, columns and rules for foreign keys.
- The tables should be in the order of relationship, the first table should be the one that has no secondary keys and no bindings to other tables (primary). Then the children of this table that have secondary keys are related to the main one, then the tables related to them, etc.
- If in your structure the primary table has a foreign key, drop it as it isn't nessasary for this case.
- 




