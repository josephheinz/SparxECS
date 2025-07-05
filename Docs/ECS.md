# SparxECS ECS Documentation

## Overview

`ECS` (Entity Component System) is a lightweight, generic ECS implementation for managing entities and their components efficiently.

---

## Types

### `EntityID`

A readonly struct representing the ID of an entity.

- **Properties:**
  - `int Value` — The integer value of the entity ID.

- **Conversions:**
  - Implicit conversion to/from `int`.
  
- **Usage:**
  ```csharp
  EntityID id = 5;
  int rawId = id;
---

## Class: `ECS`

Manages entities and their components.

### Fields

* `int entityCount` — Number of active entities.
* `readonly int maxComponents` — Max supported component types (default 256).
* Internal collections manage component pools, type mappings, entity masks, and reusable entity IDs.

### Constructor

```csharp
public ECS()
```

Initializes internal collections.

---

### Methods

#### RegisterComponent<T>()

Registers component type `T` with the ECS.

```csharp
void RegisterComponent<T>()
```

* Creates a `SparseSet<T>` to store components.
* Maps component type `T` to an internal ID.

---

#### ValidateComponent<T>()

Checks if component type `T` is registered.

```csharp
bool ValidateComponent<T>()
```

* Returns `true` if registered, else `false`.

---

#### AddEntity()

Creates a new entity.

```csharp
EntityID AddEntity()
```

* Returns the new entity's `EntityID`.
* Reuses IDs from deleted entities if available.

---

#### Add<T>(EntityID id, T component = default!)

Adds component `T` to the entity with ID `id`.

```csharp
void Add<T>(EntityID id, T component = default!)
```

* Validates entity and component.
* Throws if component is null or not registered.
* Sets component mask bit for `T`.
* Adds the component data to the pool.

---

#### Set<T>(EntityID id, T component = default!)

Sets or replaces the component `T` on the entity.

```csharp
void Set<T>(EntityID id, T component = default!)
```

* Adds component if missing; otherwise, updates existing.
* Validates entity and component.

---

#### Get<T>(EntityID id)

Retrieves component `T` from entity if present.

```csharp
T? Get<T>(EntityID id)
```

* Returns component or `default` if missing.

---

#### Remove<T>(EntityID id)

Removes component `T` from the entity.

```csharp
void Remove<T>(EntityID id)
```

* Validates entity and component presence.
* Removes component from pool and clears mask bit.

---

#### HasComponent<T>(EntityID id)

Checks if entity has component `T`.

```csharp
bool HasComponent<T>(EntityID id)
```

* Returns `true` if component present, else `false`.

---

#### DeleteEntity(EntityID id)

Deletes entity and all its components.

```csharp
void DeleteEntity(EntityID id)
```

* Removes all components.
* Marks entity ID as reusable.

---

#### CloneEntity(EntityID id)

Creates a deep copy of the entity with all components.

```csharp
EntityID CloneEntity(EntityID id)
```

* Returns the new clone's entity ID.
* Copies all components via reflection.

---

#### CopyComponent<T>(EntityID source, EntityID destination)

Copies a component `T` from `source` entity to `destination`.

```csharp
void CopyComponent<T>(EntityID source, EntityID destination)
```

---

#### ValidateEntity(EntityID id)

Validates that the entity ID is valid and active.

```csharp
bool ValidateEntity(EntityID id)
```

---

#### Query<T>(Func\<EntityID, bool>? filter = null)

Yields all components of type `T`, optionally filtered by entity ID.

```csharp
IEnumerable<T> Query<T>(Func<EntityID, bool>? filter = null)
```

---

#### Query Overloads: Multiple Components

Supports querying entities that have multiple component types simultaneously, yielding tuples of components.

Examples:

```csharp
IEnumerable<(T1, T2)> Query<T1, T2>(Func<EntityID, bool>? filter = null)
IEnumerable<(T1, T2, T3)> Query<T1, T2, T3>(Func<EntityID, bool>? filter = null)
...
IEnumerable<(T1, T2, ..., T10)> Query<T1, T2, ..., T10>(Func<EntityID, bool>? filter = null)
```

* Returns only entities that have **all** specified components.
* Optional filter function on entity IDs.

---

## Internal Details

* Uses a `ComponentMask` per entity to track which components are present.
* Components stored in `SparseSet<T>` pools for efficient lookup.
* Entity IDs are recycled via a queue.
* Component indices are managed via `typeToId` and `idToType` mappings.
* Reflection used to clone components generically.

---

## Example Usage

```csharp
var ecs = new ECS();

// Register components
ecs.RegisterComponent<Position>();
ecs.RegisterComponent<Velocity>();

// Add entity
EntityID entity = ecs.AddEntity();

// Add components
ecs.Add(entity, new Position(10, 20));
ecs.Add(entity, new Velocity(1, 0));

// Query entities with Position and Velocity
foreach (var (pos, vel) in ecs.Query<Position, Velocity>())
{
    Console.WriteLine($"Pos: {pos}, Vel: {vel}");
}
```

---

## Notes

* `maxComponents` can be adjusted but should be a multiple of 64 for internal bitmask alignment.
* The Query methods use the smallest component pool for iteration to optimize performance.
* Throws exceptions if components are accessed without registration.
* Component `null` values are disallowed.

---
