# SparseSet<T> and ISparseSet Interface

Namespace: `SparxECS`

---

## ISparseSet Interface

Defines basic operations for a sparse set.

### Methods

- `void Delete(EntityID id)`  
  Deletes an entity by its ID from the sparse set.

- `void Clear()`  
  Clears all entities from the sparse set.

- `int Size()`  
  Returns the current number of entities in the sparse set.

- `int UnsafeGetDenseToIdDirect(int index)`  
  Retrieves the entity ID directly from the dense list at the specified index. Use with caution.

---

## SparseSet<T> Class

A generic sparse set implementation managing entities and their components using sparse and dense lists.

### Fields

- `dense` (List<T>):  
  Stores the dense list of component values.

- `sparsePages` (List<Sparse>):  
  Stores sparse pages that map entity IDs to dense indices.

- `denseToId` (List<int>):  
  Maps dense indices back to entity IDs.

---

### Constructors

- `SparseSet()`  
  Initializes empty dense, sparsePages, and denseToId lists.

---

### Methods

- `void Set(EntityID id, T item)`  
  Sets or adds a component `item` for the entity with the given `id`. Updates existing or adds new.

- `bool TryGet(EntityID id, out T value)`  
  Attempts to get the component value for the given entity `id`. Returns `true` if found.

- `bool Has(EntityID id)`  
  Checks if the entity with the given `id` exists in this sparse set.

- `void Delete(EntityID id)`  
  Removes the entity and its component from the sparse set, maintaining dense list compactness.

- `void Clear()`  
  Clears the sparse set, removing all entities and components.

- `T UnsafeGetDenseDirect(int index)`  
  Directly accesses the component at a dense index. Use with caution.

- `int UnsafeGetDenseToIdDirect(int index)`  
  Directly accesses the entity ID at a dense index. Use with caution.

- `int Size()`  
  Returns the number of entities currently stored.

---

### Private Methods

- `void SetDenseIndex(EntityID id, int index)`  
  Maps an entity ID to an index in the dense list by managing sparse pages.

- `int GetDenseIndex(EntityID id)`  
  Retrieves the dense index for a given entity ID, or `-1` if not found.

---

### Properties

- `bool IsEmpty`  
  Returns `true` if the sparse set contains no entities.

---

### Usage Notes

- This sparse set implementation allows fast lookups and iteration of entities and components.  
- The internal data structures keep dense arrays for iteration efficiency and sparse arrays for quick ID-to-index mapping.  
- Unsafe methods expose internal lists directly and should be used with care.

---

