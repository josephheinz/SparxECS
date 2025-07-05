# Changelog

## [2.0.0] - 2025-07-04

### Changed

- Namespace to `SparxECS` to match the NuGet package, breaking all current uses of the package until the `using SparxEcs;` is updated to `using SparxECS;`

## [1.0.0] - 2025-07-03

### Added

- Sparse Arrays
    - A list of pointers to indexes of other lists that functions as a page for quicker lookup times 

- Sparse Sets
    - A list of sparse arrays that contains index pointers to spots in the dense list
    - A list of instances of the type registered to this sparse set
    - A 1:1 list of indexes in the dense list to their sparse entity owner

- Component Masks
    - An array of ulongs being used to store which components an entity has, to keep from having to check every sparse set for each entity

- ECS
    - A conjoining of the three previous systems which includes a public API for adding entities and assigning them components, then querying for entities with components and changing their data

[2.0.0]: https://github.com/josephheinz/SparxECS/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/josephheinz/SparxEcs/releases/tag/v1.0.0
