# Sparse

Namespace: `SparxEcs`

---

## Overview

The `Sparse` class represents a fixed-size sparse data storage page used in the ECS system. It provides efficient storage for sparse sets by internally managing an array with a predefined maximum size.

---

## Constants

- `SPARSE_MAX_SIZE` (int):  
  The maximum size of each sparse page. Set to **2048**.

---

## Fields

- `_data` (int[]):  
  Private readonly integer array that holds the sparse data internally.

---

## Constructors

### `Sparse()`

Initializes a new `Sparse` instance with the internal array `_data` filled with `-1` values indicating empty slots.

---

## Indexer

### `int this[int index]`

Gets or sets the value at the specified `index` in the sparse data array.

---

## Properties

- `Length` (int):  
  Returns the length of the internal sparse data array (`SPARSE_MAX_SIZE`).

- `Raw` (int[]):  
  Provides direct access to the internal raw data array.

---

## Usage

The `Sparse` class can be used to manage sparse sets efficiently by mapping entity IDs or component indices to values, where `-1` typically represents an empty or uninitialized slot.

