# OpenApiQuery

**Disclaimer:** This project is still in a very early proof-of-concept phase and not ready for any production.

OpenApiQuery is a library inspired by OData to provide an easy way of interacting with your REST API resources.
It adopts from OData parts of the query syntax as described in the OData specification [OData Version 4.01. Part 2: URL Conventions]( https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part2-url-conventions.html)

The ultimate goal is to support a full OData alike experience with using an OpenAPI specification as replacement for the EDM where needed. Hence the name **OpenApi**Query.


## Framework features

| Feature             | Description                                                                   | Status            |
| ----------------    | ----------------------------------------------------------------------------- | ----------------- |
| `$select`           | Limit the returned properties                                                 | Supported         |
| `$filter`           | Filter result entities                                                        | Supported         |
| `$expand`           | Include related navigation properties                                         | Supported         |
| `$expand($filter)`  | Filter related navigation properties which are included                       | Supported         |
| `$expand($expand)`  | Filter related navigation properties which are included                       | Supported         |
| `$expand($orderby)` | Apply ordering to the expanded navigation properties                          | Supported         |
| `$expand($skip)`    | Apply paging to the expanded navigation properties                            | Supported         |
| `$expand($top)`     | Apply paging to the expanded navigation properties                            | Supported         |
| `$skip`             | Skip N elements in the result set                                             | Supported         |
| `$top`              | Select the top N elements in the result set                                   | Supported         |
| `$count`            | Provide the total count of items in the data source (with filters applied)    | Supported         |
| `Delta<T>`          | Accept a partial entity                                                       | Supported         |
| `SingleResult<T>`   | Return a single entity with select and expand capabilities                    | Supported         |
| Open Types          | Extend your entities with any dynamic property                                | Not yet supported |
| Data Aggregation    | See [OData Extension for Data Aggregation Version 4.0](http://docs.oasis-open.org/odata/odata-data-aggregation-ext/v4.0/cs01/odata-data-aggregation-ext-v4.0-cs01.html) | Not yet supported |

## OData URL Conventions Compatibility

| Feature                                               | Status |
| ---------------------------------------------         | -------|
| 3. Service Root URL                                   | Not planned                                                                              |
| **4. Resource Path**                                  | All not planned unless mentioned below                                                   |
| 4.8 Addressing the Count of a Collection              | Not yet supported (only planned for system query options)                                |
| 4.11 Addressing Derived Types                         | Not yet supported (only planned for system query options)                                |
| **5. Query Options**                                  | -                                                                                        |
| **5.1. System Query Options**                         | Partially Supported                                                                      |
| **5.1.1 Common Expression Syntax**                    | -                                                                                        |
| 5.1.1.1.1 Equals                                      | Supported                                                                                |
| 5.1.1.1.2 Not Equals                                  | Supported                                                                                |
| 5.1.1.1.3 Greater Than                                | Supported                                                                                |
| 5.1.1.1.3 Greater Than                                | Supported                                                                                |
| 5.1.1.1.4 Greater Than or Equal                       | Supported                                                                                |
| 5.1.1.1.5 Less Than                                   | Supported                                                                                |
| 5.1.1.1.6 Less Than or Equal                          | Supported                                                                                |
| 5.1.1.1.7 And                                         | Supported                                                                                |
| 5.1.1.1.8 Or                                          | Supported                                                                                |
| 5.1.1.1.9 Not                                         | Supported                                                                                |
| 5.1.1.1.10 Has                                        | Supported                                                                                |
| 5.1.1.1.11 In                                         | Supported                                                                                |
| 5.1.1.2.1 Addition                                    | Supported                                                                                |
| 5.1.1.2.2 Subtraction                                 | Supported                                                                                |
| 5.1.1.2.3 Negation                                    | Supported                                                                                |
| 5.1.1.2.4 Multiplication                              | Supported                                                                                |
| 5.1.1.2.5 Division                                    | Supported                                                                                |
| 5.1.1.2.6 Modulo                                      | Supported                                                                                |
| 5.1.1.3 Grouping                                      | Supported                                                                                |
| 5.1.1.5.1 concat                                      | Supported                                                                                |
| 5.1.1.5.2 contains                                    | Partially Supported (no collection contains collection)                                  |
| 5.1.1.5.3 endswith                                    | Partially Supported (string)                                                             |
| 5.1.1.5.4 indexof                                     | Partially Supported (string)                                                             |
| 5.1.1.5.5 length                                      | Supported                                                                                |
| 5.1.1.5.6 startswith                                  | Partially Supported (string)                                                             |
| 5.1.1.5.7 substring                                   | Supported                                                                                |
| 5.1.1.6 Collection Functions                          | Not planned (no LINQ equivalent)                                                         |
| 5.1.1.7 String Functions                              | Partially Supported (tolower, toupper, trim)                                             |
| 5.1.1.8 Date and Time Functions                       | Supported                                                                                |
| 5.1.1.9 Arithmetic Functions                          | Supported                                                                                |
| 5.1.1.10 Type Functions                               | Partially Supported (simple casts)                                                       |
| 5.1.1.11 Geo Functions                                | Not yet supported                                                                        |
| 5.1.1.12 Conditional Functions                        | Not planned                                                                              |
| 5.1.1.13 Lambda Operators                             | Not planned                                                                              |
| 5.1.1.14.1 Primitive Literals                         | Partially Supported (null, bool, int, double, single, string, dateTimeOffset, guid, long |
| 5.1.1.14.2 Complex and Collection Literals            | Partially Supported (no aliases)                                                         |
| 5.1.1.14.3 null                                       | Supported                                                                                |
| 5.1.1.14.4 $it                                        | Not yet supported                                                                        |
| 5.1.1.14.5 $root                                      | Not yet supported                                                                        |
| 5.1.1.14.6 $this                                      | Not yet supported                                                                        |
| 5.1.1.15 Path Expressions                             | Supported                                                                                |
| 5.1.2 System Query Option $filter                     | Supported                                                                                |
| 5.1.3 System Query Option $expand                     | Supported                                                                                |
| 5.1.4 System Query Option $select                     | Supported                                                                                |
| 5.1.5 System Query Option $orderby                    | Supported                                                                                |
| 5.1.6 System Query Options $top and $skip             | Supported                                                                                |
| 5.1.7 System Query Option $count                      | Supported                                                                                |
| 5.1.8 System Query Option $search                     | Not planned                                                                              |
| 5.1.9 System Query Option $format                     | Not planned                                                                              |
| 5.1.10 System Query Option $compute                   | Not planned                                                                              |
| 5.1.11 System Query Option $index                     | Not planned                                                                              |
| 5.1.12 System Query Option $schemaversion             | Not planned                                                                              |
| **5.2. Custom Query Options**                         | Supported                                                                                |
| **5.3. Parameter Aliases**                            | Not yet supported                                                                        |
