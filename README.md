# SWE3-OR-Mapper
This OR-Mapper-project was done by Florian Weiss as part of the course Software Engineering 3.



## General
For all OR-mapping functionalities the Orm class is used.
This class contains functions which can be used to create, read, update and delete objects from the database.
The framework supports 1:1, 1:N and M:N relations between objects.



## Setup
To use this OR-Mapper a postgres database is needed.
The SWE3-OR-Mapper.SampleApp project contains a `docker-compose.yaml` file, which sets up a postgres docker container.
To run the unit tests multiple postgres docker containers are recommended.
The SWE3-OR-Mapper.Tests project also contains a `docker-compose.yaml` which sets up multiple postgres docker containers.
Both `docker-compose.yaml` files can be executed with `docker-compose up -d`. To stop and remove all started containers `docker-compose down` is used.<br><br>
The required tools to be installed for this setup are docker and docker-compose.



## Usage
In all usage depictions the library example from the SWE3-OR-Mapper.SampleApp project is used.


### Attributes for classes
Attributes are needed to use custom classes with the Orm class.

#### Entity
To define a class as an entity, the `Entity` attribute is used.
```
[Entity(TableName = "LOCATIONS")]
public class Location { ... }
```
The table name can be specified, otherwise the class name is used.

#### Field
Per default, all attributes of a class marked with the `Entity` attribute are a `Field`.
To override the name of the column for the attribute, the `Field` attribute is used.
```
[Field(ColumnName = "PUBDATE")]
public DateTime PublicationDate { get; set; }
```

#### Primary key
To set an class attribute as the primary key, the `PrimaryKey` attribute is used.
```
[PrimaryKey]
public string ID { get; set; }
```

#### Foreign key
To mark an attribute as foreign key, the `ForeignKey` attribute is used.
```
[Entity(TableName = "LOCATIONS")]
public class Location {
    ... 
    [ForeignKey(ColumnName = "KLOCATION")]
    public List<Employee> Employees { get; private set; }
    ...
}
```
The `ColumnName` of the class in the remote table needs to be specified.<br>

In the other class, where the foreign key is referenced, the attribute also needs to be used with the same ColumnName.
```
[Entity(TableName = "EMPLOYEES")]
public class Employee : Person {
    ... 
    [ForeignKey(ColumnName = "KLOCATION")]
    public Location Location { get; set; }
    ...
}

```

<br>When using an M:N relation, the `ForeignKey` attribute is only used in one class.
```
[Entity(TableName = "AUTHORS")]
public class Author : Person
{
    [ForeignKey(AssignmentTable = "AUTHOR_BOOK", ColumnName = "KAUTHOR", RemoteColumnName = "KBOOK")]
    public List<Book> Books { get; set; } = new List<Book>();
}
```
`AssignmentTable`, `ColumnName` and `RemoteColumnName` need to be specified.
* `AssignmentTable`: Table name where the M:N relation is stored
* `ColumnName`: Column name of current class in the `AssignmentTable`
* `RemoteColumnName`: Column name of other class in the `AssignmentTable`

#### Ignore
To ignore an attribute of a class in the OR mapping mechanism, the `Ignore` attribute is used.
```
[Ignore]
public int InstanceNumber { get; protected set; } = _N++;
```


### Set database connection
To be able to connect to the database with the framework the `Connection` needs to be specified:  
`Orm.Connection = new NpgsqlConnection("Host=localhost;Username=user;Password=password;Database=postgres");`

Afterwards, the connection needs to be opened:  
`Orm.Connection.Open();`

When all OR operations are done, the connection can be closed:  
`Orm.Connection.Close();`


### Set cache
To use caching the `Cache` needs to be set. To use the basic cache use the `Cache` class:  
`Orm.Cache = new Cache();`

To use caching with change tracking use the `HashCache` class:  
`Orm.Cache = new HashCache();`

To remove the cache the `Cache` is set to `null`:  
`Orm.Cache = null;`


### Save object
An object is saved with the `Save()` method:
```
Location l = new Location();
l.ID = "l.0";
l.Name = "Vienna City Library";
l.Country = "Austria";
l.City = "Vienna";
l.PostalCode = 1010;
l.Street = "Kingstreet";
l.HouseNumber = 25;

Orm.Save(l);
```


### Get object
An object of a given type is retrieved with the `Get<T>()` method by its primary key (ID):
```
Location l = Orm.Get<Location>("l.0");
```


### Get all objects
All objects of a type are retrieved with the `GetAll<T>()` method:
```
IEnumerable<Location> locations = Orm.GetAll<Location>();
```
LINQ expressions can be used with this function. For example: Getting all libraries in Vienna:
```
IEnumerable<Location> locations = Orm.GetAll<Location>().Where(l => l.City == "Vienna");
```


### Count objects
The number of stored objects with specified type are retrieved with the `Count<T>()` function:
```
int count = Orm.Count<Location>();
```


### Delete object
To delete an object the `Delete()` function is used:
```
Location l = Orm.Get<Location>("l.0");
Orm.Delete(l);
```
