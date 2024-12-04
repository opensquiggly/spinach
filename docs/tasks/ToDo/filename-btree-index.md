# File Name B-Tree Index

## Overview
Create a new b-tree index structure to enable quick file lookups by name, similar to how we can currently look up files by their internal id.

## Requirements
- Create new b-tree structure for indexing files by name
- Enable fast file lookups using file names as keys
- Support for file name collisions (same name in different directories)
- Must be thread-safe for concurrent access

## Implementation Steps

### 1. Create B-Tree Structure
- [ ] Define file name key structure
- [ ] Create new b-tree class for file names
- [ ] Implement comparison logic for file name keys
- [ ] Add support for handling path components
- [ ] Implement collision handling

### 2. Add Core Operations
- [ ] Implement insertion operation
- [ ] Implement lookup operation
- [ ] Implement deletion operation
- [ ] Add thread synchronization
- [ ] Add error handling

### 3. Add Integration Points
- [ ] Add interface methods for file operations
- [ ] Integrate with existing file tracking
- [ ] Add cleanup mechanisms
- [ ] Implement persistence logic
- [ ] Add recovery mechanisms

### 4. Testing
- [ ] Create unit tests for core operations
- [ ] Add tests for collision handling
- [ ] Add concurrency tests
- [ ] Test large file sets
- [ ] Add performance benchmarks

## Technical Details
- Need to handle file paths as part of the key
- Consider case sensitivity requirements
- Must handle file renames efficiently
- Consider memory usage for large file sets

## Dependencies
- Existing b-tree implementation
- File tracking system
- Thread synchronization mechanisms
