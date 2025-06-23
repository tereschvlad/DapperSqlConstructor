# DapperSqlConstructor

**Automatically generate C# methods for Dapper based on your SQL schema and C# models.**

This project was created for reduce the amount of routine work (mapping) for .Net developers who use Dapper to work with databases.
This resource give an opportunity to construct methods for simple related table, and to do work with mapping between columns and properties less.

Application parse sql scripts for creating tables and models which related for those tables and build methods for SELECT, UPDATE and INSERT.

# Rules for sending files with scripts.
- Scripts for table should be simple without writed constrains in script of table without other rules for work with data like "CASCADE", better have only table, columns and rules for foreign keys.
- The tables should be in the order of relationship, the first table should be the one that has no secondary keys and no bindings to other tables (primary). Then the children of this table that have secondary keys are related to the main one, then the tables related to them, etc.
- If in your structure the primary table has a foreign key, drop it as it isn't nessasary for this case.
- Must be only one primary table (without foreign key).

# Example for tables script

```SQL

CREATE TABLE parent_table (
    parent_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    category VARCHAR(50)
);

CREATE TABLE child_table1 (
    child1_id SERIAL PRIMARY KEY,
    parent_id INT NOT NULL,
    child_name VARCHAR(100) NOT NULL,
    child_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (parent_id) REFERENCES parent_table(parent_id) ON DELETE CASCADE
);

CREATE TABLE child_table2 (
    child2_id SERIAL PRIMARY KEY,
    child1_id INT NOT NULL,
    child_name VARCHAR(100) NOT NULL,
    child_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (child1_id) REFERENCES child_table1(child1_id) ON DELETE CASCADE
);

```
# Rules for sending files with related models. 
- Send only models which related for sended tables.
- Write related tables for classes into comments for class, the related table must be writed like this "(Table: Related_Table)".
- Write related columns for properties into comments for property, the related table must be weited like this "(Column: Related_Column)"

# Example for C# class models

```C#
    /// <summary>
    /// Represents the parent_table in the database.
    /// (Table: parent_table)
    /// </summary>
    public class Parent
    {
        /// <summary>
        /// Gets or sets the unique identifier for the parent.
        /// (Column: parent_id)
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the parent entity.
        /// (Column: name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the parent entity.
        /// (Column: description)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// (Column: created_at)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// (Column: updated_at)
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent entity is active.
        /// (Column: is_active)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the category of the parent entity.
        /// (Column: category)
        /// </summary>
        public string Category { get; set; }
    }
	
	
	/// <summary>
    /// Represents the child_table1 in the database.
    /// (Table: child_table1)
    /// </summary>
    public class Child1
    {
        /// <summary>
        /// Gets or sets the unique identifier for the child1 entity.
        /// (Column: child1_id)
        /// </summary>
        public int Child1Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the parent entity.
        /// (Column: parent_id)
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the child1 entity.
        /// (Column: child_name)
        /// </summary>
        public string ChildName { get; set; }

        /// <summary>
        /// Gets or sets the description of the child1 entity.
        /// (Column: child_description)
        /// </summary>
        public string ChildDescription { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// (Column: created_at)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// (Column: updated_at)
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the child1 entity is active.
        /// (Column: is_active)
        /// </summary>
        public bool IsActive { get; set; }
    }
	
	/// <summary>
    /// Represents the child_table2 in the database.
    /// (Table: child_table2)
    /// </summary>
    public class Child2
    {
        /// <summary>
        /// Gets or sets the unique identifier for the child2 entity.
        /// (Column: child2_id)
        /// </summary>
        public int Child2Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the child1 entity.
        /// (Column: child1_id)
        /// </summary>
        public int Child1Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the child2 entity.
        /// (Column: child_name)
        /// </summary>
        public string ChildName { get; set; }

        /// <summary>
        /// Gets or sets the description of the child2 entity.
        /// (Column: child_description)
        /// </summary>
        public string ChildDescription { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// (Column: created_at)
        /// </summary>
		public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// (Column: updated_at)
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the child2 entity is active.
        /// (Column: is_active)
        /// </summary>
        public bool IsActive { get; set; }
    }

```
# How it work.
After you sending files you get a Dapper methods which doing select through all related tables, simple select script and method for UPDATE and INSERT into all tables. 


