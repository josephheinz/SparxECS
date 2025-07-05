# ComponentMask

Namespace: `SparxECS`

---

## Overview

The `ComponentMask` class manages a bitmask to represent the presence or absence of components in an ECS. It uses an array of 64-bit unsigned integers (`ulong[]`) to efficiently store component flags.

---

## Constructors

### `ComponentMask(int size = 64)`

Initializes a new `ComponentMask` with a specified size (in bits). The internal storage uses `size / 64` `ulong` elements. Default size is 64 bits.

---

## Methods

### `void Add(int bit)`

Sets the bit at the given index to 1, marking the component as present.

- `bit`: The index of the component to add.

---

### `void Set(int bit, byte value)`

Sets the bit at the given index to a specified value (0 or 1).

- `bit`: The index of the component bit to set.
- `value`: The value to set (0 or 1).

---

### `void Remove(int bit)`

Clears the bit at the given index, marking the component as absent.

- `bit`: The index of the component to remove.

---

### `bool Has(int bit)`

Checks whether the bit at the specified index is set (component present).

- `bit`: The index of the component to check.
- Returns `true` if the component is present; otherwise, `false`.

---

### `string ToBitString()`

Returns a string representation of the entire bitmask as binary digits, with 64-bit segments separated by underscores.

---

## Properties

- `int Length`  
  Returns the total number of bits in the mask (`masks.Length * 64`).

---

## Usage

The `ComponentMask` class is useful for quickly testing which components an entity has by setting, clearing, and checking bits corresponding to component indices.

