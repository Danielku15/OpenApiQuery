# OpenApiQuery 

OpenApiQuery is a library inspired by OData to provide an easy way of interacting with your REST api resources. It adopts from OData parts of the query syntax as described in the OData specification [OData Version 4.01. Part 2: URL Conventions]( https://docs.oasis-open.org/odata/odata/v4.01/csprd06/part2-url-conventions/odata-v4.01-csprd06-part2-url-conventions.html)

## Framework features

| Feature           | Description                                                                   | Status            |
| ----------------  | ----------------------------------------------------------------------------- | ----------------- |
| `$select`         | Limit the returned properties                                                 | Not yet supported |
| `$filter`         | Filter result entities                                                        | Supported         |
| `$expand`         | Include related navigation properties                                         | Supported         |
| `$skip`           | Skip N elements in the result set                                             | Supported         |
| `$top`            | Select the top N elements in the result set                                   | Supported         |
| `$count`          | Provide the total count of items in the data source (with filters applied)    | Supported         |
| `Delta<T>`        | Accept a partial entity                                                       | Not yet supported |
| `SingleResult<T>` | Return a single entity with select and expand capabilities                    | Not yet supported |
| Open Types        | Extend your entities with any dynamic property                                | Not yet supported |

## OData URL Conventions Compatibility

| Feature                                       | Status |
| --------------------------------------------- | -------|
| 3. Service Root URL                           | Not planned                                                                              |
| 4. Resource Path                              | Not planned                                                                              |
| **5. Query Options**                          | -                                                                                        |
| **5.1. System Query Options**                 | Partially Supported                                                                      |
| **5.1.1 Common Expression Syntax**            | -                                                                                        |
| 5.1.1.1.1 Equals                              | Supported                                                                                |
| 5.1.1.1.2 Not Equals                          | Supported                                                                                |
| 5.1.1.1.3 Greater Than                        | Supported                                                                                |
| 5.1.1.1.3 Greater Than                        | Supported                                                                                |
| 5.1.1.1.4 Greater Than or Equal               | Supported                                                                                |
| 5.1.1.1.5 Less Than                           | Supported                                                                                |
| 5.1.1.1.6 Less Than or Equal                  | Supported                                                                                |
| 5.1.1.1.7 And                                 | Supported                                                                                |
| 5.1.1.1.8 Or                                  | Supported                                                                                |
| 5.1.1.1.9 Not                                 | Supported                                                                                |
| 5.1.1.1.10 Has                                | Supported                                                                                |
| 5.1.1.1.11 In                                 | Supported                                                                                |
| 5.1.1.2.1 Addition                            | Supported                                                                                |
| 5.1.1.2.2 Subtraction                         | Supported                                                                                |
| 5.1.1.2.3 Negation                            | Supported                                                                                |
| 5.1.1.2.4 Multiplication                      | Supported                                                                                |
| 5.1.1.2.5 Division                            | Supported                                                                                |
| 5.1.1.2.6 Modulo                              | Supported                                                                                |
| 5.1.1.3 Grouping                              | Supported                                                                                |
| 5.1.1.5.1 concat                              | Partially Supported (string)                                                             |
| 5.1.1.5.2 contains                            | Partially Supported (string)                                                             |
| 5.1.1.5.3 endswith                            | Partially Supported (string)                                                             |
| 5.1.1.5.4 indexof                             | Partially Supported (string)                                                             |
| 5.1.1.5.5 length                              | Supported                                                                                |
| 5.1.1.5.6 startswith                          | Partially Supported (string)                                                             |
| 5.1.1.5.7 substring                           | Partially Supported (string)                                                             |
| 5.1.1.6 Collection Functions                  | Not yet supported                                                                        |
| 5.1.1.7 String Functions                      | Not yet supported                                                                        |
| 5.1.1.8 Date and Time Functions               | Not yet supported                                                                        |
| 5.1.1.9 Arithmetic Functions                  | Not yet supported                                                                        |
| 5.1.1.10 Type Functions                       | Not yet supported                                                                        |
| 5.1.1.11 Geo Functions                        | Not yet supported                                                                        |
| 5.1.1.12 Conditional Functions                | Not yet supported                                                                        |
| 5.1.1.13 Lambda Operators                     | Not yet supported                                                                        |
| 5.1.1.14.1 Primitive Literals                 | Partially Supported (null, bool, int, double, single, string, dateTimeOffset, guid, long |
| 5.1.1.14.2 Complex and Collection Literals    | Partially Supported (no aliases)                                                         |
| 5.1.1.14.3 null                               | Supported                                                                                |
| 5.1.1.14.4 $it                                | Not yet supported                                                                        |
| 5.1.1.14.5 $root                              | Not yet supported                                                                        |
| 5.1.1.14.6 $this                              | Not yet supported                                                                        |
| 5.1.1.15 Path Expressions                     | Supported                                                                                |
| **5.2. Custom Query Options**                 | Supported                                                                                |
| **5.3. Parameter Aliases**                    | Not yet supported                                                                        |
                                                                    